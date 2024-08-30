using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReMime.ContentResolvers
{
    public class MagicValueDatabaseEntry
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("magic")]
        public List<string> Magic { get; set; } = new List<string>();

        [JsonPropertyName("extensions")]
        public List<string> Extensions { get; set; } = new List<string>();

        public static IEnumerable<MagicValueMediaType> GetEntries(Stream str)
        {
            return JsonSerializer.Deserialize<List<MagicValueDatabaseEntry>>(str, new JsonSerializerOptions()
            {
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            })?.Select(x => (MagicValueMediaType)x)
            ?? throw new Exception();
        }

        public static explicit operator MagicValueMediaType(MagicValueDatabaseEntry entry)
        {
            return new MagicValueMediaType(
                new MediaType(entry.Type, entry.Extensions),
                entry.Magic.Select(x => (MagicValue.TryParse(x, out var value), value))
                           .Where(x => x.Item1)
                           .Select(x => (MagicValue)x.value!)
                           .ToArray()
            );
        }
    }
}