using System;
using System.Collections.Generic;
using System.Text;
using Logic.Sudoku;
using System.Collections;
using Facet.Combinatorics;
using Common.Logic;
using Common.Models.Sudoku;

namespace Logic
{
    public class SolverSudoku : SolverGeneric<BoardSudoku>
    {

        Dictionary<GroupSudoku, List<List<CellValueSudoku>>> _groupsSubGroups;
        Dictionary<CellValueSudoku, BinaryChoicesMap> _cellsChoiceMap;


        public override void SolveInitiation()
        {
            _groupsSubGroups = new Dictionary<GroupSudoku, List<List<CellValueSudoku>>>();
            foreach (GroupSudoku group in Board.Groups)
            {
                List<List<CellValueSudoku>> subGroups = new List<List<CellValueSudoku>>();
                    subGroups.Add(new List<CellValueSudoku>(group.Cells));

                _groupsSubGroups.Add(group, subGroups);
            }

            _cellsChoiceMap = new Dictionary<CellValueSudoku, BinaryChoicesMap>();
            foreach (CellValueSudoku cell in Board.CellsMatrix)
            {
                _cellsChoiceMap.Add(cell, new BinaryChoicesMap(this.Board.Size));
            }
            foreach (CellValueSudoku cell in Board.InitialCells)
            {
                _cellsChoiceMap[cell].SetToNumber((int)cell.Value);
            }
        }

        public override bool DoCompleteStep()
        {
            bool anyChange = false;

            foreach (CellValueSudoku cell in this.Board.CellsMatrix)
            {
                if (_cellsChoiceMap[cell].IsSetToNumber())
                    SetCell(cell.Row, cell.Column, _cellsChoiceMap[cell].GetNumber());
            }

            foreach (GroupSudoku group in this.Board.Groups)
            {
                anyChange |= FragmentSubGroups(group);
                anyChange |= ApplySubGroupsToSharedGroups(group);
            }

            return anyChange;
        }

        public override bool IsSolved()
        {
            foreach (CellValueSudoku cell in this.Board.CellsMatrix)
            {
                if (!cell.IsFixed)
                    return false;
            }
            return true;
        }

        public override bool IsValid()
        {
            foreach (CellValueSudoku cell in this.Board.CellsMatrix)
                if (_cellsChoiceMap[cell].Ones < 1)
                    return false;

            foreach (GroupSudoku group in this.Board.Groups)
                if (!IsGroupValid(group))
                    return false;

            return true;
        }

        public override void Reset()
        {
            foreach (CellValueSudoku cell in this.Board.CellsMatrix)
            {
                _cellsChoiceMap[cell].Reset(true);
                cell.Value = null;
            }

            this.SolveInitiation();
        }

        public override void SetCell(int row, int column, int num)
        {
            this.Board.SetCell(row, column, num);
            this._cellsChoiceMap[this.Board.GetCell(row, column) as CellValueSudoku].SetToNumber(num);
        }

        ///


        public bool FragmentSubGroups(GroupSudoku group)
        {
            bool anyChange = false;

            List<List<CellValueSudoku>> newSubGroups = new List<List<CellValueSudoku>>();

            foreach (List<CellValueSudoku> subGroup in _groupsSubGroups[group])
            {
                List<List<CellValueSudoku>> subGroups = new List<List<CellValueSudoku>>();
                FragmentSubGroupsRec(group, new List<CellValueSudoku>(subGroup), subGroups);

                newSubGroups.AddRange(subGroups);
            }

            if (_groupsSubGroups[group].Count != newSubGroups.Count)
                anyChange = true;

            _groupsSubGroups[group] = newSubGroups;

            return anyChange;
        }

        public void FragmentSubGroupsRec(GroupSudoku group, List<CellValueSudoku> subGroup, List<List<CellValueSudoku>> subGroups)
        {
            int permutationSize = 1;

            int subGroupSize = subGroup.Count;

            while (permutationSize < subGroupSize)
            {
                Combinations<CellValueSudoku> combinations = new Combinations<CellValueSudoku>(subGroup, permutationSize);

                foreach (IList<CellValueSudoku> combin in combinations)
                {
                    //create a generic list of the IList object
                    List<CellValueSudoku> combination = new List<CellValueSudoku>();
                    foreach (CellValueSudoku cell in combin)
                        combination.Add(cell);

                    //create temporary number object that represents the OR accumulative number
                    BinaryChoicesMap OrNumbers = new BinaryChoicesMap(group.Size, false);
                    foreach (CellValueSudoku cell in combination)
                        OrNumbers.OR(_cellsChoiceMap[cell]);

                    //if the positive count of the temporary number object equals to the subgroup size then it can be considered as a subGroup
                    if (OrNumbers.Ones == permutationSize)
                    {
                        //remove the cells that are in the combination from the subGroup
                        foreach (CellValueSudoku cell in combination)
                            subGroup.Remove(cell);

                        //remove the accumulated positives from remaining cells int the subgroup
                        OrNumbers.NOT();
                        foreach (CellValueSudoku cell in subGroup)
                            _cellsChoiceMap[cell].AND(OrNumbers);

                        subGroups.Add(combination);
                        FragmentSubGroupsRec(group, subGroup, subGroups);

                        return;
                    }
                }

                permutationSize++;
            }

            subGroups.Add(subGroup);
        }


        public bool ApplySubGroupsToSharedGroups(GroupSudoku group)
        {

            //  for each subgroup
            //      for each pos in subgroup
            //          List cells with that pos in that subgroup
            //          apply pos to all shared groups of that cell list

            bool anyChange = false;

            foreach (List<CellValueSudoku> subGroup in _groupsSubGroups[group])
            {
                for (int i = 0; i < group.Size; i++)
                {
                    List<CellValueSudoku> cellsWithPos = new List<CellValueSudoku>();
                    foreach (CellValueSudoku cell in subGroup)
                        if (_cellsChoiceMap[cell].GetSingleBit(i) == true)
                            cellsWithPos.Add(cell);

                    if (cellsWithPos.Count > 0)
                    {
                        List<GroupSudoku> uniqueSharedGroup = new List<GroupSudoku>();
                        foreach (CellValueSudoku cell in cellsWithPos)
                            foreach (GroupSudoku cellGroup in cell.Groups)
                                if (!uniqueSharedGroup.Contains(cellGroup)) 
                                    uniqueSharedGroup.Add(cellGroup);

                        for (int j = 0; j < uniqueSharedGroup.Count; j++)
                        {
                            bool shared = true;
                            foreach (CellValueSudoku cell in cellsWithPos)
                                if (!cell.Groups.Contains(uniqueSharedGroup[j]))
                                {
                                    shared = false;
                                    break;
                                }

                            if (!shared)
                            {
                                uniqueSharedGroup.RemoveAt(j);
                                j--;
                            }
                        }


                        foreach (GroupSudoku sharedGroup in uniqueSharedGroup)
                            foreach (CellValueSudoku cell in sharedGroup.Cells)
                                if (!cellsWithPos.Contains(cell))
                                    if (_cellsChoiceMap[cell].GetSingleBit(i))
                                    {
                                        _cellsChoiceMap[cell].SetSingleBit(i, false);
                                        anyChange = true;
                                    }
                    }

                }
            }


            return anyChange;
        }



        public bool IsGroupValid(GroupSudoku group)
        {
            BinaryChoicesMap ORNumbers = new BinaryChoicesMap(group.Size, false);
            foreach (CellValueSudoku cell in group.Cells)
                ORNumbers.OR(_cellsChoiceMap[cell]);

            if (ORNumbers.Zeros > 0)
                return false;
            else
                return true;
        }
        ///

        public BinaryChoicesMap GetChoiceMapForCell(CellValueSudoku cell)
        {
            return _cellsChoiceMap[cell];
        }
    }
}
