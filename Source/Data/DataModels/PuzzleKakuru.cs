using System;
using System.Collections.Generic;
using System.Text;
using Data.DataModels;

namespace Data.DataModels
{
    [System.Xml.Serialization.XmlType("KakuruPuzzle")]
    public class PuzzleKakuru : PuzzleBase
    {
        private int _rows;

        public int Rows
        {
            get { return _rows; }
            set { _rows = value; }
        }

        private int _columns;

        public int Columns
        {
            get { return _columns; }
            set { _columns = value; }
        }

        private List<int> _numberRange;

        public List<int> NumberRange
        {
            get { return _numberRange; }
            set { _numberRange = value; }
        }


        private List<DefinedGroupKakuru> _sumLines;

        public List<DefinedGroupKakuru> SumLines
        {
            get { return _sumLines; }
            set { _sumLines = value; }
        }

        private List<FixedCellKakuru> _fixedCell;

        public List<FixedCellKakuru> FixedCells
        {
            get { return _fixedCell; }
            set { _fixedCell = value; }
        }

        public PuzzleKakuru()
        {
            this.SumLines = new List<DefinedGroupKakuru>();
            this.FixedCells = new List<FixedCellKakuru>();
        }

        public PuzzleKakuru(List<DefinedGroupKakuru> sumLines)
        {
            this.SumLines = sumLines;
        }
    }

    [System.Xml.Serialization.XmlType("KakuruSumLine")]
    public class DefinedGroupKakuru
    {
        private int _rowI;

        public int RowI
        {
            get { return _rowI; }
            set { _rowI = value; }
        }

        private int _columnI;

        public int ColumnI
        {
            get { return _columnI; }
            set { _columnI = value; }
        }

        private bool _horizontalVertical;

        public bool HorizontalVertical
        {
            get { return _horizontalVertical; }
            set { _horizontalVertical = value; }
        }

        private int _sum;

        public int Sum
        {
            get { return _sum; }
            set { _sum = value; }
        }

        private int _size;

        public int Size
        {
            get { return _size; }
            set { _size = value; }
        }


        public override string ToString()
        {
            return string.Format("RowI={0};ColumnI={1};HV={2};Sum={3};Size={4}", RowI, ColumnI, HorizontalVertical ? "V" : "H", Sum, Size);
        }
    }


    [System.Xml.Serialization.XmlType("KakuruFixedCell")]
    public class FixedCellKakuru
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

        private int _value;

        public int Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public FixedCellKakuru()
        {

        }

        public FixedCellKakuru(int row, int column, int value)
        {
            this.Row = row;
            this.Column = column;
            this.Value = value;
        }
    }
}
