using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BencodeNET
{
    public static class UtilityExtensions
    {
        public static bool EndOfStream(this Stream stream)
        {
            return stream.Position == stream.Length;
        }

        public static void WriteChar(this Stream stream, char c)
        {
            stream.WriteByte((byte)c);
        }

        public static string AsString(this IEnumerable<char> chars)
        {
            if (chars == null)
                return null;

            var sb = new StringBuilder();
            foreach (var c in chars)
                sb.Append(c);
            return sb.ToString();
        }

        public static char ReadCharOrDefault(this BinaryReader reader)
        {
            return reader.ReadChars(1).FirstOrDefault();
        }
    }
}
