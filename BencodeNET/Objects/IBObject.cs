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
        string EncodeAsString();

        /// <summary>
        /// Encodes the object and returns the result as a string using the specified encoding.
        /// </summary>
        /// <param name="encoding">The encoding used to convert the encoded bytes to a string.</param>
        /// <returns>
        /// The object bencoded and converted to a string using the specified encoding.
        /// </returns>
        string EncodeAsString(Encoding encoding);

        /// <summary>
        /// Encodes the object and returns the raw bytes.
        /// </summary>
        /// <returns>The raw bytes of the bencoded object.</returns>
        byte[] EncodeAsBytes();

        /// <summary>
        /// Writes the object as bencode to the specified stream.
        /// </summary>
        /// <typeparam name="TStream">The type of stream.</typeparam>
        /// <param name="stream">The stream to write to.</param>
        /// <returns>The used stream.</returns>
        TStream EncodeTo<TStream>(TStream stream) where TStream : Stream;

        /// <summary>
        /// Asynchronously writes the object as bencode to the specified stream.
        /// </summary>
        /// <typeparam name="TStream">The type of stream.</typeparam>
        /// <param name="stream">The stream to write to.</param>
        /// <returns>The used stream.</returns>
        Task<TStream> EncodeToAsync<TStream>(TStream stream) where TStream : Stream;

        /// <summary>
        /// Writes the object as bencode to the specified stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <returns>The used stream.</returns>
        BencodeStream EncodeTo(BencodeStream stream);

        /// <summary>
        /// Asynchronously writes the object as bencode to the specified stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <returns>The used stream.</returns>
        Task<BencodeStream> EncodeToAsync(BencodeStream stream);

        /// <summary>
        /// Writes the object as bencode to the specified file.
        /// </summary>
        /// <param name="filePath">The file path to write the encoded object to.</param>
        void EncodeTo(string filePath);

        /// <summary>
        /// Asynchronously writes the object as bencode to the specified file.
        /// </summary>
        /// <param name="filePath">The file path to write the encoded object to.</param>
        /// <returns></returns>
        Task EncodeToAsync(string filePath);
    }
}
