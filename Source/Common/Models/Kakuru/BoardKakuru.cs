using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Common.Logic;
using System.ComponentModel;
using Common.Models.Base;

namespace Common.Models.Kakuru
{
    public class BoardKakuru : BoardGeneric<GroupKakuru, CellValueKakuru, CellGroupHolderKakuru>
    {

        private List<int> _numberList = null!;
        public List<int> NumberRange
        {
            get { return _numberList; }
            set { _numberList = value; }
        }

        private readonly Dictionary<CellValueKakuru, HashSet<int>> _topoCache = new();

        public HashSet<int> GetCellTopologyValues(CellValueKakuru cell)
        {
            if (_topoCache.TryGetValue(cell, out var cached)) return cached;

            HashSet<int>? intersection = null;
            foreach (var group in cell.Groups)
            {
                var groupUnion = new HashSet<int>();
                CollectGroupValueUnion(group.Sum, group.Size, 0, groupUnion, new List<int>());
                intersection = intersection == null
                    ? groupUnion
                    : new HashSet<int>(intersection.Where(groupUnion.Contains));
            }

            var result = intersection ?? new HashSet<int>();
            _topoCache[cell] = result;
            return result;
        }

        private void CollectGroupValueUnion(int remainingSum, int remainingCells, int startIndex, HashSet<int> union, List<int> chosen)
        {
            if (remainingCells == 0)
            {
                if (remainingSum == 0)
                    foreach (int v in chosen) union.Add(v);
                return;
            }
            for (int i = startIndex; i < _numberList.Count; i++)
            {
                int v = _numberList[i];
                if (v > remainingSum) break;
                chosen.Add(v);
                CollectGroupValueUnion(remainingSum - v, remainingCells - 1, i + 1, union, chosen);
                chosen.RemoveAt(chosen.Count - 1);
            }
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
                        sb.Append((this.CellsMatrix[i, j] as CellValueKakuru)!.Value == null ? 0 : (this.CellsMatrix[i, j] as CellValueKakuru)!.Value);
                    else
                        sb.Append("N");
                }
                sb.Append("\n");
            }

            return sb.ToString();
        }


    }
}
