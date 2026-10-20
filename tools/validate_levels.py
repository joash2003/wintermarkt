#!/usr/bin/env python3
"""Independent JSON/map checks; does not compile or test Unity runtime code."""
import json
from collections import deque
from pathlib import Path

def route(rows, start, end):
    seen = {start}
    queue = deque([start])
    while queue:
        x, y = queue.popleft()
        if (x, y) == end:
            return True
        for nx, ny in ((x+1,y),(x-1,y),(x,y+1),(x,y-1)):
            if 0 <= ny < len(rows) and 0 <= nx < len(rows[0]) and rows[ny][nx] != '#' and (nx,ny) not in seen:
                seen.add((nx,ny))
                queue.append((nx,ny))
    return False

def validate(path, expected):
    data = json.loads(path.read_text())
    rows = data['rows']
    width, height, count = expected
    assert data['id'] == int(path.stem[-2:]), f'{path}: wrong id'
    assert len(rows) == height and all(len(r) == width for r in rows), f'{path}: dimensions'
    positions = {c: [] for c in 'PEX'}
    for y, row in enumerate(rows):
        for x, c in enumerate(row):
            assert c in '#.PEX', f'{path}: unsupported symbol {c}'
            assert c == '#' or (0 < x < width-1 and 0 < y < height-1), f'{path}: open border'
            if c in positions:
                positions[c].append((x,y))
    assert len(positions['P']) == len(positions['X']) == 1, f'{path}: P/X count'
    assert route(rows, positions['P'][0], positions['X'][0]), f'{path}: exit unreachable'
    assert len(positions['E']) == len(data['enemyPatrols']) == count, f'{path}: enemy count'
    for spawn, patrol in zip(positions['E'], data['enemyPatrols']):
        assert spawn == (patrol['spawnX'], patrol['spawnY']), f'{path}: spawn/order mismatch'
        assert patrol['waypoints'], f'{path}: empty patrol'
        for w in patrol['waypoints']:
            assert route(rows, spawn, (w['x'],w['y'])), f'{path}: invalid patrol waypoint'
    print(f'PASS {path.name}: {width}x{height}, reachable exit, {count} valid enemy patrols')

if __name__ == '__main__':
    root = Path(__file__).resolve().parents[1] / 'Assets/_EchoMaze/Resources/Levels'
    for n, expected in enumerate(((13,9,0),(17,11,1),(21,13,2)),1):
        validate(root / f'level_{n:02}.json', expected)
