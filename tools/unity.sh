#!/bin/bash
set -euo pipefail
project_dir="$(cd "$(dirname "$0")/.." && pwd)"
unity_editor="${UNITY_EDITOR:-}"
if [ -z "$unity_editor" ]; then
  pinned=$(sed -n 's/^m_EditorVersion: //p' "$project_dir/ProjectSettings/ProjectVersion.txt" | tr -d '\r')
  # Newest installed Unity on macOS, Windows (Git Bash) or Linux; the pinned version wins when it is installed.
  # ponytail: "newest" is glob order, so a future 6000.10 sorts before 6000.9; switch to a version sort if that happens.
  for candidate in /Applications/Unity/Hub/Editor/*/Unity.app/Contents/MacOS/Unity "/c/Program Files/Unity/Hub/Editor/"*/Editor/Unity.exe "$HOME"/Unity/Hub/Editor/*/Editor/Unity \
    "/Applications/Unity/Hub/Editor/$pinned/Unity.app/Contents/MacOS/Unity" "/c/Program Files/Unity/Hub/Editor/$pinned/Editor/Unity.exe" "$HOME/Unity/Hub/Editor/$pinned/Editor/Unity"; do
    if [ -x "$candidate" ]; then unity_editor=$candidate; fi
  done
fi
if [ ! -x "$unity_editor" ]; then echo 'Unity not found. Install it through Unity Hub or set UNITY_EDITOR to the Unity executable.' >&2; exit 1; fi
mkdir -p "$project_dir/Logs"
case "${1:-}" in
  setup) exec "$unity_editor" -batchmode -nographics -quit -projectPath "$project_dir" -executeMethod Wintermarkt.Editor.WintermarktProjectBootstrap.CreateOrRepair -logFile "$project_dir/Logs/wintermarkt-bootstrap.log" ;;
  data) exec "$unity_editor" -batchmode -nographics -quit -projectPath "$project_dir" -executeMethod Wintermarkt.Editor.WintermarktProjectBootstrap.ValidateData -logFile "$project_dir/Logs/wintermarkt-data.log" ;;
  test) exec "$unity_editor" -batchmode -nographics -projectPath "$project_dir" -runTests -testPlatform EditMode -testResults "$project_dir/Logs/wintermarkt-editmode.xml" -logFile "$project_dir/Logs/wintermarkt-editmode.log" ;;
  build) exec "$unity_editor" -batchmode -nographics -quit -projectPath "$project_dir" -executeMethod Wintermarkt.Editor.WintermarktProjectBootstrap.BuildRelease -logFile "$project_dir/Logs/wintermarkt-build.log" ;;
  legacy-setup) exec "$unity_editor" -batchmode -nographics -quit -projectPath "$project_dir" -executeMethod EchoMaze.Editor.EchoMazeProjectBootstrap.Create -logFile "$project_dir/Logs/legacy-bootstrap.log" ;;
  *) echo "Usage: bash tools/unity.sh setup|data|test|build|legacy-setup  (Unity: $unity_editor)" >&2; exit 2 ;;
esac
