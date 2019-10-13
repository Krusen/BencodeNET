using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

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
        /// Writes the object as bencode to the specified <see cref="PipeWriter"/> without flushing the writer,
        /// you should do that manually.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        public void EncodeTo(PipeWriter writer)
        {
            EncodeObject(writer);
        }

        /// <summary>
        /// Writes the object as bencode to the specified <see cref="PipeWriter"/> and flushes the writer afterwards.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="cancellationToken"></param>
        public ValueTask<FlushResult> EncodeToAsync(PipeWriter writer, CancellationToken cancellationToken = default)
        {
            return EncodeObjectAsync(writer, cancellationToken);
        }

        /// <summary>
        /// Writes the object asynchronously as bencode to the specified <see cref="Stream"/> using a <see cref="PipeWriter"/>.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="writerOptions">The options for the <see cref="PipeWriter"/>.</param>
        /// <param name="cancellationToken"></param>
        public ValueTask<FlushResult> EncodeToAsync(Stream stream, StreamPipeWriterOptions writerOptions = null, CancellationToken cancellationToken = default)
        {
            return EncodeObjectAsync(PipeWriter.Create(stream, writerOptions), cancellationToken);
        }

        /// <summary>
        /// Implementations of this method should encode their
        /// underlying value to bencode and write it to the stream.
        /// </summary>
        /// <param name="stream">The stream to encode to.</param>
        protected abstract void EncodeObject(Stream stream);

        /// <summary>
        /// Implementations of this method should encode their underlying value to bencode and write it to the <see cref="PipeWriter"/>.
        /// </summary>
        /// <param name="writer">The writer to encode to.</param>
        protected abstract void EncodeObject(PipeWriter writer);

        /// <summary>
        /// Encodes and writes the underlying value to the <see cref="PipeWriter"/> and flushes the writer afterwards.
        /// </summary>
        /// <param name="writer">The writer to encode to.</param>
        /// <param name="cancellationToken"></param>
        protected virtual ValueTask<FlushResult> EncodeObjectAsync(PipeWriter writer, CancellationToken cancellationToken)
        {
            EncodeObject(writer);
            return writer.FlushAsync(cancellationToken);
        }
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
