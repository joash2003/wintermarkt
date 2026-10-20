#!/bin/bash
set -euo pipefail
project_dir="$(cd "$(dirname "$0")/.." && pwd)"
unity_editor="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.6.4f1/Unity.app/Contents/MacOS/Unity}"
if [ ! -x "$unity_editor" ]; then echo 'Set UNITY_EDITOR to the Unity 6000.6.4f1 executable.' >&2; exit 1; fi
mkdir -p "$project_dir/Logs"
case "${1:-}" in
  setup) exec "$unity_editor" -batchmode -nographics -quit -projectPath "$project_dir" -executeMethod Wintermarkt.Editor.WintermarktProjectBootstrap.CreateOrRepair -logFile "$project_dir/Logs/wintermarkt-bootstrap.log" ;;
  data) exec "$unity_editor" -batchmode -nographics -quit -projectPath "$project_dir" -executeMethod Wintermarkt.Editor.WintermarktProjectBootstrap.ValidateData -logFile "$project_dir/Logs/wintermarkt-data.log" ;;
  test) exec "$unity_editor" -batchmode -nographics -projectPath "$project_dir" -runTests -testPlatform EditMode -testResults "$project_dir/Logs/wintermarkt-editmode.xml" -logFile "$project_dir/Logs/wintermarkt-editmode.log" ;;
  build) exec "$unity_editor" -batchmode -nographics -quit -projectPath "$project_dir" -executeMethod Wintermarkt.Editor.WintermarktProjectBootstrap.BuildRelease -logFile "$project_dir/Logs/wintermarkt-build.log" ;;
  legacy-setup) exec "$unity_editor" -batchmode -nographics -quit -projectPath "$project_dir" -executeMethod EchoMaze.Editor.EchoMazeProjectBootstrap.Create -logFile "$project_dir/Logs/legacy-bootstrap.log" ;;
  *) echo 'Usage: bash tools/unity.sh setup|data|test|build|legacy-setup' >&2; exit 2 ;;
esac
