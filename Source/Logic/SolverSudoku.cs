using System;
using System.Collections.Generic;
using System.Text;
using Logic.Sudoku;
using System.Collections;
using Combinatorics.Collections;
using Common.Logic;
using Common.Models.Sudoku;

namespace Logic
{
    public class SolverSudoku : SolverGeneric<BoardSudoku>
    {

        Dictionary<GroupSudoku, List<List<CellValueSudoku>>> _groupsSubGroups = null!;
        Dictionary<CellValueSudoku, BinaryChoicesMap> _cellsChoiceMap = null!;
        Dictionary<int, List<CellValueSudoku>> _cellsByRow = null!;
        Dictionary<int, List<CellValueSudoku>> _cellsByColumn = null!;


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
                _cellsChoiceMap[cell].SetToNumber(cell.Value.GetValueOrDefault());
            }

            _cellsByRow = new Dictionary<int, List<CellValueSudoku>>();
            _cellsByColumn = new Dictionary<int, List<CellValueSudoku>>();
            foreach (CellValueSudoku cell in Board.CellsMatrix)
            {
                if (!_cellsByRow.ContainsKey(cell.Row)) _cellsByRow[cell.Row] = new List<CellValueSudoku>();
                _cellsByRow[cell.Row].Add(cell);
                if (!_cellsByColumn.ContainsKey(cell.Column)) _cellsByColumn[cell.Column] = new List<CellValueSudoku>();
                _cellsByColumn[cell.Column].Add(cell);
            }
        }

        public override bool DoCompleteStep()
        {
            // Level 1: naked singles — propagate newly-determined single-candidate cells
            bool anySet = false;
            foreach (CellValueSudoku cell in Board.CellsMatrix)
                if (!cell.IsFixed && _cellsChoiceMap[cell].IsSetToNumber())
                { 
                    SetCell(cell.Row, cell.Column, _cellsChoiceMap[cell].GetNumber()); 
                        anySet = true;
                }
            if (anySet) 
                return true;

            // Level 2: hidden singles — candidate appears in only one cell in its group
            if (FindHiddenSingles()) 
                return true;

            // Level 3: naked sets; if a new set is found, apply pointing pairs immediately
            foreach (GroupSudoku group in Board.Groups)
                if (FragmentSubGroups(group))
                {
                    ApplySubGroupsToSharedGroups(group);
                    return true;
                }

            // Level 4: hidden sets (pairs / triples)
            foreach (GroupSudoku group in Board.Groups)
                if (FindHiddenSets(group)) 
                    return true;

            // Level 5: X-Wing (fish size 2)
            if (FindFish(2))
                return true;

            // Level 6: Swordfish (fish size 3)
            if (FindFish(3))
                return true;

            return false;
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
            this._cellsChoiceMap[(this.Board.GetCell(row, column) as CellValueSudoku)!].SetToNumber(num);
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

            if (subGroup.Count > 0)
                subGroups.Add(subGroup);
        }


        public bool ApplySubGroupsToSharedGroups(GroupSudoku group)
        {
            bool anyChange = false;

            foreach (List<CellValueSudoku> subGroup in _groupsSubGroups[group])
            {
                for (int i = 0; i < group.Size; i++)
                {
                    HashSet<CellValueSudoku> cellsWithPos = new HashSet<CellValueSudoku>();
                    foreach (CellValueSudoku cell in subGroup)
                        if (_cellsChoiceMap[cell].GetSingleBit(i))
                            cellsWithPos.Add(cell);

                    if (cellsWithPos.Count == 0)
                        continue;

                    // intersect each cell's group list -> only groups shared by every cell with bit i set
                    HashSet<GroupSudoku>? sharedGroups = null;
                    foreach (CellValueSudoku cell in cellsWithPos)
                    {
                        if (sharedGroups == null)
                            sharedGroups = new HashSet<GroupSudoku>(cell.Groups);
                        else
                            sharedGroups.IntersectWith(cell.Groups);
                    }

                    foreach (GroupSudoku sharedGroup in sharedGroups!)
                        foreach (CellValueSudoku cell in sharedGroup.Cells)
                            if (!cellsWithPos.Contains(cell))
                                if (_cellsChoiceMap[cell].GetSingleBit(i))
                                {
                                    _cellsChoiceMap[cell].SetSingleBit(i, false);
                                    anyChange = true;
                                }
                }
            }

            return anyChange;
        }


        // Hidden single: if a candidate d appears in only one unsolved cell in a group,
        // that cell must be d — reduce it to that single candidate.
        internal bool FindHiddenSingles()
        {
            foreach (GroupSudoku group in Board.Groups)
            {
                int size = group.Size;
                for (int d = 0; d < size; d++)
                {
                    // count ALL cells (fixed + unfixed) with candidate d so a fixed cell
                    // already holding d prevents us from incorrectly firing for d again
                    CellValueSudoku? singleCell = null;
                    bool moreThanOne = false;
                    foreach (CellValueSudoku cell in group.Cells)
                    {
                        if (_cellsChoiceMap[cell].GetSingleBit(d))
                        {
                            if (singleCell == null) singleCell = cell;
                            else { moreThanOne = true; break; }
                        }
                    }
                    if (singleCell != null && !moreThanOne
                        && !singleCell.IsFixed && !_cellsChoiceMap[singleCell].IsSetToNumber())
                    {
                        _cellsChoiceMap[singleCell].SetToNumber(d);
                        return true;
                    }
                }
            }
            return false;
        }

        // Hidden pairs/triples: if k candidates appear only in exactly k cells of a group,
        // all other candidates can be removed from those k cells.
        internal bool FindHiddenSets(GroupSudoku group)
        {
            bool anyChange = false;
            int size = group.Size;

            List<CellValueSudoku> unsolvedCells = new List<CellValueSudoku>();
            foreach (CellValueSudoku cell in group.Cells)
                if (!_cellsChoiceMap[cell].IsSetToNumber())
                    unsolvedCells.Add(cell);

            List<int> activeCandidates = new List<int>();
            for (int d = 0; d < size; d++)
                foreach (CellValueSudoku cell in unsolvedCells)
                    if (_cellsChoiceMap[cell].GetSingleBit(d))
                    { activeCandidates.Add(d); break; }

            for (int setSize = 2; setSize <= 3; setSize++)
            {
                if (unsolvedCells.Count <= setSize || activeCandidates.Count < setSize)
                    continue;

                Combinations<int> combos = new Combinations<int>(activeCandidates, setSize);
                foreach (IList<int> candidateSet in combos)
                {
                    List<CellValueSudoku> cellsWithAny = new List<CellValueSudoku>();
                    foreach (CellValueSudoku cell in unsolvedCells)
                        foreach (int d in candidateSet)
                            if (_cellsChoiceMap[cell].GetSingleBit(d))
                            { cellsWithAny.Add(cell); break; }

                    if (cellsWithAny.Count != setSize)
                        continue;

                    foreach (CellValueSudoku cell in cellsWithAny)
                        for (int d = 0; d < size; d++)
                            if (!candidateSet.Contains(d) && _cellsChoiceMap[cell].GetSingleBit(d))
                            {
                                _cellsChoiceMap[cell].SetSingleBit(d, false);
                                anyChange = true;
                            }
                }
            }

            return anyChange;
        }

        // X-Wing (fishSize=2) and Swordfish (fishSize=3): if candidate d appears in exactly
        // fishSize base lines and those lines share exactly fishSize cover positions, eliminate
        // d from all other cells in those cover positions.
        internal bool FindFish(int fishSize)
        {
            bool anyChange = false;
            anyChange |= FindFishInDirection(fishSize, _cellsByRow, _cellsByColumn, c => c.Row, c => c.Column);
            anyChange |= FindFishInDirection(fishSize, _cellsByColumn, _cellsByRow, c => c.Column, c => c.Row);
            return anyChange;
        }

        internal bool FindFishInDirection(
            int fishSize,
            Dictionary<int, List<CellValueSudoku>> baseLines,
            Dictionary<int, List<CellValueSudoku>> coverLines,
            Func<CellValueSudoku, int> getBaseIndex,
            Func<CellValueSudoku, int> getCoverIndex)
        {
            bool anyChange = false;
            int size = Board.Size;

            for (int d = 0; d < size; d++)
            {
                Dictionary<int, List<int>> baseToCoverPositions = new Dictionary<int, List<int>>();
                foreach (var kvp in baseLines)
                {
                    List<int> positions = new List<int>();
                    foreach (CellValueSudoku cell in kvp.Value)
                        if (!_cellsChoiceMap[cell].IsSetToNumber() && _cellsChoiceMap[cell].GetSingleBit(d))
                            positions.Add(getCoverIndex(cell));

                    if (positions.Count >= 2 && positions.Count <= fishSize)
                        baseToCoverPositions[kvp.Key] = positions;
                }

                if (baseToCoverPositions.Count < fishSize)
                    continue;

                List<int> baseLineKeys = new List<int>(baseToCoverPositions.Keys);
                Combinations<int> combos = new Combinations<int>(baseLineKeys, fishSize);

                foreach (IList<int> baseCombo in combos)
                {
                    HashSet<int> coverPositions = new HashSet<int>();
                    foreach (int baseKey in baseCombo)
                        foreach (int pos in baseToCoverPositions[baseKey])
                            coverPositions.Add(pos);

                    if (coverPositions.Count != fishSize)
                        continue;

                    HashSet<int> baseComboSet = new HashSet<int>(baseCombo);
                    foreach (int coverPos in coverPositions)
                        foreach (CellValueSudoku cell in coverLines[coverPos])
                            if (!baseComboSet.Contains(getBaseIndex(cell))
                                && !_cellsChoiceMap[cell].IsSetToNumber()
                                && _cellsChoiceMap[cell].GetSingleBit(d))
                            {
                                _cellsChoiceMap[cell].SetSingleBit(d, false);
                                anyChange = true;
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
