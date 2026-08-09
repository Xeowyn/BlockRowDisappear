# Block Row Disappear

Stack the falling blocks. Fill a whole row and it blows up and vanishes. It's a Tetris-style game.

## How to run it

This game only runs on Windows.

**If you have the .exe:** just double-click `BlockRowDisappear.exe`.

**To run it from the code (the .exe isn't included in this repo):**

1. Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) if you don't have it
2. Open a terminal in the `source/BlockRowDisappear` folder
3. Run:

```
dotnet run
```

That builds the game and opens it in a window.

## Controls

| Key | What it does |
|---|---|
| Left / A | Move left |
| Right / D | Move right |
| Up | Spin the piece |
| Z | Spin the other way |
| Down | Drop faster |
| Space | Drop all the way, instantly |
| P | Pause |
| R | Start over |
| M | Mute / unmute |
| Esc | Quit |

**Scoring:** 1 row = 100 points, 2 rows = 300, 3 rows = 500, 4 rows at once = 800. Every 10 rows you clear, you level up and the blocks fall faster.

## Screens

- **Title** — "PRESS ANY KEY" when you first open the game
- **GO!** — shows right before the first block drops
- **GOOD JOB!** — shows every time you level up (the message changes)
- **PAUSED**
- **GAME OVER** — shows your score and your best score; press any key to play again

## The effects

- **Explosion** — a full row blows up in an 8-frame fireball before it clears
- **Sparks** — bits of fire fly out of a cleared row and fall with gravity
- **Dust** — a small puff when a block lands
- **Screen shake** — shakes harder on a hard drop, even harder on a 4-row clear
- **White flash** — flashes bright on a 4-row clear and when you level up
- **Ghost** — a faint outline showing where the piece will land
- **Score popups** — "+800" or "QUAD! +800" floats up off the board
- **Scanlines** — thin dark lines over everything, like an old TV
- **Sunset background** — a pixel sun and a neon grid floor behind the game
- **Pixel text** — the big text is drawn small and blown up, so the letters look chunky, matching the art style

## What's in this folder

- `assets/` — every picture and sound. Swap a file to reskin the game — no code changes needed.
- `source/` — the C# code, split into three projects (see below)
- `tools/` — the script that generates some of the assets, plus credits for the art
- `rebuild.bat` — rebuilds the game into a single .exe file after you change the code

## Changing the graphics

Everything is a `.png` file in `assets/`. Replace one with your own picture and the game just uses it.

- `block_1.png` through `block_7.png` — the seven falling piece shapes
- `empty.png` — an empty board square
- `ghost.png` — the landing-outline piece
- `background.png` — the sunset background
- `boom_1.png`, `boom_2.png`, etc. — explosion frames

The blocks are drawn at 16x16 and blown up to 32x32 with no smoothing — that's what makes them look chunky and pixelated.

Want more explosion frames? Just add `boom_9.png`, `boom_10.png`, and so on — the game counts how many exist when it starts and the explosion runs longer automatically.

To regenerate the built-in pictures instead of drawing your own, edit `tools/make_assets.py` and run `python3 make_assets.py`. The colors of the 7 pieces are the `PIECE_HUES` list near the top of that file — plain numbers from 0.0 to 1.0 going around the color wheel. Change one number and that piece's color changes, shine and shadow included.

## Changing the sounds

Same idea — `.wav` files in `assets/`:

- `move.wav` — moving left or right
- `spin.wav` — spinning a piece
- `land.wav` — a block landing
- `line.wav` — clearing 1 to 3 rows at once
- `quad.wav` — clearing 4 rows at once
- `boom.wav` — the explosion
- `levelup.wav` — reaching a new level
- `go.wav` — the "GO!" banner
- `gameover.wav` — game over

Drop in any `.wav` file you like. Windows only plays one sound at a time, so a new sound cuts off whatever was playing — keep them short.

## The code

Three projects in `source/`:

- `BlockRowDisappear.Core/` — the game rules, no windows or drawing involved
  - `Piece.cs` — one falling piece: its shape, and how it spins
  - `Board.cs` — the 10x20 grid: walls, floor, and checking for full rows
  - `Game.cs` — the rules: falling, scoring, levels, which screen is showing
  - `Particle.cs` — one spark
  - `Popup.cs` — one bit of floating score text
  - `Effects.cs` — holds all the sparks, popups, screen shake, and flash
  - `Sound.cs` — plays the `.wav` files
- `BlockRowDisappear/` — the actual Windows app. References `BlockRowDisappear.Core`.
  - `Assets.cs` — loads the pictures
  - `Renderer.cs` — draws everything (never changes the game's rules)
  - `GameForm.cs` — the window, keyboard input, and the game timer
  - `Program.cs` — just opens the window
- `BlockRowDisappear.Tests/` — unit tests for `BlockRowDisappear.Core`

`Game.cs` has no drawing code in it at all, and `Renderer.cs` never changes the game's state — that split is what makes the rules testable without opening a window.

`Particle` and `Popup` store their position in board squares, not pixels, so they don't care how big the blocks are drawn on screen.

## Rebuilding the .exe after changing the code

Double-click `rebuild.bat`. Takes about 20 seconds. It builds a self-contained .exe, so it runs on any Windows PC with nothing else installed.
