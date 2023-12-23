using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Common.Models.Base;

namespace Common.Models.Triddler
{
    public class BoardTriddler : BoardGeneric<GroupTriddler, CellValueTriddler, CellGroupHolderTriddler>
    {
        protected CellValueTriddler[,] _cellsMatrixRight;
        public CellValueTriddler[,] CellsMatrixRight
        {
            get { return _cellsMatrixRight; }
            set { _cellsMatrixRight = value; }
        }

        protected CellValueTriddler[,] _cellsMatrixLeft;
        public CellValueTriddler[,] CellsMatrixLeft
        {
            get { return _cellsMatrixLeft; }
            set { _cellsMatrixLeft = value; }
        }


        public override string ToString()
        {
            string str = "";
            for (int i = 0; i < _cellsMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < _cellsMatrix.GetLength(1); j++)
                {
                    str += _cellsMatrix[i, j].ToString();
                }
                str += "\n";
            }
            return str;
        }

        public override List<CellValueTriddler> ValueCells
        {
            get
            {
                List<CellValueTriddler> valueCells = new List<CellValueTriddler>();
                foreach (CellBase cell in this.CellsMatrixLeft)
                    if (cell != null && cell.GetType() == typeof(CellValueTriddler))
                        valueCells.Add(cell as CellValueTriddler);
                foreach (CellBase cell in this.CellsMatrixRight)
                    if (cell != null && cell.GetType() == typeof(CellValueTriddler))
                        valueCells.Add(cell as CellValueTriddler);
                return valueCells;
            }
        }

        public override int Rows { get { return CellsMatrixLeft.GetLength(0); } }

        public override int Columns { get { return CellsMatrixLeft.GetLength(1); } }

    }
}