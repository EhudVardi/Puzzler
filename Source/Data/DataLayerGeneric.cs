using System;
using System.Collections.Generic;
using System.IO;
using Common;

namespace Data
{
    public class DataLayerGeneric<TPuzzle>
    {
        protected string PuzzleName;

        public PuzzlerOptions Options { get; set; } = PuzzlerOptions.CreateDefault();

        public DataLayerGeneric()
        {
            this.PuzzleName = "Base";
        }

        public virtual TPuzzle? TextToPuzzleObject(string text)
        {
            return default(TPuzzle);
        }
        public virtual TPuzzle? WebToPuzzleObject(string url)
        {
            return default(TPuzzle);
        }

        public virtual TPuzzle XMLToPuzzle(string XmlFileName)
        {
            return (TPuzzle)(new SerializeDeserializeObject().DeserializePuzzle(XmlFileName, typeof(TPuzzle))!);
        }
        public virtual void PuzzleToXML(TPuzzle puzzle, string XmlFileName)
        {
            string? dir = Path.GetDirectoryName(XmlFileName);
            if (dir != null && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            new SerializeDeserializeObject().SerializePuzzle(XmlFileName, puzzle!, typeof(TPuzzle));
        }

        public Dictionary<string, List<string>> GetFileList()
        {
            Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();
            string[] keys = new string[] { Options.FromGeneratorFolder, Options.FromTextFolder, Options.FromWebFolder };
            for (int i = 0; i < keys.Length; i++)
            {
                string puzzleType = keys[i];
                string folder = GetPuzzleTypeDocumentsPath() + Options.PuzzlesLibraryFolder + puzzleType;
                Directory.CreateDirectory(folder);
                dic.Add(puzzleType, new List<string>(Directory.GetFiles(folder)));
            }
            return dic;
        }

        public string GetPuzzleTypeDocumentsPath()
        {
            return Options.GetDocumentPath() + GetPuzzleName() + "\\";
        }

        public string GetPuzzleName() { return this.PuzzleName; }

        public virtual void WritePuzzle(TPuzzle puzzle, string sourceTypeFolder)
        {
            string filePath = GetPuzzleTypeDocumentsPath() +
                Options.PuzzlesLibraryFolder +
                sourceTypeFolder +
                DateTime.Now.ToString("yyyy-MM-dd.hh.mm.ss") +
                ".xml";
            PuzzleToXML(puzzle, filePath);
        }

        public virtual string GetPuzzleSizeLabel(string filePath) => string.Empty;
    }
}
