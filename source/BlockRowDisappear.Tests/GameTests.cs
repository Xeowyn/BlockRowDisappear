using System;
using System.IO;
using Xunit;

namespace BlockRowDisappear.Tests;

public class GameTests
{
    // a folder that does not exist, so Sound loads nothing and stays silent -
    // Sound.Add already skips missing files on purpose, so this is a supported no-op
    private static Sound SilentSound() =>
        new Sound(Path.Combine(Path.GetTempPath(), "no-such-folder-" + Guid.NewGuid()));

    private static Game NewGame() => new Game(SilentSound(), 8);

    [Theory]
    [InlineData(1, 100)]
    [InlineData(2, 300)]
    [InlineData(3, 500)]
    [InlineData(4, 800)]
    public void AddPoints_GivesTheRightScoreForEachLineClear(int linesCleared, int expectedPoints)
    {
        var game = NewGame();
        game.AddPoints(linesCleared, 0);

        Assert.Equal(expectedPoints, game.Score);
        Assert.Equal(linesCleared, game.Lines);
    }

    [Fact]
    public void AddPoints_KeepsBestScoreUpToDate()
    {
        var game = NewGame();
        game.AddPoints(4, 0);

        Assert.Equal(800, game.Best);
    }

    [Fact]
    public void Level_StaysAtOne_UntilTenLinesAreCleared()
    {
        var game = NewGame();
        for (int i = 0; i < 9; i++)
        {
            game.AddPoints(1, 0);
        }

        Assert.Equal(1, game.Level);
    }

    [Fact]
    public void Level_GoesUpOnTheTenthLine_AndFallDelaySpeedsUp()
    {
        var game = NewGame();
        double delayBefore = game.FallDelay;

        for (int i = 0; i < 10; i++)
        {
            game.AddPoints(1, 0);
        }

        Assert.Equal(2, game.Level);
        Assert.True(game.FallDelay < delayBefore);
        Assert.Equal(460, game.FallDelay);
    }

    [Fact]
    public void FallDelay_NeverDropsBelowTheFloorOfAHundredMs()
    {
        var game = NewGame();

        // enough lines to push the level far past where the delay would go negative
        for (int i = 0; i < 200; i++)
        {
            game.AddPoints(1, 0);
        }

        Assert.Equal(100, game.FallDelay);
    }

    [Fact]
    public void Game_StartsOnTheTitleScreen()
    {
        var game = NewGame();
        Assert.Equal(Mode.Title, game.State);
    }

    [Fact]
    public void PressAnyKey_FromTitle_ShowsTheGoBanner_ThenStartsPlaying()
    {
        var game = NewGame();
        game.PressAnyKey();
        Assert.Equal(Mode.Banner, game.State);

        game.Update(2000);   // longer than the banner's own timer
        Assert.Equal(Mode.Playing, game.State);
    }

    [Fact]
    public void TogglePause_SwitchesBetweenPlayingAndPaused()
    {
        var game = NewGame();
        game.PressAnyKey();
        game.Update(2000);
        Assert.Equal(Mode.Playing, game.State);

        game.TogglePause();
        Assert.Equal(Mode.Paused, game.State);

        game.TogglePause();
        Assert.Equal(Mode.Playing, game.State);
    }

    [Fact]
    public void NewPiece_EndsTheGame_WhenThereIsNowhereForItToSpawn()
    {
        var game = NewGame();
        game.PressAnyKey();
        game.Update(2000);

        // fill the whole board so no piece can possibly spawn
        for (int row = 0; row < Board.Rows; row++)
        {
            for (int col = 0; col < Board.Cols; col++)
            {
                game.Board.SetCell(row, col, 1);
            }
        }

        game.NewPiece();

        Assert.Equal(Mode.Dead, game.State);
    }

    [Fact]
    public void NewPiece_RemembersTheBestScore_WhenTheGameEnds()
    {
        var game = NewGame();
        game.PressAnyKey();
        game.Update(2000);
        game.AddPoints(4, 0);   // Score = 800

        for (int row = 0; row < Board.Rows; row++)
        {
            for (int col = 0; col < Board.Cols; col++)
            {
                game.Board.SetCell(row, col, 1);
            }
        }

        game.NewPiece();

        Assert.Equal(800, game.Best);
    }

    [Fact]
    public void Reset_ClearsTheBoardScoreAndLevel()
    {
        var game = NewGame();
        game.AddPoints(4, 0);

        game.Reset();

        Assert.Equal(0, game.Score);
        Assert.Equal(0, game.Lines);
        Assert.Equal(1, game.Level);
        for (int row = 0; row < Board.Rows; row++)
        {
            for (int col = 0; col < Board.Cols; col++)
            {
                Assert.Equal(0, game.Board.GetCell(row, col));
            }
        }
    }
}
