using System;
using System.IO;
using BencodeNET.IO;
using BencodeNET.Objects;

namespace BencodeNET.Parsing
{
    public static class BObjectParserExtensions
    {
        /// <summary>
        /// Parses a bencoded string into an <see cref="IBObject"/>.
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="bencodedString">The bencoded string to parse.</param>
        /// <returns>The parsed object.</returns>
        public static IBObject ParseString(this IBObjectParser parser, string bencodedString)
        {
            using (var stream = bencodedString.AsStream(parser.Encoding))
            {
                return parser.Parse(stream);
            }
        }

        /// <summary>
        /// Parses a byte array into an <see cref="IBObject"/>.
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="bytes">The bytes to parse.</param>
        /// <returns>The parsed object.</returns>
        public static IBObject Parse(this IBObjectParser parser, byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                return parser.Parse(stream);
            }
        }

        /// <summary>
        /// Parses a bencoded stream into an <see cref="IBObject"/>.
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="stream">The bencoded stream to parse.</param>
        /// <returns>The parsed object.</returns>
        [Obsolete("Use Parse(Stream) or Parse(BencodeReader) instead.")]
        public static IBObject Parse(this IBObjectParser parser, BencodeStream stream)
            => parser.Parse(stream.InnerStream);

        /// <summary>
        /// Parses a bencoded string into an <see cref="IBObject"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="bencodedString">The bencoded string to parse.</param>
        /// <returns>The parsed object.</returns>
        public static T ParseString<T>(this IBObjectParser<T> parser, string bencodedString) where T : IBObject
        {
            using (var stream = bencodedString.AsStream(parser.Encoding))
            {
                return parser.Parse(stream);
            }
        }

        /// <summary>
        /// Parses a byte array into an <see cref="IBObject"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="bytes">The bytes to parse.</param>
        /// <returns>The parsed object.</returns>
        public static T Parse<T>(this IBObjectParser<T> parser, byte[] bytes) where T : IBObject
        {
            using (var stream = new MemoryStream(bytes))
            {
                return parser.Parse(stream);
            }
        }

        /// <summary>
        /// Parses a bencoded stream into an <see cref="IBObject"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="stream">The bencoded stream to parse.</param>
        /// <returns>The parsed object.</returns>
        [Obsolete("Use Parse(Stream) or Parse(BencodeReader) instead.")]
        public static T Parse<T>(this IBObjectParser<T> parser, BencodeStream stream) where T : IBObject
            => parser.Parse(stream.InnerStream);
    }
}
