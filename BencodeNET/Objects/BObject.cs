using System.IO;

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
        /// Calculates the (encoded) size of the object in bytes.
        /// </summary>
        public abstract int GetSizeInBytes();

        /// <summary>
        /// Writes the object as bencode to the specified stream.
        /// </summary>
        /// <typeparam name="TStream">The type of stream.</typeparam>
        /// <param name="stream">The stream to write to.</param>
        /// <returns>The used stream.</returns>
        public TStream EncodeTo<TStream>(TStream stream) where TStream : Stream
        {
            var size = GetSizeInBytes();
            stream.TrySetLength(size);
            EncodeObject(stream);
            return stream;
        }

        /// <summary>
        /// Implementations of this method should encode their
        /// underlying value to bencode and write it to the stream.
        /// </summary>
        /// <param name="stream">The stream to encode to.</param>
        protected abstract void EncodeObject(Stream stream);
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
