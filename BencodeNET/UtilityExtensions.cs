using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BencodeNET
{
    public static class UtilityExtensions
    {
        public static bool IsDigit(this char c)
        {
            return (c >= '0' && c <= '9');
        }

        public static void Write(this Stream stream, char c)
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
    }
}
