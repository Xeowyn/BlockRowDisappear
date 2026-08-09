using System;

// One block piece. It knows its own shape, which picture to use,
// and where it is sitting on the board.
class Piece
{
    // the shape. a 1 means there is a block there.
    // it is private so nothing outside this class can mess it up.
    private int[,] shape;
    private int kind;

    public int Row;
    public int Col;

    // make a normal piece. kind is 1 to 7.
    public Piece(int kind)
    {
        this.shape = MakeShape(kind);
        this.kind = kind;
        Row = 0;
        Col = 0;
    }

    // this one is only used inside this class, when spinning
    private Piece(int[,] newShape, int newKind)
    {
        shape = newShape;
        kind = newKind;
        Row = 0;
        Col = 0;
    }

    // which of the 7 pictures this piece uses
    public int Kind
    {
        get { return kind; }
    }

    // how wide and tall the shape box is
    public int Size
    {
        get { return shape.GetLength(0); }
    }

    public bool HasBlockAt(int row, int col)
    {
        return shape[row, col] != 0;
    }

    // gives back a NEW piece that is spun round.
    // the old one is left alone.
    //
    // spinning right: the top row becomes the right column.
    // spinning left is the same idea the other way.
    public Piece Spun(bool clockwise)
    {
        int size = Size;
        int[,] temp = new int[size, size];

        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                if (clockwise)
                {
                    temp[col, size - 1 - row] = shape[row, col];
                }
                else
                {
                    temp[size - 1 - col, row] = shape[row, col];
                }
            }
        }

        Piece spun = new Piece(temp, kind);
        spun.Row = Row;
        spun.Col = Col;
        return spun;
    }

    // if you do not say which way, it spins right
    public Piece Spun()
    {
        return Spun(true);
    }

    // picks one of the 7 pieces at random
    public static Piece Random(Random rnd)
    {
        return new Piece(rnd.Next(1, 8));
    }

    // the 7 block shapes
    private static int[,] MakeShape(int kind)
    {
        if (kind == 1)
        {
            // the long one
            return new int[,] {
                {0,0,0,0},
                {1,1,1,1},
                {0,0,0,0},
                {0,0,0,0} };
        }
        if (kind == 2)
        {
            // the square
            return new int[,] {
                {1,1},
                {1,1} };
        }
        if (kind == 3)
        {
            // T shape
            return new int[,] {
                {0,1,0},
                {1,1,1},
                {0,0,0} };
        }
        if (kind == 4)
        {
            // S shape
            return new int[,] {
                {0,1,1},
                {1,1,0},
                {0,0,0} };
        }
        if (kind == 5)
        {
            // Z shape
            return new int[,] {
                {1,1,0},
                {0,1,1},
                {0,0,0} };
        }
        if (kind == 6)
        {
            // J shape
            return new int[,] {
                {1,0,0},
                {1,1,1},
                {0,0,0} };
        }

        // L shape
        return new int[,] {
            {0,0,1},
            {1,1,1},
            {0,0,0} };
    }
}
