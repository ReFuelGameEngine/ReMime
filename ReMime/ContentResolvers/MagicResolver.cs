using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace ReMime.ContentResolvers
{
    public record MagicValueMediaType(MagicValue Magic, MediaType MediaType);

    public class MagicContentResolver : IMediaContentResolver
    {
        private readonly List<MediaType> _mediaTypes = new List<MediaType>();
        private readonly Dictionary<string, MediaType> _extensions = new Dictionary<string, MediaType>();
        private readonly Tree _tree = new Tree();
        private int _maxBytes = 0;

        public MagicContentResolver(IEnumerable<MagicValueMediaType> values) : this()
        {
            AddMagicValues(values);
        }

        public MagicContentResolver()
        {
            Image.ImageMagicValues.AddToMagicResolver(this);
        }

        public IReadOnlyCollection<MediaType> MediaTypes => _mediaTypes.AsReadOnly();

        public void AddMagicValue(MagicValueMediaType value)
        {
            _maxBytes = Math.Max(_maxBytes, value.Magic.Value.Length);
            _mediaTypes.Add(value.MediaType);
            _tree.Add(value);

            foreach (string extension in value.MediaType.Extensions)
            {
                _extensions[extension] = value.MediaType;
            }
        }

        public void AddMagicValues(IEnumerable<MagicValueMediaType> values)
        {
            foreach (MagicValueMediaType value in values)
            {
                AddMagicValue(value);
            }
        }

        public bool TryResolve(Stream str, [NotNullWhen(true)] out MediaType? mediaType)
        {
            Span<byte> bytes = stackalloc byte[_maxBytes];
            str.Read(bytes);
            return TryResolve(bytes, out mediaType);
        }

        public bool TryResolve(ReadOnlySpan<byte> content, [NotNullWhen(true)] out MediaType? mediaType)
        {
            MagicValueMediaType? type = _tree[content];

            if (type == null)
            {
                mediaType = null;
                return false;
            }
            else
            {
                mediaType = type.MediaType;
                return true;
            }
        }

        public bool TryResolve(string extension, out MediaType? mediaType)
        {
            return _extensions.TryGetValue(extension, out mediaType);            
        }

        private class Tree
        {
            public MagicValueMediaType? Node { get; private set; }
            public Dictionary<byte, Tree>? Children { get; private set; }

            public MagicValueMediaType? this[ReadOnlySpan<byte> bytes]
            {
                get
                {
                    if (bytes.Length == 0)
                        return Node;

                    if (Children == null)
                        return null;

                    byte b = bytes[0];

                    if (!Children.TryGetValue(b, out Tree? subtree))
                    {
                        return null;
                    }

                    return subtree[bytes.Slice(1)];
                }
            }

            private void AddInternal(MagicValueMediaType magic, ReadOnlySpan<byte> bytes)
            {
                if (bytes.Length == 0)
                {
                    Node = magic;
                    return;
                }
                
                if (Children == null)
                {
                    Children = new Dictionary<byte, Tree>();
                }
                
                if (!Children.TryGetValue(bytes[0], out Tree? tree))
                {
                    tree = new Tree();
                    Children[bytes[0]] = tree;
                }

                tree.AddInternal(magic, bytes.Slice(1));
            }

            public void Add(MagicValueMediaType magic)
            {
                ReadOnlySpan<byte> bytes = magic.Magic.Value;
                AddInternal(magic, bytes);
            }
        }
    }
}