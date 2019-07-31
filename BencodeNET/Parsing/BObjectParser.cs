using System;
using System.IO;
using System.Text;
using BencodeNET.IO;
using BencodeNET.Objects;

namespace BencodeNET.Parsing
{
    /// <summary>
    /// Abstract base parser for parsing bencode of specific types.
    /// </summary>
    /// <typeparam name="T">The type of bencode object the parser returns.</typeparam>
    public abstract class BObjectParser<T> : IBObjectParser<T> where T : IBObject
    {
        /// <summary>
        /// The encoding used for parsing.
        /// </summary>
        protected abstract Encoding Encoding { get; }

        /// <summary>
        /// Parses a bencoded string into an <see cref="IBObject"/>.
        /// </summary>
        /// <param name="bencodedString">The bencoded string to parse.</param>
        /// <returns>The parsed object.</returns>
        IBObject IBObjectParser.ParseString(string bencodedString) => ParseString(bencodedString);

        /// <summary>
        /// Parses a byte array into an <see cref="IBObject"/>.
        /// </summary>
        /// <param name="bytes">The bytes to parse.</param>
        /// <returns>The parsed object.</returns>
        IBObject IBObjectParser.Parse(byte[] bytes) => Parse(bytes);

        /// <summary>
        /// Parses a stream into an <see cref="IBObject"/>.
        /// </summary>
        /// <param name="stream">The stream to parse.</param>
        /// <returns>The parsed object.</returns>
        IBObject IBObjectParser.Parse(Stream stream) => Parse(stream);

        /// <summary>
        /// Parses a bencoded stream into an <see cref="IBObject"/>.
        /// </summary>
        /// <param name="stream">The bencoded stream to parse.</param>
        /// <returns>The parsed object.</returns>
        [Obsolete("Use Parse(Stream) or Parse(BencodeReader) instead.")]
        IBObject IBObjectParser.Parse(BencodeStream stream) => Parse(stream.InnerStream);

        IBObject IBObjectParser.Parse(BencodeReader reader) => Parse(reader);

        /// <summary>
        /// Parses a bencoded string into an <see cref="IBObject"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="bencodedString">The bencoded string to parse.</param>
        /// <returns>The parsed object.</returns>
        public virtual T ParseString(string bencodedString)
        {
            using (var stream = bencodedString.AsStream(Encoding))
            {
                return Parse(stream);
            }
        }

        /// <summary>
        /// Parses a byte array into an <see cref="IBObject"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="bytes">The bytes to parse.</param>
        /// <returns>The parsed object.</returns>
        public virtual T Parse(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                return Parse(stream);
            }
        }

        /// <summary>
        /// Parses a stream into an <see cref="IBObject"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="stream">The stream to parse.</param>
        /// <returns>The parsed object.</returns>
        public virtual T Parse(Stream stream)
        {
            return Parse(new BencodeReader(stream));
        }

        /// <summary>
        /// Parses a bencoded stream into an <see cref="IBObject"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="stream">The bencoded stream to parse.</param>
        /// <returns>The parsed object.</returns>
        [Obsolete("Use Parse(Stream) or Parse(BencodeReader) instead.")]
        public virtual T Parse(BencodeStream stream) => Parse(stream.InnerStream);

        // TODO: Make protected?
        public abstract T Parse(BencodeReader reader);
    }
}