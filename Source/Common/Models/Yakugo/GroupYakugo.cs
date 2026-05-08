using System.Collections.Generic;
using System.Linq;
using Common.Models.Base;

namespace Common.Models.Yakugo
{
    public class GroupYakugo : GroupBase<CellValueYakugo>
    {
        public string SourceText { get; set; } = "";
        public string TargetText { get; set; } = "";
        public List<int>? LengthPattern { get; set; }
        public int OriginRow { get; set; }
        public int OriginCol { get; set; }
        public YGDirection Direction { get; set; }

        public GroupYakugo()
        {
            this.Cells = new List<CellValueYakugo>();
        }

        public string TargetLetters
        {
            get
            {
                string s = this.TargetText ?? "";
                var sb = new System.Text.StringBuilder(s.Length);
                foreach (char c in s)
                    if (c != ' ' && c != '-')
                        sb.Append(c);
                return sb.ToString();
            }
        }

        public string CurrentLetters
        {
            get
            {
                var sb = new System.Text.StringBuilder(this.Cells.Count);
                foreach (var cell in this.Cells)
                    sb.Append(cell.Value ?? '_');
                return sb.ToString();
            }
        }

        public bool IsSolved
        {
            get
            {
                if (this.Cells.Any(c => c.Value == null))
                    return false;
                return this.CurrentLetters == this.TargetLetters;
            }
        }

        public override bool IsValid()
        {
            string target = this.TargetLetters;
            if (target.Length != this.Cells.Count)
                return false;
            for (int i = 0; i < this.Cells.Count; i++)
            {
                char? v = this.Cells[i].Value;
                if (v.HasValue && v.Value != target[i])
                    return false;
            }
            return true;
        }
    }
}
