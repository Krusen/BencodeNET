using System.IO;
using System.Text;
using BencodeNET.IO;
using NSubstitute.Core;

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

        internal static void SkipBytes(this BencodeReader reader, int length)
        {
            reader.Read(new byte[length]);
        }

        internal static ConfiguredCall AndSkipsAhead(this ConfiguredCall call, int length)
        {
            return call.AndDoes(x => x.Arg<BencodeReader>().SkipBytes(length));
        }
    }
}
