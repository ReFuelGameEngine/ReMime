using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;

namespace ReMime.Platform
{
    /// <summary>
    /// Media type resolver for *nix systems that have a "/etc/mime.types" file.
    /// </summary>
    public class UnixMediaTypeResolver : IMediaTypeResolver
    {
        private readonly Dictionary<string, MediaType> _extensionsMap = new Dictionary<string, MediaType>();
        public IReadOnlyCollection<MediaType> MediaTypes { get; }

        public UnixMediaTypeResolver()
        {
            {
                bool valid = OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || OperatingSystem.IsFreeBSD();
                if (!valid)
                    throw new PlatformNotSupportedException("This media type resolver is only for *nix systems.");
            }

            List<MediaType> mediaTypes = new List<MediaType>();

            {
                using Stream str = File.OpenRead("/etc/mime.types");
                StreamReader reader = new StreamReader(str);
                DigestMimeDatabase(reader, mediaTypes);
            }

            string localPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".mime.types");
            if (File.Exists(localPath))
            {
                using Stream str = File.OpenRead(localPath);
                StreamReader reader = new StreamReader(str);
                DigestMimeDatabase(reader, mediaTypes);
            }

            foreach (MediaType type in mediaTypes)
            {
                foreach (string extension in type.Extensions)
                {
                    _extensionsMap[extension] = type;
                }
            }

            MediaTypes = mediaTypes.ToImmutableList();
        }

        public bool TryResolve(string extension, out MediaType? mediaType)
        {
            return _extensionsMap.TryGetValue(extension, out mediaType);
        }

        private static readonly char[] s_delimeters = new char[] { '\t', ' ' };

        private static void DigestMimeDatabase(TextReader reader, List<MediaType> types)
        {
            string? line;
            string type = string.Empty;
            List<string> extensions = new List<string>();

            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                line = line.Trim();

                if (line.StartsWith('#'))
                    continue;


                extensions.Clear();
                string[] parts = line.Split(s_delimeters, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < parts.Length; i++)
                {
                    if (i == 0)
                    {
                        type = parts[0];
                    }
                    else
                    {
                        extensions.Add(parts[i]);
                    }
                }
                types.Add(new MediaType(type!, extensions));
            }
        }
    }
}