using System;

// A bit of text that floats up off the board and fades out,
// like the "+800" you see when you clear some rows.
//
// Like Particle, its position is in board squares, not pixels.
class Popup
{
    public string Text;
    public double Col;
    public double Row;
    public double Life;
    public double StartLife;

    public int Red;
    public int Green;
    public int Blue;

    public Popup(string text, double col, double row, double life,
                 int red, int green, int blue)
    {
        Text = text;
        Col = col;
        Row = row;
        Life = life;
        StartLife = life;
        Red = red;
        Green = green;
        Blue = blue;
    }

    public bool IsDead
    {
        get { return Life <= 0; }
    }

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
        Row = Row - seconds * 2.2;   // drift upwards
        Life = Life - seconds;
    }
}
