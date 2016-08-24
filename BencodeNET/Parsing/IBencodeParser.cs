using System.IO;
using System.Text;
using BencodeNET.IO;
using BencodeNET.Objects;

namespace BencodeNET.Parsing
{
    /// <summary>
    /// Represents a parser capable of parsing bencode.
    /// </summary>
    public interface IBencodeParser
    {
        /// <summary>
        /// The encoding use for parsing.
        /// </summary>
        Encoding Encoding { get; }

        /// <summary>
        /// Parses a bencoded string into an <see cref="IBObject"/>.
        /// </summary>
        /// <param name="bencodedString">The bencoded string to parse.</param>
        /// <returns>The parsed object.</returns>
        IBObject ParseString(string bencodedString);

        /// <summary>
        /// Parses a bencoded array of bytes into an <see cref="IBObject"/>.
        /// </summary>
        /// <param name="bytes">The bencoded bytes to parse.</param>
        /// <returns>The parsed object.</returns>
        IBObject Parse(byte[] bytes);

        /// <summary>
        /// Parses a stream into an <see cref="IBObject"/>.
        /// </summary>
        /// <param name="stream">The stream to parse.</param>
        /// <returns>The parsed object.</returns>
        IBObject Parse(Stream stream);

        /// <summary>
        /// Parses a <see cref="BencodeStream"/> into an <see cref="IBObject"/>.
        /// </summary>
        /// <param name="stream">The stream to parse.</param>
        /// <returns>The parsed object.</returns>
        IBObject Parse(BencodeStream stream);

        /// <summary>
        /// Parses a bencoded file into an <see cref="IBObject"/>.
        /// </summary>
        /// <param name="filePath">The path to the file to parse.</param>
        /// <returns>The parsed object.</returns>
        IBObject Parse(string filePath);

        /// <summary>
        /// Parses a bencoded string into an <see cref="IBObject"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IBObject"/> to parse as.</typeparam>
        /// <param name="bencodedString">The bencoded string to parse.</param>
        /// <returns>The parsed object.</returns>
        T ParseString<T>(string bencodedString) where T : class, IBObject;

        /// <summary>
        /// Parses a bencoded array of bytes into an <see cref="IBObject"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IBObject"/> to parse as.</typeparam>
        /// <param name="bytes">The bencoded bytes to parse.</param>
        /// <returns>The parsed object.</returns>
        T Parse<T>(byte[] bytes) where T : class, IBObject;

        /// <summary>
        /// Parses a stream into an <see cref="IBObject"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IBObject"/> to parse as.</typeparam>
        /// <param name="stream">The bencoded string to parse.</param>
        /// <returns>The parsed object.</returns>
        T Parse<T>(Stream stream) where T : class, IBObject;

        /// <summary>
        /// Parses a <see cref="BencodeStream"/> into an <see cref="IBObject"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IBObject"/> to parse as.</typeparam>
        /// <param name="stream">The bencoded string to parse.</param>
        /// <returns>The parsed object.</returns>
        T Parse<T>(BencodeStream stream) where T : class, IBObject;

        /// <summary>
        /// Parses a bencoded file into an <see cref="IBObject"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="filePath">The path to the file to parse.</param>
        /// <returns>The parsed object.</returns>
        T Parse<T>(string filePath) where T : class, IBObject;
    }
}