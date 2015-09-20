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
    }
}
