using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

// The actual window. It listens for key presses, keeps a timer
// running, and asks the Renderer to paint the game.
class GameForm : Form
{
    private Assets assets;
    private Sound sound;
    private Renderer renderer;
    private Game game;

    private System.Windows.Forms.Timer timer;
    private Stopwatch clock = new Stopwatch();
    private double lastTime = 0;

    public GameForm()
    {
        assets = new Assets();
        sound = new Sound(assets.Folder);
        renderer = new Renderer(assets);
        game = new Game(sound, assets.Boom.Length);

        Text = "Block Row Disappear";
        ClientSize = new Size(Renderer.WindowWidth, Renderer.WindowHeight);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(10, 10, 16);
        DoubleBuffered = true;
        KeyPreview = true;

        timer = new System.Windows.Forms.Timer();
        timer.Interval = 16;          // about 60 times a second
        timer.Tick += OnTick;

        clock.Start();
        timer.Start();
    }

    private void OnTick(object sender, EventArgs e)
    {
        double now = clock.Elapsed.TotalMilliseconds;
        double gap = now - lastTime;
        lastTime = now;

        // if the window was dragged about, do not let one huge jump through
        if (gap > 100)
        {
            gap = 100;
        }

        renderer.Time = now;
        game.Update(gap);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        renderer.Draw(e.Graphics, game);
    }

    // without this the arrow keys get eaten by the window
    // before they ever reach OnKeyDown
    protected override bool IsInputKey(Keys keyData)
    {
        if (keyData == Keys.Left || keyData == Keys.Right ||
            keyData == Keys.Up || keyData == Keys.Down)
        {
            return true;
        }
        return base.IsInputKey(keyData);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            Close();
            return;
        }

        // on the title screen and the game over screen, any key starts a game
        if (game.State == Mode.Title || game.State == Mode.Dead)
        {
            game.PressAnyKey();
            Invalidate();
            return;
        }

        // LEFT and RIGHT slide the piece across
        if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A)
        {
            game.MoveLeft();
        }
        if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D)
        {
            game.MoveRight();
        }

        // UP spins it round
        if (e.KeyCode == Keys.Up)
        {
            game.Rotate(true);       // spin clockwise
        }
        if (e.KeyCode == Keys.Z)
        {
            game.Rotate(false);      // spin the other way
        }

        if (e.KeyCode == Keys.Down)
        {
            game.Down();
        }
        if (e.KeyCode == Keys.Space)
        {
            game.HardDrop();
        }
        if (e.KeyCode == Keys.P)
        {
            game.TogglePause();
        }
        if (e.KeyCode == Keys.R)
        {
            game.StartGame();
        }
        if (e.KeyCode == Keys.M)
        {
            sound.On = !sound.On;
        }

        Invalidate();
    }
}
