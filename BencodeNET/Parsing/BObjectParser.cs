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
    /// Abstract base parser for parsing bencode of specific types.
    /// </summary>
    /// <typeparam name="T">The type of bencode object the parser returns.</typeparam>
    public abstract class BObjectParser<T> : IBObjectParser<T> where T : IBObject
    {
        /// <summary>
        /// The encoding used for parsing.
        /// </summary>
        public abstract Encoding Encoding { get; }

        IBObject IBObjectParser.Parse(Stream stream)
        {
            return Parse(stream);
        }

        IBObject IBObjectParser.Parse(BencodeReader reader)
        {
            return Parse(reader);
        }

        async ValueTask<IBObject> IBObjectParser.ParseAsync(PipeReader pipeReader, CancellationToken cancellationToken)
        {
            return await ParseAsync(new PipeBencodeReader(pipeReader), cancellationToken).ConfigureAwait(false);
        }

        async ValueTask<IBObject> IBObjectParser.ParseAsync(PipeBencodeReader pipeReader, CancellationToken cancellationToken)
        {
            return await ParseAsync(pipeReader, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Parses a stream into an <see cref="IBObject"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="stream">The stream to parse.</param>
        /// <returns>The parsed object.</returns>
        public virtual T Parse(Stream stream) => Parse(new BencodeReader(stream, leaveOpen: true));

        public abstract T Parse(BencodeReader reader);

        public ValueTask<T> ParseAsync(PipeReader pipeReader, CancellationToken cancellationToken = default)
            => ParseAsync(new PipeBencodeReader(pipeReader), cancellationToken);

        public abstract ValueTask<T> ParseAsync(PipeBencodeReader pipeReader, CancellationToken cancellationToken = default);
    }
}