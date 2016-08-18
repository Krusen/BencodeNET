using System.IO;
using System.Text;
using System.Threading.Tasks;
using BencodeNET.IO;

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
        /// Writes the object as bencode to the specified stream.
        /// </summary>
        /// <typeparam name="TStream">The type of stream.</typeparam>
        /// <param name="stream">The stream to write to.</param>
        /// <returns>The used stream.</returns>
        TStream EncodeToStream<TStream>(TStream stream) where TStream : Stream;

        /// <summary>
        /// Writes the object as bencode to the specified stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <returns>The used stream.</returns>
        BencodeStream EncodeToStream(BencodeStream stream);

        /// <summary>
        /// Asynchronously writes the object as bencode to the specified stream.
        /// </summary>
        /// <typeparam name="TStream">The type of stream.</typeparam>
        /// <param name="stream">The stream to write to.</param>
        /// <returns>The used stream.</returns>
        Task<TStream> EncodeToStreamAsync<TStream>(TStream stream) where TStream : Stream;

        /// <summary>
        /// Asynchronously writes the object as bencode to the specified stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <returns>The used stream.</returns>
        Task<BencodeStream> EncodeToStreamAsync(BencodeStream stream);

        /// <summary>
        /// Writes the object as bencode to the specified file path.
        /// </summary>
        /// <param name="path">The file path to write the encoded object to.</param>
        void EncodeToFile(string path);

        /// <summary>
        /// Asynchronously writes the object as bencode to the specified file path.
        /// </summary>
        /// <param name="path">The file path to write the encoded object to.</param>
        /// <returns></returns>
        Task EncodeToFileAsync(string path);
    }
}
