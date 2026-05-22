using System;
using System.IO;

namespace Common
{
    public class PuzzlerOptions
    {
        public string DocumentsRelativePath { get; set; } = @"..\..\..\..\..\Documents\";
        public string PuzzlesLibraryFolder  { get; set; } = @"Puzzles\";
        public string ScrapedSource         { get; set; } = "Scraped";
        public string TypedSource           { get; set; } = "Typed";
        public string GeneratedSource       { get; set; } = "Generated";

        public string GetDocumentPath()
        {
            return Path.Combine(AppContext.BaseDirectory, DocumentsRelativePath);
        }

        public static PuzzlerOptions CreateDefault() => new PuzzlerOptions();
    }
}
