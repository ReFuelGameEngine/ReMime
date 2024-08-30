using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using ReMime.ContentResolvers;
using ReMime.Platform;

namespace ReMime
{
    /// <summary>
    /// Resolve media types from file names and file contents.
    /// </summary>
    public static class MediaTypeResolver
    {
        private static readonly SortedList<int, IMediaTypeResolver> s_resolvers = new SortedList<int, IMediaTypeResolver>();
        private static IReadOnlyList<MediaType>? s_mediaTypes = null;

        /// <summary>
        /// Enumeration of currently available media type resolvers.
        /// </summary>
        public static IEnumerable<IMediaTypeResolver> Resolvers => s_resolvers.Values;

        /// <summary>
        /// Enumeration of detectable media types.
        /// </summary>
        public static IEnumerable<MediaType> KnownTypes
        {
            get
            {
                if (s_mediaTypes != null)
                {
                    return s_mediaTypes;
                }

                IEnumerable<MediaType>? x = null;
                foreach (IEnumerable<MediaType> types in s_resolvers.Values.Select(x => x.MediaTypes))
                {
                    x = (x == null) ? types : x.Concat(types);
                }

                if (x == null)
                {
                    return Enumerable.Empty<MediaType>();
                }

                return s_mediaTypes = x.DistinctBy(x => x.FullTypeNoParameters).ToImmutableList();
            }
        }

        static MediaTypeResolver()
        {
            AddResolver(RiffResolver.Instance, 1000);
            AddResolver(MagicContentResolver.Instance, 1001);

            if (Win32MediaTypeResolver.Instance != null)
                AddResolver(Win32MediaTypeResolver.Instance, 1002);
            if (UnixMediaTypeResolver.Instance != null)
                AddResolver(UnixMediaTypeResolver.Instance, 1002);
        }

        /// <summary>
        /// Add a media type resolver.
        /// </summary>
        /// <param name="resolver">The resolver instance to add.</param>
        /// <param name="priority">The resolver priority. Less is more prescedent.</param>
        public static void AddResolver(IMediaTypeResolver resolver, int priority = 9999)
        {
            s_resolvers.Add(priority, resolver);
        }

        /// <summary>
        /// Try to resolve the media type from a path.
        /// </summary>
        /// <param name="path">The path string.</param>
        /// <param name="mediaType">The result media type.</param>
        /// <returns>True if there was a matching media type.</returns>
        /// <exception cref="ArgumentException">Issues with <paramref name="path"> string. See <see cref="Path.GetFileName"/>.</exception>
        public static bool TryResolve(ReadOnlySpan<char> path, out MediaType mediaType)
        {
            path = Path.GetFileName(path);
            if (path.Length == 0)
            {
                throw new ArgumentException("Path is not a file path.");
            }

            ReadOnlySpan<char> span = path;
            Stack<int> indices = new Stack<int>();

            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] == '.')
                    indices.Push(i);
            }

            while (indices.TryPop(out int dot))
            {
                string value = new string(path.Slice(dot+1)).ToLowerInvariant();

                foreach (IMediaTypeResolver resolver in Resolvers)
                {
                    if (resolver.TryResolve(value, out mediaType!))
                    {
                        return true;
                    }
                }
            }

            // Could not resolve media type, oh well.
            mediaType = MediaType.OctetStream;
            return false;
        }
  
        /// <summary>
        /// Try to resolve the media type from a stream.
        /// </summary>
        /// <param name="stream">The stream to inspect.</param>
        /// <param name="mediaType">The result media type.</param>
        /// <returns>True if the type was resolved.</returns>
        /// <exception cref="ArgumentException">The <paramref name="stream"/> is unseekable.</exception>
        public static bool TryResolve(Stream stream, out MediaType mediaType)
        {
            if (!stream.CanSeek)
            {
                throw new ArgumentException("This stream is not seekable, cannot resolve unseekable streams.", nameof(stream));
            }

            foreach (IMediaTypeResolver resolver in Resolvers)
            {
                if (resolver is not IMediaContentResolver contentResolver)
                {
                    continue;
                }

                stream.Seek(0, SeekOrigin.Begin);
                if (contentResolver.TryResolve(stream, out mediaType!))
                {
                    return true;
                }
            }

            mediaType = MediaType.OctetStream;
            return false;
        }

        /// <summary>
        /// Try to resolve the media type from a span.
        /// </summary>
        /// <param name="bytes">A span of bytes from the start of the media.</param>
        /// <param name="mediaType">The result media type.</param>
        /// <returns>True if the type was resolved.</returns>
        public static bool TryResolve(ReadOnlySpan<byte> bytes, out MediaType mediaType)
        {
            foreach (IMediaTypeResolver resolver in Resolvers)
            {
                if (resolver is not IMediaContentResolver contentResolver)
                {
                    continue;
                }

                if (contentResolver.TryResolve(bytes, out mediaType!))
                {
                    return true;
                }
            }

            mediaType = MediaType.OctetStream;
            return false;
        }

        /// <summary>
        /// Try to resolve the media type.
        /// </summary>
        /// <param name="path">The path string.</param>
        /// <param name="bytes">A span of bytes from the start of the media.</param>
        /// <param name="mediaType">The result media type.</param>
        /// <returns><see cref="MediaTypeResult.None"/> if none matched.</returns>
        /// <exception cref="ArgumentException">
        ///     The <paramref name="stream"/> is unseekable, or issues with <paramref name="path"> string.
        ///     See <see cref="Path.GetFileName"/>
        /// </exception>
        public static MediaTypeResult TryResolve(ReadOnlySpan<char> path, ReadOnlySpan<byte> bytes, out MediaType mediaType)
        {
            if (TryResolve(bytes, out mediaType))
            {
                // Only return both matched if the media types agree.
                return
                    (!TryResolve(path, out MediaType mt2) || mt2.FullTypeNoParameters != mediaType.FullTypeNoParameters)
                        ? MediaTypeResult.Content
                        : MediaTypeResult.Extension | MediaTypeResult.Content;
            }
            else if (TryResolve(path, out mediaType))
            {
                return MediaTypeResult.Extension;
            }
            else
            {
                mediaType = MediaType.OctetStream;
                return MediaTypeResult.None;
            }
        }

        /// <summary>
        /// Try to resolve the media type.
        /// </summary>
        /// <param name="path">The path string.</param>
        /// <param name="stream">The stream to inspect.</param>
        /// <param name="mediaType">The result media type.</param>
        /// <returns><see cref="MediaTypeResult.None"/> if none matched.</returns>
        /// <exception cref="ArgumentException">
        ///     The <paramref name="stream"/> is unseekable, or issues with <paramref name="path"> string.
        ///     See <see cref="Path.GetFileName"/>
        /// </exception>
        public static MediaTypeResult TryResolve(ReadOnlySpan<char> path, Stream stream, out MediaType mediaType)
        {
            if (TryResolve(stream, out mediaType))
            {
                // Only return both matched if the media types agree.
                return
                    (!TryResolve(path, out MediaType mt2) || mt2.FullTypeNoParameters != mediaType.FullTypeNoParameters)
                        ? MediaTypeResult.Content
                        : MediaTypeResult.Extension | MediaTypeResult.Content;
            }
            else if (TryResolve(path, out mediaType))
            {
                return MediaTypeResult.Extension;
            }
            else
            {
                mediaType = MediaType.OctetStream;
                return MediaTypeResult.None;
            }
        }

        /// <summary>
        /// Try to resolve the media type.
        /// </summary>
        /// <param name="fileInfo">The FileInfo object to the file.</param>
        /// <param name="mediaType">The result media type.</param>
        /// <param name="open">True to open the file and inspect the contents as well.</param>
        /// <returns><see cref="MediaTypeResult.None"/> if none matched.</returns>
        public static MediaTypeResult TryResolve(FileInfo fileInfo, out MediaType mediaType, bool open = true)
        {
            if (open)
            {
                using Stream str = fileInfo.OpenRead();
                return TryResolve(fileInfo.Name, str, out mediaType);
            }
            else
            {
                return TryResolve(fileInfo.Name, out mediaType) ? MediaTypeResult.Extension : 0;
            }
        }

    }
}