using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ReMime.ContentResolvers
{
    public class RiffResolver : IMediaContentResolver, IMagicValueResolver
    {
        public readonly List<MediaType> _mediaTypes = new List<MediaType>();
        public readonly Dictionary<string, MediaType> _extensions = new Dictionary<string, MediaType>();
        public readonly Dictionary<int, MediaType> _magicValues = new Dictionary<int, MediaType>();

        public IReadOnlyCollection<MediaType> MediaTypes { get; }

        private RiffResolver()
        {
            MediaTypes = _mediaTypes.AsReadOnly();

            IEnumerable<MagicValueMediaType> entries;
            
            using (Stream str = typeof(MagicContentResolver).Assembly.GetManifestResourceStream("ReMime.ContentResolvers.riff.jsonc")!)
            {
                entries = MagicValueDatabaseEntry.GetEntries(str);
            }

            AddMagicValues(entries);
        }

        public void AddMagicValues(IEnumerable<MagicValueMediaType> entries)
        {
            foreach (var entry in entries)
            {
                AddMagicValue(entry);
            }
        }

        public bool TryResolve(Stream str, [NotNullWhen(true)] out MediaType? mediaType)
        {
            Span<byte> content = stackalloc byte[Unsafe.SizeOf<RiffChunk>()];
            str.Read(content);
            return TryResolve(content, out mediaType);
        }

        public bool TryResolve(ReadOnlySpan<byte> content, [NotNullWhen(true)] out MediaType? mediaType)
        {
            mediaType = null;

            if (content.Length < Unsafe.SizeOf<RiffChunk>())
                return false;

            ref readonly RiffChunk chunk = ref MemoryMarshal.Cast<byte, RiffChunk>(content)[0];

            if (chunk.Riff != RiffChunk.RiffValue)
                return false;

            return _magicValues.TryGetValue(chunk.FirstChunkType, out mediaType);
        }

        public bool TryResolve(string extension, out MediaType? mediaType)
        {
            return _extensions.TryGetValue(extension, out mediaType);
        }

        /// <summary>
        /// Add a RIFF sub-magic value to this resolver.
        /// </summary>
        /// <param name="type"></param>
        public void AddMagicValue(MagicValueMediaType type)
        {
            if (type.MagicValues.Length == 0)
                throw new ArgumentException("Expected at least one media type.");
            
            _mediaTypes.Add(type.MediaType);

            foreach (string extension in type.Extensions)
            {
                _extensions.Add(extension, type.MediaType);
            }

            foreach (MagicValue magic in type.MagicValues)
            {
                if (magic.Value.Length != 4)
                    continue;

                int i = MemoryMarshal.Cast<byte, int>(magic.Value)[0];

                _magicValues.Add(i, type.MediaType);
            }
        }

        public static RiffResolver Instance { get; } = new RiffResolver();

        [StructLayout(LayoutKind.Auto, Size = 12)]
        private struct RiffChunk
        {
            public int Riff;
            public int Size;
            public int FirstChunkType;

            public const int RiffValue = 1179011410;
        }
    }
}