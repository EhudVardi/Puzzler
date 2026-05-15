using Common.Models.Yakugo;

namespace Logic
{
    public class SolverYakugo : SolverGeneric<BoardYakugo>
    {
        public override bool IsSolved()
        {
            foreach (var cell in Board.ValueCells)
                if (!cell.Value.HasValue) return false;

            foreach (var group in Board.Groups)
                if (!group.IsSolved) return false;

            return true;
        }

        public override bool IsValid()
        {
            foreach (var group in Board.Groups)
                if (!group.IsValid()) return false;
            return true;
        }

        // Fill any letter cell that is uniquely constrained by all its groups.
        public override bool DoCompleteStep()
        {
            bool progress = false;

            foreach (var cell in Board.ValueCells)
            {
                if (cell.Value.HasValue) continue;

                char? forced = null;
                bool conflict = false;

                foreach (var group in cell.Groups)
                {
                    string target = group.TargetLetters;
                    int idx = group.Cells.IndexOf(cell);
                    if (idx < 0 || idx >= target.Length) continue;

                    char needed = target[idx];
                    if (forced == null)
                        forced = needed;
                    else if (forced != needed)
                    {
                        conflict = true;
                        break;
                    }
                }

                if (!conflict && forced.HasValue)
                {
                    cell.Value = forced;
                    progress = true;
                }
            }

            return progress;
        }

        public override void SolveBoard()
        {
            SolveInitiation();
            while (!IsSolved() && DoCompleteStep())
                ReportProgress(0);
        }
    }
}
