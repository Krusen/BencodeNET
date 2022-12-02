using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Text;

namespace BencodeNET
{
    internal static class UtilityExtensions
    {
        public static bool IsDigit(this char c)
        {
            return c >= '0' && c <= '9';
        }

        public static MemoryStream AsStream(this string str, Encoding encoding)
        {
            return new MemoryStream(encoding.GetBytes(str));
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.TryGetValue(key, out var value) ? value : default;
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source)
        {
            return source.SelectMany(x => x);
        }

        public static int DigitCount(this int value) => DigitCount((long)value);

        public static int DigitCount(this long value)
        {
            var sign = value < 0 ? 1 : 0;

            if (value == long.MinValue)
                return 20;

            value = Math.Abs(value);

            if (value < 10)
                return sign + 1;
            if (value < 100)
                return sign + 2;
            if (value < 1000)
                return sign + 3;
            if (value < 10000)
                return sign + 4;
            if (value < 100000)
                return sign + 5;
            if (value < 1000000)
                return sign + 6;
            if (value < 10000000)
                return sign + 7;
            if (value < 100000000)
                return sign + 8;
            if (value < 1000000000)
                return sign + 9;
            if (value < 10000000000)
                return sign + 10;
            if (value < 100000000000)
                return sign + 11;
            if (value < 1000000000000)
                return sign + 12;
            if (value < 10000000000000)
                return sign + 13;
            if (value < 100000000000000)
                return sign + 14;
            if (value < 1000000000000000)
                return sign + 15;
            if (value < 10000000000000000)
                return sign + 16;
            if (value < 100000000000000000)
                return sign + 17;
            if (value < 1000000000000000000)
                return sign + 18;

            return sign + 19;
        }

        public static bool TrySetLength(this Stream stream, long length)
        {
            if (!stream.CanWrite || !stream.CanSeek)
                return false;

            try
            {
                if (stream.Length >= length)
                    return false;

                stream.SetLength(length);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void Write(this Stream stream, int number)
        {
            Span<byte> buffer = stackalloc byte[11];
            var bytesRead = Encoding.ASCII.GetBytes(number.ToString().AsSpan(), buffer);
            stream.Write(buffer.Slice(0, bytesRead));
        }

        public static void Write(this Stream stream, long number)
        {
            Span<byte> buffer = stackalloc byte[20];
            var bytesRead = Encoding.ASCII.GetBytes(number.ToString().AsSpan(), buffer);
            stream.Write(buffer.Slice(0, bytesRead));
        }

        public static void Write(this Stream stream, char c)
        {
            stream.WriteByte((byte)c);
        }

        public static void WriteChar(this PipeWriter writer, char c)
        {
            writer.GetSpan(1)[0] = (byte)c;
            writer.Advance(1);
        }

        public static void WriteCharAt(this Span<byte> bytes, char c, int index)
        {
            bytes[index] = (byte)c;
        }

#if !NETSTANDARD2_0
        public static string AsString(this ReadOnlySpan<char> chars)
        {
            return new string(chars);
        }

        public static string AsString(this Span<char> chars)
        {
            return new string(chars);
        }

        public static string AsString(this Memory<char> chars)
        {
            return new string(chars.Span);
        }
#else
        public static string AsString(this ReadOnlySpan<char> chars)
        {
            return new string(chars.ToArray());
        }

        public static string AsString(this Span<char> chars)
        {
            return new string(chars.ToArray());
        }

        public static string AsString(this Memory<char> chars)
        {
            return new string(chars.ToArray());
        }
#endif
    }
}
