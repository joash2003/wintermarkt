#!/usr/bin/env bash
# Quality gate. The pre-commit hook runs it before every commit; run it by hand any time.
# Never pipe it into head/tail/grep: the pipe exits with the filter's status and a red gate reads green.
# ponytail: checks the working tree, not only the staged files; stage everything you changed before committing.
set -uo pipefail
cd "$(dirname "$0")/.."

# Windows often has only `python` or `py`, and its `python3` can be a Microsoft Store stub that fails.
py=python3
for candidate in python3 python py; do
  if "$candidate" -c 'import sys; sys.exit(sys.version_info < (3, 9))' >/dev/null 2>&1; then py=$candidate; break; fi
done

failed=()
check() {
  local name=$1; shift
  printf '  %-26s ' "$name"
  local out
  if out=$("$@" 2>&1); then
    printf '\033[32mPASS\033[0m\n'
  else
    printf '\033[31mFAIL\033[0m\n'
    printf '%s\n' "$out" | tail -25 | sed 's/^/      /'
    failed+=("$name")
  fi
}

json_data() {
  local f bad=0
  while IFS= read -r f; do
    "$py" -m json.tool "$f" >/dev/null || { echo "invalid JSON: $f"; bad=1; }
  done < <(git ls-files 'Assets/*.json')
  return $bad
}

echo "Quality gate"
check "Secret scan (staged)" gitleaks git --staged --no-banner --redact
check "Whitespace + conflicts" git diff --cached --check
check "JSON data" json_data
check "Issue files" "$py" -B tools/issue.py check --offline
check "Issue tool tests" "$py" -B -m unittest discover -s tools -p "test_*.py"
check "C# compile (Unity API)" "$py" -B tools/compile-market.py
check "Core tests" bash tools/test-market-core.sh

if [ ${#failed[@]} -gt 0 ]; then
  printf '\n\033[31mFAILED:\033[0m %s\n' "$(IFS=,; echo "${failed[*]}")"
  exit 1
fi
printf '\n\033[32mPASSED\033[0m\n'
