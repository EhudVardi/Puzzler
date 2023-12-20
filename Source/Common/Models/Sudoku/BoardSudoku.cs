using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Common.Logic;
using Common.Models.Base;

namespace Common.Models.Sudoku
{
    public class BoardSudoku : BoardGeneric<GroupSudoku, CellValueSudoku, CellValueSudoku>
    {
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



        public override int Size { get { return N * M; } }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < CellsMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < CellsMatrix.GetLength(1); j++)
                {
                    sb.Append(CellsMatrix[i, j].ToString());
                }
                sb.AppendLine();
            }


            return sb.ToString();
        }



        public override void SetCell(int i, int j, int num)
        {
            (this.CellsMatrix[i, j] as CellValueSudoku).Value = num;
        }

        public override CellBase GetCell(int i, int j)
        {
            return this.CellsMatrix[i, j] as CellValueSudoku;
        }


    }
}
