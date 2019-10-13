using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        /// The encoding used for parsing.
        /// </summary>
        Encoding Encoding { get; }

        /// <summary>
        /// Parses a stream into an <see cref="IBObject"/>.
        /// </summary>
        /// <param name="stream">The stream to parse.</param>
        /// <returns>The parsed object.</returns>
        IBObject Parse(Stream stream);

        /// <summary>
        /// Parses an <see cref="IBObject"/> from a <see cref="BencodeReader"/>.
        /// </summary>
        IBObject Parse(BencodeReader reader);

        /// <summary>
        /// Parses an <see cref="IBObject"/> from a <see cref="PipeReader"/>.
        /// </summary>
        /// <param name="pipeReader">The pipe reader to read from.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The parsed object.</returns>
        ValueTask<IBObject> ParseAsync(PipeReader pipeReader, CancellationToken cancellationToken = default);

        /// <summary>
        /// Parses an <see cref="IBObject"/> from a <see cref="PipeBencodeReader"/>.
        /// </summary>
        /// <param name="pipeReader">The pipe reader to read from.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The parsed object.</returns>
        ValueTask<IBObject> ParseAsync(PipeBencodeReader pipeReader, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// A contract for parsing bencode from different sources as type <typeparamref name="T"/> inheriting <see cref="IBObject"/>.
    /// </summary>
    public interface IBObjectParser<T> : IBObjectParser where T : IBObject
    {
        /// <summary>
        /// Parses a stream into an <see cref="IBObject"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="stream">The stream to parse.</param>
        /// <returns>The parsed object.</returns>
        new T Parse(Stream stream);

        /// <summary>
        /// Parses an <see cref="IBObject"/> of type <typeparamref name="T"/> from a <see cref="BencodeReader"/>.
        /// </summary>
        new T Parse(BencodeReader reader);

        /// <summary>
        /// Parses an <see cref="IBObject"/> of type <typeparamref name="T"/> from a <see cref="PipeReader"/>.
        /// </summary>
        /// <param name="pipeReader">The pipe reader to read from.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The parsed object.</returns>
        new ValueTask<T> ParseAsync(PipeReader pipeReader, CancellationToken cancellationToken = default);

        /// <summary>
        /// Parses an <see cref="IBObject"/> of type <typeparamref name="T"/> from a <see cref="PipeBencodeReader"/>.
        /// </summary>
        /// <param name="pipeReader">The pipe reader to read from.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The parsed object.</returns>
        new ValueTask<T> ParseAsync(PipeBencodeReader pipeReader, CancellationToken cancellationToken = default);
    }
}
