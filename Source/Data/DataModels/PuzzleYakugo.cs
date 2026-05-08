using System.Collections.Generic;

namespace Data.DataModels
{
    public class PuzzleYakugo : PuzzleBase
    {
        public int Rows { get; set; }
        public int Cols { get; set; }
        public string SourceLanguage { get; set; } = "en";
        public string TargetLanguage { get; set; } = "he";
        public List<PuzzleCellYG> Cells { get; set; } = new();
    }

    public class PuzzleCellYG
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public string Kind { get; set; } = "Letter";   // "Letter" | "Clue"
        public string? Initial { get; set; }            // pre-filled letter (Letter cells only)
        public List<PuzzleClueYG>? Clues { get; set; } // 1..2 entries (Clue cells only)
    }

    public class PuzzleClueYG
    {
        public string Source { get; set; } = "";
        public string Target { get; set; } = "";
        public string Dir { get; set; } = "Right";     // Right | Down | Left | Up
        public List<int>? Pattern { get; set; }        // e.g. [5,4] for two-word targets
    }
}
