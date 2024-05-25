using System.Runtime.CompilerServices;
using ReMime.Platform;

namespace ReMime.Tests
{
    public abstract class MediaTypesByExtension<T> where T : IMediaTypeResolver, new()
    {
        private T CIT;

        protected MediaTypesByExtension()
        {
            Unsafe.SkipInit(out CIT);
        }

        [TestInitialize]
        public virtual void Initialize()
        {
            CIT = new T();
        }

        readonly (string extension, string type)[] ExampleMimeTypes = new (string, string)[] {
            ("png", "image/png"),
            ("gif", "image/gif"),
            ("jpeg", "image/jpeg"),
            ("jpg", "image/jpeg"),
            ("txt", "text/plain"),
            ("css", "text/css"),
            ("mp4", "video/mp4"),
            ("ttf", "font/ttf")
        };

        [TestMethod]
        public void PassKnownTypes()
        {
            foreach (var(ext, type) in ExampleMimeTypes)
            {
                Assert.IsTrue(CIT.TryResolve(ext, out MediaType? result));
                Assert.AreEqual(result!.FullType, type);
                Assert.IsTrue(result.Extensions.Contains(ext));
            }
        }
    }

    [TestClass]
    public class UnixMediaTypes : MediaTypesByExtension<UnixMediaTypeResolver>
    {
        [TestInitialize]
        public override void Initialize()
        {
            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || OperatingSystem.IsFreeBSD())
            {
                base.Initialize();
            }
            else
            {
                Assert.Inconclusive("Cannot test this in this platform.");
            }
        }
    }

    [TestClass]
    public class Win32MediaTypes : MediaTypesByExtension<Win32MediaTypeResolver>
    {
        [TestInitialize]
        public override void Initialize()
        {
            if (OperatingSystem.IsWindows())
            {
                base.Initialize();
            }
            else
            {
                Assert.Inconclusive("Cannot test this in this platform.");
            }
        }
    }
}
