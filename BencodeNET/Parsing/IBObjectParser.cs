using System;
using System.IO;
using BencodeNET.IO;
using BencodeNET.Objects;

namespace BencodeNET.Parsing
{
    /// <summary>
    /// A contract for parsing bencode from different sources as an <see cref="IBObject"/>.
    /// </summary>
    public interface IBObjectParser
    {
        /// <summary>
        /// Parses a bencoded string into an <see cref="IBObject"/>.
        /// </summary>
        /// <param name="bencodedString">The bencoded string to parse.</param>
        /// <returns>The parsed object.</returns>
        IBObject ParseString(string bencodedString);

        /// <summary>
        /// Parses a byte array into an <see cref="IBObject"/>.
        /// </summary>
        /// <param name="bytes">The bytes to parse.</param>
        /// <returns>The parsed object.</returns>
        IBObject Parse(byte[] bytes);

        /// <summary>
        /// Parses a stream into an <see cref="IBObject"/>.
        /// </summary>
        /// <param name="stream">The stream to parse.</param>
        /// <returns>The parsed object.</returns>
        IBObject Parse(Stream stream);

        /// <summary>
        /// Parses a bencoded stream into an <see cref="IBObject"/>.
        /// </summary>
        /// <param name="stream">The bencoded stream to parse.</param>
        /// <returns>The parsed object.</returns>
        [Obsolete("Use Parse(Stream) or Parse(BencodeReader) instead.")]
        IBObject Parse(BencodeStream stream);

        IBObject Parse(BencodeReader reader);

    }

    /// <summary>
    /// A contract for parsing bencode from different sources as type <typeparamref name="T"/> inheriting <see cref="IBObject"/>.
    /// </summary>
    public interface IBObjectParser<out T> : IBObjectParser where T : IBObject
    {
        /// <summary>
        /// Parses a bencoded string into an <see cref="IBObject"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="bencodedString">The bencoded string to parse.</param>
        /// <returns>The parsed object.</returns>
        new T ParseString(string bencodedString);

        /// <summary>
        /// Parses a byte array into an <see cref="IBObject"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="bytes">The bytes to parse.</param>
        /// <returns>The parsed object.</returns>
        new T Parse(byte[] bytes);

        /// <summary>
        /// Parses a stream into an <see cref="IBObject"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="stream">The stream to parse.</param>
        /// <returns>The parsed object.</returns>
        new T Parse(Stream stream);

        /// <summary>
        /// Parses a bencoded stream into an <see cref="IBObject"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="stream">The bencoded stream to parse.</param>
        /// <returns>The parsed object.</returns>
        [Obsolete("Use Parse(Stream) or Parse(BencodeReader) instead.")]
        new T Parse(BencodeStream stream);

        new T Parse(BencodeReader reader);
    }
}
