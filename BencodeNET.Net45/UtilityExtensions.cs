using System.Collections.Generic;
using System.IO;
using System.Text;
#if !NET35
using System.Threading.Tasks;
#endif

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

#if !NET35
        public static async Task<int> ReadByteAsync(this Stream stream)
        {

            var data = new byte[1];
            var bytesRead = await stream.ReadAsync(data, 0, 1).ConfigureAwait(false);
            if (bytesRead == 0)
                return -1;
            return data[0];

        }

        public static Task WriteAsync(this Stream stream, char c)
        {
            return stream.WriteAsync(new [] {(byte) c}, 0, 1);
        }
#endif
    }
}
