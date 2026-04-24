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
            List<List<int>> variations = new List<List<int>>();

            Combinatorics.Collections.Variations<int> combs = new Combinatorics.Collections.Variations<int>(this.Board.NumberRange, group.Size, Combinatorics.Collections.GenerateOption.WithoutRepetition);

            foreach (IList<int> comb in combs)
                if (ValidateSumOfNumberList(group, comb, group.Sum))
                    variations.Add(new List<int>(comb));

            return variations;
        }



        private bool ValidateSumOfNumberList(GroupKakuru group, IList<int> comb, int targetSum)
        {
            //validate sum of variation
            int sumTemp = 0;
            foreach (int num in comb)
                sumTemp += num;

            bool isValid = true;

            if (sumTemp == targetSum) // if variation sum matches
            {
                //validate combination according to fixed cells.
                for (int i = 0; i < comb.Count; i++)
                    if (group.Cells[i].IsFixed && group.Cells[i].Value != comb[i])
                    {
                        isValid = false;
                        break;
                    }
            }
            else
            {
                isValid = false;
            }
            return isValid;
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
