using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Logic.Kakuru;
using Common.Models.Base;
using Common.Models.Kakuru;

namespace Logic
{
    public class SolverKakuru : SolverGeneric<BoardKakuru>
    {

        Dictionary<GroupKakuru, List<List<int>>> _groupsValidVariations = null!;
        Dictionary<(CellValueKakuru, GroupKakuru), int> _cellPositionInGroup = null!;



        public override void SolveInitiation()
        {
            _groupsValidVariations = new Dictionary<GroupKakuru, List<List<int>>>();
            _cellPositionInGroup = new Dictionary<(CellValueKakuru, GroupKakuru), int>();

            foreach (GroupKakuru group in this.Board.Groups)
            {
                _groupsValidVariations.Add(group, CalculateAllSumValidVariations(group));
                for (int i = 0; i < group.Cells.Count; i++)
                    _cellPositionInGroup[(group.Cells[i], group)] = i;
            }
        }

        public override bool DoCompleteStep()
        {
            bool anyChanges = false;

            //for each fill cell, cross check each group variation for at least one matching variation in every other group.
            foreach (CellValueKakuru fillCell in this.Board.ValueCells)
            {
                var groups = fillCell.Groups;
                for (int a = 0; a < groups.Count; a++)
                {
                    GroupKakuru firstGroup = groups[a];
                    int posInFirst = _cellPositionInGroup[(fillCell, firstGroup)];

                    for (int b = a + 1; b < groups.Count; b++)
                    {
                        GroupKakuru secondGroup = groups[b];
                        int posInSecond = _cellPositionInGroup[(fillCell, secondGroup)];

                        anyChanges |= FilterVariationsByPosition(firstGroup, posInFirst, secondGroup, posInSecond);
                        anyChanges |= FilterVariationsByPosition(secondGroup, posInSecond, firstGroup, posInFirst);
                    }
                }
            }

            //for each group check if there's only one valid variation. if so, then fix all cells according to that variation
            foreach (GroupKakuru group in this.Board.Groups)
                anyChanges |= FixAllCellsByOneValidVariation(group);

            return anyChanges;
        }

        private bool FilterVariationsByPosition(GroupKakuru targetGroup, int targetPos,
                                                 GroupKakuru referenceGroup, int referencePos)
        {
            List<List<int>> targetVariations = _groupsValidVariations[targetGroup];
            List<List<int>> referenceVariations = _groupsValidVariations[referenceGroup];

            List<List<int>> survivors = new List<List<int>>(targetVariations.Count);
            bool removed = false;

            foreach (List<int> targetVariation in targetVariations)
            {
                int targetValue = targetVariation[targetPos];
                bool anyMatch = false;
                foreach (List<int> referenceVariation in referenceVariations)
                {
                    if (referenceVariation[referencePos] == targetValue)
                    {
                        anyMatch = true;
                        break;
                    }
                }
                if (anyMatch)
                    survivors.Add(targetVariation);
                else
                    removed = true;
            }

            _groupsValidVariations[targetGroup] = survivors;
            return removed;
        }



        public override bool IsSolved()
        {
            bool isSolved = true;
            foreach (CellValueKakuru fillCell in this.Board.ValueCells)
                if (!fillCell.IsFixed)
                {
                    isSolved = false;
                    break;
                }

            return isSolved;
        }

        public override bool IsValid()
        {
            foreach (GroupKakuru group in this.Board.Groups)
                if (_groupsValidVariations.ContainsKey(group))
                {
                    if (_groupsValidVariations[group].Count < 1)
                        return false;
                }
                else
                {
                    return false;
                }

            return true;
        }



        public override void Reset() { }






        ///

        public List<List<int>> CalculateAllSumValidVariations(GroupKakuru group)
        {
            List<List<int>> results = new List<List<int>>();
            List<int> numberRange = this.Board.NumberRange; // sorted ascending
            bool[] used = new bool[numberRange.Count];
            CollectVariations(group, numberRange, used, new List<int>(group.Size), 0, 0, group.Sum, results);
            return results;
        }

        private void CollectVariations(GroupKakuru group, List<int> numberRange, bool[] used,
                                       List<int> current, int position, int runningSum,
                                       int targetSum, List<List<int>> results)
        {
            int remaining = group.Size - position;

            if (remaining == 0)
            {
                if (runningSum == targetSum)
                    results.Add(new List<int>(current));
                return;
            }

            int needed = targetSum - runningSum;

            for (int di = 0; di < numberRange.Count; di++)
            {
                if (used[di]) continue;
                int digit = numberRange[di];

                // numberRange is sorted ascending: once digit exceeds what's needed, all later ones will too
                if (digit > needed)
                    break;

                if (group.Cells[position].IsFixed && group.Cells[position].Value != digit)
                    continue;

                used[di] = true;
                current.Add(digit);
                CollectVariations(group, numberRange, used, current, position + 1,
                                  runningSum + digit, targetSum, results);
                current.RemoveAt(current.Count - 1);
                used[di] = false;
            }
        }



        public bool FixAllCellsByOneValidVariation(GroupKakuru group)
        {
            bool anyChanges = false;
            if (_groupsValidVariations[group].Count == 1)
            {
                for (int i = 0; i < group.Cells.Count; i++)
                {
                    if (group.Cells[i].Value == null)
                        anyChanges = true;
                    group.Cells[i].Value = _groupsValidVariations[group][0][i];
                }
            }
            return anyChanges;
        }




        ///
    }
}
