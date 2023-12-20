using System;
using System.Collections.Generic;
using System.Text;
using Common;
using Data;
using Data.DataModels;
using Common.Models.Kakuru;
using Logic.Kakuru;

namespace Logic
{
    public class LogicLayerKakuru : LogicLayerGeneric<PuzzleKakuru, BoardKakuru >
    {
        public LogicLayerKakuru()
        {
            this.DataProxy = new DataLayerKakuru();

            this.FactoryModule = new FactoryKakuru();
            this.SolverModule = new SolverKakuru();
            AttachSolverEvents();
        }
    }
}
