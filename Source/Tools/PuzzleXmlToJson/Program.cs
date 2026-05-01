using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;
using Data;
using Data.DataModels;

// One-off converter: reads all *.xml puzzle files, writes *.json equivalents,
// round-trip verifies each file, then deletes the original XML.
// Usage:
//   dotnet run --project Source/Tools/PuzzleXmlToJson -- <rootPath> [<extraPath> ...]
// Examples:
//   dotnet run --project Source/Tools/PuzzleXmlToJson -- e:\...\Documents
//   dotnet run --project Source/Tools/PuzzleXmlToJson -- e:\...\Source\Tests\TestData

new Converter().Run(args);

class Converter
{
    // XML root element names come from the original [XmlType] attributes on each model class.
    // These differ from the C# class names for Kakuru, Griddler, and Triddler.
    private readonly Dictionary<string, (Type type, string xmlRoot)> _typeMap = new()
    {
        { "Sudoku",   (typeof(SudokuPuzzle),   "SudokuPuzzle") },
        { "Kakuru",   (typeof(PuzzleKakuru),   "KakuruPuzzle") },
        { "Griddler", (typeof(PuzzleGriddler), "GriddlerPuzzle") },
        { "Triddler", (typeof(PuzzleTriddler), "TriddlerPuzzle") },
    };

    private readonly JsonSerializerOptions _canonical = new()
    {
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
    };

    private readonly SerializeDeserializeObject _sdo = new();

    public void Run(string[] args)
    {
        string[] roots = args.Length > 0 ? args : new[] { AppContext.BaseDirectory };

        int converted = 0, failed = 0;

        foreach (string root in roots)
        {
            if (!Directory.Exists(root))
            {
                Console.Error.WriteLine($"ERROR: directory not found: {root}");
                failed++;
                continue;
            }

            foreach (var (xmlPath, type, xmlRoot) in CollectFiles(root))
            {
                if (ConvertFile(xmlPath, type, xmlRoot))
                    converted++;
                else
                    failed++;
            }
        }

        Console.WriteLine($"\nDone: {converted} converted, {failed} failed.");
        if (failed > 0)
        {
            Console.Error.WriteLine("Some files failed. Original XMLs for failed files are preserved.");
            Environment.Exit(1);
        }
    }

    private IEnumerable<(string xmlPath, Type type, string xmlRoot)> CollectFiles(string root)
    {
        bool isDocumentsRoot = false;
        foreach (string key in _typeMap.Keys)
        {
            if (Directory.Exists(Path.Combine(root, key))) { isDocumentsRoot = true; break; }
        }

        if (isDocumentsRoot)
        {
            foreach (var kv in _typeMap)
            {
                string subtree = Path.Combine(root, kv.Key);
                if (!Directory.Exists(subtree)) continue;
                foreach (string xmlPath in Directory.GetFiles(subtree, "*.xml", SearchOption.AllDirectories))
                    yield return (xmlPath, kv.Value.type, kv.Value.xmlRoot);
            }
        }
        else
        {
            foreach (string xmlPath in Directory.GetFiles(root, "*.xml", SearchOption.AllDirectories))
            {
                string name = Path.GetFileNameWithoutExtension(xmlPath).ToLowerInvariant();
                bool found = false;
                foreach (var kv in _typeMap)
                {
                    if (name.StartsWith(kv.Key.ToLowerInvariant()))
                    {
                        yield return (xmlPath, kv.Value.type, kv.Value.xmlRoot);
                        found = true;
                        break;
                    }
                }
                if (!found)
                    Console.Error.WriteLine($"SKIP (unknown type): {xmlPath}");
            }
        }
    }

    private bool ConvertFile(string xmlPath, Type puzzleType, string xmlRoot)
    {
        try
        {
            var xmlSerializer = new XmlSerializer(puzzleType, new XmlRootAttribute(xmlRoot));
            object? puzzle;
            using (var reader = new StreamReader(xmlPath))
                puzzle = xmlSerializer.Deserialize(reader.BaseStream);

            if (puzzle == null)
            {
                Console.Error.WriteLine($"FAIL (null deserialization): {xmlPath}");
                return false;
            }

            string jsonPath = Path.ChangeExtension(xmlPath, ".json");
            _sdo.SerializePuzzleJson(jsonPath, puzzle, puzzleType);

            object? roundTripped = _sdo.DeserializePuzzleJson(jsonPath, puzzleType);
            string originalJson  = JsonSerializer.Serialize(puzzle,       puzzleType, _canonical);
            string roundTripJson = JsonSerializer.Serialize(roundTripped, puzzleType, _canonical);
            if (originalJson != roundTripJson)
            {
                Console.Error.WriteLine($"FAIL (round-trip mismatch): {xmlPath}");
                File.Delete(jsonPath);
                return false;
            }

            File.Delete(xmlPath);
            Console.WriteLine($"OK: {xmlPath}");
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"FAIL ({ex.GetType().Name}): {xmlPath} — {ex.Message}");
            return false;
        }
    }
}
