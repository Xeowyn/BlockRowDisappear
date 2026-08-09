using System;
using System.Collections.Generic;

// Keeps track of all the eye candy: the sparks, the floating text,
// and how much the screen is shaking.
//
// It does not draw anything itself. The Renderer asks it what to draw.
public class Effects
{
    public List<Particle> Particles = new List<Particle>();
    public List<Popup> Popups = new List<Popup>();

    // how hard the screen is shaking, in pixels. it dies down on its own.
    public double Shake = 0;

    // a white flash over the whole screen, 0 = none, 1 = full white
    public double Flash = 0;

    private Random rnd = new Random();

    // stop everything, used when the game restarts
    public void Clear()
    {
        Particles.Clear();
        Popups.Clear();
        Shake = 0;
        Flash = 0;
    }

    public void Shove(double amount)
    {
        if (amount > Shake)
        {
            Shake = amount;
        }
    }

    public void Blink(double amount)
    {
        if (amount > Flash)
        {
            Flash = amount;
        }
    }

    public void AddPopup(string text, double col, double row, int r, int g, int b)
    {
        Popups.Add(new Popup(text, col, row, 1.1, r, g, b));
    }

    // throw a bunch of sparks out from one square
    public void Burst(double col, double row, int howMany, int r, int g, int b)
    {
        for (int i = 0; i < howMany; i++)
        {
            double angle = rnd.NextDouble() * Math.PI * 2;
            double speed = 2.0 + rnd.NextDouble() * 9.0;

            double sc = Math.Cos(angle) * speed;
            double sr = Math.Sin(angle) * speed - 3.0;   // bias upwards

            double life = 0.35 + rnd.NextDouble() * 0.55;
            double size = 0.10 + rnd.NextDouble() * 0.22;

            // wobble the colour a bit so it does not look flat
            int rr = Clamp(r + rnd.Next(-30, 31));
            int gg = Clamp(g + rnd.Next(-30, 31));
            int bb = Clamp(b + rnd.Next(-30, 31));

            Particles.Add(new Particle(col, row, sc, sr, life, rr, gg, bb, size));
        }
    }

    // sparks along a whole row that is being blown up
    public void BurstRow(int row, int cols)
    {
        for (int col = 0; col < cols; col++)
        {
            Burst(col + 0.5, row + 0.5, 7, 255, 190, 70);
        }
    }

    private static int Clamp(int v)
    {
        if (v < 0) return 0;
        if (v > 255) return 255;
        return v;
    }

    public void Update(double ms)
    {
        double seconds = ms / 1000.0;

        for (int i = Particles.Count - 1; i >= 0; i--)
        {
            Particles[i].Update(seconds);

            if (Particles[i].IsDead)
            {
                Particles.RemoveAt(i);
            }
        }

        for (int i = Popups.Count - 1; i >= 0; i--)
        {
            Popups[i].Update(seconds);

            if (Popups[i].IsDead)
            {
                Popups.RemoveAt(i);
            }
        }

        // the shake and the flash calm down over time
        Shake = Shake * Math.Pow(0.004, seconds);
        if (Shake < 0.05)
        {
            Shake = 0;
        }

        Flash = Flash - seconds * 3.0;
        if (Flash < 0)
        {
            Flash = 0;
        }
    }
}
