#!/usr/bin/env python3
"""Compile the market assemblies against the installed Unity 6.4 C# references without launching UPM."""
import os, shlex, subprocess, sys
from pathlib import Path

root = Path(__file__).resolve().parents[1]
unity = Path(os.environ.get('UNITY_APP', '/Applications/Unity/Hub/Editor/6000.6.4f1/Unity.app/Contents'))
managed = unity / 'Resources/Scripting/Managed'
engine = managed / 'UnityEngine'
dotnet = unity / 'Resources/Scripting/DotNetSdk/dotnet'
csc = unity / 'Resources/Scripting/DotNetSdk/sdk/8.0.318/Roslyn/bincore/csc.dll'
netstandard = unity / 'Resources/Scripting/NetStandard/ref/2.1.0/netstandard.dll'
ui = unity / 'Resources/PackageManager/ProjectTemplates/libcache/com.unity.template.2d-cross-platform-2d-7.0.0/ScriptAssemblies/UnityEngine.UI.dll'
if not csc.exists() or not ui.exists() or not netstandard.exists():
    raise SystemExit('Unity 6000.6.4f1 references not found. Set UNITY_APP to Unity.app/Contents.')
logs = root / 'Logs/market-compile'; logs.mkdir(parents=True, exist_ok=True)
refs = [netstandard, managed/'UnityEngine.dll', managed/'UnityEditor.dll'] + list(engine.glob('*.dll'))
refs = [p for p in refs if p.exists()]
refs.append(ui)
def compile(name, files, extra=()):
    output = logs / (name + '.dll')
    command = [str(dotnet), str(csc), '-nologo', '-nostdlib+', '-target:library', '-out:' + str(output)]
    command += ['-reference:' + str(p) for p in refs]
    command += ['-reference:' + str(p) for p in extra]
    command += [str(p) for p in files]
    print('Compiling ' + name + '...')
    result = subprocess.run(command, cwd=root, text=True, stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
    (logs / (name + '.log')).write_text(result.stdout)
    if result.stdout: print(result.stdout, end='')
    if result.returncode: raise SystemExit(result.returncode)
    return output
core_files = sorted((root/'Assets/_Wintermarkt/Core').glob('*.cs'))
core = compile('Wintermarkt.Core', core_files)
runtime_files = sorted((root/'Assets/_Wintermarkt/Runtime').glob('*.cs'))
runtime = compile('Wintermarkt.Runtime', runtime_files, [core])
editor_files = sorted((root/'Assets/_Wintermarkt/Editor').glob('*.cs'))
if editor_files:
    compile('Wintermarkt.Editor', editor_files, [core, runtime])
print('Unity API assemblies compiled. This is not a Unity Editor import or Play Mode run.')
