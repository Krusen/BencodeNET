using System;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using BencodeNET.IO;
using BencodeNET.Objects;
using BencodeNET.Parsing;
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

        internal static Task SkipBytesAsync(this PipeBencodeReader reader, int length)
        {
            return reader.ReadAsync(new byte[length]).AsTask();
        }

        internal static ConfiguredCall AndSkipsAhead(this ConfiguredCall call, int length)
        {
            return call.AndDoes(x => x.Arg<BencodeReader>().SkipBytes(length));
        }

        internal static ConfiguredCall AndSkipsAheadAsync(this ConfiguredCall call, int length)
        {
            return call.AndDoes(async x => await x.Arg<PipeBencodeReader>().SkipBytesAsync(length));
        }

        internal static async ValueTask<IBObject> ParseStringAsync(this IBObjectParser parser, string bencodedString)
        {
            var bytes = Encoding.UTF8.GetBytes(bencodedString).AsMemory();
            var (reader, writer) = new Pipe();
            await writer.WriteAsync(bytes);
            writer.Complete();
            return await parser.ParseAsync(reader);
        }

        internal static async ValueTask<T> ParseStringAsync<T>(this IBObjectParser<T> parser, string bencodedString) where T : IBObject
        {
            var bytes = Encoding.UTF8.GetBytes(bencodedString).AsMemory();
            var (reader, writer) = new Pipe();
            await writer.WriteAsync(bytes);
            writer.Complete();
            return await parser.ParseAsync(reader);
        }

        internal static void Deconstruct(this Pipe pipe, out PipeReader reader, out PipeWriter writer)
        {
            reader = pipe.Reader;
            writer = pipe.Writer;
        }
    }
}
