using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace ReMime.ContentResolvers
{
    /// <summary>
    /// A magic value to identify file types.
    /// </summary>
    /// <param name="Value">The byte array that makes up the magic value.</param>
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

        public static bool TryParse(ReadOnlySpan<char> magic, [NotNullWhen(true)] out MagicValue? value)
        {
            List<byte> bytes = new List<byte>();
            StringBuilder builder = new StringBuilder();

            value = null;

            for (int i = 0; i < magic.Length; i++)
            {
                char chr = magic[i];
                char chr2;
                switch (chr)
                {
                case '\'':
                    builder.Clear();

                    int j;
                    for (j = i + 1; j < magic.Length; j++)
                    {
                        chr = magic[j];
                        if (chr == '\'')
                        {
                            bytes.AddRange(Encoding.ASCII.GetBytes(builder.ToString()));
                            break;
                        }
                        else if (chr == '\\')
                        {
                            if (j+1 >= magic.Length)
                                return false;

                            chr2 = magic[j++];

                            builder.Append(chr2 switch {
                                'n' => '\n',
                                'r' => '\r',
                                'a' => '\a',
                                'b' => '\b',
                                'f' => 'f',
                                'v' => '\v',
                                '?' => '?',
                                '\\' => '\\',
                                '\'' => '\'',
                                '\"' => '\"',
                                _ => '\0'
                            });
                        }
                        else
                        {
                            builder.Append(chr);
                        }
                    }

                    if (j == magic.Length)
                    {
                        // ASCII string overrun.
                        return false;
                    }

                    i = j;

                    break;
                case '0': case '1': case '2': case '3':
                case '4': case '5': case '6': case '7':
                case '8': case '9': case 'A': case 'B':
                case 'C': case 'D': case 'E': case 'F':
                case 'a': case 'b': case 'c': case 'd':
                case 'e': case 'f':
                    // Misaligned hex string.
                    if (i+1 >= magic.Length)
                        return false;

                    chr2 = magic[++i];
                    bytes.Add((byte)(AsciiToInt(chr) << 4 | AsciiToInt(chr2)));
                    break;

                case '\n': case '\f': case '\r': case '\t':
                case ' ':
                    // generic whitespace.
                    continue;
                }
            }

            // No bytes to match.
            if (bytes.Count == 0)
                return false;

            value = new MagicValue(bytes.ToArray());
            return true;

            static int AsciiToInt(char a)
            {
                if (a >= '0' && a <= '9')
                    return a - '0';
                else if (a >= 'A' && a <= 'F')
                    return a - 'A' + 10;
                else if (a >= 'a' && a <= 'f')
                    return a - 'a' + 10;
                else
                    return -1;
            }
        }
    }
}