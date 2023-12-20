using System;
using System.Collections.Generic;
using System.Text;
using Common;
using Data;
using Data.DataModels;
using Common.Models.Griddler;
using Logic.Griddler;

namespace Logic
{
    public class LogicLayerGriddler : LogicLayerGeneric<PuzzleGriddler, BoardGriddler>
    {
        public LogicLayerGriddler()
        {
            this.DataProxy = new DataLayerGriddler();

            this.FactoryModule = new FactoryGriddler();
            this.SolverModule = new SolverGriddler();
            AttachSolverEvents();
        }
    }
}
