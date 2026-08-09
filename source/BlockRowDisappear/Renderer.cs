using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

// Draws everything on the screen.
// This class only looks at the game, it never changes it.
class Renderer
{
    // the pictures are 16x16 but we draw them at 32x32,
    // so every pixel of art becomes 2 pixels on screen and looks chunky
    public const int Tile = 32;
    // a fat margin so the sunset behind the game still shows round the edges
    public const int Margin = 34;
    public const int PanelWidth = 215;

    private Assets assets;
    private Random rnd = new Random();

    // keeps counting up, used for blinking and the moving background
    public double Time = 0;

    private Font labelFont = new Font("Consolas", 10, FontStyle.Regular);
    private Font numberFont = new Font("Consolas", 16, FontStyle.Bold);
    private Font tinyFont = new Font("Consolas", 9, FontStyle.Bold);

    public Renderer(Assets assets)
    {
        this.assets = assets;
    }

    public static int WindowWidth
    {
        get { return Margin * 3 + Board.Cols * Tile + PanelWidth; }
    }

    public static int WindowHeight
    {
        get { return Margin * 2 + Board.Rows * Tile; }
    }

    public void Draw(Graphics g, Game game)
    {
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = PixelOffsetMode.Half;

        DrawBackground(g);

        // shake the whole picture about when something big happens
        int shakeX = 0;
        int shakeY = 0;
        if (game.Effects.Shake > 0.1)
        {
            double amount = game.Effects.Shake;
            shakeX = (int)((rnd.NextDouble() * 2 - 1) * amount);
            shakeY = (int)((rnd.NextDouble() * 2 - 1) * amount);
        }

        GraphicsState saved = g.Save();
        g.TranslateTransform(shakeX, shakeY);

        int boardX = Margin;
        int boardY = Margin;

        DrawBoard(g, game, boardX, boardY);

        if (game.State == Mode.Playing || game.State == Mode.Paused)
        {
            DrawGhost(g, game, boardX, boardY);
        }

        if (game.Exploding == false && game.State != Mode.Dead && game.State != Mode.Title)
        {
            DrawPiece(g, game.Current, game.Current.Row, boardX, boardY, Tile, 1.0f);
        }

        DrawSparks(g, game, boardX, boardY);
        DrawPopups(g, game, boardX, boardY);

        // frame around the play area
        g.DrawRectangle(new Pen(Color.FromArgb(105, 105, 135), 2),
                        boardX - 2, boardY - 2,
                        Board.Cols * Tile + 3, Board.Rows * Tile + 3);

        DrawPanel(g, game, boardX + Board.Cols * Tile + Margin * 2, boardY);

        g.Restore(saved);

        DrawScanlines(g);
        DrawFlash(g, game);
        DrawScreens(g, game, boardX, boardY);
    }

    // ------------------------------------------------------------
    // the board
    // ------------------------------------------------------------

    private void DrawBackground(Graphics g)
    {
        // the sunset stays put. it used to scroll, but a sun that
        // slides off the bottom and comes back round the top looks silly.
        g.DrawImage(assets.Background, 0, 0, WindowWidth, WindowHeight);
    }

    private void DrawBoard(Graphics g, Game game, int boardX, int boardY)
    {
        for (int row = 0; row < Board.Rows; row++)
        {
            for (int col = 0; col < Board.Cols; col++)
            {
                int x = boardX + col * Tile;
                int y = boardY + row * Tile;

                g.DrawImage(assets.Empty, x, y, Tile, Tile);

                // is this row blowing up right now?
                if (game.Exploding == true && game.Board.IsRowFull(row))
                {
                    int frame = game.BoomFrame;
                    if (frame >= assets.Boom.Length)
                    {
                        frame = assets.Boom.Length - 1;
                    }
                    g.DrawImage(assets.Boom[frame], x, y, Tile, Tile);
                }
                else if (game.Board.GetCell(row, col) != 0)
                {
                    g.DrawImage(assets.Blocks[game.Board.GetCell(row, col)], x, y, Tile, Tile);
                }
            }
        }
    }

    // the faint outline showing where the piece will land
    private void DrawGhost(Graphics g, Game game, int boardX, int boardY)
    {
        int ghostRow = game.GhostRow;

        if (ghostRow == game.Current.Row)
        {
            return;   // the piece is already there, no point
        }

        Piece p = game.Current;

        for (int row = 0; row < p.Size; row++)
        {
            for (int col = 0; col < p.Size; col++)
            {
                if (p.HasBlockAt(row, col))
                {
                    int r = ghostRow + row;
                    int c = p.Col + col;

                    if (r >= 0 && r < Board.Rows)
                    {
                        g.DrawImage(assets.Ghost,
                                    boardX + c * Tile, boardY + r * Tile, Tile, Tile);
                    }
                }
            }
        }
    }

    private void DrawPiece(Graphics g, Piece p, int atRow, int atX, int atY, int size, float alpha)
    {
        for (int row = 0; row < p.Size; row++)
        {
            for (int col = 0; col < p.Size; col++)
            {
                if (p.HasBlockAt(row, col))
                {
                    int r = atRow + row;
                    int c = p.Col + col;

                    if (r >= 0)
                    {
                        g.DrawImage(assets.Blocks[p.Kind],
                                    atX + c * size, atY + r * size, size, size);
                    }
                }
            }
        }
    }

    // ------------------------------------------------------------
    // the eye candy
    // ------------------------------------------------------------

    private void DrawSparks(Graphics g, Game game, int boardX, int boardY)
    {
        Rectangle inside = new Rectangle(boardX, boardY, Board.Cols * Tile, Board.Rows * Tile);
        Region old = g.Clip;
        g.SetClip(inside);

        foreach (Particle p in game.Effects.Particles)
        {
            int alpha = (int)(255 * p.Fade);
            if (alpha < 6)
            {
                continue;
            }

            int size = (int)Math.Max(2, p.Size * Tile);
            int x = boardX + (int)(p.Col * Tile) - size / 2;
            int y = boardY + (int)(p.Row * Tile) - size / 2;

            using (SolidBrush brush = new SolidBrush(
                       Color.FromArgb(alpha, p.Red, p.Green, p.Blue)))
            {
                g.FillRectangle(brush, x, y, size, size);
            }
        }

        g.Clip = old;
    }

    private void DrawPopups(Graphics g, Game game, int boardX, int boardY)
    {
        foreach (Popup p in game.Effects.Popups)
        {
            int alpha = (int)(255 * Math.Min(1.0, p.Fade * 1.6));
            if (alpha < 6)
            {
                continue;
            }

            float x = boardX + (float)(p.Col * Tile);
            float y = boardY + (float)(p.Row * Tile);

            RectangleF box = new RectangleF(x - 130, y - 16, 260, 34);
            DrawPixelText(g, p.Text, box, 2,
                          Color.FromArgb(alpha, p.Red, p.Green, p.Blue));
        }
    }

    // thin dark lines across the screen, like an old TV
    private void DrawScanlines(Graphics g)
    {
        using (Pen pen = new Pen(Color.FromArgb(38, 0, 0, 0)))
        {
            for (int y = 0; y < WindowHeight; y += 3)
            {
                g.DrawLine(pen, 0, y, WindowWidth, y);
            }
        }
    }

    private void DrawFlash(Graphics g, Game game)
    {
        if (game.Effects.Flash <= 0.01)
        {
            return;
        }

        int alpha = (int)(Math.Min(1.0, game.Effects.Flash) * 190);
        using (SolidBrush brush = new SolidBrush(Color.FromArgb(alpha, 255, 255, 255)))
        {
            g.FillRectangle(brush, 0, 0, WindowWidth, WindowHeight);
        }
    }

    // ------------------------------------------------------------
    // the side panel
    // ------------------------------------------------------------

    private void DrawPanel(Graphics g, Game game, int x, int y)
    {
        Brush white = Brushes.White;
        Brush grey = new SolidBrush(Color.FromArgb(175, 175, 205));

        // a dark sheet behind the panel, otherwise the bright sunset
        // shows through and you cannot read any of the numbers
        using (SolidBrush back = new SolidBrush(Color.FromArgb(215, 14, 7, 30)))
        {
            g.FillRectangle(back, x - 12, y - 12, PanelWidth + 4, WindowHeight - Margin * 2 + 24);
        }
        g.DrawRectangle(new Pen(Color.FromArgb(90, 60, 130)),
                        x - 12, y - 12, PanelWidth + 4, WindowHeight - Margin * 2 + 24);

        DrawPixelText(g, "BLOCK ROW", new RectangleF(x - 8, y - 4, PanelWidth, 26), 2,
                      Color.FromArgb(120, 235, 255));
        DrawPixelText(g, "DISAPPEAR", new RectangleF(x - 8, y + 20, PanelWidth, 26), 2,
                      Color.FromArgb(255, 135, 205));

        g.DrawString("SCORE", labelFont, grey, x, y + 46);
        g.DrawString("" + game.Score, numberFont, white, x, y + 62);

        g.DrawString("LINES", labelFont, grey, x, y + 100);
        g.DrawString("" + game.Lines, numberFont, white, x, y + 116);

        g.DrawString("LEVEL", labelFont, grey, x, y + 154);
        g.DrawString("" + game.Level, numberFont, white, x, y + 170);

        g.DrawString("BEST", labelFont, grey, x + 110, y + 154);
        g.DrawString("" + game.Best, numberFont, white, x + 110, y + 170);

        g.DrawString("NEXT", labelFont, grey, x, y + 216);

        int boxSize = 24;
        int boxX = x;
        int boxY = y + 234;

        g.FillRectangle(new SolidBrush(Color.FromArgb(160, 14, 14, 22)),
                        boxX, boxY, boxSize * 4 + 8, boxSize * 4 + 8);
        g.DrawRectangle(new Pen(Color.FromArgb(80, 80, 105)),
                        boxX, boxY, boxSize * 4 + 8, boxSize * 4 + 8);

        for (int row = 0; row < game.Next.Size; row++)
        {
            for (int col = 0; col < game.Next.Size; col++)
            {
                if (game.Next.HasBlockAt(row, col))
                {
                    g.DrawImage(assets.Blocks[game.Next.Kind],
                                boxX + 4 + col * boxSize,
                                boxY + 4 + row * boxSize,
                                boxSize, boxSize);
                }
            }
        }

        int helpY = boxY + boxSize * 4 + 30;
        g.DrawString("CONTROLS", labelFont, grey, x, helpY);
        g.DrawString("left / right  move", labelFont, white, x, helpY + 19);
        g.DrawString("up            spin", labelFont, white, x, helpY + 34);
        g.DrawString("down          faster", labelFont, white, x, helpY + 49);
        g.DrawString("space         drop", labelFont, white, x, helpY + 64);
        g.DrawString("p  pause   m  mute", labelFont, white, x, helpY + 83);
        g.DrawString("r  restart esc quit", labelFont, white, x, helpY + 98);
    }

    // ------------------------------------------------------------
    // the title / banner / game over screens
    // ------------------------------------------------------------

    private void DrawScreens(Graphics g, Game game, int boardX, int boardY)
    {
        int w = Board.Cols * Tile;
        int h = Board.Rows * Tile;

        if (game.State == Mode.Title)
        {
            Dim(g, boardX, boardY, w, h, 215);
            DrawPixelText(g, "BLOCK ROW", new RectangleF(boardX, boardY + h / 2 - 168, w, 58), 3,
                          Color.FromArgb(120, 235, 255));
            DrawPixelText(g, "DISAPPEAR", new RectangleF(boardX, boardY + h / 2 - 106, w, 58), 3,
                          Color.FromArgb(255, 135, 205));

            // blink the prompt on and off
            if ((int)(Time / 500) % 2 == 0)
            {
                DrawPixelText(g, "PRESS ANY KEY", new RectangleF(boardX, boardY + h / 2 - 10, w, 34), 2,
                              Color.White);
            }

            DrawPixelText(g, "arrows to move", new RectangleF(boardX, boardY + h / 2 + 60, w, 26), 2,
                          Color.FromArgb(170, 170, 200));
            DrawPixelText(g, "up to spin", new RectangleF(boardX, boardY + h / 2 + 88, w, 26), 2,
                          Color.FromArgb(170, 170, 200));
        }
        else if (game.State == Mode.Banner)
        {
            Dim(g, boardX, boardY + h / 2 - 70, w, 140, 165);
            DrawPixelText(g, game.BannerBig, new RectangleF(boardX, boardY + h / 2 - 60, w, 62), 3,
                          Color.FromArgb(255, 240, 130));
            DrawPixelText(g, game.BannerSmall, new RectangleF(boardX, boardY + h / 2 + 10, w, 34), 2,
                          Color.White);
        }
        else if (game.State == Mode.Paused)
        {
            Dim(g, boardX, boardY + h / 2 - 60, w, 120, 180);
            DrawPixelText(g, "PAUSED", new RectangleF(boardX, boardY + h / 2 - 46, w, 52), 3, Color.White);
            DrawPixelText(g, "press P to carry on", new RectangleF(boardX, boardY + h / 2 + 14, w, 26), 2,
                          Color.FromArgb(180, 180, 210));
        }
        else if (game.State == Mode.Dead)
        {
            Dim(g, boardX, boardY, w, h, 200);
            DrawPixelText(g, "GAME", new RectangleF(boardX, boardY + h / 2 - 140, w, 60), 3,
                          Color.FromArgb(255, 110, 110));
            DrawPixelText(g, "OVER", new RectangleF(boardX, boardY + h / 2 - 82, w, 60), 3,
                          Color.FromArgb(255, 110, 110));

            DrawPixelText(g, "SCORE " + game.Score, new RectangleF(boardX, boardY + h / 2 - 6, w, 34), 2,
                          Color.White);
            DrawPixelText(g, "BEST  " + game.Best, new RectangleF(boardX, boardY + h / 2 + 26, w, 34), 2,
                          Color.FromArgb(255, 235, 130));

            if ((int)(Time / 500) % 2 == 0)
            {
                DrawPixelText(g, "PRESS ANY KEY", new RectangleF(boardX, boardY + h / 2 + 90, w, 30), 2,
                              Color.FromArgb(190, 190, 220));
            }
        }
    }

    private void Dim(Graphics g, int x, int y, int w, int h, int alpha)
    {
        using (SolidBrush brush = new SolidBrush(Color.FromArgb(alpha, 8, 8, 14)))
        {
            g.FillRectangle(brush, x, y, w, h);
        }
    }

    // ------------------------------------------------------------
    // chunky pixel writing
    //
    // The trick: draw the words tiny on a small picture, then blow that
    // picture up with no smoothing. The letters come out big and blocky
    // instead of smooth, so they match the pixel art.
    // ------------------------------------------------------------

    private void DrawPixelText(Graphics g, string text, RectangleF box, int scale, Color colour)
    {
        int smallW = (int)Math.Max(1, box.Width / scale);
        int smallH = (int)Math.Max(1, box.Height / scale);

        using (Bitmap small = new Bitmap(smallW, smallH))
        {
            using (Graphics sg = Graphics.FromImage(small))
            {
                sg.Clear(Color.Transparent);

                // no anti aliasing, we want hard edges
                sg.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;

                StringFormat middle = new StringFormat();
                middle.Alignment = StringAlignment.Center;
                middle.LineAlignment = StringAlignment.Center;

                // size the letters to fit the little picture, and measure in
                // pixels so they do not get chopped about
                float size = smallH * 0.62f;
                if (size < 5) size = 5;

                using (Font font = new Font("Consolas", size, FontStyle.Bold, GraphicsUnit.Pixel))
                using (SolidBrush brush = new SolidBrush(colour))
                {
                    // shrink the letters if the words are too long to fit
                    SizeF wide = sg.MeasureString(text, font);
                    if (wide.Width > smallW && wide.Width > 0)
                    {
                        float smaller = size * (smallW / wide.Width) * 0.96f;
                        if (smaller < 5) smaller = 5;

                        using (Font fitted = new Font("Consolas", smaller, FontStyle.Bold, GraphicsUnit.Pixel))
                        {
                            sg.DrawString(text, fitted, brush,
                                          new RectangleF(0, 0, smallW, smallH), middle);
                        }
                    }
                    else
                    {
                        sg.DrawString(text, font, brush,
                                      new RectangleF(0, 0, smallW, smallH), middle);
                    }
                }
            }

            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(small, box);
        }
    }
}
