using System;
using System.Collections.Generic;
using System.IO;
using Common;

namespace Data
{
    public class DataLayerGeneric<P>
    {
        protected string PuzzleName;

        public PuzzlerOptions Options { get; set; } = PuzzlerOptions.CreateDefault();

        public DataLayerGeneric()
        {
            this.PuzzleName = "Base";
        }

        public virtual P TextToPuzzleObject(string text)
        {
            return default(P);
        }
        public virtual P WebToPuzzleObject(string url)
        {
            return default(P);
        }

        public virtual P XMLToPuzzle(string XmlFileName)
        {
            return (P)(new SerializeDeserializeObject().DeserializePuzzle(XmlFileName, typeof(P)));
        }
        public virtual void PuzzleToXML(P puzzle, string XmlFileName)
        {
            if (!Directory.Exists(Path.GetDirectoryName(XmlFileName)))
                Directory.CreateDirectory(Path.GetDirectoryName(XmlFileName));
            new SerializeDeserializeObject().SerializePuzzle(XmlFileName, puzzle, typeof(P));
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

        public virtual void WritePuzzle(P puzzle, string sourceTypeFolder)
        {
            string filePath = GetPuzzleTypeDocumentsPath() +
                Options.PuzzlesLibraryFolder +
                sourceTypeFolder +
                DateTime.Now.ToString("yyyy-MM-dd.hh.mm.ss") +
                ".xml";
            PuzzleToXML(puzzle, filePath);
        }
    }
}
