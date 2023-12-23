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

        Dictionary<GroupKakuru, List<List<int>>> _groupsValidVariations;



        public override void SolveInitiation()
        {
            _groupsValidVariations = new Dictionary<GroupKakuru, List<List<int>>>();

            foreach (GroupKakuru group in this.Board.Groups)
                _groupsValidVariations.Add(group, CalculateAllSumValidVariations(group));
        }

        public override bool DoCompleteStep()
        {
            bool anyChanges = false;

            //for each fill cell, cross check each right group variation for at least one matching variation in the down group, and vice versa.
            foreach (CellValueKakuru fillCell in this.Board.ValueCells)
                foreach (GroupKakuru firstGroup in fillCell.Groups)
                    foreach (GroupKakuru secondGroup in fillCell.Groups)
                        if (!object.ReferenceEquals(firstGroup, secondGroup))
                        {
                            int cellPositionInFirstGroup = firstGroup.Cells.IndexOf(fillCell);
                            int cellPositionInSecondGroup = secondGroup.Cells.IndexOf(fillCell);

                            for (int i = 0; i < _groupsValidVariations[firstGroup].Count; i++)
                            {
                                List<int> firstGroupVariation = _groupsValidVariations[firstGroup][i];

                                bool anyMatch = false;

                                for (int j = 0; j < _groupsValidVariations[secondGroup].Count; j++)
                                {
                                    List<int> secondGroupVariation = _groupsValidVariations[secondGroup][j];

                                    if (firstGroupVariation[cellPositionInFirstGroup] == secondGroupVariation[cellPositionInSecondGroup])
                                    {
                                        anyMatch = true;
                                        break;
                                    }
                                }

                                if (anyMatch == false)
                                {
                                    _groupsValidVariations[firstGroup].RemoveAt(i);
                                    i--;
                                    anyChanges = true;
                                }
                            }


                        }


            //for each group check if there's only one valid variation. if so, then fix all cells according to that variation
            foreach (GroupKakuru group in this.Board.Groups)
            {
                anyChanges |= FixAllCellsByOneValidVariation(group);
            }



            return anyChanges;
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

            Facet.Combinatorics.Variations<int> combs = new Facet.Combinatorics.Variations<int>(this.Board.NumberRange, group.Size, Facet.Combinatorics.GenerateOption.WithoutRepetition);

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
                if (bg != null)
                {
                    bg.ReportProgress(0, -1);
                    System.Threading.Thread.Sleep(100);
                }
            }
            return anyChanges;
        }




        ///
    }
}
