using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using BencodeNET.IO;
using BencodeNET.Objects;

namespace BencodeNET.Parsing
{
    /// <summary>
    /// Extensions to simplify parsing strings, byte arrays or files directly.
    /// </summary>
    public static class BencodeParserExtensions
    {
        /// <summary>
        /// Parses a bencoded string into an <see cref="IBObject"/>.
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="bencodedString">The bencoded string to parse.</param>
        /// <returns>The parsed object.</returns>
        public static IBObject ParseString(this IBencodeParser parser, string bencodedString)
        {
            using var stream = bencodedString.AsStream(parser.Encoding);
            return parser.Parse(stream);
        }

        /// <summary>
        /// Parses a bencoded array of bytes into an <see cref="IBObject"/>.
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="bytes">The bencoded bytes to parse.</param>
        /// <returns>The parsed object.</returns>
        public static IBObject Parse(this IBencodeParser parser, byte[] bytes)
        {
            using var stream = new MemoryStream(bytes);
            return parser.Parse(stream);
        }

        /// <summary>
        /// Parses a bencoded file into an <see cref="IBObject"/>.
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="filePath">The path to the file to parse.</param>
        /// <returns>The parsed object.</returns>
        public static IBObject Parse(this IBencodeParser parser, string filePath)
        {
            using var stream = File.OpenRead(filePath);
            return parser.Parse(stream);
        }

        /// <summary>
        /// Parses a bencoded string into an <see cref="IBObject"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IBObject"/> to parse as.</typeparam>
        /// <param name="parser"></param>
        /// <param name="bencodedString">The bencoded string to parse.</param>
        /// <returns>The parsed object.</returns>
        public static T ParseString<T>(this IBencodeParser parser, string bencodedString) where T : class, IBObject
        {
            using var stream = bencodedString.AsStream(parser.Encoding);
            return parser.Parse<T>(stream);
        }

        /// <summary>
        /// Parses a bencoded array of bytes into an <see cref="IBObject"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IBObject"/> to parse as.</typeparam>
        /// <param name="parser"></param>
        /// <param name="bytes">The bencoded bytes to parse.</param>
        /// <returns>The parsed object.</returns>
        public static T Parse<T>(this IBencodeParser parser, byte[] bytes) where T : class, IBObject
        {
            using var stream = new MemoryStream(bytes);
            return parser.Parse<T>(stream);
        }

        /// <summary>
        /// Parses a bencoded file into an <see cref="IBObject"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="filePath">The path to the file to parse.</param>
        /// <returns>The parsed object.</returns>
        public static T Parse<T>(this IBencodeParser parser, string filePath) where T : class, IBObject
        {
            using var stream = File.OpenRead(filePath);
            return parser.Parse<T>(stream);
        }

        /// <summary>
        /// Parses a stream into an <see cref="IBObject"/>.
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="stream">The stream to parse.</param>
        /// <returns>The parsed object.</returns>
        public static IBObject Parse(this IBencodeParser parser, Stream stream)
        {
            using var reader = new BencodeReader(stream, leaveOpen: true);
            return parser.Parse(reader);
        }

        /// <summary>
        /// Parses a stream into an <see cref="IBObject"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IBObject"/> to parse as.</typeparam>
        /// <param name="parser"></param>
        /// <param name="stream">The stream to parse.</param>
        /// <returns>The parsed object.</returns>
        public static T Parse<T>(this IBencodeParser parser, Stream stream) where T : class, IBObject
        {
            using var reader = new BencodeReader(stream, leaveOpen: true);
            return parser.Parse<T>(reader);
        }

        /// <summary>
        /// Parses an <see cref="IBObject"/> from the <see cref="PipeReader"/>.
        /// </summary>
        public static ValueTask<IBObject> ParseAsync(this IBencodeParser parser, PipeReader pipeReader, CancellationToken cancellationToken = default)
        {
            var reader = new PipeBencodeReader(pipeReader);
            return parser.ParseAsync(reader, cancellationToken);
        }

        /// <summary>
        /// Parses an <see cref="IBObject"/> of type <typeparamref name="T"/> from the <see cref="PipeReader"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IBObject"/> to parse as.</typeparam>
        public static ValueTask<T> ParseAsync<T>(this IBencodeParser parser, PipeReader pipeReader, CancellationToken cancellationToken = default) where T : class, IBObject
        {
            var reader = new PipeBencodeReader(pipeReader);
            return parser.ParseAsync<T>(reader, cancellationToken);
        }

        /// <summary>
        /// Parses an <see cref="IBObject"/> from the <see cref="Stream"/> asynchronously using a <see cref="PipeReader"/>.
        /// </summary>
        public static ValueTask<IBObject> ParseAsync(this IBencodeParser parser, Stream stream, StreamPipeReaderOptions readerOptions = null, CancellationToken cancellationToken = default)
        {
            var reader = PipeReader.Create(stream, readerOptions);
            return parser.ParseAsync(reader, cancellationToken);
        }

        /// <summary>
        /// Parses an <see cref="IBObject"/>  of type <typeparamref name="T"/> from the <see cref="Stream"/> asynchronously using a <see cref="PipeReader"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IBObject"/> to parse as.</typeparam>
        public static ValueTask<T> ParseAsync<T>(this IBencodeParser parser, Stream stream, StreamPipeReaderOptions readerOptions = null, CancellationToken cancellationToken = default) where T : class, IBObject
        {
            var reader = PipeReader.Create(stream, readerOptions);
            return parser.ParseAsync<T>(reader, cancellationToken);
        }
    }
}
