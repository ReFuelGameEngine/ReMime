using System.Collections.Generic;

namespace ReMime.ContentResolvers.Image
{
    public static class ImageMagicValues
    {
        private static readonly MediaType Tiff = new MediaType("image/tiff", new string[] { "nif", "tif", "tiff"});
        private static readonly MediaType Jpeg = new MediaType("image/jpeg", new string[] { "jpg", "jpeg" });

        public static readonly IReadOnlyList<MagicValueMediaType> List = new List<MagicValueMediaType>() {
            new MagicValueMediaType(new MagicValue("BM"), new MediaType("image/bmp")),
            new MagicValueMediaType(new MagicValue("GIF8"), new MediaType("image/gif")),
            new MagicValueMediaType(new MagicValue("IIN1"), Tiff),
            new MagicValueMediaType(new MagicValue(new byte[] { 0x4d, 0x4d, 0x00, 0x2a }), Tiff),
            new MagicValueMediaType(new MagicValue(new byte[] { 0x49, 0x49, 0x2a, 0x00 }), Tiff),
            new MagicValueMediaType(new MagicValue(new byte[] { 0x89, 0x50, 0x4e, 0x47 }), new MediaType("image/png")),

            /* Yes this is how we are doing JPEG, I don't want to modify my thing to allow for magic values to be defined in terms of bits. */
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xe0 }), Jpeg),
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xe1 }), Jpeg),
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xe2 }), Jpeg),
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xe3 }), Jpeg),
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xe4 }), Jpeg),
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xe5 }), Jpeg),
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xe6 }), Jpeg),
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xe7 }), Jpeg),
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xe8 }), Jpeg),
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xe9 }), Jpeg),
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xea }), Jpeg),
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xeb }), Jpeg),
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xec }), Jpeg),
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xed }), Jpeg),
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xee }), Jpeg),
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xef }), Jpeg),
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xf0 }), Jpeg),
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xf1 }), Jpeg),
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xf2 }), Jpeg),
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xf3 }), Jpeg),
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xf4 }), Jpeg),
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xf5 }), Jpeg),
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xf6 }), Jpeg),
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xf7 }), Jpeg),
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xf8 }), Jpeg),
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xf9 }), Jpeg),
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xfa }), Jpeg),
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xfb }), Jpeg),
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xfc }), Jpeg),
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xfd }), Jpeg),
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xfe }), Jpeg),
            new MagicValueMediaType(new MagicValue(new byte[] { 0xff, 0xd8, 0xff, 0xff }), Jpeg),
        }.AsReadOnly();

        public static void AddToMagicResolver(MagicContentResolver resolver)
        {
            resolver.AddMagicValues(List);
        }
    }
}