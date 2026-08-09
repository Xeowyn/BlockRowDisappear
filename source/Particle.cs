using System;

// One little spark flying through the air.
//
// Its position is measured in BOARD SQUARES, not pixels, so this class
// does not need to know how big anything is on screen. The Renderer
// turns squares into pixels when it draws.
class Particle
{
    public double Col;      // across the board
    public double Row;      // down the board
    public double SpeedCol; // squares per second
    public double SpeedRow;

    public double Life;     // seconds left
    public double StartLife;

    public int Red;
    public int Green;
    public int Blue;

    public double Size;     // how many squares across (usually much less than 1)

    private const double Gravity = 22.0;   // squares per second per second

    public Particle(double col, double row,
                    double speedCol, double speedRow,
                    double life, int red, int green, int blue, double size)
    {
        Col = col;
        Row = row;
        SpeedCol = speedCol;
        SpeedRow = speedRow;
        Life = life;
        StartLife = life;
        Red = red;
        Green = green;
        Blue = blue;
        Size = size;
    }

    public bool IsDead
    {
        get { return Life <= 0; }
    }

    // 1.0 when it is brand new, down to 0.0 when it dies
    public double Fade
    {
        get
        {
            if (StartLife <= 0)
            {
                return 0;
            }
            return Life / StartLife;
        }
    }

    public void Update(double seconds)
    {
        SpeedRow = SpeedRow + Gravity * seconds;

        Col = Col + SpeedCol * seconds;
        Row = Row + SpeedRow * seconds;

        Life = Life - seconds;
    }
}
