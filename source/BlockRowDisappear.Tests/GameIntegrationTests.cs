using System;
using System.IO;
using Xunit;

namespace BlockRowDisappear.Tests;

// These play the game through Game's real public methods instead of poking
// state directly, to catch bugs that only show up once several parts
// (falling, landing, exploding, scoring) work together.
public class GameIntegrationTests
{
    private static Sound SilentSound() =>
        new Sound(Path.Combine(Path.GetTempPath(), "no-such-folder-" + Guid.NewGuid()));

    private static Game PlayingGame()
    {
        var game = new Game(SilentSound(), 8);
        game.PressAnyKey();
        game.Update(2000);   // clear the "GO!" banner
        return game;
    }

    [Fact]
    public void HardDrop_ClearsAFullRow_AndAddsScore_AfterTheExplosionFinishes()
    {
        var game = PlayingGame();

        // fill the bottom row except the four columns the piece will drop into
        for (int col = 4; col < Board.Cols; col++)
        {
            game.Board.SetCell(Board.Rows - 1, col, 1);
        }

        game.Current = new Piece(1);   // the long piece, one row tall
        game.Current.Row = 0;
        game.Current.Col = 0;

        game.HardDrop();

        Assert.True(game.Exploding);
        Assert.Equal(0, game.Score);   // points aren't added until the explosion finishes

        // run enough ticks to get through all 8 explosion frames (55ms each)
        for (int i = 0; i < 10; i++)
        {
            game.Update(60);
        }

        Assert.False(game.Exploding);
        Assert.Equal(100, game.Score);
        Assert.Equal(1, game.Lines);

        for (int col = 0; col < Board.Cols; col++)
        {
            Assert.Equal(0, game.Board.GetCell(Board.Rows - 1, col));
        }
    }

    [Fact]
    public void HardDrop_WithNoFullRow_HandsOutANewPieceRightAway()
    {
        var game = PlayingGame();
        var dropped = game.Current;

        game.HardDrop();

        Assert.False(game.Exploding);
        Assert.NotSame(dropped, game.Current);
    }

    [Fact]
    public void Rotate_KicksThePieceBackInBounds_AgainstTheRightWall()
    {
        var game = PlayingGame();

        game.Current = new Piece(1);   // the long piece, 4 wide when flat
        game.Current.Row = 5;
        game.Current.Col = Board.Cols - 4;   // flush against the right wall

        game.Rotate(true);

        Assert.InRange(game.Current.Col, 0, Board.Cols - game.Current.Size);
        Assert.True(game.Board.Fits(game.Current, game.Current.Row, game.Current.Col));
    }

    [Fact]
    public void Rotate_DoesNothing_WhenNoKickPositionFits()
    {
        var game = PlayingGame();

        // wall off both sides so every possible kick position is blocked
        for (int row = 0; row < Board.Rows; row++)
        {
            for (int col = 0; col < Board.Cols; col++)
            {
                game.Board.SetCell(row, col, 1);
            }
        }
        for (int col = 3; col <= 6; col++)
        {
            game.Board.SetCell(5, col, 0);
        }

        game.Current = new Piece(1);
        game.Current.Row = 5;
        game.Current.Col = 3;
        var before = game.Current;

        game.Rotate(true);

        Assert.Same(before, game.Current);   // rotation was rejected, piece unchanged
    }

    [Fact]
    public void GhostRow_IsWhereThePieceWouldLand()
    {
        var game = PlayingGame();

        game.Board.SetCell(Board.Rows - 1, 0, 1);

        game.Current = new Piece(2);   // square
        game.Current.Row = 0;
        game.Current.Col = 0;

        Assert.Equal(Board.Rows - 3, game.GhostRow);
    }

    [Fact]
    public void HardDrop_ClearsFourRowsAtOnce_AndAwardsTheQuadBonus()
    {
        var game = PlayingGame();

        for (int row = 16; row < Board.Rows; row++)
        {
            for (int col = 0; col < Board.Cols; col++)
            {
                if (col != 5)
                {
                    game.Board.SetCell(row, col, 1);
                }
            }
        }

        // rotate the long piece upright so it is one column, four rows tall,
        // and line its filled column up with the gap at column 5
        var vertical = new Piece(1).Spun(true);
        game.Current = vertical;
        game.Current.Row = 16;
        game.Current.Col = 3;

        game.HardDrop();

        Assert.True(game.Exploding);

        for (int i = 0; i < 10; i++)
        {
            game.Update(60);
        }

        Assert.False(game.Exploding);
        Assert.Equal(800, game.Score);
        Assert.Equal(4, game.Lines);

        for (int row = 16; row < Board.Rows; row++)
        {
            for (int col = 0; col < Board.Cols; col++)
            {
                Assert.Equal(0, game.Board.GetCell(row, col));
            }
        }
    }

    [Fact]
    public void MoveLeft_StopsAtTheWall()
    {
        var game = PlayingGame();
        game.Current = new Piece(2);
        game.Current.Row = 0;
        game.Current.Col = 0;

        game.MoveLeft();

        Assert.Equal(0, game.Current.Col);
    }
}
