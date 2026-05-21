using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Tools.PuzzleLibraryMigrate
{
    internal static class Program
    {
        private static readonly string[] PuzzleTypes =
            { "Sudoku", "Kakuru", "Griddler", "Triddler", "Yakugo", "Kurodoko" };
        private static readonly string[] SourceFolders =
            { "FromGenerator", "FromText", "FromWeb" };

        private static readonly JsonSerializerOptions WriteOptions = new()
        {
            WriteIndented = true,
        };

        private static int Main(string[] args)
        {
            bool dryRun = Array.Exists(args, a => a == "--dry-run");

            string? docsRoot = ResolveDocumentsRoot(args);
            if (docsRoot == null || !Directory.Exists(docsRoot))
            {
                Console.Error.WriteLine($"Documents root not found: {docsRoot}");
                Console.Error.WriteLine("Pass --docs <path> or run from the repo root.");
                return 1;
            }

            Console.WriteLine($"Documents root: {docsRoot}");
            Console.WriteLine(dryRun ? "Mode: DRY RUN (no changes will be written)" : "Mode: LIVE");
            Console.WriteLine();

            int totalMoved = 0, totalSkipped = 0, totalErrors = 0;

            foreach (string type in PuzzleTypes)
            {
                string typeRoot = Path.Combine(docsRoot, type, "puzzles");
                if (!Directory.Exists(typeRoot))
                {
                    Console.WriteLine($"[{type}] no puzzles folder, skipping");
                    continue;
                }

                int typeMoved = 0, typeSkipped = 0, typeErrors = 0;

                foreach (string source in SourceFolders)
                {
                    string sourceDir = Path.Combine(typeRoot, source);
                    if (!Directory.Exists(sourceDir))
                        continue;

                    string[] files = Directory.GetFiles(sourceDir, "*.json");
                    Console.WriteLine($"[{type}/{source}] {files.Length} file(s)");

                    foreach (string file in files)
                    {
                        try
                        {
                            string result = MigrateFile(file, typeRoot, type, source, dryRun);
                            if (result == "moved")      typeMoved++;
                            else if (result == "skipped") typeSkipped++;
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine($"  ERROR {Path.GetFileName(file)}: {ex.Message}");
                            typeErrors++;
                        }
                    }

                    if (!dryRun)
                    {
                        try
                        {
                            if (Directory.Exists(sourceDir) && Directory.GetFiles(sourceDir).Length == 0)
                            {
                                Directory.Delete(sourceDir, recursive: false);
                                Console.WriteLine($"  removed empty folder {source}/");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine($"  could not remove {sourceDir}: {ex.Message}");
                        }
                    }
                }

                Console.WriteLine($"[{type}] moved={typeMoved} skipped={typeSkipped} errors={typeErrors}");
                Console.WriteLine();

                totalMoved   += typeMoved;
                totalSkipped += typeSkipped;
                totalErrors  += typeErrors;
            }

            Console.WriteLine($"DONE  moved={totalMoved} skipped={totalSkipped} errors={totalErrors}");
            return totalErrors == 0 ? 0 : 2;
        }

        private static string MigrateFile(string file, string typeRoot, string type, string source, bool dryRun)
        {
            string raw = File.ReadAllText(file);
            JsonNode? root;
            try
            {
                root = JsonNode.Parse(raw);
            }
            catch (JsonException jex)
            {
                throw new InvalidDataException("malformed JSON: " + jex.Message);
            }

            if (root is not JsonObject obj)
                throw new InvalidDataException("root is not a JSON object");

            string originalName = Path.GetFileNameWithoutExtension(file);

            if (obj.ContainsKey("Type"))
            {
                // Already migrated: just ensure it's at the flat root + GUID location.
                string flat = Path.Combine(typeRoot, originalName + ".json");
                if (string.Equals(Path.GetFullPath(file), Path.GetFullPath(flat), StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"  SKIP   {originalName}.json (already migrated and at flat root)");
                    return "skipped";
                }
            }

            // Build a new JsonObject with metadata first, then original keys (minus any already-present metadata fields).
            JsonObject @new = new();
            @new["Type"]        = type;
            @new["Source"]      = source;
            @new["Name"]        = originalName;
            @new["DateCreated"] = File.GetLastWriteTime(file).ToString("o");

            foreach (var kvp in obj)
            {
                if (kvp.Key == "Type" || kvp.Key == "Source" || kvp.Key == "Name" || kvp.Key == "DateCreated")
                    continue;
                // Detach value from old parent before adding to new object.
                JsonNode? clone = kvp.Value?.DeepClone();
                @new[kvp.Key] = clone;
            }

            string newId   = Guid.NewGuid().ToString();
            string newPath = Path.Combine(typeRoot, newId + ".json");

            Console.WriteLine($"  MOVE   {source}/{originalName}.json -> {newId}.json");

            if (!dryRun)
            {
                File.WriteAllText(newPath, @new.ToJsonString(WriteOptions));
                File.Delete(file);
            }

            return "moved";
        }

        private static string? ResolveDocumentsRoot(string[] args)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "--docs")
                    return Path.GetFullPath(args[i + 1]);
            }

            // Climb up from CWD looking for a "Documents" folder that contains a known puzzle type.
            string? cur = Directory.GetCurrentDirectory();
            for (int i = 0; i < 8 && cur != null; i++)
            {
                string candidate = Path.Combine(cur, "Documents");
                if (Directory.Exists(candidate) && Directory.Exists(Path.Combine(candidate, "Sudoku")))
                    return candidate;
                cur = Path.GetDirectoryName(cur);
            }
            return null;
        }
    }
}
