using System;
using System.Collections.Generic;
using System.Text;
using Data.DataModels;

namespace Data.DataModels
{
    [System.Xml.Serialization.XmlType("GriddlerPuzzle")]
    public class PuzzleGriddler : PuzzleBase
    {
        private List<List<int>> _rows;

        public List<List<int>> Rows
        {
            get { return _rows; }
            set { _rows = value; }
        }

        private List<List<int>> _columns;

        public List<List<int>> Columns
        {
            get { return _columns; }
            set { _columns = value; }
        }

        private int _rowsLength;

        public int RowsLength
        {
            get { return _rowsLength; }
            set { _rowsLength = value; }
        }

        private int _columnLength;

        public int ColumnLength
        {
            get { return _columnLength; }
            set { _columnLength = value; }
        }



        public PuzzleGriddler()
        {

        }

        public PuzzleGriddler(List<List<int>> Rows, List<List<int>> Columns, int RowsLength, int ColumnLength)
        {
            this.Rows = Rows;
            this.Columns = Columns;
            this.RowsLength = RowsLength;
            this.ColumnLength = ColumnLength;
        }

    }
}
