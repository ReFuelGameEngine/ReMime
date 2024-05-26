using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using ReMime.ContentResolvers;
using ReMime.Platform;

namespace ReMime
{
    public static class MediaTypeResolver
    {
        private static readonly SortedList<int, IMediaTypeResolver> s_resolvers = new SortedList<int, IMediaTypeResolver>();
        private static IReadOnlyList<MediaType>? s_mediaTypes = null;

        public static IEnumerable<IMediaTypeResolver> Resolvers => s_resolvers.Values;

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
            AddResolver(new MagicContentResolver(), 9998);

            if (OperatingSystem.IsWindows())
            {
                AddResolver(new Win32MediaTypeResolver());
            }
            else if (OperatingSystem.IsLinux())
            {
                AddResolver(new UnixMediaTypeResolver());
                // TODO: add freedesktop mime type database.
            }
            else if (OperatingSystem.IsMacOS())
            {
                AddResolver(new UnixMediaTypeResolver()); //?
            }
        }

        public static void AddResolver(IMediaTypeResolver resolver, int priority = 9999)
        {
            s_resolvers.Add(priority, resolver);
        }

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
  
        public static bool TryResolve(Stream stream, out MediaType mediaType)
        {
            if (!stream.CanSeek)
            {
                throw new Exception("This stream is not seekable, cannot resolve unseekable streams.");
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