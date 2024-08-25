using System;
using System.Text;

namespace ReMime.ContentResolvers
{
    /// <summary>
    /// A magic value to identify file types.
    /// </summary>
    /// <param name="Value">The byte arary that makes up the magic value.</param>
    public record struct MagicValue(byte[] Value)
    {
        public MagicValue(int value) : this(BitConverter.GetBytes(value)) { }
        public MagicValue(short value) : this(BitConverter.GetBytes(value)) { }
        public MagicValue(string value, Encoding? encoding = null)
            : this((encoding ?? Encoding.ASCII).GetBytes(value)) { }
        public MagicValue(ReadOnlySpan<byte> bytes) : this(bytes.ToArray()) { }

        /// <summary>
        /// Check if <paramref name="haystack"/> matches this magic value.
        /// </summary>
        /// <param name="haystack"></param>
        /// <returns></returns>
        public bool Matches(ReadOnlySpan<byte> haystack)
        {
            for (int i = 0; i < haystack.Length && i < Value.Length; i++)
            {
                if (haystack[i] != Value[i])
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            // Uses the FVN-1A algorithm in 32-bit mode.
            const int PRIME = 0x01000193;
            const int BASIS = unchecked((int)0x811c9dc5);

            int hash = BASIS;
            for (int i = 0; i < Value.Length; i++)
            {
                hash ^= Value[i];
                hash *= PRIME;
            }

            return hash;
        }
    }
}