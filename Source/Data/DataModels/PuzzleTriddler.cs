using System;
using System.Collections.Generic;
using System.Text;
using Data.DataModels;

namespace Data.DataModels
{
    public class PuzzleTriddler : PuzzleBase
    {
        private List<List<int>> _horizontals = null!;
        public List<List<int>> Horizontals
        {
            get { return _horizontals; }
            set { _horizontals = value; }
        }

        private List<List<int>> _verticals = null!;
        public List<List<int>> Verticals
        {
            get { return _verticals; }
            set { _verticals = value; }
        }

        private List<List<int>> _diagonals = null!;
        public List<List<int>> Diagonals
        {
            get { return _diagonals; }
            set { _diagonals = value; }
        }

        private int _rowsLength;
        public int BaseRowsCount
        {
            get { return _rowsLength; }
            set { _rowsLength = value; }
        }

        private int _columnLength;
        public int BaseColumnCount
        {
            get { return _columnLength; }
            set { _columnLength = value; }
        }

        public PuzzleTriddler()
        {
        }
    }
}
