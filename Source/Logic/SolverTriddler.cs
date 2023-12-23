using System;
using System.Collections.Generic;
using System.Text;
using Logic.Griddler;
using System.Collections;
using Common.Models.Base;
using Common.Models.Triddler;
using Common.Models.Griddler;

namespace Logic
{
    public class SolverTriddler:SolverGeneric<BoardTriddler>
    {
        public override void SolveInitiation()
        {
            _groupsVariations = new Dictionary<GroupGriddler, List<BitArray>>();

            foreach (GroupGriddler group in this.Board.Groups)
                _groupsVariations.Add(group, SolverGriddler.CalcAllValidVariations(group));
        }
        public override bool DoCompleteStep()
        {
            foreach (GroupGriddler group in this.Board.Groups)
                SolverGriddler.ReflectVariationsToCells(_groupsVariations[group], group);

            foreach (GroupGriddler group in this.Board.Groups)
                SolverGriddler.ReflectCellsToVariations(_groupsVariations[group], group);

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
    }
}
