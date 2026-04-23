using System;
using System.IO;

namespace Common
{
    public class PuzzlerOptions
    {
        public string DocumentsRelativePath { get; set; } = @".\..\..\..\..\Documents\";
        public string PuzzlesLibraryFolder  { get; set; } = @"Puzzles\";
        public string FromWebFolder         { get; set; } = @"FromWeb\";
        public string FromTextFolder        { get; set; } = @"FromText\";
        public string FromGeneratorFolder   { get; set; } = @"FromGenerator\";

        public string GetDocumentPath()
        {
            return Path.Combine(AppContext.BaseDirectory, DocumentsRelativePath);
        }

        public static PuzzlerOptions CreateDefault() => new PuzzlerOptions();
    }
}
