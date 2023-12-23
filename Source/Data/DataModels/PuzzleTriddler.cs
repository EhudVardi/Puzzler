using System;
using System.Collections.Generic;
using System.Text;
using Data.DataModels;

namespace Data.DataModels
{
    [System.Xml.Serialization.XmlType("TriddlerPuzzle")]
    public class PuzzleTriddler : PuzzleBase
    {
        private List<List<int>> _horizontals;
        public List<List<int>> Horizontals
        {
            get { return _horizontals; }
            set { _horizontals = value; }
        }

        private List<List<int>> _verticals;
        public List<List<int>> Verticals
        {
            get { return _verticals; }
            set { _verticals = value; }
        }

        private List<List<int>> _diagonals;
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


        private int _n;
        public int N
        {
            get { return _n; }
            set { _n = value; }
        }

        private int _n2;
        public int N2
        {
            get { return _n2; }
            set { _n2 = value; }
        }

        private int _m;
        public int M
        {
            get { return _m; }
            set { _m = value; }
        }

        private int _m2;
        public int M2
        {
            get { return _m2; }
            set { _m2 = value; }
        }


        public PuzzleTriddler()
        {

        }

        public PuzzleTriddler(List<List<int>> Horizontals, List<List<int>> Verticals, List<List<int>> Diagonals, int RowsLength, int ColumnLength, int n, int n2, int m, int m2)
        {
            this.Horizontals = Horizontals;
            this.Verticals = Verticals;
            this.Diagonals = Diagonals;
            this.BaseRowsCount = RowsLength;
            this.BaseColumnCount = ColumnLength;
            this.N = n;
            this.N2 = n2;
            this.M = m;
            this.M2 = m2;
        }

    }
}
