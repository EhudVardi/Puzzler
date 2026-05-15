using System.Collections.Generic;
using Common.Models.Base;

namespace Common.Models.Kurodoko
{
    public class BoardKurodoko : BoardGeneric<GroupKurodoko, CellValueKurodoko, CellValueKurodoko>
    {
        private int _rows;
        private int _columns;

        public override int Rows    => _rows;
        public override int Columns => _columns;

        public void SetDimensions(int rows, int columns)
        {
            _rows    = rows;
            _columns = columns;
        }

        public CellValueKurodoko Cell(int row, int col) =>
            (CellValueKurodoko)CellsMatrix[row, col];

        public IEnumerable<CellValueKurodoko> Neighbors(CellValueKurodoko cell)
        {
            int r = cell.Row, c = cell.Column;
            if (r > 0)              yield return Cell(r - 1, c);
            if (r < _rows - 1)     yield return Cell(r + 1, c);
            if (c > 0)             yield return Cell(r, c - 1);
            if (c < _columns - 1)  yield return Cell(r, c + 1);
        }

        public IEnumerable<CellValueKurodoko> RayFrom(CellValueKurodoko start, int dr, int dc)
        {
            int r = start.Row + dr;
            int c = start.Column + dc;
            while (r >= 0 && r < _rows && c >= 0 && c < _columns)
            {
                yield return Cell(r, c);
                r += dr;
                c += dc;
            }
        }
    }
}
