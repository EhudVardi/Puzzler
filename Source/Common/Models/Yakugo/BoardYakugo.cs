using System.Collections.Generic;
using Common.Models.Base;

namespace Common.Models.Yakugo
{
    public class BoardYakugo
        : BoardGeneric<GroupYakugo, CellValueYakugo, CellGroupHolderYakugo>
    {
        public string SourceLanguage { get; set; } = "en";
        public string TargetLanguage { get; set; } = "he";

        public int FilledCount
        {
            get
            {
                int count = 0;
                foreach (var cell in ValueCells)
                    if (cell.Value.HasValue) count++;
                return count;
            }
        }

        public int TotalCount => ValueCells.Count;
    }
}
