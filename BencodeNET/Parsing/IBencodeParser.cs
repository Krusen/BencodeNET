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
        /// Parses a stream into an <see cref="IBObject"/>.
        /// </summary>
        /// <param name="stream">The stream to parse.</param>
        /// <returns>The parsed object.</returns>
        IBObject Parse(Stream stream);

        /// <summary>
        /// Parses a stream into an <see cref="IBObject"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IBObject"/> to parse as.</typeparam>
        /// <param name="stream">The bencoded string to parse.</param>
        /// <returns>The parsed object.</returns>
        T Parse<T>(Stream stream) where T : class, IBObject;

        /// <summary>
        ///  Parses an <see cref="IBObject"/> from the reader.
        /// </summary>
        /// <param name="reader"></param>
        IBObject Parse(BencodeReader reader);

        /// <summary>
        /// Parse an <see cref="IBObject"/> of type <typeparamref name="T"/> from the reader.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IBObject"/> to parse as.</typeparam>
        /// <param name="reader"></param>
        T Parse<T>(BencodeReader reader) where T : class, IBObject;
    }
}