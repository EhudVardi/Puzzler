using System.Collections.Generic;
using Common.Models.Base;

namespace Common.Models.Kurodoko
{
    public class CellValueKurodoko : CellValueBase<bool?, GroupKurodoko>
    {
        private bool _isFixed;
        public override bool IsFixed => _isFixed;

        public int? ClueNumber { get; set; }

        public CellValueKurodoko(int row, int column) : base(row, column)
        {
            this.Groups = new List<GroupKurodoko>();
        }

        public void SetFixed(bool isFixed) => _isFixed = isFixed;

        public override string ToString() =>
            $"({Row},{Column}) clue={ClueNumber?.ToString() ?? "-"} value={Value?.ToString() ?? "?"}";
    }
}
