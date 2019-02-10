using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

#if NETSTANDARD1_3
using System;
using System.Reflection;
#endif

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
            return dictionary.TryGetValue(key, out TValue value) ? value : default(TValue);
        }

        public static void Write(this Stream stream, char c)
        {
            stream.WriteByte((byte)c);
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source)
        {
            return source.SelectMany(x => x);
        }

#if NETSTANDARD1_3
        public static bool IsAssignableFrom(this Type type, Type otherType)
        {
            return type.GetTypeInfo().IsAssignableFrom(otherType.GetTypeInfo());
        }
#endif
    }
}
