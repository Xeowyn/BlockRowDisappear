using Xunit;

namespace BlockRowDisappear.Tests;

public class BoardTests
{
    // the square piece (kind 2) is fully filled in both columns and both rows,
    // which makes it the simplest shape for checking wall/floor edges
    private static Piece Square() => new Piece(2);

    [Fact]
    public void Fits_TrueInsideEmptyBoard()
    {
        var board = new Board();
        Assert.True(board.Fits(Square(), 0, 0));
    }

    [Fact]
    public void Fits_FalseOffLeftWall()
    {
        var board = new Board();
        Assert.False(board.Fits(Square(), 0, -1));
    }

    [Fact]
    public void Fits_TrueAtLeftWall()
    {
        var board = new Board();
        Assert.True(board.Fits(Square(), 0, 0));
    }

    [Fact]
    public void Fits_FalseOffRightWall()
    {
        var board = new Board();
        Assert.False(board.Fits(Square(), 0, Board.Cols - 1));
    }

    [Fact]
    public void Fits_TrueAtRightWall()
    {
        var board = new Board();
        Assert.True(board.Fits(Square(), 0, Board.Cols - 2));
    }

    [Fact]
    public void Fits_FalseThroughFloor()
    {
        var board = new Board();
        Assert.False(board.Fits(Square(), Board.Rows - 1, 0));
    }

    [Fact]
    public void Fits_TrueAtFloor()
    {
        var board = new Board();
        Assert.True(board.Fits(Square(), Board.Rows - 2, 0));
    }

    [Fact]
    public void Fits_FalseWhenLandingOnAnExistingBlock()
    {
        var board = new Board();
        board.SetCell(5, 0, 1);

        // square dropped so its bottom row lands on row 5
        Assert.False(board.Fits(Square(), 4, 0));
    }

    [Fact]
    public void Fits_TrueWhenPieceIsPartlyAboveTheTopOfTheBoard()
    {
        var board = new Board();

        // the T piece has an empty top-left corner, so putting it at row -1
        // pushes its top block off the board while the rest is still on it.
        // this has to be allowed since pieces spawn this way and fall in.
        var t = new Piece(3);
        Assert.True(board.Fits(t, -1, 0));
    }

    [Fact]
    public void Stamp_CopiesEveryBlockOfThePieceOntoTheBoard()
    {
        var board = new Board();
        var square = Square();
        square.Row = 3;
        square.Col = 4;

        board.Stamp(square);

        Assert.Equal(2, board.GetCell(3, 4));
        Assert.Equal(2, board.GetCell(3, 5));
        Assert.Equal(2, board.GetCell(4, 4));
        Assert.Equal(2, board.GetCell(4, 5));
    }

    [Fact]
    public void Stamp_IgnoresTheBlocksThatAreOffTheTopOfTheBoard()
    {
        var board = new Board();
        var t = new Piece(3);
        t.Row = -1;
        t.Col = 0;

        board.Stamp(t);

        // only the middle row of the T (which lands on row 0) should appear
        Assert.Equal(3, board.GetCell(0, 0));
        Assert.Equal(3, board.GetCell(0, 1));
        Assert.Equal(3, board.GetCell(0, 2));
    }

    [Fact]
    public void FindFullRows_ZeroOnAnEmptyBoard()
    {
        var board = new Board();
        Assert.Equal(0, board.FindFullRows());
    }

    [Fact]
    public void FindFullRows_FindsOneFullRow()
    {
        var board = new Board();
        FillRow(board, 10);

        Assert.Equal(1, board.FindFullRows());
        Assert.True(board.IsRowFull(10));
    }

    [Fact]
    public void FindFullRows_FindsEveryRowWhenTheWholeBoardIsFull()
    {
        var board = new Board();
        for (int row = 0; row < Board.Rows; row++)
        {
            FillRow(board, row);
        }

        Assert.Equal(Board.Rows, board.FindFullRows());
    }

    [Fact]
    public void FindFullRows_ARowMissingOneCellIsNotFull()
    {
        var board = new Board();
        FillRow(board, 5);
        board.SetCell(5, 3, 0);   // knock one cell back out

        Assert.Equal(0, board.FindFullRows());
        Assert.False(board.IsRowFull(5));
    }

    [Fact]
    public void RemoveFullRows_ShiftsThingsDownCorrectly_WhenTwoNonAdjacentRowsAreFull()
    {
        var board = new Board();

        // give every row a marker in column 0 so we can track where it ends up
        for (int row = 0; row < Board.Rows; row++)
        {
            board.SetCell(row, 0, row + 1);
        }

        FillRow(board, 5);
        FillRow(board, 10);

        Assert.Equal(2, board.FindFullRows());
        board.RemoveFullRows();

        // rows 5 and 10 are gone, so everything that was above each of them
        // drops down by one - twice for rows that were above both
        Assert.Equal(0, board.GetCell(0, 0));   // new empty row
        Assert.Equal(0, board.GetCell(1, 0));   // new empty row
        Assert.Equal(1, board.GetCell(2, 0));   // was row 0
        Assert.Equal(2, board.GetCell(3, 0));   // was row 1
        Assert.Equal(3, board.GetCell(4, 0));   // was row 2
        Assert.Equal(4, board.GetCell(5, 0));   // was row 3
        Assert.Equal(5, board.GetCell(6, 0));   // was row 4
        Assert.Equal(7, board.GetCell(7, 0));   // was row 6
        Assert.Equal(8, board.GetCell(8, 0));   // was row 7
        Assert.Equal(9, board.GetCell(9, 0));   // was row 8
        Assert.Equal(10, board.GetCell(10, 0)); // was row 9
        Assert.Equal(12, board.GetCell(11, 0)); // was row 11, untouched
        Assert.Equal(20, board.GetCell(19, 0)); // was row 19, untouched
    }

    [Fact]
    public void RemoveFullRows_ClearsEverything_WhenTheWholeBoardIsFull()
    {
        var board = new Board();
        for (int row = 0; row < Board.Rows; row++)
        {
            FillRow(board, row);
        }

        board.FindFullRows();
        board.RemoveFullRows();

        for (int row = 0; row < Board.Rows; row++)
        {
            for (int col = 0; col < Board.Cols; col++)
            {
                Assert.Equal(0, board.GetCell(row, col));
            }
        }
    }

    private static void FillRow(Board board, int row)
    {
        for (int col = 0; col < Board.Cols; col++)
        {
            board.SetCell(row, col, 1);
        }
    }
}
