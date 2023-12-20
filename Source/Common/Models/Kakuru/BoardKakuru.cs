using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Common.Logic;
using System.ComponentModel;
using Common.Models.Base;

namespace Common.Models.Kakuru
{
    public class BoardKakuru : BoardGeneric<GroupKakuru, CellValueKakuru, CellGroupHolderKakuru>
    {

        private List<int> _numberList;
        public List<int> NumberRange
        {
            get { return _numberList; }
            set { _numberList = value; }
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < this.CellsMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < this.CellsMatrix.GetLength(1); j++)
                {
                    if (this.CellsMatrix[i, j] == null)
                        sb.Append("-");
                    else if (this.CellsMatrix[i, j].GetType() == typeof(CellGroupHolderKakuru))
                        sb.Append("X");
                    else if (this.CellsMatrix[i, j].GetType() == typeof(CellValueKakuru))
                        sb.Append((this.CellsMatrix[i, j] as CellValueKakuru).Value == null ? 0 : (this.CellsMatrix[i, j] as CellValueKakuru).Value);
                    else
                        sb.Append("N");
                }
                sb.Append("\n");
            }

            return sb.ToString();
        }


    }
}
