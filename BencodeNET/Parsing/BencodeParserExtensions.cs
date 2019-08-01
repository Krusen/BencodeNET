using System;
using System.IO;
using BencodeNET.IO;
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
        /// Parses a <see cref="BencodeStream"/> into an <see cref="IBObject"/>.
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="stream">The stream to parse.</param>
        /// <returns>The parsed object.</returns>
        [Obsolete("Use Parse(Stream) or Parse(BencodeReader) instead.")]
        public static IBObject Parse(this IBencodeParser parser, BencodeStream stream) => parser.Parse(stream.InnerStream);

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
        /// Parses a <see cref="BencodeStream"/> into an <see cref="IBObject"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IBObject"/> to parse as.</typeparam>
        /// <param name="parser"></param>
        /// <param name="stream">The stream to parse.</param>
        /// <returns>The parsed object.</returns>
        [Obsolete("Use Parse(Stream) or Parse(BencodeReader) instead.")]
        public static T Parse<T>(this IBencodeParser parser, BencodeStream stream) where T : class, IBObject
            => parser.Parse<T>(stream.InnerStream);

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
    }
}
