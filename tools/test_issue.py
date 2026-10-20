"""Every rule of the tracker, exercised against a temp tree and a fake git.

Standard library only, so it runs on every team machine: python3 -m unittest discover -s tools
"""

from __future__ import annotations

import contextlib
import datetime as dt
import io
import tempfile
import unittest
from pathlib import Path

import issue as tracker


class FakeGit:
    """Records calls; answers ls-remote/ls-tree from configured state and lets a
    test decide which tag pushes are rejected."""

    def __init__(self, tags=None, filed=None, reject=None):
        self.tags = set(tags or set())
        self.filed = set(filed or set())
        self.reject = set(reject or set())
        self.calls: list[list[str]] = []
        self.offline = False

    def __call__(self, args: list[str]) -> tuple[int, str, str]:
        self.calls.append(args)
        if self.offline:
            return 128, "", "could not read from remote"
        if args[:2] == ["ls-remote", "--tags"]:
            return 0, "".join(f"sha\trefs/tags/issue/{n}\n" for n in sorted(self.tags)), ""
        if args[:1] == ["ls-tree"]:
            return 0, "".join(f"docs/issues/open/{n}_X.md\n" for n in sorted(self.filed)), ""
        if args[:1] == ["push"] and "--delete" not in args:
            number = int(args[2].rsplit("/", 1)[1])
            if number in self.reject:
                return 1, "", "! [rejected] (already exists)"
            self.tags.add(number)
            return 0, "", ""
        if args[:2] == ["config", "user.name"]:
            return 0, "Joash\n", ""
        if args[:1] == ["rev-parse"]:
            return 0, "main\n", ""
        return 0, "", ""


class TrackerTest(unittest.TestCase):
    def setUp(self):
        temp = tempfile.TemporaryDirectory()
        self.addCleanup(temp.cleanup)
        self.tree = Path(temp.name)
        for state in ("open", "wont-fix"):
            (self.tree / state).mkdir()

    def file(self, git, title="Rotate The Password", type_="chore", priority="medium", parent=None) -> Path:
        return tracker.create_issue(tracker.NewIssue(title, type_, priority, parent), git, self.tree)

    # --- helpers ----------------------------------------------------------------

    def test_slug_is_title_cased_words_joined_by_underscores(self):
        self.assertEqual(tracker.slugify("rotate the supabase password!"), "Rotate_The_Supabase_Password")
        self.assertEqual(tracker.slugify("  api: v2 / CORS  "), "Api_V2_CORS")
        self.assertEqual(tracker.slugify("Move DNS to Vercel's records"), "Move_DNS_To_Vercels_Records")

    def test_front_matter_round_trips_and_drops_quotes(self):
        meta, body = tracker.parse_front_matter("---\ntype: bug\nparent: '7'\n---\n# 7.01 — X\n")
        self.assertEqual(meta, {"type": "bug", "parent": "7"})
        self.assertTrue(body.startswith("# 7.01"))
        self.assertEqual(tracker.render_front_matter(meta), "---\ntype: bug\nparent: 7\n---\n")

    def test_text_without_a_fence_has_no_front_matter(self):
        self.assertEqual(tracker.parse_front_matter("# just a heading\n"), ({}, "# just a heading\n"))

    # --- reservation --------------------------------------------------------------

    def test_reservation_takes_max_of_tags_and_filed_plus_one(self):
        git = FakeGit(tags={3}, filed={5})
        self.assertEqual(tracker.reserve_number(git, self.tree), 6)
        self.assertIn(6, git.tags)
        pushed = [c for c in git.calls if c[:1] == ["push"]]
        self.assertEqual(pushed, [["push", "origin", "refs/tags/issue/6", "--force-with-lease=refs/tags/issue/6:"]])

    def test_a_rejected_push_retries_with_the_next_number(self):
        git = FakeGit(tags={3}, reject={4})
        self.assertEqual(tracker.reserve_number(git, self.tree), 5)
        self.assertIn(["tag", "-d", "issue/4"], git.calls)

    def test_offline_reservation_needs_the_flag_and_warns(self):
        git = FakeGit()
        git.offline = True
        with self.assertRaisesRegex(SystemExit, "cannot reach origin"):
            tracker.reserve_number(git, self.tree)
        err = io.StringIO()
        with contextlib.redirect_stderr(err):
            self.assertEqual(tracker.reserve_number(git, self.tree, offline=True), 1)
        self.assertIn("not reserved on origin", err.getvalue())

    def test_a_non_race_push_failure_is_reported_once_with_the_reason(self):
        class DeniedGit(FakeGit):
            def __call__(self, args):
                if args[:1] == ["push"]:
                    self.calls.append(args)
                    return 128, "", "remote: Permission to joash2003/wintermarkt.git denied"
                return super().__call__(args)

        git = DeniedGit()
        with self.assertRaisesRegex(SystemExit, "Permission to .* denied"):
            tracker.reserve_number(git, self.tree)
        self.assertEqual(len([c for c in git.calls if c[:1] == ["push"]]), 1)

    def test_prune_removes_tags_whose_issue_has_landed(self):
        git = FakeGit(tags={1, 2, 3}, filed={1, 3})
        self.assertEqual(tracker.prune_tags(git), [1, 3])
        self.assertIn(["push", "origin", "--delete", "refs/tags/issue/1"], git.calls)

    # --- creating -------------------------------------------------------------------

    def test_new_issue_lands_in_open_with_the_template(self):
        path = self.file(FakeGit(), "Rotate The Password", type_="bug", priority="high")
        self.assertEqual(path, self.tree / "open" / "1_Rotate_The_Password.md")
        text = path.read_text(encoding="utf-8")
        self.assertTrue(text.startswith("---\ntype: bug\npriority: high\ncreated: " + dt.date.today().isoformat()))
        self.assertIn("\n# 1 — Rotate The Password\n", text)
        for section in tracker.REQUIRED_SECTIONS:
            self.assertIn(section, text)

    def test_epic_gets_dot_zero_zero_and_sub_issues_attach_to_it(self):
        git = FakeGit()
        epic = self.file(git, "Deployment", type_="epic")
        self.assertEqual(epic.name, "1.00_Deployment.md")
        sub = self.file(git, "DNS Records", parent=1)
        self.assertEqual(sub.name, "1.01_DNS_Records.md")
        self.assertIn("parent: '1'", sub.read_text(encoding="utf-8"))
        self.file(git, "Env Vars", parent=1)
        epic_text = epic.read_text(encoding="utf-8")
        listing = epic_text[epic_text.index("## Sub-issues") : epic_text.index("## Outcome")]
        self.assertEqual(listing.splitlines()[2:4], ["- [ ] 1.01 — DNS Records", "- [ ] 1.02 — Env Vars"])
        # Sub-issues never reserve: exactly one tag was pushed, for the epic.
        self.assertEqual(
            [c for c in git.calls if c[:1] == ["push"]],
            [["push", "origin", "refs/tags/issue/1", "--force-with-lease=refs/tags/issue/1:"]],
        )

    def test_sub_issue_without_an_epic_is_refused(self):
        with self.assertRaisesRegex(SystemExit, "no epic 9.00"):
            self.file(FakeGit(), "Orphan", parent=9)

    def test_bad_type_or_priority_is_refused(self):
        with self.assertRaisesRegex(SystemExit, "type must be"):
            self.file(FakeGit(), "X", type_="wish")
        with self.assertRaisesRegex(SystemExit, "priority must be"):
            self.file(FakeGit(), "X", priority="urgent")

    # --- moving ---------------------------------------------------------------------

    def test_moving_to_in_progress_stamps_assignee_and_branch(self):
        git = FakeGit()
        self.file(git)
        moved = tracker.move_issue("1", "in-progress", git, self.tree)
        self.assertEqual(moved.parent.name, "in-progress")
        meta, _ = tracker.parse_front_matter(moved.read_text(encoding="utf-8"))
        self.assertEqual((meta["assignee"], meta["branch"]), ("Joash", "main"))
        self.assertFalse((self.tree / "open" / moved.name).exists())
        self.assertTrue(any(c[:2] == ["add", "-A"] for c in git.calls))

    def test_closing_stamps_the_date_and_reopening_clears_it(self):
        git = FakeGit()
        self.file(git)
        closed = tracker.move_issue("1", "closed", git, self.tree)
        meta = tracker.parse_front_matter(closed.read_text(encoding="utf-8"))[0]
        self.assertEqual(meta["closed"], dt.date.today().isoformat())
        reopened = tracker.move_issue("1", "open", git, self.tree)
        self.assertNotIn("closed", tracker.parse_front_matter(reopened.read_text(encoding="utf-8"))[0])

    def test_moving_to_the_same_state_is_a_no_op(self):
        git = FakeGit()
        path = self.file(git)
        self.assertEqual(tracker.move_issue("1", "open", git, self.tree), path)

    # --- checking -------------------------------------------------------------------

    def test_a_freshly_filed_tree_passes_every_check(self):
        git = FakeGit()
        self.file(git, "Alpha", type_="epic")
        self.file(git, "Beta", parent=1)
        self.file(git, "Gamma")
        self.assertEqual(tracker.check(git, self.tree), [])

    def test_check_catches_each_shape_fault(self):
        bad = self.tree / "open" / "7_Bad.md"
        bad.write_text("---\ntype: wish\ncreated: yesterday\n---\n# 8 — Wrong Number\n\n## Context\n", encoding="utf-8")
        problems = "\n".join(tracker.check(FakeGit(tags={7}), self.tree))
        self.assertIn("heading must be '# 7 — Title'", problems)
        self.assertIn("needs type", problems)
        self.assertIn("created: YYYY-MM-DD", problems)
        self.assertIn("missing section(s)", problems)

    def test_check_refuses_closing_with_a_placeholder_outcome(self):
        git = FakeGit()
        self.file(git)
        tracker.move_issue("1", "closed", git, self.tree)
        self.assertTrue(any("closed without writing the Outcome" in p for p in tracker.check(git, self.tree)))

    def test_check_catches_lifecycle_and_relation_faults(self):
        git = FakeGit(tags={1, 2, 3})
        self.file(git, "Alpha")
        path = self.file(git, "Beta")
        # a terminal issue with no closed: date, and a duplicate number in another folder
        (self.tree / "wont-fix" / path.name).write_text(path.read_text(encoding="utf-8"), encoding="utf-8")
        orphan = self.tree / "open" / "3.01_Orphan.md"
        orphan.write_text(
            tracker.render_issue("3.01", "Orphan", {"type": "chore", "created": "2026-01-01"}), encoding="utf-8"
        )
        problems = "\n".join(tracker.check(git, self.tree))
        self.assertIn("need closed:", problems)
        self.assertIn("exists more than once", problems)
        self.assertIn("sub-issue without an epic 3.00", problems)
        self.assertIn("needs parent: '3'", problems)

    def test_check_flags_unreserved_numbers_online_and_skips_offline(self):
        git = FakeGit()
        self.file(git)  # reserves 1
        (self.tree / "open" / "2_Unreserved.md").write_text(
            tracker.render_issue("2", "Unreserved", {"type": "chore", "created": "2026-01-01"}), encoding="utf-8"
        )
        self.assertTrue(any("neither tagged on origin nor filed" in p for p in tracker.check(git, self.tree)))
        self.assertFalse(any("neither tagged" in p for p in tracker.check(git, self.tree, offline=True)))
        git.offline = True
        err = io.StringIO()
        with contextlib.redirect_stderr(err):
            self.assertFalse(any("neither tagged" in p for p in tracker.check(git, self.tree)))
        self.assertIn("reservation not verified", err.getvalue())


if __name__ == "__main__":
    unittest.main()
