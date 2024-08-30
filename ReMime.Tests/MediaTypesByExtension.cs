using System.Runtime.CompilerServices;
using ReMime.Platform;

namespace ReMime.Tests
{
    [TestClass]
    public class MediaTypesByExtension
    {
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
                Assert.IsTrue(MediaTypeResolver.TryResolve(ext, out MediaType? result));
                Assert.AreEqual(result!.FullType, type);
                Assert.IsTrue(result.Extensions.Contains(ext));
            }
        }
    }
}
