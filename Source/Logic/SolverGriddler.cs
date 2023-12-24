using System;
using System.Collections.Generic;
using System.Text;
using Logic.Griddler;
using System.Collections;
using Common.Models.Base;
using Common.Models.Griddler;

namespace Logic
{
    public class SolverGriddler:SolverGeneric<BoardGriddler>
    {
        public class GroupVariations
        {
            public GroupGriddler Group { get; set; }
            public int VariationCount { get; set; }

            public GroupVariations(GroupGriddler group, int variationCount)
            {
                this.Group = group;
                this.VariationCount = variationCount;
            }
        }

        public override void SolveInitiation()
        {
            List<GroupVariations> groupsSortedByvariationCount = new List<GroupVariations>();
            foreach (GroupGriddler group in this.Board.Groups)
                groupsSortedByvariationCount.Add(new GroupVariations(group, CalcValidVariationsCount(group)));

            groupsSortedByvariationCount.Sort(delegate(GroupVariations gv1, GroupVariations gv2)
            {
                return gv1.VariationCount.CompareTo(gv2.VariationCount);
            });

            _groupsVariations = new Dictionary<GroupGriddler, List<BitArray>>();
            foreach (var gv in groupsSortedByvariationCount)
            {
                GroupGriddler group = gv.Group;
                //List<BitArray> variations = CalcAllValidVariations(group);
                List<BitArray> variations = CalcAllValidVariationConsideringExsitingLine(group);
                
                _groupsVariations.Add(group, variations);
                ReflectIntegratedVariationToCells(group, variations);
                ReportProgress((int)(groupsSortedByvariationCount.IndexOf(gv)), null);
            }
        }

        public override bool DoCompleteStep()
        {
            foreach (GroupGriddler group in this.Board.Groups)
                if (_groupsVariations[group].Count > 1 || group.Cells.FindAll(c=>c.Value==null).Count > 0) //if group is not all fixed (solved)
                    ReflectIntegratedVariationToCells(group, _groupsVariations[group]);

            foreach (GroupGriddler group in this.Board.Groups)
                ReflectCellsToVariationsList(group);

            return true;
        }
        public override bool IsSolved()
        {
            foreach (CellValueGriddler valueCell in this.Board.ValueCells)
                if (valueCell.Value == null)
                    return false;

            return true;
        }
        public override bool IsValid()
        {
            return true;

            //foreach (CellValueGriddler cell in this.Board.CellsMatrix)
            //    foreach (GroupGriddler firstGroup in cell.Groups)
            //        foreach (GroupGriddler secondGroup in cell.Groups)
            //            if (!object.ReferenceEquals(firstGroup, secondGroup))
            //            {
            //                GroupGriddler firstIntegratedGroup = SolverGriddler.GetIntegratedGroup(firstGroup);
            //                GroupGriddler secondIntegratedGroup = SolverGriddler.GetIntegratedGroup(secondGroup);

            //                bool? firstGroupCellValue = firstIntegratedGroup.Cells[firstGroup.Cells.IndexOf(cell)].Value;
            //                bool? secondGroupCellValue = secondIntegratedGroup.Cells[secondGroup.Cells.IndexOf(cell)].Value;

            //                if ((bool)CellValueGriddler.XOR(firstGroupCellValue, secondGroupCellValue))
            //                    return false;
            //            }

            //return true;
        }
        public override void Reset() { }

        public Dictionary<GroupGriddler, List<BitArray>> _groupsVariations;



        ///
        private List<BitArray> CalcAllValidVariationConsideringExsitingLine(GroupGriddler group)
        {
            List<BitArray> lines = new List<BitArray>();

            BitArray templateLine = new BitArray(group.Size);
            templateLine.SetAll(false);

            bool?[] existingLine = new bool?[group.Cells.Count];
            for (int i = 0; i < existingLine.Length; i++)
                existingLine[i] = group.Cells[i].Value;

            CalcAllValidVariationsRecursive(group.Size, group.Numbers, -1, 0, lines, templateLine, group.Cells, existingLine);

            return lines;
        }

        private void CalcAllValidVariationsRecursive(int n, List<int> nums, int currentNumI, int start, List<BitArray> lines, BitArray currentLine, List<CellValueGriddler> cells, bool?[] existingLine)
        {
            if (currentNumI < nums.Count - 1)
            {
                //calculate the minimum space that we need in order to insert the remaining nums
                int minCellsCount = 0;
                for (int i = currentNumI + 1; i < nums.Count; i++)
                {
                    minCellsCount += nums[i] + 1;
                }
                minCellsCount -= 2;

                //calculate the gap that is the space that we can place the remaining cells.
                int gap = n - minCellsCount - start;

                //place the remaining cells in all the possible ways
                for (int i = 0; i < gap; i++)
                {
                    //clone the line for the recursive call
                    BitArray aLine = new BitArray(currentLine.Length);
                    for (int k = 0; k < currentLine.Count; k++)
                    {
                        aLine.Set(k, currentLine[k]);
                    }

                    bool breaked = false;
                    //paint the current num on the relevant cells
                    int lastIndex = 0;
                    for (int j = 0; j < nums[currentNumI + 1]; j++)
                    {
                        int index = start + j + i;
                        lastIndex = index;
                        aLine.Set(index, true);

                        if (existingLine[index].HasValue && existingLine[index].Value != aLine[index])
                        { breaked = true; break; }
                    }
                    if (breaked) 
                        continue;

                    //recursive call to the next possibility
                    CalcAllValidVariationsRecursive(n, nums, currentNumI + 1, start + nums[currentNumI + 1] + i + 1, lines, aLine, cells, existingLine);
                }
            }
            else if (currentNumI == nums.Count - 1)
            {
                for (int i = 0; i < currentLine.Count; i++)
                {
                    bool? xor = CellValueGriddler.XOR(currentLine[i], cells[i].Value);
                    if (xor.HasValue && xor.Value == true)
                        return;
                }

                lines.Add(currentLine);
            }

        }


        private List<BitArray> CalcAllValidVariations(GroupGriddler group)
        {
            List<BitArray> lines = new List<BitArray>();

            BitArray templateLine = new BitArray(group.Size);
            templateLine.SetAll(false);

            CalcAllValidVariationsRecursive(group.Size, group.Numbers, -1, 0, lines, templateLine, group.Cells);

            return lines;
        }

        private void CalcAllValidVariationsRecursive(int n, List<int> nums, int currentNumI, int start, List<BitArray> lines, BitArray currentLine, List<CellValueGriddler> cells)
        {
            if (currentNumI < nums.Count - 1)
            {
                //calculate the minimum space that we need in order to insert the remaining nums
                int minCellsCount = 0;
                for (int i = currentNumI + 1; i < nums.Count; i++)
                {
                    minCellsCount += nums[i] + 1;
                }
                minCellsCount -= 2;

                //calculate the gap that is the space that we can place the remaining cells.
                int gap = n - minCellsCount - start;

                //place the remaining cells in all the possible ways
                for (int i = 0; i < gap; i++)
                {
                    //clone the line for the recursive call
                    BitArray aLine = new BitArray(currentLine.Length);
                    for (int k = 0; k < currentLine.Count; k++)
                    {
                        aLine.Set(k, currentLine[k]);
                    }

                    //paint the current num on the relevant cells
                    int lastIndex = 0;
                    for (int j = 0; j < nums[currentNumI + 1]; j++)
                    {
                        int index = start + j + i;
                        lastIndex = index;
                        aLine.Set(index, true);
                    }

                    //recursive call to the next possibility
                    CalcAllValidVariationsRecursive(n, nums, currentNumI + 1, start + nums[currentNumI + 1] + i + 1, lines, aLine, cells);
                }
            }
            else if (currentNumI == nums.Count - 1)
            {
                for (int i = 0; i < currentLine.Count; i++)
                {
                    bool? xor = CellValueGriddler.XOR(currentLine[i], cells[i].Value);
                    if (xor.HasValue && xor.Value == true)
                        return;
                }

                lines.Add(currentLine);
            }

        }

        private int CalcValidVariationsCount(GroupGriddler group)
        {
            int lines = 0;

            CalcAllValidVariationsRecursive(group.Size, group.Numbers, -1, 0, ref lines);

            return lines;
        }

        private void CalcAllValidVariationsRecursive(int n, List<int> nums, int currentNumI, int start, ref int lines)
        {
            if (currentNumI < nums.Count - 1)
            {
                //calculate the minimum space that we need in order to insert the remaining nums
                int minCellsCount = 0;
                for (int i = currentNumI + 1; i < nums.Count; i++)
                {
                    minCellsCount += nums[i] + 1;
                }
                minCellsCount -= 2;

                //calculate the gap that is the space that we can place the remaining cells.
                int gap = n - minCellsCount - start;

                //place the remaining cells in all the possible ways
                for (int i = 0; i < gap; i++)
                {
                    //recursive call to the next possibility
                    CalcAllValidVariationsRecursive(n, nums, currentNumI + 1, start + nums[currentNumI + 1] + i + 1, ref lines);
                }
            }
            else if (currentNumI == nums.Count - 1)
            {
                lines++;
            }

        }



        private void ReflectIntegratedVariationToCells(GroupGriddler group, List<BitArray> _groupsVariations)
        {
            GroupGriddler MPLine = GetIntegratedGroup(group, _groupsVariations);

            for (int i = 0; i < group.Size; i++)
                if (MPLine.Cells[i].Value != null)
                    group.Cells[i].Value = MPLine.Cells[i].Value;
        }

        private void ReflectCellsToVariationsList(GroupGriddler group)
        {
            List<BitArray> integratedArray = _groupsVariations[group];

            for (int i = 0; i < integratedArray.Count; i++)
                for (int j = 0; j < group.Size; j++)
                    if (group.Cells[j].Value != null)
                        if (integratedArray[i].Get(j) != group.Cells[j].Value)
                        {
                            integratedArray.RemoveAt(i);
                            break;
                        }
        }



        private GroupGriddler GetIntegratedGroup(GroupGriddler group, List<BitArray> _groupsVariations)
        {
            GroupGriddler l = new GroupGriddler(group.Size);

            bool?[] and = GetIntegratedAndArray(group, _groupsVariations);
            bool?[] or = GetIntegratedOrArray(group, _groupsVariations);

            for (int i = 0; i < group.Size; i++)
            {
                if (and[i] == null && or[i] == null)
                    l.Cells[i].Value = null;
                else if (and[i] == true)
                    l.Cells[i].Value = true;
                else if (or[i] == false)
                    l.Cells[i].Value = false;
                else
                    throw new Exception();
            }

            return l;

        }


        private bool?[] GetIntegratedOrArray(GroupGriddler group, List<BitArray> _groupsVariations)
        {
            bool?[] ORLineArray = new bool?[group.Size];
            for (int i = 0; i < ORLineArray.Length; i++)
                ORLineArray[i] = false;

            List<BitArray> _multiplexedLines = _groupsVariations;

            for (int i = 0; i < ORLineArray.Length; i++)
                for (int j = 0; j < _multiplexedLines.Count; j++)
                    ORLineArray[i] = CellValueGriddler.OR(ORLineArray[i].Value, _multiplexedLines[j].Get(i));

            for (int i = 0; i < ORLineArray.Length; i++)
                if (ORLineArray[i] == true)
                    ORLineArray[i] = null;

            return ORLineArray;
        }

        private bool?[] GetIntegratedAndArray(GroupGriddler group, List<BitArray> _groupsVariations)
        {
            bool?[] ANDLineArray = new bool?[group.Size];
            for (int i = 0; i < ANDLineArray.Length; i++)
                ANDLineArray[i] = true;

            List<BitArray> _multiplexedLines = _groupsVariations;

            for (int i = 0; i < ANDLineArray.Length; i++)
                for (int j = 0; j < _multiplexedLines.Count; j++)
                    ANDLineArray[i] = CellValueGriddler.AND(ANDLineArray[i].Value, _multiplexedLines[j].Get(i));

            for (int i = 0; i < ANDLineArray.Length; i++)
                if (ANDLineArray[i] == false)
                    ANDLineArray[i] = null;

            return ANDLineArray;

        }


        ///
    }
}
