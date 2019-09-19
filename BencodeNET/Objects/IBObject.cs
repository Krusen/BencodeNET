using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace BencodeNET.Objects
{
    /// <summary>
    /// Represent a bencode value that can be encoded to bencode.
    /// </summary>
    public interface IBObject
    {
        /// <summary>
        /// Calculates the (encoded) size of the object in bytes.
        /// </summary>
        int GetSizeInBytes();

        /// <summary>
        /// Writes the object as bencode to the specified stream.
        /// </summary>
        /// <typeparam name="TStream">The type of stream.</typeparam>
        /// <param name="stream">The stream to write to.</param>
        /// <returns>The used stream.</returns>
        TStream EncodeTo<TStream>(TStream stream) where TStream : Stream;

        /// <summary>
        /// Writes the object as bencode to the specified <see cref="PipeWriter"/>.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="cancellationToken"></param>
        ValueTask<FlushResult> EncodeToAsync(PipeWriter writer, CancellationToken cancellationToken = default);
    }
}
