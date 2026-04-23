using System;
using System.IO;

namespace Common
{
    public class Configuration
    {
        private static string DocumentsPath = @".\..\..\..\..\Documents\";

        public static string PuzzlesLibraryFolder = @"Puzzles\";

        public static string FromWebFolder = @"FromWeb\";
        public static string FromTextFolder = @"FromText\";
        public static string FromGeneratorFolder = @"FromGenerator\";

        public static string GetDocumentPath()
        {
            return Path.Combine(AppContext.BaseDirectory, DocumentsPath);
        }
    }
}
