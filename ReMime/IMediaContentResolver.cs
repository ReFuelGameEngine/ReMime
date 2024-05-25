using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace ReMime
{
    public interface IMediaContentResolver : IMediaTypeResolver
    {
        /// <summary>
        /// Resolve media type via file extension.
        /// </summary>
        /// <param name="str">The stream to match the content with.</param>
        /// <param name="mediaType">The media type for this extension.</param>
        /// <returns>True if content matched.</returns>
        bool TryResolve(Stream str, [NotNullWhen(true)] out MediaType? mediaType);

        /// <summary>
        /// Resolve media type via file extension.
        /// </summary>
        /// <param name="content">The stream to match the content with.</param>
        /// <param name="mediaType">The media type for this extension.</param>
        /// <returns>True if content matched.</returns>
        bool TryResolve(ReadOnlySpan<byte> content, [NotNullWhen(true)] out MediaType? mediaType);
    }
}