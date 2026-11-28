#!/usr/bin/env python3
"""Compile the market assemblies against the installed Unity C# references, at Unity's C# 9 language level, without launching UPM."""
import json, os, re, shlex, subprocess, sys
from pathlib import Path

root = Path(__file__).resolve().parents[1]
hubs = [Path('/Applications/Unity/Hub/Editor'), Path('C:/Program Files/Unity/Hub/Editor'), Path.home() / 'Unity/Hub/Editor']
installed = sorted(p for hub in hubs for p in [*hub.glob('*/Unity.app/Contents'), *hub.glob('*/Editor')])
# The version ProjectVersion.txt pins wins when it is installed (issue 45); otherwise the newest by glob order.
pinned = re.search(r'm_EditorVersion: *(\S+)', (root / 'ProjectSettings/ProjectVersion.txt').read_text()).group(1)
preferred = [p for p in installed if pinned in p.parts]
unity = Path(os.environ['UNITY_APP']) if 'UNITY_APP' in os.environ else (preferred or installed)[-1] if installed else Path('/missing')
data = unity / 'Resources/Scripting' if (unity / 'Resources/Scripting').is_dir() else unity / 'Data'
packages = unity / 'Resources/PackageManager' if (unity / 'Resources/PackageManager').is_dir() else data / 'Resources/PackageManager'
managed = data / 'Managed'
engine = managed / 'UnityEngine'
mono = data / 'MonoBleedingEdge/bin' / ('mono.exe' if os.name == 'nt' else 'mono')
csc = data / 'MonoBleedingEdge/lib/mono/4.5/csc.exe'
netstandard = data / 'NetStandard/ref/2.1.0/netstandard.dll'
ui = next(iter(sorted(packages.glob('ProjectTemplates/libcache/*/ScriptAssemblies/UnityEngine.UI.dll'))), Path('/missing'))
if not mono.exists() or not csc.exists() or not ui.exists() or not netstandard.exists():
    raise SystemExit('Unity references not found. Install Unity through Unity Hub or set UNITY_APP to Unity.app/Contents (macOS) or the version\'s Editor folder (Windows/Linux).')
logs = root / 'Logs/market-compile'; logs.mkdir(parents=True, exist_ok=True)
# Only the engine modules the manifest enables, directly or through a dependency, are referenced (issue 29): Unity rejects
# code that uses a disabled module, so the check must too. Modules without a built-in package (Core, InputLegacy, ...) are always on.
builtin = packages / 'BuiltInPackages'
lock = root / 'Packages/packages-lock.json'
graph = {name: info.get('dependencies', {}) for name, info in json.loads(lock.read_text())['dependencies'].items()} if lock.exists() else {}
def dependencies(name):
    if name in graph: return graph[name]
    package = builtin / name / 'package.json'
    return json.loads(package.read_text()).get('dependencies', {}) if package.exists() else {}
enabled, todo = set(), list(json.loads((root / 'Packages/manifest.json').read_text())['dependencies'])
while todo:
    name = todo.pop()
    if name not in enabled: enabled.add(name); todo += dependencies(name)
prefix = 'com.unity.modules.'
packaged = {p.name[len(prefix):] for p in builtin.glob(prefix + '*') if p.is_dir()}
modules = {n[len(prefix):] for n in enabled if n.startswith(prefix)}
def wanted(dll):
    if not dll.name.endswith('Module.dll'): return True
    module = dll.stem[len('UnityEngine.'):-len('Module')].lower()
    return module not in packaged or module in modules
refs = [netstandard, managed/'UnityEngine.dll', managed/'UnityEditor.dll'] + list(engine.glob('*.dll'))
refs = [p for p in refs if p.exists()]
refs.append(ui)
runtime_refs = [p for p in refs if wanted(p)]   # Unity gives Editor assemblies every module, player assemblies only the enabled ones
def compile(name, files, extra=(), editor=False):
    output = logs / (name + '.dll')
    command = [str(mono), str(csc), '-nologo', '-noconfig', '-langversion:9.0', '-nostdlib+', '-target:library', '-out:' + str(output)]
    command += ['-reference:' + str(p) for p in (refs if editor else runtime_refs)]
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
    compile('Wintermarkt.Editor', editor_files, [core, runtime], editor=True)
print('Unity API assemblies compiled against ' + unity.parts[-2] + ' with ' + str(len(modules)) + ' enabled modules. This is not a Unity Editor import or Play Mode run.')
