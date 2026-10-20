#!/bin/bash
set -euo pipefail
project_dir="$(cd "$(dirname "$0")/.." && pwd)"
mono_root="${UNITY_MONO_ROOT:-/Applications/Unity/Hub/Editor/6000.6.4f1/Unity.app/Contents/Resources/Scripting/MonoBleedingEdge}"
mkdir -p "$project_dir/Logs/core-tests"
"$mono_root/bin/mono" "$mono_root/lib/mono/4.5/csc.exe" -nologo -target:exe -out:"$project_dir/Logs/core-tests/MarketTests.exe" "$project_dir"/Assets/_Wintermarkt/Core/*.cs "$project_dir/Assets/_Wintermarkt/Tests/MarketTestCases.cs" "$project_dir/tools/tests/MarketRunner.cs"
"$mono_root/bin/mono" "$project_dir/Logs/core-tests/MarketTests.exe"
