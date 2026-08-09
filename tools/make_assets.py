#!/usr/bin/env python3
"""
Makes all the pictures and sounds the game needs.

Run it like this:   python3 make_assets.py
Everything goes into the ..\assets folder.

The blocks come from arcadArne_sheet.png, which is a CC0 (public domain)
pixel art sheet from OpenGameArt - see ASSET_CREDITS.txt. One tile is
taken from it and recoloured 7 times, once for each block piece.

Everything else is drawn here.

All the pictures are small on purpose (16x16 for blocks). The game
blows them up with no smoothing, so you get big chunky pixels.
"""

import math
import os
import struct
import wave
import colorsys

from PIL import Image, ImageDraw

HERE = os.path.dirname(os.path.abspath(__file__))
ASSETS = os.path.join(HERE, "..", "assets")
SHEET = os.path.join(HERE, "arcadArne_sheet.png")

TILE = 16          # the blocks are drawn at 16x16 pixels
RATE = 44100


# ----------------------------------------------------------------------
# the blocks, taken from the CC0 sheet and recoloured
# ----------------------------------------------------------------------

# where the block we borrow sits on the sheet
BASE_X = 268
BASE_Y = 348

# the colour each piece gets. these are hues, 0.0 to 1.0.
#   0.0 = red, 0.16 = yellow, 0.33 = green, 0.5 = cyan, 0.66 = blue, 0.8 = purple
PIECE_HUES = {
    1: 0.50,   # I  cyan
    2: 0.14,   # O  yellow
    3: 0.79,   # T  purple
    4: 0.33,   # S  green
    5: 0.99,   # Z  red
    6: 0.62,   # J  blue
    7: 0.07,   # L  orange
}


def hsv(h, s, v):
    r, g, b = colorsys.hsv_to_rgb(h % 1.0, max(0.0, min(1.0, s)), max(0.0, min(1.0, v)))
    return (int(r * 255), int(g * 255), int(b * 255))


def blend(a, b, t):
    return (int(a[0] + (b[0] - a[0]) * t),
            int(a[1] + (b[1] - a[1]) * t),
            int(a[2] + (b[2] - a[2]) * t))


def make_glossy_block(hue):
    """
    A bright shiny block, drawn one pixel at a time.

    The trick to making it look like glass or candy is:
      - the top is much lighter than the bottom
      - there is a hard white shine near the top left
      - the bottom right has a dark edge so it looks rounded
      - the corners are cut off so it is not a plain square
    """
    img = Image.new("RGBA", (TILE, TILE), (0, 0, 0, 0))

    top = hsv(hue, 0.42, 1.00)     # pale and bright
    mid = hsv(hue, 0.80, 0.98)     # the main colour
    low = hsv(hue, 0.95, 0.62)     # deep shadow at the bottom
    edge = hsv(hue, 1.00, 0.30)    # the outline
    shine = hsv(hue, 0.10, 1.00)   # the highlight

    last = TILE - 1

    for y in range(TILE):
        for x in range(TILE):

            # cut the corners off to round it a little
            corner = (x + y < 2) or (x + (last - y) < 2) or \
                     ((last - x) + y < 2) or ((last - x) + (last - y) < 2)
            if corner:
                continue

            along = y / float(last)

            # top half fades from pale to the main colour,
            # bottom half fades from the main colour into shadow
            if along < 0.5:
                colour = blend(top, mid, along * 2.0)
            else:
                colour = blend(mid, low, (along - 0.5) * 2.0)

            img.putpixel((x, y), colour + (255,))

    d = ImageDraw.Draw(img)

    # dark outline round the whole thing
    d.line([(2, 0), (last - 2, 0)], fill=edge + (255,))
    d.line([(2, last), (last - 2, last)], fill=edge + (255,))
    d.line([(0, 2), (0, last - 2)], fill=edge + (255,))
    d.line([(last, 2), (last, last - 2)], fill=edge + (255,))
    d.point([(1, 1), (last - 1, 1), (1, last - 1), (last - 1, last - 1)], fill=edge + (255,))

    # bright inner edge along the top and the left
    d.line([(3, 2), (last - 3, 2)], fill=blend(top, (255, 255, 255), 0.55) + (255,))
    d.line([(2, 3), (2, last - 4)], fill=blend(top, (255, 255, 255), 0.35) + (255,))

    # dark inner edge along the bottom and the right
    d.line([(3, last - 2), (last - 3, last - 2)], fill=low + (255,))
    d.line([(last - 2, 3), (last - 2, last - 3)], fill=low + (255,))

    # the shine: a little block of white near the top left
    d.rectangle([4, 4, 6, 5], fill=shine + (255,))
    d.point((4, 6), fill=shine + (190,))

    # a tiny sparkle bottom right so it catches the eye
    d.point((last - 4, last - 5), fill=blend(top, (255, 255, 255), 0.7) + (170,))

    return img


def recolour(tile, hue):
    """
    Repaints the tile in a new colour but keeps all the light and dark
    parts exactly where they were, so it still looks hand drawn.
    """
    out = Image.new("RGBA", tile.size, (0, 0, 0, 0))
    src = tile.load()
    dst = out.load()

    for x in range(tile.width):
        for y in range(tile.height):
            r, g, b, a = src[x, y]

            if a < 40:
                continue

            oldHue, oldSat, val = colorsys.rgb_to_hsv(r / 255.0, g / 255.0, b / 255.0)

            # very dark pixels are the outline, leave them dark
            if val < 0.18:
                dst[x, y] = (r // 2, g // 2, b // 2, a)
                continue

            # keep grey pixels grey, only repaint the coloured ones
            sat = oldSat
            if sat > 0.08:
                sat = min(1.0, oldSat * 1.25 + 0.25)

            nr, ng, nb = colorsys.hsv_to_rgb(hue, sat, min(1.0, val * 1.12))
            dst[x, y] = (int(nr * 255), int(ng * 255), int(nb * 255), a)

    return out


def make_blocks():
    for kind in range(1, 8):
        block = make_glossy_block(PIECE_HUES[kind])
        block.save(os.path.join(ASSETS, "block_%d.png" % kind))

    # the ghost piece: a faint outline showing where the piece will land
    ghost = Image.new("RGBA", (TILE, TILE), (0, 0, 0, 0))
    d = ImageDraw.Draw(ghost)
    last = TILE - 1
    d.line([(2, 0), (last - 2, 0)], fill=(190, 210, 255, 120))
    d.line([(2, last), (last - 2, last)], fill=(190, 210, 255, 120))
    d.line([(0, 2), (0, last - 2)], fill=(190, 210, 255, 120))
    d.line([(last, 2), (last, last - 2)], fill=(190, 210, 255, 120))
    d.rectangle([3, 3, last - 3, last - 3], outline=(140, 165, 220, 55))
    ghost.save(os.path.join(ASSETS, "ghost.png"))


def make_empty():
    """An empty square on the board."""
    img = Image.new("RGBA", (TILE, TILE), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rectangle([0, 0, TILE - 1, TILE - 1], fill=(26, 12, 46, 255))
    d.rectangle([0, 0, TILE - 1, TILE - 1], outline=(48, 26, 78, 255))
    d.point((TILE // 2, TILE // 2), fill=(74, 44, 114, 255))
    img.save(os.path.join(ASSETS, "empty.png"))


# ----------------------------------------------------------------------
# the explosion
# ----------------------------------------------------------------------

# a small palette so it looks like an old game
FIRE = [
    (255, 255, 245),
    (255, 240, 160),
    (255, 195, 60),
    (255, 130, 30),
    (225, 65, 30),
    (140, 30, 25),
]


def make_boom(step, total, name):
    """
    One frame of the explosion, drawn as chunky pixels.
    It starts as a white flash and blows out into a ring of fire.
    """
    img = Image.new("RGBA", (TILE, TILE), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)

    mid = (TILE - 1) / 2.0
    grow = step / float(total - 1)
    radius = 4.0 + grow * (TILE * 0.52)
    fade = (1.0 - grow) ** 0.45

    if fade <= 0.02:
        img.save(os.path.join(ASSETS, name))
        return

    # work out a colour for every pixel based on how far out it is
    for x in range(TILE):
        for y in range(TILE):
            dist = math.hypot(x - mid, y - mid)

            if dist > radius:
                continue

            # how far through the fireball this pixel is, 0 = middle
            depth = dist / max(0.6, radius)

            # early on it is a solid ball, later only the outer ring burns
            if grow > 0.45:
                inner = 0.45 + (grow - 0.45) * 1.3
                if depth < inner:
                    continue

            shade = int(depth * (len(FIRE) - 1) + grow * 2.2)
            if shade > len(FIRE) - 1:
                shade = len(FIRE) - 1

            colour = FIRE[shade]
            alpha = int(255 * fade)
            img.putpixel((x, y), colour + (alpha,))

    # sparks shooting out
    sparks = 8
    for i in range(sparks):
        angle = (math.pi * 2 / sparks) * i + grow * 1.4
        far = radius * (1.05 + 0.45 * ((i % 3) / 2.0))
        sx = int(round(mid + math.cos(angle) * far))
        sy = int(round(mid + math.sin(angle) * far))
        if 0 <= sx < TILE and 0 <= sy < TILE:
            d.point((sx, sy), fill=FIRE[1] + (int(230 * fade),))

    img.save(os.path.join(ASSETS, name))


BOOM_FRAMES = 8


# ----------------------------------------------------------------------
# the background
# ----------------------------------------------------------------------

def make_background():
    """
    A bright sunset scene: a purple and pink sky, a big striped sun,
    and a glowing grid floor running off into the distance.

    It is drawn small (160 wide) and blown up by the game, so it comes
    out chunky and pixelated like an old arcade machine.
    """
    w, h = 160, 190
    horizon = 118

    img = Image.new("RGB", (w, h), (20, 8, 40))
    d = ImageDraw.Draw(img)

    # --- the sky, fading from deep purple down to hot pink ---
    sky_top = (28, 10, 58)
    sky_mid = (96, 28, 118)
    sky_low = (232, 78, 140)

    for y in range(horizon):
        along = y / float(horizon)
        if along < 0.55:
            colour = blend(sky_top, sky_mid, along / 0.55)
        else:
            colour = blend(sky_mid, sky_low, (along - 0.55) / 0.45)
        d.line([(0, y), (w, y)], fill=colour)

    # --- a few stars, only up in the dark part ---
    seed = 7
    for i in range(38):
        seed = (seed * 1103515245 + 12345) % 2147483648
        x = seed % w
        seed = (seed * 1103515245 + 12345) % 2147483648
        y = seed % int(horizon * 0.5)
        seed = (seed * 1103515245 + 12345) % 2147483648
        bright = 150 + (seed % 100)
        d.point((x, y), fill=(bright, bright, 255))

    # --- the sun, with slots cut across it ---
    sun_x = w // 2
    sun_y = horizon - 24
    sun_r = 30

    for y in range(sun_y - sun_r, sun_y + sun_r + 1):
        if y >= horizon:
            break
        for x in range(sun_x - sun_r, sun_x + sun_r + 1):
            if math.hypot(x - sun_x, y - sun_y) > sun_r:
                continue
            if x < 0 or x >= w or y < 0:
                continue

            # the sun goes yellow at the top and pink at the bottom
            along = (y - (sun_y - sun_r)) / float(sun_r * 2)
            colour = blend((255, 238, 120), (255, 72, 132), along)

            # cut horizontal slots, wider towards the bottom
            gap = 2 + int(along * 7)
            if along > 0.45 and (y % (gap + 2)) < 2:
                continue

            img.putpixel((x, y), colour)

    # --- the ground ---
    for y in range(horizon, h):
        along = (y - horizon) / float(h - horizon)
        d.line([(0, y), (w, y)], fill=blend((38, 8, 62), (12, 4, 26), along))

    # a bright line right on the horizon
    d.line([(0, horizon), (w, horizon)], fill=(255, 150, 200))
    d.line([(0, horizon - 1), (w, horizon - 1)], fill=(180, 70, 140))

    # --- the grid floor ---
    grid = (120, 235, 245)

    # lines running away from us, all pointing at the middle
    for i in range(-9, 10):
        far_x = sun_x + i * 4
        near_x = sun_x + i * 42
        steps = h - horizon
        for s in range(steps):
            t = s / float(steps)
            x = int(far_x + (near_x - far_x) * (t * t))
            y = horizon + s
            if 0 <= x < w and y < h:
                fade = 0.30 + 0.70 * t
                old = img.getpixel((x, y))
                img.putpixel((x, y), blend(old, grid, fade))

    # lines running across, squashed together near the horizon
    step = 1.0
    y = float(horizon)
    while y < h:
        yy = int(y)
        if yy >= horizon:
            fade = 0.25 + 0.65 * ((yy - horizon) / float(h - horizon))
            for x in range(w):
                old = img.getpixel((x, yy))
                img.putpixel((x, yy), blend(old, grid, fade))
        step = step * 1.42 + 0.7
        y = y + step

    img.save(os.path.join(ASSETS, "background.png"))


# ----------------------------------------------------------------------
# sounds
# ----------------------------------------------------------------------

def save_wav(name, samples):
    path = os.path.join(ASSETS, name)
    with wave.open(path, "w") as f:
        f.setnchannels(1)
        f.setsampwidth(2)
        f.setframerate(RATE)
        f.writeframes(b"".join(
            struct.pack("<h", int(max(-1.0, min(1.0, s)) * 32000)) for s in samples))


def tone(freq, ms, shape="square", volume=0.45, end_freq=None):
    count = int(RATE * ms / 1000.0)
    out = []
    seed = 12345

    for i in range(count):
        along = i / float(count)
        f = freq if end_freq is None else freq + (end_freq - freq) * along
        phase = 2.0 * math.pi * f * (i / float(RATE))

        if shape == "sine":
            value = math.sin(phase)
        elif shape == "noise":
            seed = (seed * 1103515245 + 12345) % 2147483648
            value = (seed / 1073741824.0) - 1.0
        else:
            value = 1.0 if math.sin(phase) >= 0 else -1.0

        env = 1.0
        edge = 0.012
        if along < edge:
            env = along / edge
        elif along > 0.75:
            env = (1.0 - along) / 0.25

        out.append(value * env * volume)

    return out


def quiet(ms):
    return [0.0] * int(RATE * ms / 1000.0)


def mix(a, b):
    return [x + y for x, y in zip(a, b)]


def make_sounds():
    save_wav("move.wav", tone(180, 45, "square", 0.25))
    save_wav("spin.wav", tone(360, 70, "square", 0.30, end_freq=620))
    save_wav("land.wav", mix(tone(110, 90, "sine", 0.5), tone(0, 90, "noise", 0.12)))

    save_wav("line.wav", tone(660, 90, "square", 0.4) + tone(880, 130, "square", 0.4))

    save_wav("quad.wav", tone(523, 90, "square", 0.42) +
                           tone(659, 90, "square", 0.42) +
                           tone(784, 90, "square", 0.42) +
                           tone(1047, 260, "square", 0.45))

    save_wav("boom.wav", mix(tone(0, 260, "noise", 0.5),
                             tone(700, 260, "sine", 0.3, end_freq=90)))

    save_wav("levelup.wav", tone(700, 80, "square", 0.4) +
                            tone(900, 80, "square", 0.4) +
                            tone(1180, 190, "square", 0.42))

    # "GO!" at the start of a level
    save_wav("go.wav", tone(880, 90, "square", 0.42) +
                       quiet(30) +
                       tone(1320, 220, "square", 0.45))

    save_wav("gameover.wav", tone(392, 180, "square", 0.4) +
                             tone(330, 180, "square", 0.4) +
                             tone(262, 180, "square", 0.4) +
                             quiet(40) +
                             tone(196, 520, "square", 0.42))


# ----------------------------------------------------------------------

def main():
    if not os.path.isdir(ASSETS):
        os.makedirs(ASSETS)

    # get rid of explosion frames from an older run
    for old in os.listdir(ASSETS):
        if old.startswith("boom_") and old.endswith(".png"):
            os.remove(os.path.join(ASSETS, old))

    make_blocks()
    make_empty()
    make_background()

    for i in range(BOOM_FRAMES):
        make_boom(i, BOOM_FRAMES, "boom_%d.png" % (i + 1))

    make_sounds()

    print("made everything into", os.path.abspath(ASSETS))


main()
