using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BencodeNET.Objects
{
    /// <summary>
    /// Represent a bencode value that can be encoded to bencode.
    /// </summary>
    public interface IBObject
    {
        /// <summary>
        /// Encodes the object and returns the result as a string using <see cref="Encoding.UTF8"/>.
        /// </summary>
        /// <returns>
        /// The object bencoded and converted to a string using <see cref="Encoding.UTF8"/>.
        /// </returns>
        string Encode();

        /// <summary>
        /// Encodes the object and returns the result as a string using the specified encoding.
        /// </summary>
        /// <param name="encoding">The encoding used to convert the encoded bytes to a string.</param>
        /// <returns>
        /// The object bencoded and converted to a string using the specified encoding.
        /// </returns>
        string Encode(Encoding encoding);

        /// <summary>
        /// Encodes the object to the specified stream and returns a reference to the stream.
        /// </summary>
        /// <typeparam name="TStream">The type of stream.</typeparam>
        /// <param name="stream">The stream to encode the object to.</param>
        /// <returns>The supplied stream.</returns>
        T EncodeToStream<T>(T stream) where T : Stream;

        /// <summary>
        /// Encodes the object to the specified stream and returns a reference to the stream.
        /// </summary>
        /// <typeparam name="TStream">The type of stream.</typeparam>
        /// <param name="stream">The stream to encode the object to.</param>
        /// <returns>The supplied stream.</returns>
        Task<T> EncodeToStreamAsync<T>(T stream) where T : Stream;

        /// <summary>
        /// Encodes the object to the specified file path.
        /// </summary>
        /// <param name="path">The file path to write the encoded object to.</param>
        void EncodeToFile(string path);

        /// <summary>
        /// Encodes the object asynchronously to the specified file path.
        /// </summary>
        /// <param name="path">The file path to write the encoded object to.</param>
        /// <returns></returns>
        Task EncodeToFileAsync(string path);
    }
}
