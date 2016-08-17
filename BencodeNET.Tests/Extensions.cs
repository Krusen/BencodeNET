using System.IO;
using System.Text;

namespace BencodeNET.Tests
{
    internal static class Extensions
    {
        internal static string AsString(this Stream stream)
        {
            stream.Position = 0;
            var sr = new StreamReader(stream, Encoding.UTF8);
            return sr.ReadToEnd();
        }

        internal static string AsString(this Stream stream, Encoding encoding)
        {
            stream.Position = 0;
            var sr = new StreamReader(stream, encoding);
            return sr.ReadToEnd();
        }
    }
}
