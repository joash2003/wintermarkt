#!/usr/bin/env python3
"""The issue tracker lives in the repository.

    docs/issues/<state>/<N>_<Title>.md          N = 7, or 7.00 (epic), or 7.01 (sub-issue)

The folder is the status. A number is reserved by pushing a create-only git tag
``issue/N`` to origin, so two clones or two agents can never mint the same
number; the reservation is a tag, not a commit. Git I/O goes through one
injectable runner so every rule here is unit-tested without a repository.

Ported from the iam project's tracker. Runs on macOS, Linux and Windows with
Python 3.9+: python3 tools/issue.py --help  (Windows: py tools/issue.py --help)
"""

from __future__ import annotations

import argparse
import datetime as dt
import os
import re
import shutil
import subprocess
import sys
import time
from collections.abc import Callable, Iterable
from dataclasses import dataclass
from functools import lru_cache
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
ISSUES_DIR = ROOT / "docs" / "issues"

STATES = ("open", "in-progress", "blocked", "closed", "wont-fix")
TERMINAL_STATES = frozenset({"closed", "wont-fix"})
TYPES = ("feature", "enhancement", "bug", "refactor", "chore", "epic")
PRIORITIES = ("high", "medium", "low")
TAG_PREFIX = "issue/"
MAX_RESERVE_ATTEMPTS = 10
# Only these push failures mean "someone else took it" and are worth a retry.
RETRIABLE = re.compile(r"rejected|already exists|cannot lock ref|failed to update ref|stale info")

FILENAME = re.compile(r"^(\d+)(?:\.(\d{2}))?_([A-Za-z0-9][A-Za-z0-9_]*)\.md$")
HEADING = re.compile(r"^# (\d+(?:\.\d{2})?) — (.+)$", re.MULTILINE)
REMOTE_TAG = re.compile(r"refs/tags/issue/(\d+)$")
REMOTE_FILE = re.compile(r"docs/issues/[a-z-]+/(\d+)(?:\.\d{2})?_")
REQUIRED_SECTIONS = ("## Context", "## Goal", "## Acceptance criteria", "## Tasks", "## Outcome")
OUTCOME_PLACEHOLDER = "_Filled in when closed"

GitResult = tuple[int, str, str]
GitRunner = Callable[[list[str]], GitResult]


@lru_cache(maxsize=None)
def git_env() -> dict[str, str]:
    """With several GitHub accounts logged in to gh, git's gh credential helper
    serves only the ACTIVE one, which may not own this repo. Use the token of the
    account that owns origin instead. Skipped when gh is absent or GH_TOKEN is set."""
    env = dict(os.environ)
    if "GH_TOKEN" in env or not shutil.which("gh"):
        return env
    url = subprocess.run(["git", "remote", "get-url", "origin"], cwd=ROOT, capture_output=True, text=True).stdout
    owner = re.search(r"github\.com[:/]([^/]+)/", url)
    if owner:
        token = subprocess.run(["gh", "auth", "token", "--user", owner.group(1)], capture_output=True, text=True)
        if token.returncode == 0 and token.stdout.strip():
            env["GH_TOKEN"] = token.stdout.strip()
    return env


def run_git(args: list[str]) -> GitResult:
    # S603/S607: fixed argv, `git` from PATH, arguments built from validated values.
    proc = subprocess.run(  # noqa: S603
        ["git", *args],  # noqa: S607
        cwd=ROOT,
        capture_output=True,
        text=True,
        encoding="utf-8",
        env=git_env(),
        check=False,
    )
    return proc.returncode, proc.stdout, proc.stderr


# --- front matter --------------------------------------------------------------


def parse_front_matter(text: str) -> tuple[dict[str, str], str]:
    """Split ``---`` fenced ``key: value`` lines from the body. Values keep their
    text; surrounding quotes are dropped so ``parent: '7'`` reads as ``7``."""
    lines = text.splitlines()
    if not lines or lines[0].strip() != "---":
        return {}, text
    meta: dict[str, str] = {}
    for index, line in enumerate(lines[1:], start=1):
        if line.strip() == "---":
            return meta, "\n".join(lines[index + 1 :])
        key, _, value = line.partition(":")
        if key.strip():
            meta[key.strip()] = value.strip().strip("'\"")
    return {}, text


def render_front_matter(meta: dict[str, str]) -> str:
    # Empty values render as "assignee:" with no trailing space, which `git diff --check` would flag.
    return "---\n" + "".join(f"{k}: {v}".rstrip() + "\n" for k, v in meta.items()) + "---\n"


def slugify(title: str) -> str:
    # Apostrophes vanish rather than split: "Vercel's" -> "Vercels", not "Vercel_S".
    words = re.sub(r"[^A-Za-z0-9]+", " ", title.replace("'", "").replace("\u2019", "")).split()
    return "_".join(word[:1].upper() + word[1:] for word in words)


# --- model ----------------------------------------------------------------------


@dataclass(frozen=True)
class Issue:
    path: Path
    state: str
    number: str  # "7", "7.00", "7.01"
    meta: dict[str, str]
    body: str

    @property
    def base(self) -> int:
        return int(self.number.split(".")[0])

    @property
    def sub(self) -> int | None:
        return int(self.number.split(".")[1]) if "." in self.number else None

    @property
    def title(self) -> str:
        match = HEADING.search(self.body)
        return match.group(2).strip() if match else self.path.stem

    @property
    def heading_number(self) -> str | None:
        match = HEADING.search(self.body)
        return match.group(1) if match else None


def load_issues(issues_dir: Path = ISSUES_DIR) -> list[Issue]:
    found: list[Issue] = []
    for state in STATES:
        for path in sorted((issues_dir / state).glob("*.md")):
            match = FILENAME.match(path.name)
            number = match.group(1) + (f".{match.group(2)}" if match.group(2) else "") if match else path.stem
            meta, body = parse_front_matter(path.read_text(encoding="utf-8"))
            found.append(Issue(path, state, number, meta, body))
    return found


def find_issue(number: str, issues_dir: Path = ISSUES_DIR) -> Issue:
    for issue in load_issues(issues_dir):
        if issue.number == number:
            return issue
    raise SystemExit(f"no issue {number!r} in any of {', '.join(STATES)}")


# --- reservation ----------------------------------------------------------------


def remote_numbers(git: GitRunner) -> set[int] | None:
    """Base numbers already taken on origin, from tags and from filed issues.
    None means origin could not be reached."""
    rc, tags, _ = git(["ls-remote", "--tags", "origin", f"refs/tags/{TAG_PREFIX}*"])
    if rc != 0:
        return None
    taken = {int(m.group(1)) for m in map(REMOTE_TAG.search, tags.splitlines()) if m}
    rc, files, _ = git(["ls-tree", "-r", "--name-only", "origin/main", "docs/issues"])
    if rc == 0:
        taken |= {int(m.group(1)) for m in map(REMOTE_FILE.search, files.splitlines()) if m}
    return taken


def local_numbers(issues_dir: Path = ISSUES_DIR) -> set[int]:
    return {issue.base for issue in load_issues(issues_dir)}


def reserve_number(git: GitRunner, issues_dir: Path = ISSUES_DIR, offline: bool = False) -> int:
    """Take the next free base number by pushing a create-only tag.

    Git's receive-pack refuses a tag that already exists, so of two concurrent
    attempts exactly one wins; the loser re-reads and tries the next number.
    An annotated tag with a timestamp and PID in its message gives each attempt
    a distinct object, so a same-name push is never a silent no-op.
    """
    taken = remote_numbers(git)
    if taken is None:
        if not offline:
            raise SystemExit("cannot reach origin to reserve a number (pass --offline to skip)")
        print("warning: origin unreachable - number not reserved on origin", file=sys.stderr)
        return max(local_numbers(issues_dir), default=0) + 1

    last_error = ""
    for _ in range(MAX_RESERVE_ATTEMPTS):
        candidate = max(taken | local_numbers(issues_dir), default=0) + 1
        ref = f"refs/tags/{TAG_PREFIX}{candidate}"
        stamp = f"reserved {dt.datetime.now(dt.timezone.utc).isoformat()} pid={os.getpid()} t={time.time_ns()}"
        git(["tag", "-a", f"{TAG_PREFIX}{candidate}", "-m", stamp])
        rc, _, err = git(["push", "origin", ref, f"--force-with-lease={ref}:"])
        if rc == 0:
            return candidate
        git(["tag", "-d", f"{TAG_PREFIX}{candidate}"])
        last_error = err.strip()
        if not RETRIABLE.search(err):
            break  # not a race - retrying the next number cannot help
        taken = remote_numbers(git) or taken
        taken.add(candidate)
    raise SystemExit(f"could not reserve a number - git said:\n{last_error}")


def prune_tags(git: GitRunner) -> list[int]:
    """Drop reservation tags whose issue has since been filed on origin/main."""
    rc, tags, _ = git(["ls-remote", "--tags", "origin", f"refs/tags/{TAG_PREFIX}*"])
    rc2, files, _ = git(["ls-tree", "-r", "--name-only", "origin/main", "docs/issues"])
    if rc != 0 or rc2 != 0:
        return []
    filed = {int(m.group(1)) for m in map(REMOTE_FILE.search, files.splitlines()) if m}
    tagged = {int(m.group(1)) for m in map(REMOTE_TAG.search, tags.splitlines()) if m}
    pruned = sorted(tagged & filed)
    for number in pruned:
        git(["push", "origin", "--delete", f"refs/tags/{TAG_PREFIX}{number}"])
        git(["tag", "-d", f"{TAG_PREFIX}{number}"])
    return pruned


# --- creating -------------------------------------------------------------------


def render_issue(number: str, title: str, meta: dict[str, str]) -> str:
    return (
        render_front_matter(meta) + f"\n# {number} — {title}\n\n"
        "## Context\n\n_Why this exists: what was observed, what is at stake._\n\n"
        "## Goal\n\n_The one-sentence outcome that makes this done._\n\n"
        "## Acceptance criteria\n\n- [ ] \n\n"
        "## Tasks\n\n- [ ] \n\n"
        "## Outcome\n\n"
        f"{OUTCOME_PLACEHOLDER}: what shipped, the commit(s), and what was left out and why._\n"
    )


def next_sub_number(parent: int, issues_dir: Path) -> str:
    subs = [i.sub or 0 for i in load_issues(issues_dir) if i.base == parent and i.sub is not None]
    if not subs:
        raise SystemExit(f"no epic {parent}.00 exists to attach a sub-issue to")
    return f"{parent}.{max(subs) + 1:02d}"


def append_to_epic(parent: int, line: str, issues_dir: Path) -> None:
    """Add a sub-issue line to the epic's Sub-issues list, creating the section
    (just above Outcome) the first time, and keeping later entries in order."""
    epic = find_issue(f"{parent}.00", issues_dir)
    text = epic.path.read_text(encoding="utf-8")
    if "## Sub-issues" not in text:
        text = text.replace("\n## Outcome", f"\n## Sub-issues\n\n{line}\n\n## Outcome", 1)
    else:
        start = text.index("## Sub-issues")
        end = text.index("\n## ", start + 1)
        section = text[start:end].rstrip("\n")
        text = text[:start] + f"{section}\n{line}\n" + text[end:]
    epic.path.write_text(text, encoding="utf-8")


@dataclass(frozen=True)
class NewIssue:
    title: str
    type_: str
    priority: str
    parent: int | None = None
    offline: bool = False


def create_issue(spec: NewIssue, git: GitRunner, issues_dir: Path = ISSUES_DIR) -> Path:
    if spec.type_ not in TYPES:
        raise SystemExit(f"type must be one of {', '.join(TYPES)}")
    if spec.priority not in PRIORITIES:
        raise SystemExit(f"priority must be one of {', '.join(PRIORITIES)}")

    meta: dict[str, str] = {
        "type": spec.type_,
        "priority": spec.priority,
        "created": dt.date.today().isoformat(),
        "assignee": "",
        "branch": "",
    }
    if spec.parent is not None:
        number = next_sub_number(spec.parent, issues_dir)
        meta["parent"] = f"'{spec.parent}'"
    elif spec.type_ == "epic":
        number = f"{reserve_number(git, issues_dir, spec.offline)}.00"
    else:
        number = str(reserve_number(git, issues_dir, spec.offline))

    path = issues_dir / "open" / f"{number}_{slugify(spec.title)}.md"
    path.parent.mkdir(exist_ok=True)
    path.write_text(render_issue(number, spec.title, meta), encoding="utf-8")
    if spec.parent is not None:
        append_to_epic(spec.parent, f"- [ ] {number} — {spec.title}", issues_dir)
    return path


# --- moving ---------------------------------------------------------------------


def move_issue(number: str, target: str, git: GitRunner, issues_dir: Path = ISSUES_DIR) -> Path:
    if target not in STATES:
        raise SystemExit(f"state must be one of {', '.join(STATES)}")
    issue = find_issue(number, issues_dir)
    if issue.state == target:
        return issue.path

    meta = dict(issue.meta)
    today = dt.date.today().isoformat()
    if target == "in-progress":
        meta["assignee"] = meta.get("assignee") or git(["config", "user.name"])[1].strip()
        meta["branch"] = meta.get("branch") or git(["rev-parse", "--abbrev-ref", "HEAD"])[1].strip()
    if target in TERMINAL_STATES:
        meta["closed"] = today
    elif "closed" in meta:
        del meta["closed"]

    destination = issues_dir / target / issue.path.name
    destination.parent.mkdir(exist_ok=True)  # git does not keep empty state folders
    destination.write_text(render_front_matter(meta) + issue.body, encoding="utf-8")
    issue.path.unlink()
    git(["add", "-A", "--", str(issue.path), str(destination)])
    return destination


# --- checking -------------------------------------------------------------------


def _check_shape(issue: Issue) -> list[str]:
    where = issue.path.relative_to(ROOT) if issue.path.is_relative_to(ROOT) else issue.path
    problems: list[str] = []
    if not FILENAME.match(issue.path.name):
        problems.append(f"{where}: filename must look like 7_Title.md, 7.00_Epic.md or 7.01_Sub.md")
    if issue.heading_number != issue.number:
        problems.append(f"{where}: heading must be '# {issue.number} — Title'")
    if issue.meta.get("type") not in TYPES:
        problems.append(f"{where}: front matter needs type: one of {', '.join(TYPES)}")
    if issue.meta.get("priority", "medium") not in PRIORITIES:
        problems.append(f"{where}: priority must be one of {', '.join(PRIORITIES)}")
    if not re.fullmatch(r"\d{4}-\d{2}-\d{2}", issue.meta.get("created", "")):
        problems.append(f"{where}: front matter needs created: YYYY-MM-DD")
    missing = [s for s in REQUIRED_SECTIONS if s not in issue.body]
    if missing:
        problems.append(f"{where}: missing section(s) {', '.join(missing)}")
    return problems


def _check_lifecycle(issue: Issue) -> list[str]:
    where = issue.path.relative_to(ROOT) if issue.path.is_relative_to(ROOT) else issue.path
    problems: list[str] = []
    terminal = issue.state in TERMINAL_STATES
    if terminal and not issue.meta.get("closed"):
        problems.append(f"{where}: {issue.state} issues need closed: YYYY-MM-DD")
    if not terminal and issue.meta.get("closed"):
        problems.append(f"{where}: still carries closed: while in {issue.state}")
    if issue.state == "closed" and OUTCOME_PLACEHOLDER in issue.body:
        problems.append(f"{where}: closed without writing the Outcome - say what shipped")
    return problems


def _check_relations(issues: list[Issue]) -> list[str]:
    problems: list[str] = []
    numbers = [i.number for i in issues]
    problems.extend(
        f"issue {duplicate} exists more than once"
        for duplicate in sorted({n for n in numbers if numbers.count(n) > 1})
    )
    epics = {i.base for i in issues if i.sub == 0}
    for issue in issues:
        where = issue.path.relative_to(ROOT) if issue.path.is_relative_to(ROOT) else issue.path
        if issue.sub == 0 and issue.meta.get("type") != "epic":
            problems.append(f"{where}: a .00 file must have type: epic")
        if issue.sub not in (None, 0):
            if issue.base not in epics:
                problems.append(f"{where}: sub-issue without an epic {issue.base}.00")
            if issue.meta.get("parent") != str(issue.base):
                problems.append(f"{where}: sub-issue needs parent: '{issue.base}'")
    return problems


def _check_reserved(issues: list[Issue], git: GitRunner) -> list[str]:
    taken = remote_numbers(git)
    if taken is None:
        print("warning: origin unreachable - reservation not verified", file=sys.stderr)
        return []
    return [
        f"issue {i.number}: number {i.base} is neither tagged on origin nor filed there"
        for i in issues
        if i.base not in taken
    ]


def check(git: GitRunner, issues_dir: Path = ISSUES_DIR, offline: bool = False) -> list[str]:
    issues = load_issues(issues_dir)
    problems: list[str] = []
    for issue in issues:
        problems += _check_shape(issue) + _check_lifecycle(issue)
    problems += _check_relations(issues)
    if not offline:
        problems += _check_reserved(issues, git)
    return problems


# --- cli -------------------------------------------------------------------------


def print_list(issues: Iterable[Issue]) -> None:
    by_state: dict[str, list[Issue]] = {s: [] for s in STATES}
    for issue in issues:
        by_state[issue.state].append(issue)
    for state in STATES:
        if not by_state[state]:
            continue
        print(f"{state} ({len(by_state[state])})")
        for issue in sorted(by_state[state], key=lambda i: (i.base, i.sub or -1)):
            tag = f"{issue.meta.get('type', '?')}/{issue.meta.get('priority', '-')}"
            print(f"  {issue.number:>7}  {tag:<19} {issue.title}")


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(prog="issue", description=__doc__.split("\n\n")[0])
    sub = parser.add_subparsers(dest="cmd", required=True)

    new = sub.add_parser("new", help="reserve a number and file an issue under open/")
    new.add_argument("title")
    new.add_argument("--type", dest="type_", default="chore", choices=TYPES)
    new.add_argument("--priority", default="medium", choices=PRIORITIES)
    new.add_argument("--parent", type=int, help="attach as a sub-issue of epic N")
    new.add_argument("--offline", action="store_true", help="skip the origin reservation")

    mv = sub.add_parser("move", help="move an issue to another state")
    mv.add_argument("number")
    mv.add_argument("state", choices=STATES)

    sub.add_parser("list", help="issues grouped by state")
    sub.add_parser("show", help="print an issue").add_argument("number")
    sub.add_parser("next", help="the next number that would be reserved")
    sub.add_parser("prune", help="delete reservation tags whose issue is on main")
    chk = sub.add_parser("check", help="validate every issue file")
    chk.add_argument("--offline", action="store_true")

    args = parser.parse_args(argv)
    if args.cmd == "new":
        spec = NewIssue(args.title, args.type_, args.priority, args.parent, args.offline)
        print(create_issue(spec, run_git).relative_to(ROOT))
    elif args.cmd == "move":
        print(move_issue(args.number, args.state, run_git).relative_to(ROOT))
    elif args.cmd == "list":
        print_list(load_issues())
    elif args.cmd == "show":
        print(find_issue(args.number).path.read_text(encoding="utf-8"), end="")
    elif args.cmd == "next":
        taken = remote_numbers(run_git) or set()
        print(max(taken | local_numbers(), default=0) + 1)
    elif args.cmd == "prune":
        print("pruned:", prune_tags(run_git) or "nothing")
    elif args.cmd == "check":
        problems = check(run_git, offline=args.offline)
        for problem in problems:
            print(problem)
        print(f"{len(load_issues())} issue file(s), {len(problems)} problem(s)")
        return 1 if problems else 0
    return 0


if __name__ == "__main__":
    sys.exit(main())
