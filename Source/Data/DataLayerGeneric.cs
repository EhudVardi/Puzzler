using System;
using System.Collections.Generic;
using System.Text;
using Data.DataModels;
using System.IO;
using Common;

namespace Data
{
    public class DataLayerGeneric<P>
    {
        protected string PuzzleName;

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
            string[] keys = new string[] { "FromGenerator", "FromText", "FromWeb" };
            for (int i = 0; i < keys.Length; i++)
            {
                string puzzleType = keys[i];
                string folder = GetPuzzleTypeDocumentsPath() + Configuration.PuzzlesLibraryFolder + puzzleType;
                dic.Add(puzzleType, new List<string>(Directory.GetFiles(folder)));
            }
            return dic;
        }

        public string GetPuzzleTypeDocumentsPath()
        {
            return Configuration.GetDocumentPath() + GetPuzzleName() + "\\";
        }

        public string GetPuzzleName() { return this.PuzzleName; }

        public virtual void WritePuzzle(P puzzle, string sourceTypeFolder)
        {
            string filePath = GetPuzzleTypeDocumentsPath() +
                Configuration.PuzzlesLibraryFolder +
                sourceTypeFolder +
                DateTime.Now.ToString("yyyy-MM-dd.hh.mm.ss") +
                ".xml";
            PuzzleToXML(puzzle, filePath);
        }
    }
}
