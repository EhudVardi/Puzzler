using Common.Models.Yakugo;
using Data;
using Data.DataModels;

namespace Logic
{
    public class LogicLayerYakugo : LogicLayerGeneric<PuzzleYakugo, BoardYakugo>
    {
        public LogicLayerYakugo()
        {
            this.DataProxy     = new DataLayerYakugo();
            this.FactoryModule = new FactoryYakugo();
            this.SolverModule  = new SolverYakugo();
            AttachSolverEvents();
        }
    }
}
