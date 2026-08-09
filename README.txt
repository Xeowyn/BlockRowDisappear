BLOCK ROW DISAPPEAR
===================

Stack the falling blocks. Fill a whole row and it blows up and vanishes.

To play:  double-click BlockRowDisappear.exe


Controls
--------
  LEFT arrow   move left      (A does the same)
  RIGHT arrow  move right     (D does the same)
  UP arrow     spin the piece
  Z            spin the other way
  DOWN arrow   drop faster
  SPACE        drop all the way
  P            pause
  R            start again
  M            mute / unmute
  ESC          quit

Scoring: 1 row = 100, 2 rows = 300, 3 rows = 500, 4 rows = 800.
Every 10 rows you go up a level and the blocks fall faster.


Screens
-------
  Title      "PRESS ANY KEY" when you start the game up
  GO!        before the first block drops
  GOOD JOB!  every time you reach a new level (the message changes)
  PAUSED
  GAME OVER  shows your score and your best, any key plays again


The effects
-----------
  explosion      full rows blow up in an 8 frame fireball before clearing
  sparks         bits of fire fly out of the rows and fall with gravity
  dust           a small puff when a block lands
  screen shake   shakes harder for a hard drop, much harder for 4 rows
  white flash    a bright flash on a 4 row clear and when you level up
  ghost          a faint outline showing where the block will land
  score popups   "+800" or "QUAD! +800" floats up off the board
  scanlines      thin dark lines over everything, like an old TV
  sunset         a pixel sun and a neon grid floor behind the game
  pixel text     all the big writing is drawn tiny and blown up, so the
                 letters are chunky and match the artwork


What is in this folder
----------------------
  BlockRowDisappear.exe   the game
  assets\                 all the pictures and sounds - swap to reskin it
  source\                 the C# code
  tools\                  the script that makes the assets, and credits
  rebuild.bat             rebuilds the game after you change the code


Changing the graphics
---------------------
Everything is a .png in the assets folder. Replace a file with your own
and the game just uses it. No code changes.

  block_1.png    the long piece         block_5.png    the Z piece
  block_2.png    the square             block_6.png    the J piece
  block_3.png    the T piece            block_7.png    the L piece
  block_4.png    the S piece
  empty.png      an empty square        ghost.png      the landing outline
  background.png the sunset             boom_1.png ... the explosion

The blocks are 16x16 and the game blows them up to 32x32 with NO
smoothing. That is what makes them look chunky and pixelated.

You can add MORE explosion frames just by dropping in boom_9.png,
boom_10.png and so on. The game counts them at startup and the
explosion automatically runs longer.

To change the generated ones instead, edit tools\make_assets.py and run:
    python3 make_assets.py

The colours of the 7 blocks are the PIECE_HUES list near the top of that
file - just numbers from 0.0 to 1.0 going round the rainbow. Change one
number and that piece changes colour, shine and shadow included.


Changing the sounds
-------------------
Same idea, the .wav files in assets:

  move.wav      moving left or right      boom.wav      the explosion
  spin.wav      spinning                  levelup.wav   new level
  land.wav      a block landing           go.wav        the GO! banner
  line.wav      clearing 1 to 3 rows      gameover.wav  game over
  quad.wav      clearing 4 rows at once

Drop in any .wav you like. Windows only plays one sound at a time, so a
new sound cuts off the one before it - keep them short.


The code
--------
Split into classes, one job each:

  Piece.cs      one falling piece - its shape, and how it spins
  Board.cs      the 10x20 grid - walls, floor, and full rows
  Game.cs       the rules - falling, scoring, levels, screens
  Particle.cs   one spark
  Popup.cs      one bit of floating text
  Effects.cs    holds the sparks, the popups, the shake and the flash
  Assets.cs     loads the pictures
  Sound.cs      plays the wav files
  Renderer.cs   draws everything (it never changes the game)
  GameForm.cs   the window, the keyboard and the timer
  Program.cs    just opens the window

Game.cs has no drawing code in it at all, and Renderer.cs never changes
the game. That split is why the rules can be tested without opening a
window.

Particle and Popup store their position in BOARD SQUARES, not pixels,
so they do not care how big the blocks are drawn.


Rebuilding after you change the code
------------------------------------
Double-click rebuild.bat. Takes about 20 seconds.

It uses the .NET 8 SDK installed inside WSL. This PC only has the .NET 6
runtime, so the game is built self-contained - that is why the .exe is a
big file. It will run on any Windows PC with nothing installed.
