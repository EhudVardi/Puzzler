using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Data
{
    public class SerializeDeserializeObject
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        public void SerializePuzzleJson(string path, object puzzle, Type type)
        {
            using var stream = File.Create(path);
            JsonSerializer.Serialize(stream, puzzle, type, JsonOptions);
        }

        public object? DeserializePuzzleJson(string path, Type type)
        {
            using var stream = File.OpenRead(path);
            return JsonSerializer.Deserialize(stream, type, JsonOptions);
        }
    }
}
