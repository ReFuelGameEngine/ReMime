using System;

namespace ReMime
{
    /// <summary>
    /// The result of a combined media type query.
    /// </summary>
    [Flags]
    public enum MediaTypeResult
    {
        /// <summary>
        /// No match was found.
        /// </summary>
        None = 0,

        /// <summary>
        /// Matched via file extension.
        /// </summary>
        Extension = 1 << 0,

        /// <summary>
        /// Matched via both file extension and file contents.
        /// </summary>
        Content = 1 << 1,
    }
}