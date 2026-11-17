#!/bin/bash
# Scripted players against the real simulation and data files: prints one result row per strategy.
# Not part of the quality gate: use it before and after changing market.json or the simulation rules.
set -euo pipefail
cd "$(dirname "$0")/.."
mono_root="${UNITY_MONO_ROOT:-}"
if [ -z "$mono_root" ]; then
  for unity in /Applications/Unity/Hub/Editor/*/Unity.app/Contents "/c/Program Files/Unity/Hub/Editor/"*/Editor "$HOME"/Unity/Hub/Editor/*/Editor "${UNITY_APP:-}"; do
    for candidate in "$unity/Resources/Scripting/MonoBleedingEdge" "$unity/Data/MonoBleedingEdge"; do
      if [ -f "$candidate/lib/mono/4.5/csc.exe" ]; then mono_root=$candidate; fi
    done
  done
fi
if [ -z "$mono_root" ]; then echo 'Unity not found. Install it through Unity Hub or set UNITY_APP.' >&2; exit 1; fi
mono="$mono_root/bin/mono"
if [ "${OS:-}" = Windows_NT ]; then mono="$mono.exe"; fi
mkdir -p Logs/core-tests
"$mono" "$mono_root/lib/mono/4.5/csc.exe" -nologo -target:exe -r:System.Web.Extensions.dll -out:Logs/core-tests/BalanceBots.exe Assets/_Wintermarkt/Core/*.cs tools/tests/BalanceBots.cs
"$mono" Logs/core-tests/BalanceBots.exe Assets/_Wintermarkt/Resources/Wintermarkt/market.json Assets/_Wintermarkt/Resources/Wintermarkt/site.json
