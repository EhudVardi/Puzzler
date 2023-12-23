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
        
        public override void SolveInitiation()
        {
            _groupsVariations = new Dictionary<GroupGriddler, List<BitArray>>();

            foreach (GroupGriddler group in this.Board.Groups)
                _groupsVariations.Add(group, CalcAllValidVariations(group));
        }
        public override bool DoCompleteStep()
        {
            foreach (GroupGriddler group in this.Board.Groups)
                ReflectVariationsToCells(_groupsVariations[group], group);

            foreach (GroupGriddler group in this.Board.Groups)
                ReflectCellsToVariations(_groupsVariations[group], group);


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
            //                GroupGriddler firstIntegratedGroup = GetIntegratedGroup(firstGroup);
            //                GroupGriddler secondIntegratedGroup = GetIntegratedGroup(secondGroup);
            //
            //                bool? firstGroupCellValue = firstIntegratedGroup.Cells[firstGroup.Cells.IndexOf(cell)].Value;
            //                bool? secondGroupCellValue = secondIntegratedGroup.Cells[secondGroup.Cells.IndexOf(cell)].Value;
            //
            //                if ((bool)CellValueGriddler.XOR(firstGroupCellValue, secondGroupCellValue))
            //                    return false;
            //            }
            
            return true;
        }
        public override void Reset() { }


        ///
        public Dictionary<GroupGriddler, List<BitArray>> _groupsVariations;
        public static List<BitArray> CalcAllValidVariations(GroupGriddler group)
        {
            List<BitArray> lines = new List<BitArray>();
            BitArray templateLine = new BitArray(group.Size);
            templateLine.SetAll(false);
            CalcAllValidVariationsRecursive(group.Size, group.Numbers, -1, 0, lines, templateLine);
            return lines;
        }
        public static void CalcAllValidVariationsRecursive(int n, List<int> nums, int currentNumI, int start, List<BitArray> lines, BitArray currentLine)
        {
            if (currentNumI < nums.Count - 1)
            {
                //calculate the minimum space that we need in order to insert the remaining nums
                int minCellsCount = 0;
                for (int i = currentNumI + 1; i < nums.Count; i++)
                    minCellsCount += nums[i] + 1;
                minCellsCount -= 2;

                //calculate the gap that is the space that we can place the remaining cells.
                int gap = n - minCellsCount - start;

                //place the remaining cells in all the possible ways
                for (int i = 0; i < gap; i++)
                {
                    //clone the line for the recursive call
                    BitArray aLine = new BitArray(currentLine.Length);
                    for (int k = 0; k < currentLine.Count; k++)
                        aLine.Set(k, currentLine[k]);

                    //paint the current num on the relevant cells
                    for (int j = 0; j < nums[currentNumI + 1]; j++)
                        aLine.Set(start + j + i, true);

                    //recursive call to the next possibility
                    CalcAllValidVariationsRecursive(n, nums, currentNumI + 1, start + nums[currentNumI + 1] + i + 1, lines, aLine);
                }
            }
            else if (currentNumI == nums.Count - 1)
                lines.Add(currentLine);

        }


        public static void ReflectVariationsToCells(List<BitArray> _groupsVariation, GroupGriddler group)
        {
            GroupGriddler MPLine = new GroupGriddler(group.Size);

            bool?[] and = GetIntegratedAndArray(_groupsVariation);
            bool?[] or = GetIntegratedOrArray(_groupsVariation);

            for (int i = 0; i < group.Size; i++)
            {
                if (and[i] == null && or[i] == null)
                    MPLine.Cells[i].Value = null;
                else if (and[i] == true)
                    MPLine.Cells[i].Value = true;
                else if (or[i] == false)
                    MPLine.Cells[i].Value = false;
                //else
                //    throw new Exception();
            }

            for (int i = 0; i < group.Size; i++)
                if (MPLine.Cells[i].Value != null)
                    group.Cells[i].Value = MPLine.Cells[i].Value;
        }

        public static void ReflectCellsToVariations(List<BitArray> _groupsVariation, GroupGriddler group)
        {
            List<BitArray> integratedArray = _groupsVariation;
            for (int i = 0; i < integratedArray.Count; i++)
                for (int j = 0; j < group.Size; j++)
                    if (group.Cells[j].Value != null)
                        if (integratedArray[i].Get(j) != group.Cells[j].Value)
                        {
                            integratedArray.RemoveAt(i);
                            break;
                        }
        }



        public static bool?[] GetIntegratedOrArray(List<BitArray> _multiplexedLines)
        {
            if (_multiplexedLines.Count < 1)
                return null;

            bool?[] ORLineArray = new bool?[_multiplexedLines[0].Count];
            for (int i = 0; i < ORLineArray.Length; i++)
                ORLineArray[i] = false;

           
            for (int i = 0; i < ORLineArray.Length; i++)
                for (int j = 0; j < _multiplexedLines.Count; j++)
                    ORLineArray[i] = CellValueGriddler.OR(ORLineArray[i].Value, _multiplexedLines[j].Get(i));

            for (int i = 0; i < ORLineArray.Length; i++)
                if (ORLineArray[i] == true)
                    ORLineArray[i] = null;

            return ORLineArray;
        }

        public static bool?[] GetIntegratedAndArray(List<BitArray> _multiplexedLines)
        {
            if (_multiplexedLines.Count < 1)
                return null;

            bool?[] ANDLineArray = new bool?[_multiplexedLines[0].Count];
            for (int i = 0; i < ANDLineArray.Length; i++)
                ANDLineArray[i] = true;

           
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
