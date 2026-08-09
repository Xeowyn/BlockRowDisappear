using System;

// which screen we are on
public enum Mode
{
    Title,      // the front screen when you start the game up
    Banner,     // "GO!" or "GOOD JOB!" is showing, the board is frozen
    Playing,    // normal play
    Paused,     // player pressed P
    Dead        // game over
}

// The rules of the game.
// This class does not know anything about windows or drawing.
// It just keeps track of what is happening, so it is easy to test.
public class Game
{
    public Board Board = new Board();
    public Effects Effects = new Effects();

    public Piece Current;
    public Piece Next;

    public int Score = 0;
    public int Lines = 0;
    public int Level = 1;
    public int Best = 0;

    public Mode State = Mode.Title;

    // the big message in the middle of the screen
    public string BannerBig = "";
    public string BannerSmall = "";
    private double bannerTimer = 0;

    // explosion stuff
    public bool Exploding = false;
    public int BoomFrame = 0;
    public int BoomFrameCount = 8;

    // how long each explosion picture stays on screen
    private const double BoomFrameMs = 55;

    // how the fall speed ramps up with level
    private const double StartingFallDelayMs = 500;
    private const double FallDelayStepPerLevelMs = 40;
    private const double MinFallDelayMs = 100;

    private const int LinesPerLevel = 10;

    private double fallTimer = 0;
    private double boomTimer = 0;
    private int rowsWaiting = 0;

    private Sound sound;
    private Random rnd = new Random();

    public Game(Sound sound, int boomFrameCount)
    {
        this.sound = sound;
        this.BoomFrameCount = boomFrameCount;

        Reset();
        State = Mode.Title;
    }

    public void Reset()
    {
        Board.Clear();
        Effects.Clear();

        Score = 0;
        Lines = 0;
        Level = 1;

        Exploding = false;
        BoomFrame = 0;
        fallTimer = 0;
        boomTimer = 0;
        rowsWaiting = 0;

        Next = Piece.Random(rnd);
        NewPiece();

        State = Mode.Playing;
    }

    // called when you press a key on the title screen or after dying
    public void StartGame()
    {
        Reset();
        ShowBanner("GO!", "LEVEL 1", 1100);
        sound.Go();
    }

    public void ShowBanner(string big, string small, double ms)
    {
        BannerBig = big;
        BannerSmall = small;
        bannerTimer = ms;
        State = Mode.Banner;
    }

    // how long to wait between drops. higher level = faster.
    public double FallDelay
    {
        get
        {
            double delay = StartingFallDelayMs - (Level - 1) * FallDelayStepPerLevelMs;
            if (delay < MinFallDelayMs)
            {
                delay = MinFallDelayMs;
            }
            return delay;
        }
    }

    // where the piece would land if you dropped it right now.
    // used to draw the faint ghost at the bottom.
    public int GhostRow
    {
        get
        {
            int row = Current.Row;
            while (Board.Fits(Current, row + 1, Current.Col))
            {
                row = row + 1;
            }
            return row;
        }
    }

    public void NewPiece()
    {
        Current = Next;
        Next = Piece.Random(rnd);

        Current.Row = 0;
        Current.Col = Board.Cols / 2 - Current.Size / 2;

        // if the new piece has nowhere to go, the game is over
        if (Board.Fits(Current, Current.Row, Current.Col) == false)
        {
            State = Mode.Dead;
            Effects.Shove(9);

            if (Score > Best)
            {
                Best = Score;
            }

            sound.Dead();
        }
    }

    // the window calls this every frame and says how many
    // milliseconds went by since last time
    public void Update(double ms)
    {
        // the sparks keep flying no matter what screen we are on
        Effects.Update(ms);

        if (State == Mode.Title || State == Mode.Dead || State == Mode.Paused)
        {
            return;
        }

        if (State == Mode.Banner)
        {
            bannerTimer = bannerTimer - ms;

            if (bannerTimer <= 0)
            {
                State = Mode.Playing;
            }

            return;
        }

        // while the rows are blowing up, nothing else happens
        if (Exploding == true)
        {
            boomTimer = boomTimer + ms;

            while (boomTimer >= BoomFrameMs)
            {
                boomTimer = boomTimer - BoomFrameMs;
                BoomFrame = BoomFrame + 1;

                if (BoomFrame >= BoomFrameCount)
                {
                    FinishExplosion();
                    return;
                }
            }

            return;
        }

        fallTimer = fallTimer + ms;

        if (fallTimer >= FallDelay)
        {
            fallTimer = 0;
            Down();
        }
    }

    // can the player do things right now?
    private bool Busy
    {
        get { return State != Mode.Playing || Exploding == true; }
    }

    public void MoveLeft()
    {
        if (Busy) return;

        if (Board.Fits(Current, Current.Row, Current.Col - 1))
        {
            Current.Col = Current.Col - 1;
            sound.Move();
        }
    }

    public void MoveRight()
    {
        if (Busy) return;

        if (Board.Fits(Current, Current.Row, Current.Col + 1))
        {
            Current.Col = Current.Col + 1;
            sound.Move();
        }
    }

    // spin the piece. if it does not fit, scoot it sideways and try again.
    public void Rotate(bool clockwise)
    {
        if (Busy) return;

        Piece spun = Current.Spun(clockwise);
        int[] nudges = { 0, -1, 1, -2, 2 };

        for (int i = 0; i < nudges.Length; i++)
        {
            int tryCol = Current.Col + nudges[i];

            if (Board.Fits(spun, Current.Row, tryCol))
            {
                spun.Col = tryCol;
                spun.Row = Current.Row;
                Current = spun;
                sound.Spin();
                return;
            }
        }
    }

    // move down one row. if it cannot move, the piece has landed.
    public void Down()
    {
        if (Busy) return;

        if (Board.Fits(Current, Current.Row + 1, Current.Col))
        {
            Current.Row = Current.Row + 1;
        }
        else
        {
            Land();
        }
    }

    // drop it all the way to the bottom
    public void HardDrop()
    {
        if (Busy) return;

        int fell = 0;
        while (Board.Fits(Current, Current.Row + 1, Current.Col))
        {
            Current.Row = Current.Row + 1;
            fell = fell + 1;
        }

        // the further it fell the harder it hits
        Effects.Shove(2 + fell * 0.35);
        Land();
    }

    private void Land()
    {
        Board.Stamp(Current);
        fallTimer = 0;

        // a puff of dust where the piece landed
        for (int col = 0; col < Current.Size; col++)
        {
            for (int row = 0; row < Current.Size; row++)
            {
                if (Current.HasBlockAt(row, col))
                {
                    Effects.Burst(Current.Col + col + 0.5,
                                  Current.Row + row + 0.5, 2, 190, 190, 210);
                }
            }
        }

        rowsWaiting = Board.FindFullRows();

        if (rowsWaiting > 0)
        {
            // start the explosion. the rows do not disappear
            // until it has finished playing.
            Exploding = true;
            BoomFrame = 0;
            boomTimer = 0;

            Effects.Shove(4 + rowsWaiting * 3);
            Effects.Blink(rowsWaiting >= 4 ? 0.85 : 0.35);

            for (int row = 0; row < Board.Rows; row++)
            {
                if (Board.IsRowFull(row))
                {
                    Effects.BurstRow(row, Board.Cols);
                }
            }

            sound.Boom();
        }
        else
        {
            Effects.Shove(1.5);
            sound.Land();
            NewPiece();
        }
    }

    private void FinishExplosion()
    {
        Exploding = false;
        BoomFrame = 0;

        // remember where to put the "+800" before the rows vanish
        double popupRow = Board.Rows - 1;
        for (int row = 0; row < Board.Rows; row++)
        {
            if (Board.IsRowFull(row))
            {
                popupRow = row;
                break;
            }
        }

        Board.RemoveFullRows();
        AddPoints(rowsWaiting, popupRow);
        sound.Lines(rowsWaiting);

        rowsWaiting = 0;

        // even if a level banner just came up, still hand out the next piece
        NewPiece();
    }

    public void AddPoints(int howMany, double popupRow)
    {
        int oldLevel = Level;
        int before = Score;

        if (howMany == 1)
        {
            Score = Score + 100;
        }
        if (howMany == 2)
        {
            Score = Score + 300;
        }
        if (howMany == 3)
        {
            Score = Score + 500;
        }
        if (howMany == 4)
        {
            Score = Score + 800;   // QUAD!!
        }

        if (Score > before)
        {
            string text = "+" + (Score - before);
            if (howMany == 4)
            {
                text = "QUAD! +800";
            }
            Effects.AddPopup(text, Board.Cols / 2.0, popupRow, 255, 235, 120);
        }

        Lines = Lines + howMany;
        Level = (Lines / LinesPerLevel) + 1;

        if (Score > Best)
        {
            Best = Score;
        }

        if (Level > oldLevel)
        {
            Effects.Blink(0.7);
            Effects.Shove(6);
            ShowBanner(WellDone(), "LEVEL " + Level, 1400);
            sound.LevelUp();
        }
    }

    // a different cheer each time so it does not get boring
    private string WellDone()
    {
        string[] cheers = { "GOOD JOB!", "NICE ONE!", "SPEEDING UP!", "KEEP GOING!", "WELL PLAYED!" };
        return cheers[rnd.Next(cheers.Length)];
    }

    public void TogglePause()
    {
        if (State == Mode.Playing)
        {
            State = Mode.Paused;
        }
        else if (State == Mode.Paused)
        {
            State = Mode.Playing;
        }
    }

    // any key on the title screen starts a game
    public void PressAnyKey()
    {
        if (State == Mode.Title || State == Mode.Dead)
        {
            StartGame();
        }
    }
}
