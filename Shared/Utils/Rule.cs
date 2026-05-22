using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Utils
{
    public static class Rule
    {
        public static bool CheckRow(Board board, int row, int value)
        {
            for (int col = 0; col < 9; col++)
            {
                if (board.Cells[row, col].Value == value)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool CheckCol(Board board, int col, int value)
        {
            for (int row = 0; row < 9; row++)
            {
                if (board.Cells[row, col].Value == value)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool CheckBox(Board board, int row, int col, int value)
        {
            int startRow = row / 3 * 3;
            int startCol = col / 3 * 3;

            for (int r = startRow; r < startRow + 3; r++)
            {
                for (int c = startCol; c < startCol + 3; c++)
                {
                    if (board.Cells[r, c].Value == value)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

    }
}
