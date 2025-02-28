using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Win32;

namespace ReMime.Platform
{
    /// <summary>
    /// Media type resolver for Windows systems.
    /// </summary>
    public class Win32MediaTypeResolver : IMediaTypeResolver
    {
        private readonly Dictionary<string, MediaType> _extensionsMap = new Dictionary<string, MediaType>();
        public IReadOnlyCollection<MediaType> MediaTypes { get; }
        
        private Win32MediaTypeResolver()
        {
            if (!OperatingSystem.IsWindows())
                throw new PlatformNotSupportedException();

            Dictionary<string, List<string>> map = new Dictionary<string, List<string>>();
            List<MediaType> list = new List<MediaType>();

            foreach (var name in Registry.ClassesRoot.GetSubKeyNames().Where(x => x.StartsWith('.')))
            {
                RegistryKey? key = Registry.ClassesRoot.OpenSubKey(name);
                string? value = key?.GetValue("Content Type") as string;

                if (value == null)
                    continue;

                if (!map.TryGetValue(value, out List<string>? extensions))
                {
                    extensions = new List<string>();
                    map[value] = extensions;
                }

                extensions.Add(name.Substring(1));
                key!.Dispose();
            }

            foreach (var(type, extensions) in map)
            {
                MediaType mediaType = new MediaType(type, extensions);
                list.Add(mediaType);

                foreach (string extension in extensions)
                {
                    _extensionsMap[extension] = mediaType;
                }
            }

            MediaTypes = list.AsReadOnly();
        }

        public bool TryResolve(string extension, out MediaType? mediaType)
        {
            return _extensionsMap.TryGetValue(extension, out mediaType);
        }

        public static Win32MediaTypeResolver? Instance { get; } = null;

        static Win32MediaTypeResolver()
        {
            try
            {
                Instance = new Win32MediaTypeResolver();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}