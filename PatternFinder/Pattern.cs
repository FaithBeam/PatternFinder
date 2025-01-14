﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PatternFinder
{
    public static class Pattern
    {
        public struct Byte
        {
            public struct Nibble
            {
                public bool Wildcard;
                public byte Data;
            }

            public Nibble N1;
            public Nibble N2;
        }

        private static string Format(string pattern)
        {
            var length = pattern.Length;
            var result = new StringBuilder(length);
            for (var i = 0; i < length; i++)
            {
                var ch = pattern[i];
                if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f' || ch == '?')
                    result.Append(ch);
            }
            return result.ToString();
        }

        private static int HexChToInt(char ch)
        {
            if (ch >= '0' && ch <= '9')
                return ch - '0';
            if (ch >= 'A' && ch <= 'F')
                return ch - 'A' + 10;
            if (ch >= 'a' && ch <= 'f')
                return ch - 'a' + 10;
            return -1;
        }

        public static Byte[] Transform(string pattern)
        {
            pattern = Format(pattern);
            var length = pattern.Length;
            if (length == 0)
                return null;
            var result = new List<Byte>((length + 1) / 2);
            if (length % 2 != 0)
            {
                pattern += "?";
                length++;
            }
            var newByte = new Byte();
            for (int i = 0, j = 0; i < length; i++)
            {
                var ch = pattern[i];
                if (ch == '?') //wildcard
                {
                    if (j == 0)
                        newByte.N1.Wildcard = true;
                    else
                        newByte.N2.Wildcard = true;
                }
                else //hex
                {
                    if (j == 0)
                    {
                        newByte.N1.Wildcard = false;
                        newByte.N1.Data = (byte)(HexChToInt(ch) & 0xF);
                    }
                    else
                    {
                        newByte.N2.Wildcard = false;
                        newByte.N2.Data = (byte)(HexChToInt(ch) & 0xF);
                    }
                }

                j++;
                if (j != 2) continue;
                j = 0;
                result.Add(newByte);
            }
            return result.ToArray();
        }

        public static bool Find(byte[] data, Byte[] pattern)
        {
            return Find(data, pattern, out _);
        }

        private static bool MatchByte(byte b, ref Byte p)
        {
            if (!p.N1.Wildcard) //if not a wildcard we need to compare the data.
            {
                var n1 = b >> 4;
                if (n1 != p.N1.Data) //if the data is not equal b doesn't match p.
                    return false;
            }

            if (p.N2.Wildcard) return true; //if not a wildcard we need to compare the data.
            var n2 = b & 0xF;
            return n2 == p.N2.Data; //if the data is not equal b doesn't match p.
        }

        public static bool Find(byte[] data, Byte[] pattern, out long offsetFound, long offset = 0)
        {
            offsetFound = -1;
            if (data == null || pattern == null)
                return false;
            var patternSize = pattern.LongLength;
            if (data.LongLength == 0 || patternSize == 0)
                return false;

            for (long i = offset, pos = 0; i < data.LongLength; i++)
            {
                if (MatchByte(data[i], ref pattern[pos])) //check if the current data byte matches the current pattern byte
                {
                    pos++;
                    if (pos != patternSize) continue; //everything matched
                    offsetFound = i - patternSize + 1;
                    return true;
                }

                //fix by Computer_Angel
                i -= pos;
                pos = 0; //reset current pattern position
            }

            return false;
        }

        public static bool FindAll(byte[] data, Byte[] pattern, out List<long> offsetsFound)
        {
            offsetsFound = new List<long>();
            long size = data.Length, pos = 0;
            while (size > pos)
            {
                if (Find(data, pattern, out var offsetFound, pos))
                {
                    offsetsFound.Add(offsetFound);
                    pos = offsetFound + pattern.Length;
                }
                else
                    break;
            }
            return offsetsFound.Count > 0;
        }

        public static Signature[] Scan(byte[] data, IEnumerable<Signature> signatures)
        {
            var found = new ConcurrentBag<Signature>();
            Parallel.ForEach(signatures, signature =>
            {
                if (Find(data, signature.Pattern, out signature.FoundOffset))
                    found.Add(signature);
            });
            return found.ToArray();
        }
    }
}