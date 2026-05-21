using System;
using System.Collections.Generic;
using System.IO;
using Common;
using Data.DataModels;

namespace Data
{
    public class DataLayerGeneric<TPuzzle> where TPuzzle : PuzzleBase
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

        public virtual TPuzzle LoadPuzzle(string fileName)
        {
            return (TPuzzle)(new SerializeDeserializeObject().DeserializePuzzleJson(fileName, typeof(TPuzzle))!);
        }
        public virtual void SavePuzzle(TPuzzle puzzle, string fileName)
        {
            string? dir = Path.GetDirectoryName(fileName);
            if (dir != null && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            new SerializeDeserializeObject().SerializePuzzleJson(fileName, puzzle!, typeof(TPuzzle));
        }

        public List<string> GetFileList()
        {
            string folder = GetPuzzleTypeDocumentsPath() + Options.PuzzlesLibraryFolder;
            Directory.CreateDirectory(folder);
            return new List<string>(Directory.GetFiles(folder, "*.json"));
        }

        public virtual PuzzleBase ReadMetadata(string filePath)
        {
            PuzzleBase? md = null;
            try
            {
                md = (PuzzleBase?)new SerializeDeserializeObject().DeserializePuzzleJson(filePath, typeof(PuzzleBase));
            }
            catch { }

            md ??= new PuzzleBase();

            if (string.IsNullOrEmpty(md.Type))
                md.Type = GetPuzzleName();
            if (string.IsNullOrEmpty(md.Name))
                md.Name = Path.GetFileNameWithoutExtension(filePath);
            if (md.DateCreated == default)
                md.DateCreated = File.GetLastWriteTime(filePath);

            return md;
        }

        public string GetPuzzleTypeDocumentsPath()
        {
            return Options.GetDocumentPath() + GetPuzzleName() + "\\";
        }

        public string GetPuzzleName() { return this.PuzzleName; }

        public virtual void WritePuzzle(TPuzzle puzzle, string sourceTypeFolder)
        {
            string id = Guid.NewGuid().ToString();
            puzzle.Type        = GetPuzzleName();
            puzzle.Source      = sourceTypeFolder.Trim('\\', '/');
            puzzle.Name        = id;
            puzzle.DateCreated = DateTime.Now;

            string filePath = GetPuzzleTypeDocumentsPath()
                            + Options.PuzzlesLibraryFolder
                            + id + ".json";
            SavePuzzle(puzzle, filePath);
        }

        public virtual string GetPuzzleSizeLabel(string filePath) => string.Empty;
    }
}
