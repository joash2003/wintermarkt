#!/usr/bin/env python3
"""Generate Assets/_Wintermarkt/Resources/Wintermarkt/site.json from the Volksgarten geography below.

Every point was traced on a north-up Google Maps screenshot (map + satellite, 2026-10-20) and is
written as screen pixels; P() turns them into metres, x east and y north, origin Goethekreuzung.
SCALE is approximate (see docs/LOCATION_AND_TRANSIT.md). Change the data here and re-run; never
hand-edit site.json. Run: python3 tools/build_site.py
"""
import json, math, random
from pathlib import Path

ORIGIN = (370, 90)     # Goethekreuzung crossing on the reference screenshot
SCALE = 0.45           # metres per screenshot pixel, +-15%
CELL = 4               # metres per grid cell, matches the stall footprints in market.json
PLOT_RADIUS = 70       # metres from the Goethekreuzung entrance: only this corner of the park is the playable market (issue 21)

def P(px, py): return ((px - ORIGIN[0]) * SCALE, (ORIGIN[1] - py) * SCALE)
def Ps(*pts): return [P(*p) for p in pts]

PARK = Ps((62, 222), (350, 100), (372, 104), (532, 368), (520, 380), (520, 532), (398, 735), (380, 735), (100, 322))
PATHS = [  # principal paths only
    Ps((80, 245), (335, 140)),
    Ps((335, 140), (350, 300), (370, 440), (378, 560), (386, 728)),
    Ps((105, 300), (200, 330), (265, 385), (370, 440)),
    Ps((370, 440), (470, 445), (515, 450)),
    Ps((366, 108), (430, 265), (470, 420), (470, 445)),
]
STREETS = [  # (name, centreline, width m); unnamed where the map shows no label
    ("Stelzhamerstraße", Ps((30, 212), (372, 82)), 14),
    ("Goethestraße", Ps((372, 82), (560, 0)), 16),
    ("", Ps((372, 82), (350, -80)), 16),
    ("", Ps((372, 82), (548, 370)), 18),
    ("Blumauerstraße", Ps((548, 370), (720, 335)), 16),
    ("Bahnhofstraße", Ps((532, 542), (720, 540)), 14),
    ("", Ps((548, 370), (535, 540), (400, 748)), 14),
    ("", Ps((70, -10), (70, 200), (95, 320), (385, 748)), 12),
    ("Coulinstraße", Ps((230, 690), (392, 750)), 12),
    ("Bahnhofplatz", Ps((392, 750), (520, 830)), 16),
]
BUILDINGS = [  # (name, centreline, depth m, height m)
    ("Musiktheater", Ps((620, 385), (620, 528)), 85, 22),
    ("Arbeiterkammer", Ps((200, 500), (200, 640)), 50, 35),
    ("", Ps((40, 150), (140, 108)), 16, 18), ("", Ps((160, 100), (250, 62)), 16, 21), ("", Ps((270, 55), (340, 25)), 16, 18),
    ("", Ps((420, 40), (540, -15)), 18, 20), ("", Ps((455, 110), (520, 220)), 22, 19), ("", Ps((560, 230), (690, 230)), 30, 22),
    ("", Ps((25, 250), (25, 330)), 22, 16), ("", Ps((35, 400), (110, 520)), 30, 17), ("", Ps((480, 600), (600, 690)), 60, 22),
]
LABELS = [("GOETHEKREUZUNG", P(430, 120)), ("VOLKSGARTEN", P(300, 470)),
          ("MUSIKTHEATER", P(620, 455)), ("ARBEITERKAMMER", P(200, 570))]
TRAM = (Ps((350, -80), (372, 82), (548, 370), (720, 335)), P(390, 150))   # route, stop
BUS = (Ps((720, 540), (532, 542), (400, 748)), P(575, 545))

def inside(x, y, poly):
    hit = False
    for (x1, y1), (x2, y2) in zip(poly, poly[1:] + poly[:1]):
        if (y1 > y) != (y2 > y) and x < x1 + (y - y1) * (x2 - x1) / (y2 - y1): hit = not hit
    return hit

def distance(x, y, line):
    best = math.inf
    for (x1, y1), (x2, y2) in zip(line, line[1:]):
        dx, dy = x2 - x1, y2 - y1
        t = max(0, min(1, ((x - x1) * dx + (y - y1) * dy) / (dx * dx + dy * dy)))
        best = min(best, math.hypot(x - x1 - t * dx, y - y1 - t * dy))
    return best

def clip(subject, clipper):
    """Sutherland-Hodgman: the part of `subject` inside the convex, counter-clockwise `clipper`."""
    out = subject
    for (ax, ay), (bx, by) in zip(clipper, clipper[1:] + clipper[:1]):
        side = lambda p: (bx - ax) * (p[1] - ay) - (by - ay) * (p[0] - ax)
        pts, out = out, []
        for p, q in zip(pts, pts[1:] + pts[:1]):
            sp, sq = side(p), side(q)
            if sp >= 0: out.append(p)
            if (sp >= 0) != (sq >= 0): out.append((p[0] + (q[0] - p[0]) * sp / (sp - sq), p[1] + (q[1] - p[1]) * sp / (sp - sq)))
    return out

def build():
    xs, ys = [p[0] for p in PARK], [p[1] for p in PARK]
    width, height = math.ceil((max(xs) - min(xs)) / CELL) + 2, math.ceil((max(ys) - min(ys)) / CELL) + 2
    cx, cy = (min(xs) + max(xs)) / 2, (min(ys) + max(ys)) / 2   # grid centre = world origin
    centre = lambda gx, gy: (cx + (gx - (width - 1) / 2) * CELL, cy + ((height - 1) / 2 - gy) * CELL)
    grid = [['.' if inside(*centre(gx, gy), PARK) else '#' for gx in range(width)] for gy in range(height)]
    for gy in range(height):
        for gx in range(width):
            if grid[gy][gx] == '.' and min(distance(*centre(gx, gy), line) for line in PATHS) <= 2.6: grid[gy][gx] = 'p'
    ex, ey = min(((gx, gy) for gy in range(height) for gx in range(width) if grid[gy][gx] != '#'),
                 key=lambda c: math.hypot(*centre(*c)))
    grid[ey][ex] = 'E'
    rng = random.Random(42)
    for gy in range(1, height - 1):
        for gx in range(1, width - 1):
            ring = [grid[gy + dy][gx + dx] for dy in (-1, 0, 1) for dx in (-1, 0, 1)]
            # A tree only where all 8 neighbours are free lawn or path, so it can never cut a route.
            if grid[gy][gx] == '.' and all(t in '.p' for t in ring) and abs(gx - ex) + abs(gy - ey) > 6 and rng.random() < .07:
                grid[gy][gx] = 'T'
    # Only the corner within PLOT_RADIUS of the entrance is the market; trees further out become scenery.
    gate, scenery = centre(ex, ey), []
    for gy in range(height):
        for gx in range(width):
            if grid[gy][gx] in '.pT' and math.dist(centre(gx, gy), gate) > PLOT_RADIUS:
                if grid[gy][gx] == 'T': scenery.append(centre(gx, gy))
                grid[gy][gx] = '#'
    seen, todo = {(ex, ey)}, [(ex, ey)]
    while todo:
        x, y = todo.pop()
        for nx, ny in ((x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)):
            if 0 <= nx < width and 0 <= ny < height and grid[ny][nx] in '.p' and (nx, ny) not in seen:
                seen.add((nx, ny)); todo.append((nx, ny))
    for gy in range(height):   # slivers the entrance cannot reach are not part of the playable park
        for gx in range(width):
            if grid[gy][gx] in '.p' and (gx, gy) not in seen: grid[gy][gx] = '#'
    # Crop to the plot with a one-cell border; the cropped grid's centre is the new world origin.
    used = [(gx, gy) for gy in range(height) for gx in range(width) if grid[gy][gx] != '#']
    x0, x1, y0, y1 = min(c[0] for c in used) - 1, max(c[0] for c in used) + 1, min(c[1] for c in used) - 1, max(c[1] for c in used) + 1
    cx, cy = centre((x0 + x1) / 2, (y0 + y1) / 2)
    grid = [row[x0:x1 + 1] for row in grid[y0:y1 + 1]]
    seen = {(x - x0, y - y0) for x, y in seen}; width, height = x1 - x0 + 1, y1 - y0 + 1
    circle = [(gate[0] + PLOT_RADIUS * math.cos(a * math.pi / 24), gate[1] + PLOT_RADIUS * math.sin(a * math.pi / 24)) for a in range(48)]
    world = lambda pts: [round(v, 1) for x, y in pts for v in (x - cx, y - cy)]
    def route(kind, name, data):
        pts, stop = data
        lengths = [math.dist(a, b) for a, b in zip(pts, pts[1:])]
        along = min((sum(lengths[:i]) + math.dist(pts[i], stop), i) for i in range(len(lengths)))[0]
        return {"kind": kind, "name": name, "points": world(pts), "stopAt": round(along / sum(lengths), 3)}
    features = ([{"kind": "park", "name": "", "points": world(PARK)},
                 {"kind": "plot", "name": "", "points": world(clip(PARK, circle)), "width": PLOT_RADIUS},
                 {"kind": "tree", "name": "", "points": world(scenery)}]
                + [{"kind": "path", "name": "", "points": world(line), "width": 5} for line in PATHS]
                + [{"kind": "street", "name": n, "points": world(line), "width": w} for n, line, w in STREETS]
                + [{"kind": "building", "name": n, "points": world(line), "width": d, "height": h} for n, line, d, h in BUILDINGS]
                + [{"kind": "label", "name": n, "points": world([p])} for n, p in LABELS]
                + [route("tram", "Tram, Goethekreuzung stop", TRAM), route("bus", "Bus, Bahnhofstraße stop", BUS)])
    rows = [''.join(r) for r in grid]
    buildable = sum(r.count('.') for r in rows)
    assert buildable > 150 and all(grid[y][x] != '.' or (x, y) in seen for y in range(height) for x in range(width))
    return rows, features, buildable

if __name__ == '__main__':
    rows, features, buildable = build()
    status = ("Approximate blockout traced from a Google Maps screenshot (2026-10-20) at about 0.45 m per pixel, +-15%. "
              "Principal paths only; trees are placeholders; tram and bus alignment approximate. Not surveyed; not an exact replica. "
              f"Only the corner within {PLOT_RADIUS} m of the Goethekreuzung entrance is the playable market; the rest of the park is scenery.")
    head = json.dumps({"name": "Volksgarten · Goethekreuzung", "geographyStatus": status, "metersPerCell": CELL}, indent=2)[:-2]
    text = (head + ',\n  "rows": [\n' + ',\n'.join('    ' + json.dumps(r) for r in rows) + '\n  ],\n  "features": [\n'
            + ',\n'.join('    ' + json.dumps(f, ensure_ascii=False) for f in features) + '\n  ]\n}\n')
    out = Path(__file__).resolve().parents[1] / 'Assets/_Wintermarkt/Resources/Wintermarkt/site.json'
    out.write_text(text, encoding='utf-8')
    print(f"{out.name}: {len(rows[0])}x{len(rows)} cells, {buildable} buildable, {len(features)} features")
