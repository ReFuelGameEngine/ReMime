
using System;

namespace ReMime.ContentResolvers
{
    /* You've heard of regular expressions, now prepare for magical expressions :sparkles:. */
    /// <summary>
    /// Bit pattern detecting state machine inspired by text regular expressions.
    /// </summary>
    public class MagEx
    {
        /**
         * 0 1 2 3 4 5 6 7 8 9 a b c d e f      4-bit patterns to match.
         * l h                                  Single bit pattern.
         * *                                    Any bit pattern.
         * ?                                    Any 4-bit pattern.
         * 'pattern'                            ASCII pattern with no terminator. Implies @.
         * @                                    Align to 8-bits.
         * %                                    Align to 4-bits.
         */

        public string Pattern { get; }

        public MagEx(string pattern)
        {
            Pattern = pattern;
        }

        public bool Match(ReadOnlySpan<byte> bytes)
        {
            byte current;
            int needle;
            int haystack;
            int bits;
            int pi = 0;
            ReadOnlySpan<char> ascii = ReadOnlySpan<char>.Empty;
            for (int i = 0; i < bytes.Length; i++)
            {
                current = bytes[i];
                bits = 8;

                while (bits > 0)
                {
                    char pat = Pattern[pi];
                    switch (pat)
                    {
                    case '0': case '1': case '2': case '3':
                    case '4': case '5': case '6': case '7':
                    case '8': case '9': case 'a': case 'A':
                    case 'b': case 'B': case 'c': case 'C':
                    case 'd': case 'D': case 'e': case 'E':
                    case 'f': case 'F':
                        haystack = current & 0xF;
                        current >>= 4;
                        bits -= 4;

                        if (pat >= '0' && pat <= '9')
                        {
                            needle = pat - '0';
                        }
                        else if (pat >= 'a' && pat <= 'f')
                        {
                            needle = pat - 'a';
                        }
                        else
                        {
                            needle = pat - 'A';
                        }

                        if (haystack == needle)
                        {
                            pi++;
                        }
                        else
                        {
                            
                        }
                        break;
                    }
                }
            }

            return false;
        }
    }
}