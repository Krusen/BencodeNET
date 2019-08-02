using System.IO;
using BencodeNET.Objects;

namespace BencodeNET.Parsing
{
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
            using (var stream = bencodedString.AsStream(parser.Encoding))
            {
                return parser.Parse(stream);
            }
        }

        /// <summary>
        /// Parses a bencoded array of bytes into an <see cref="IBObject"/>.
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="bytes">The bencoded bytes to parse.</param>
        /// <returns>The parsed object.</returns>
        public static IBObject Parse(this IBencodeParser parser, byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                return parser.Parse(stream);
            }
        }

        /// <summary>
        /// Parses a bencoded file into an <see cref="IBObject"/>.
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="filePath">The path to the file to parse.</param>
        /// <returns>The parsed object.</returns>
        public static IBObject Parse(this IBencodeParser parser, string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            {
                return parser.Parse(stream);
            }
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
            using (var stream = bencodedString.AsStream(parser.Encoding))
            {
                return parser.Parse<T>(stream);
            }
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
            using (var stream = new MemoryStream(bytes))
            {
                return parser.Parse<T>(stream);
            }
        }

        /// <summary>
        /// Parses a bencoded file into an <see cref="IBObject"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="filePath">The path to the file to parse.</param>
        /// <returns>The parsed object.</returns>
        public static T Parse<T>(this IBencodeParser parser, string filePath) where T : class, IBObject
        {
            using (var stream = File.OpenRead(filePath))
            {
                return parser.Parse<T>(stream);
            }
        }

#if !NETSTANDARD1_3
        /// <summary>
        /// Parses a stream through a <see cref="BufferedStream"/> into an <see cref="IBObject"/>.
        /// The input <paramref name="stream"/> is disposed when parsing is completed.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IBObject"/> to parse as.</typeparam>
        /// <param name="parser"></param>
        /// <param name="stream">The stream to parse.</param>
        /// <param name="bufferSize">The buffer size to use. Uses default size of <see cref="BufferedStream"/> if null.</param>
        /// <returns>The parsed object.</returns>
        public static IBObject ParseBuffered(this IBencodeParser parser, Stream stream, int? bufferSize = null)
        {
;
            using (var bufferedStream = bufferSize == null
                ? new BufferedStream(stream)
                : new BufferedStream(stream, bufferSize.Value))
            {
                return parser.Parse(bufferedStream);
            }
        }

        /// <summary>
        /// Parses a stream through a <see cref="BufferedStream"/> into an <see cref="IBObject"/> of type <typeparamref name="T"/>.
        /// The input <paramref name="stream"/> is disposed when parsing is completed.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IBObject"/> to parse as.</typeparam>
        /// <param name="parser"></param>
        /// <param name="stream">The stream to parse.</param>
        /// <param name="bufferSize">The buffer size to use. Uses default size of <see cref="BufferedStream"/> if null.</param>
        /// <returns>The parsed object.</returns>
        public static T ParseBuffered<T>(this IBencodeParser parser, Stream stream, int? bufferSize = null) where T : class, IBObject
        {
            using (var bufferedStream = bufferSize == null
                ? new BufferedStream(stream)
                : new BufferedStream(stream, bufferSize.Value))
            {
                return parser.Parse<T>(bufferedStream);
            }
        }
#endif
    }
}
