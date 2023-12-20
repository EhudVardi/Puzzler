using System;
using System.Collections.Generic;
using System.Text;
using Common;
using Data;
using Data.DataModels;
using Common.Models.Sudoku;
using Logic.Sudoku;

namespace Logic
{
    public class LogicLayerSudoku : LogicLayerGeneric<SudokuPuzzle, BoardSudoku>
    {
        public LogicLayerSudoku()
        {
            this.DataProxy = new DataLayerSudoku();

            this.FactoryModule = new FactorySudoku();
            this.SolverModule = new SolverSudoku();
            AttachSolverEvents();
        }
    }
}
