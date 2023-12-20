using System;
using System.Collections.Generic;
using System.Text;
using Data.DataModels;

namespace Data.DataModels
{
    [System.Xml.Serialization.XmlType("SudokuPuzzle")]
    public class SudokuPuzzle : PuzzleBase
    {
        private int _size;

        public int Size
        {
            get { return _size; }
            set { _size = value; }
        }

        private int _n;

        public int N
        {
            get { return _n; }
            set { _n = value; }
        }

        private int _m;

        public int M
        {
            get { return _m; }
            set { _m = value; }
        }


        private List<FixedCellSudoku> _fixedNumbers;

        public List<FixedCellSudoku> FixedNumbers
        {
            get { return _fixedNumbers; }
            set { _fixedNumbers = value; }
        }


        public SudokuPuzzle()
        {
            this.FixedNumbers = new List<FixedCellSudoku>();
        }

        public SudokuPuzzle(List<FixedCellSudoku> fixedNumbers)
        {
            this.FixedNumbers = fixedNumbers;
        }
    }

    [System.Xml.Serialization.XmlType("SudokuCellFixedNumber")]
    public class FixedCellSudoku
    {
        private int _row;

        public int Row
        {
            get { return _row; }
            set { _row = value; }
        }

        private int _column;

        public int Column
        {
            get { return _column; }
            set { _column = value; }
        }

        private int _number;

        public int Number
        {
            get { return _number; }
            set { _number = value; }
        }


        public FixedCellSudoku()
        {

        }

        public FixedCellSudoku(int row, int column, int num)
        {
            this.Row = row;
            this.Column = column;
            this.Number = num;
        }
    }

}
