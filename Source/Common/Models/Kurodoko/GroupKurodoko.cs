using System.Collections.Generic;
using Common.Models.Base;

namespace Common.Models.Kurodoko
{
    public class GroupKurodoko : GroupBase<CellValueKurodoko>
    {
        public GroupKurodoko(List<CellValueKurodoko> cells)
        {
            this.Cells = cells;
        }

        public override bool IsValid() => true;
    }
}
