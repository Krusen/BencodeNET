using System.IO;
using System.Text;
using System.Threading.Tasks;
using BencodeNET.IO;

namespace BencodeNET.Objects
{
    /// <summary>
    /// Abstract base class with default implementation of most methods of <see cref="IBObject"/>.
    /// </summary>
    public abstract class BObject : IBObject
    {
        internal BObject()
        { }

        /// <summary>
        /// Encodes the object and returns the result as a string using <see cref="Encoding.UTF8"/>.
        /// </summary>
        /// <returns>
        /// The object bencoded and converted to a string using <see cref="Encoding.UTF8"/>.
        /// </returns>
        public virtual string EncodeAsString()
        {
            return EncodeAsString(Encoding.UTF8);
        }

        /// <summary>
        /// Encodes the object and returns the result as a string using the specified encoding.
        /// </summary>
        /// <param name="encoding">The encoding used to convert the encoded bytes to a string.</param>
        /// <returns>
        /// The object bencoded and converted to a string using the specified encoding.
        /// </returns>
        public virtual string EncodeAsString(Encoding encoding)
        {
            using (var stream = EncodeTo(new MemoryStream()))
            {
                return encoding.GetString(stream.ToArray());
            }
        }

        /// <summary>
        /// Writes the object as bencode to the specified stream.
        /// </summary>
        /// <typeparam name="TStream">The type of stream.</typeparam>
        /// <param name="stream">The stream to write to.</param>
        /// <returns>The used stream.</returns>
        public TStream EncodeTo<TStream>(TStream stream) where TStream : Stream
        {
            EncodeObject(new BencodeStream(stream));
            return stream;
        }

        /// <summary>
        /// Asynchronously writes the object as bencode to the specified stream.
        /// </summary>
        /// <typeparam name="TStream">The type of stream.</typeparam>
        /// <param name="stream">The stream to write to.</param>
        /// <returns>The used stream.</returns>
        public async Task<TStream> EncodeToAsync<TStream>(TStream stream) where TStream : Stream
        {
            await EncodeObjectAsync(new BencodeStream(stream));
            return stream;
        }

        /// <summary>
        /// Writes the object as bencode to the specified stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <returns>The used stream.</returns>
        public BencodeStream EncodeTo(BencodeStream stream)
        {
            EncodeObject(stream);
            return stream;
        }

        /// <summary>
        /// Asynchronously writes the object as bencode to the specified stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <returns>The used stream.</returns>
        public async Task<BencodeStream> EncodeToAsync(BencodeStream stream)
        {
            await EncodeObjectAsync(stream);
            return stream;
        }

        /// <summary>
        /// Writes the object as bencode to the specified file path.
        /// </summary>
        /// <param name="filePath">The file path to write the encoded object to.</param>
        public virtual void EncodeTo(string filePath)
        {
            using (var stream = File.OpenWrite(filePath))
            {
                EncodeTo(stream);
            }
        }

        /// <summary>
        /// Asynchronously writes the object as bencode to the specified file path.
        /// </summary>
        /// <param name="filePath">The file path to write the encoded object to.</param>
        /// <returns></returns>
        public virtual Task EncodeToAsync(string filePath)
        {
            using (var stream = File.OpenWrite(filePath))
            {
                return EncodeToAsync(stream);
            }
        }

        protected abstract void EncodeObject(BencodeStream stream);

        protected abstract Task EncodeObjectAsync(BencodeStream stream);
    }

    /// <summary>
    /// Base class of bencode objects with a specific underlying value type.
    /// </summary>
    /// <typeparam name="T">Type of the underlying value.</typeparam>
    public abstract class BObject<T> : BObject
    {
        internal BObject()
        { }

        /// <summary>
        /// The underlying value of the <see cref="BObject{T}"/>.
        /// </summary>
        public abstract T Value { get; }
    }
}
