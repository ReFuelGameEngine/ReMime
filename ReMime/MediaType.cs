using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ReMime
{
    /// <summary>
    /// An IANA media type.
    /// </summary>
    [DebuggerDisplay("{FullType}")]
    public class MediaType
    {
        /// <summary>
        /// Full media type string, including parameters.
        /// </summary>
        public string FullType { get; }

        /// <summary>
        /// Media type string excluding parameters.
        /// </summary>
        public string FullTypeNoParameters { get; }

        /// <summary>
        /// Main media type, e.g. <c>image</c>
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Vendor tree, e.g. <c>vnd.microsoft</c> in <c>application/vnd.microsoft.portable-executable</c>
        /// </summary>
        public string? Tree { get; }

        /// <summary>
        /// Subtype, e.g. <c>png</c> in <c>image/png</c>
        /// </summary>
        public string SubType { get; }

        /// <summary>
        /// Media type suffix, e.g. <c>json</c> in <c>model/gltf+json</c>
        /// </summary>
        public string? Suffix { get; }

        /// <summary>
        /// Media type parameters, e.g. <c>encoding=utf-8</c> in <c>text/plain;encoding=utf-8</c>
        /// </summary>
        public string? Parameters { get; }

        /// <summary>
        /// Valid or common file extensions for this media type, excluding the dot. May be empty.
        /// </summary>
        public IReadOnlyCollection<string> Extensions { get; }

        /// <summary>
        /// Create a media type descriptor.
        /// </summary>
        /// <param name="fullType">The full media type string.</param>
        /// <param name="extensions">The file extensions if any.</param>
        /// <exception cref="Exception">Malformed media type string in <paramref name="fullType"/>.</exception>
        public MediaType(string fullType, IEnumerable<string>? extensions = null)
        {
            FullType = fullType;

            ReadOnlySpan<char> str = fullType.AsSpan();

            int slash = str.IndexOf('/');
            int plus = str.IndexOf('+');
            int semicolon = str.IndexOf(';');

            if (slash == -1)
            {
                throw new Exception("Malformed media type string.");
            }

            Type = new string(str.Slice(0, slash));

            ReadOnlySpan<char> typeTree;
            if (plus != -1)
            {
                typeTree = str.Slice(slash+1, plus - slash - 1);
            }
            else if (semicolon != -1)
            {
                typeTree = str.Slice(slash+1, semicolon - slash - 1);
            }
            else
            {
                typeTree = str.Slice(slash+1);
            }

            int dot = typeTree.LastIndexOf('.');
            if (dot == -1)
            {
                SubType = new string(typeTree);
            }
            else
            {
                SubType = new string(typeTree.Slice(dot+1));
                Tree = new string(typeTree.Slice(0, dot));
            }

            if (plus != -1)
            {
                if (semicolon != -1)
                {
                    Suffix = new string(str.Slice(plus+1, semicolon - plus - 1));
                }
                else
                {
                    Suffix = new string(str.Slice(plus + 1));
                }
            }

            if (semicolon != -1)
            {
                Parameters = new string(str.Slice(semicolon+1));
                FullTypeNoParameters = new string(str.Slice(0, semicolon));
            }
            else
            {
                FullTypeNoParameters = FullType;
            }

            Extensions = (extensions ?? Enumerable.Empty<string>()).ToArray();
        }

        /// <summary>
        /// Convert Media type to its string.
        /// </summary>
        /// <param name="includeParameters">Include media type parameters.</param>
        /// <returns>A string.</returns>
        public string ToString(bool includeParameters) => includeParameters ? FullType : FullTypeNoParameters;

        /// <inheritdoc/>
        public override string ToString() => ToString(false);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return FullType.GetHashCode();
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return FullType == (obj as MediaType)?.FullType;
        }

        public static bool operator==(MediaType a, MediaType b)
        {
            return a.FullType == b.FullType;
        }

        public static bool operator!=(MediaType a, MediaType b)
        {
            return a.FullType != b.FullType;
        }

        /// <summary>
        /// <c>application/octet-stream</c> is the default for unknown media types.
        /// </summary>
        public static readonly MediaType OctetStream = new MediaType(
            "application/octet-stream",
            new[] {"bin", "lha", "lzh", "exe", "class", "so", "dll", "img", "iso"});
    }
}