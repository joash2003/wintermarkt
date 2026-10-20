---
name: wintermarkt-contributor-tickets
description: Create, claim, investigate, fix or review Wintermarkt contributor tickets and pull requests.
---

Read CONTRIBUTING.md and docs/issues/README.md. Local Markdown tickets are authoritative until hosted issue URLs are recorded. Do not claim GitHub Issues were published when only files exist.

File with `python3 tools/issue.py new "Title" --type <type> --priority <priority>`, which reserves the next number on origin; never create ticket files by hand. See docs/issues/README.md. Include what is wrong and expected behavior, repository-relative location, observed evidence and reproduction context, a complete proposed fix/diagnostic procedure, dependencies and verification criteria. Distinguish confirmed defects, suspected defects and missing deliverables.

Claim with `python3 tools/issue.py move N in-progress`; close by writing Outcome, then `move N closed`. Use a focused N-description branch. Avoid simultaneous edits to shared scenes. Add a work log with changed files, commands and actual results. Submit a PR referencing the ticket; request peer review. Done requires acceptance evidence and review, not just written code.

Use .github issue and PR templates when a hosted tracker is available. A request to prepare a ticket does not authorize publishing or messaging other people. Preserve existing contributor work.
