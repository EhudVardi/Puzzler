using Common.Models.Kurodoko;
using Data;
using Data.DataModels;
using Logic.Kurodoko;

namespace Logic
{
    public class LogicLayerKurodoko : LogicLayerGeneric<KurodokoPuzzle, BoardKurodoko>
    {
        public LogicLayerKurodoko()
        {
            this.DataProxy     = new DataLayerKurodoko();
            this.FactoryModule = new FactoryKurodoko();
            this.SolverModule  = new SolverKurodoko();
            AttachSolverEvents();
        }
    }
}
