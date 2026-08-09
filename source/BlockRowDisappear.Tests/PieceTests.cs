using Xunit;

namespace BlockRowDisappear.Tests;

public class PieceTests
{
    [Fact]
    public void Kind1_IsTheLongPiece()
    {
        AssertShape(new Piece(1), new[,]
        {
            {0,0,0,0},
            {1,1,1,1},
            {0,0,0,0},
            {0,0,0,0},
        });
    }

    [Fact]
    public void Kind2_IsTheSquare()
    {
        AssertShape(new Piece(2), new[,]
        {
            {1,1},
            {1,1},
        });
    }

    [Fact]
    public void Kind3_IsTheTShape()
    {
        AssertShape(new Piece(3), new[,]
        {
            {0,1,0},
            {1,1,1},
            {0,0,0},
        });
    }

    [Fact]
    public void Kind4_IsTheSShape()
    {
        AssertShape(new Piece(4), new[,]
        {
            {0,1,1},
            {1,1,0},
            {0,0,0},
        });
    }

    [Fact]
    public void Kind5_IsTheZShape()
    {
        AssertShape(new Piece(5), new[,]
        {
            {1,1,0},
            {0,1,1},
            {0,0,0},
        });
    }

    [Fact]
    public void Kind6_IsTheJShape()
    {
        AssertShape(new Piece(6), new[,]
        {
            {1,0,0},
            {1,1,1},
            {0,0,0},
        });
    }

    [Fact]
    public void Kind7_IsTheLShape()
    {
        AssertShape(new Piece(7), new[,]
        {
            {0,0,1},
            {1,1,1},
            {0,0,0},
        });
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public void Spun_Clockwise_RotatesEachBlockToTheRightPlace(int kind)
    {
        var original = new Piece(kind);
        var spun = original.Spun(true);
        int size = original.Size;

        Assert.Equal(size, spun.Size);

        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                // clockwise: the top row becomes the right column
                bool expected = original.HasBlockAt(row, col);
                Assert.Equal(expected, spun.HasBlockAt(col, size - 1 - row));
            }
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public void Spun_CounterClockwise_RotatesEachBlockToTheRightPlace(int kind)
    {
        var original = new Piece(kind);
        var spun = original.Spun(false);
        int size = original.Size;

        Assert.Equal(size, spun.Size);

        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                bool expected = original.HasBlockAt(row, col);
                Assert.Equal(expected, spun.HasBlockAt(size - 1 - col, row));
            }
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public void Spun_FourTimes_ReturnsToTheOriginalShape(int kind)
    {
        var original = new Piece(kind);
        var spun = original.Spun().Spun().Spun().Spun();

        AssertSameShape(original, spun);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public void Spun_FourTimesCounterClockwise_ReturnsToTheOriginalShape(int kind)
    {
        var original = new Piece(kind);
        var spun = original.Spun(false).Spun(false).Spun(false).Spun(false);

        AssertSameShape(original, spun);
    }

    [Fact]
    public void Spun_DoesNotChangeTheOriginalPiece()
    {
        var original = new Piece(3);
        original.Spun(true);

        AssertShape(original, new[,]
        {
            {0,1,0},
            {1,1,1},
            {0,0,0},
        });
    }

    [Fact]
    public void Kind_IsKeptAfterSpinning()
    {
        var original = new Piece(6);
        var spun = original.Spun(true);

        Assert.Equal(original.Kind, spun.Kind);
    }

    private static void AssertShape(Piece piece, int[,] expected)
    {
        int size = expected.GetLength(0);
        Assert.Equal(size, piece.Size);

        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                Assert.Equal(expected[row, col] != 0, piece.HasBlockAt(row, col));
            }
        }
    }

    private static void AssertSameShape(Piece a, Piece b)
    {
        Assert.Equal(a.Size, b.Size);

        for (int row = 0; row < a.Size; row++)
        {
            for (int col = 0; col < a.Size; col++)
            {
                Assert.Equal(a.HasBlockAt(row, col), b.HasBlockAt(row, col));
            }
        }
    }
}
