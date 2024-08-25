using System;
using System.Collections.Generic;

namespace ReMime
{
    /// <summary>
    /// Interface for all media type resolvers.
    /// </summary>
    public interface IMediaTypeResolver
    {
        /// <summary>
        /// Known media types.
        /// </summary>
        IReadOnlyCollection<MediaType> MediaTypes { get; }

        /// <summary>
        /// Resolve media type via file extension.
        /// </summary>
        /// <param name="extension">The file extension of the file without the leading dot.</param>
        /// <param name="mediaType">The media type for this extension.</param>
        /// <returns>True if a type matched.</returns>
        bool TryResolve(string extension, out MediaType? mediaType);
    }
}