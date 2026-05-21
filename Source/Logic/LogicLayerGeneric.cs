using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data;
using Data.DataModels;

namespace Logic
{
    public class LogicLayerGeneric<TPuzzle, TBoard> where TPuzzle : PuzzleBase
    {
        public DataLayerGeneric<TPuzzle>       DataProxy     { get; set; } = null!;
        public FactoryGeneric<TPuzzle, TBoard> FactoryModule { get; set; } = null!;
        public SolverGeneric<TBoard>           SolverModule  { get; set; } = null!;
        public TrackerGeneric<TBoard>          TrackerModule { get; set; } = null!;

        protected void AttachSolverEvents()
        {
            this.SolverModule.StepCompleted   += new EventHandler(SolverProxy_StepCompleted);
            this.SolverModule.SolveCompleted  += new EventHandler(SolverProxy_SolveCompleted);
            this.FactoryModule.StepGenerated  += new EventHandler(FactoryModule_StepGenerated);
        }

        public event EventHandler? StepCompleted;
        public event EventHandler? SolveCompleted;
        public event EventHandler? LoadCompleted;
        public event EventHandler? StepGenerated;

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

        public List<string> ReadFileList()
        {
            return this.DataProxy.GetFileList();
        }
        public string GetPuzzleSizeLabel(string filePath) => DataProxy.GetPuzzleSizeLabel(filePath);
        public PuzzleBase GetPuzzleMetadata(string filePath) => DataProxy.ReadMetadata(filePath);
        public virtual async Task<bool> ReadFromFile(string fileName)
        {
            return await LoadFromPuzzleObject(this.DataProxy.LoadPuzzle(fileName));
        }
        public virtual async Task<bool> ReadFromWeb(string url)
        {
            TPuzzle? puzzleFromWeb = this.DataProxy.WebToPuzzleObject(url);
            if (await LoadFromPuzzleObject(puzzleFromWeb))
            {
                this.DataProxy.WritePuzzle(puzzleFromWeb!, DataProxy.Options.FromWebFolder);
                return true;
            }
            return false;
        }
        public virtual async Task<bool> ReadFromText(string text)
        {
            TPuzzle? puzzleFromText = this.DataProxy.TextToPuzzleObject(text);
            if (await LoadFromPuzzleObject(puzzleFromText))
            {
                this.DataProxy.WritePuzzle(puzzleFromText!, DataProxy.Options.FromTextFolder);
                return true;
            }
            return false;
        }

        public virtual async Task<bool> GenerateRandom()
        {
            TBoard? generatedBoard = this.FactoryModule.GenerateRandom();
            TPuzzle? puzzleFromGenerator = generatedBoard != null ? this.FactoryModule.BoardToPuzzle(generatedBoard) : default;
            if (await LoadFromPuzzleObject(puzzleFromGenerator))
            {
                this.DataProxy.WritePuzzle(puzzleFromGenerator!, DataProxy.Options.FromGeneratorFolder);
                return true;
            }
            return false;
        }

        public virtual async Task<bool> LoadFromPuzzleObject(TPuzzle? puzzle)
        {
            if (puzzle == null)
                return false;

            TBoard? board = this.FactoryModule.PuzzleToBoard(puzzle);
            if (board == null)
                return false;

            this.TrackerModule = new TrackerGeneric<TBoard>(this.FactoryModule.PuzzleToBoard(puzzle)!);

            await this.SolverModule.Initialize();
            this.SolverModule.Board = board!;
            this.SolverModule.Solve();

            this.OnLoadCompleted(EventArgs.Empty);

            return true;
        }

        public bool IsSolving => this.SolverModule.IsSolving;

        public string GetPuzzleTypeDocumentsPath() { return DataProxy.GetPuzzleTypeDocumentsPath(); }
        public string GetPuzzleName()               { return DataProxy.GetPuzzleName(); }

        void SolverProxy_SolveCompleted(object? sender, EventArgs e) { OnSolveCompleted(e); }
        void SolverProxy_StepCompleted(object? sender, EventArgs e)  { OnStepCompleted(e); }
        void FactoryModule_StepGenerated(object? sender, EventArgs e){ OnStepGenerated(e); }

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

        public TBoard? getTrackedBoard()
        {
            if (this.SolverModule != null) return this.SolverModule.Board;
            return default(TBoard);
        }
        public TBoard? getSolvedBoard()
        {
            if (this.TrackerModule != null) return this.TrackerModule.Board;
            return default(TBoard);
        }
    }
}
