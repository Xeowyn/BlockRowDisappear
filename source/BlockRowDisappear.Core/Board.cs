// The playing field. It holds all the blocks that have landed,
// and it is the thing that knows about walls, the floor,
// and which rows are full.
public class Board
{
    public const int Rows = 20;
    public const int Cols = 10;

    // 0 means empty, 1 to 7 means a colored block is there.
    // private so the rest of the game has to go through my methods.
    private int[,] cells = new int[Rows, Cols];

    // which rows are full right now (used by the explosion)
    private bool[] fullRows = new bool[Rows];

    public Board()
    {
        Clear();
    }

    public void Clear()
    {
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Cols; col++)
            {
                cells[row, col] = 0;
            }
            fullRows[row] = false;
        }
    }

    public int GetCell(int row, int col)
    {
        return cells[row, col];
    }

    public void SetCell(int row, int col, int value)
    {
        cells[row, col] = value;
    }

    public bool IsRowFull(int row)
    {
        return fullRows[row];
    }

    // can this piece sit here without hitting a wall, the floor, or a block?
    public bool Fits(Piece p, int atRow, int atCol)
    {
        for (int row = 0; row < p.Size; row++)
        {
            for (int col = 0; col < p.Size; col++)
            {
                if (p.HasBlockAt(row, col))
                {
                    int r = atRow + row;
                    int c = atCol + col;

                    if (c < 0)
                    {
                        return false;   // off the left side
                    }
                    if (c >= Cols)
                    {
                        return false;   // off the right side
                    }
                    if (r >= Rows)
                    {
                        return false;   // through the floor
                    }
                    if (r >= 0 && cells[r, c] != 0)
                    {
                        return false;   // there is already a block here
                    }
                }
            }
        }

        return true;
    }

    // copy a piece onto the board so it stays there for good
    public void Stamp(Piece p)
    {
        for (int row = 0; row < p.Size; row++)
        {
            for (int col = 0; col < p.Size; col++)
            {
                if (p.HasBlockAt(row, col))
                {
                    int r = p.Row + row;
                    int c = p.Col + col;

                    if (r >= 0 && r < Rows && c >= 0 && c < Cols)
                    {
                        cells[r, c] = p.Kind;
                    }
                }
            }
        }
    }

    // look for rows that are completely full, remember them,
    // and say how many there were
    public int FindFullRows()
    {
        int howMany = 0;

        for (int row = 0; row < Rows; row++)
        {
            bool full = true;

            for (int col = 0; col < Cols; col++)
            {
                if (cells[row, col] == 0)
                {
                    full = false;
                }
            }

            fullRows[row] = full;

            if (full == true)
            {
                howMany = howMany + 1;
            }
        }

        return howMany;
    }

    // actually delete the full rows and slide everything above them down
    public void RemoveFullRows()
    {
        for (int row = Rows - 1; row >= 0; row--)
        {
            if (fullRows[row] == true)
            {
                // slide everything above this row down by one
                for (int r = row; r > 0; r--)
                {
                    for (int col = 0; col < Cols; col++)
                    {
                        cells[r, col] = cells[r - 1, col];
                    }
                }

                // the very top row is empty now
                for (int col = 0; col < Cols; col++)
                {
                    cells[0, col] = 0;
                }

                // everything moved down so the flags have to move down too
                for (int r = row; r > 0; r--)
                {
                    fullRows[r] = fullRows[r - 1];
                }
                fullRows[0] = false;

                row = row + 1;   // check this row again, something new fell into it
            }
        }
    }
}
