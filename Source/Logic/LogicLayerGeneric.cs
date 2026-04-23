using System;
using System.Collections.Generic;
using Data;

namespace Logic
{
    public class LogicLayerGeneric<TPuzzle, TBoard>
    {
        public DataLayerGeneric<TPuzzle>       DataProxy     { get; set; }
        public FactoryGeneric<TPuzzle, TBoard> FactoryModule { get; set; }
        public SolverGeneric<TBoard>           SolverModule  { get; set; }
        public TrackerGeneric<TBoard>          TrackerModule { get; set; }

        protected void AttachSolverEvents()
        {
            this.SolverModule.StepCompleted   += new EventHandler(SolverProxy_StepCompleted);
            this.SolverModule.SolveCompleted  += new EventHandler(SolverProxy_SolveCompleted);
            this.FactoryModule.StepGenerated  += new EventHandler(FactoryModule_StepGenerated);
        }

        public event EventHandler StepCompleted;
        public event EventHandler SolveCompleted;
        public event EventHandler LoadCompleted;
        public event EventHandler StepGenerated;

        protected virtual void OnStepCompleted(EventArgs e)
        {
            if (StepCompleted != null) StepCompleted(this, e);
        }
        protected virtual void OnSolveCompleted(EventArgs e)
        {
            if (SolveCompleted != null) SolveCompleted(this, e);
        }
        protected virtual void OnLoadCompleted(EventArgs e)
        {
            if (LoadCompleted != null) LoadCompleted(this, e);
        }
        protected virtual void OnStepGenerated(EventArgs e)
        {
            if (StepGenerated != null) StepGenerated(this, e);
        }

        public Dictionary<string, List<string>> ReadFileList()
        {
            return this.DataProxy.GetFileList();
        }
        public virtual bool ReadFromFile(string fileName)
        {
            return LoadFromPuzzleObject(this.DataProxy.XMLToPuzzle(fileName));
        }
        public virtual bool ReadFromWeb(string url)
        {
            TPuzzle puzzleFromWeb = this.DataProxy.WebToPuzzleObject(url);
            if (LoadFromPuzzleObject(puzzleFromWeb))
            {
                this.DataProxy.WritePuzzle(puzzleFromWeb, DataProxy.Options.FromWebFolder);
                return true;
            }
            return false;
        }
        public virtual bool ReadFromText(string text)
        {
            TPuzzle puzzleFromText = this.DataProxy.TextToPuzzleObject(text);
            if (LoadFromPuzzleObject(puzzleFromText))
            {
                this.DataProxy.WritePuzzle(puzzleFromText, DataProxy.Options.FromTextFolder);
                return true;
            }
            return false;
        }

        public virtual bool GenerateRandom()
        {
            TPuzzle puzzleFromGenerator = this.FactoryModule.BoardToPuzzle(this.FactoryModule.GenerateRandom());
            if (LoadFromPuzzleObject(puzzleFromGenerator))
            {
                this.DataProxy.WritePuzzle(puzzleFromGenerator, DataProxy.Options.FromGeneratorFolder);
                return true;
            }
            return false;
        }

        public virtual bool LoadFromPuzzleObject(TPuzzle puzzle)
        {
            if (puzzle == null)
                return false;

            TBoard board = this.FactoryModule.PuzzleToBoard(puzzle);

            this.TrackerModule = new TrackerGeneric<TBoard>(this.FactoryModule.PuzzleToBoard(puzzle));

            this.SolverModule.Initialize();
            this.SolverModule.Board = board;
            this.SolverModule.Solve();

            this.OnLoadCompleted(EventArgs.Empty);

            return true;
        }

        public string GetPuzzleTypeDocumentsPath() { return DataProxy.GetPuzzleTypeDocumentsPath(); }
        public string GetPuzzleName()               { return DataProxy.GetPuzzleName(); }

        void SolverProxy_SolveCompleted(object sender, EventArgs e) { OnSolveCompleted(e); }
        void SolverProxy_StepCompleted(object sender, EventArgs e)  { OnStepCompleted(e); }
        void FactoryModule_StepGenerated(object sender, EventArgs e){ OnStepGenerated(e); }

        public bool? RequestSolveStatus()
        {
            if (this.SolverModule.Board != null) return this.SolverModule.IsSolved();
            return null;
        }
        public bool? RequestValidStatus()
        {
            if (this.SolverModule.Board != null) return this.SolverModule.IsValid();
            return null;
        }

        public TBoard getTrackedBoard()
        {
            if (this.SolverModule != null) return this.SolverModule.Board;
            return default(TBoard);
        }
        public TBoard getSolvedBoard()
        {
            if (this.TrackerModule != null) return this.TrackerModule.Board;
            return default(TBoard);
        }
    }
}
