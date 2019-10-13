using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace BencodeNET.IO
{
    /// <summary>
    /// Reads chars and bytes from a <see cref="PipeReader"/>.
    /// </summary>
    public class PipeBencodeReader
    {
        /// <summary>
        /// The <see cref="PipeReader"/> to read from.
        /// </summary>
        protected PipeReader Reader { get; }

        /// <summary>
        /// Indicates if the <see cref="PipeReader"/> has been completed (i.e. "end of stream").
        /// </summary>
        protected bool ReaderCompleted { get; set; }

        /// <summary>
        /// The position in the pipe (number of read bytes/characters) (does not included peeked char).
        /// </summary>
        public virtual long Position { get; protected set; }

        /// <summary>
        /// The previously read/consumed char (does not include peeked char).
        /// </summary>
        public virtual char PreviousChar { get; protected set; }

        /// <summary>
        /// Creates a <see cref="PipeBencodeReader"/> that reads from the specified <see cref="PipeReader"/>.
        /// </summary>
        /// <param name="reader"></param>
        public PipeBencodeReader(PipeReader reader)
        {
            Reader = reader;
        }

        /// <summary>
        /// Peek at the next char in the pipe, without advancing the reader.
        /// </summary>
        public virtual ValueTask<char> PeekCharAsync(CancellationToken cancellationToken = default)
            => ReadCharAsync(peek: true, cancellationToken);

        /// <summary>
        /// Read the next char in the pipe and advance the reader.
        /// </summary>
        public virtual ValueTask<char> ReadCharAsync(CancellationToken cancellationToken = default)
            => ReadCharAsync(peek: false, cancellationToken);

        private ValueTask<char> ReadCharAsync(bool peek = false, CancellationToken cancellationToken = default)
        {
            if (ReaderCompleted)
                return new ValueTask<char>(default(char));

            if (Reader.TryRead(out var result))
                return new ValueTask<char>(ReadCharConsume(result.Buffer, peek));

            return ReadCharAwaitedAsync(peek, cancellationToken);
        }

        private async ValueTask<char> ReadCharAwaitedAsync(bool peek, CancellationToken cancellationToken)
        {
            var result = await Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
            return ReadCharConsume(result.Buffer, peek);
        }

        /// <summary>
        /// Reads the next char in the pipe and consumes it (advances the reader),
        /// unless <paramref name="peek"/> is <c>true</c>.
        /// </summary>
        /// <param name="buffer">The buffer to read from</param>
        /// <param name="peek">If true the char will not be consumed, i.e. the reader should not be advanced.</param>
        protected virtual char ReadCharConsume(in ReadOnlySequence<byte> buffer, bool peek)
        {
            if (buffer.IsEmpty)
            {
                // TODO: Add IsCompleted check?
                ReaderCompleted = true;
                return default;
            }

            var c = (char) buffer.First.Span[0];

            if (peek)
            {
                // Advance reader to start (i.e. don't advance)
                Reader.AdvanceTo(buffer.Start);
                return c;
            }

            // Consume char by advancing reader
            Position++;
            PreviousChar = c;
            Reader.AdvanceTo(buffer.GetPosition(1));
            return c;
        }

        /// <summary>
        /// Read bytes from the pipe.
        /// Returns the number of bytes actually read.
        /// </summary>
        /// <param name="bytes">The amount of bytes to read.</param>
        /// <param name="cancellationToken"></param>
        public virtual ValueTask<long> ReadAsync(Memory<byte> bytes, CancellationToken cancellationToken = default)
        {
            if (bytes.Length == 0 || ReaderCompleted)
                return new ValueTask<long>(0);

            if (Reader.TryRead(out var result) && TryReadConsume(result, bytes.Span, out var bytesRead))
            {
                return new ValueTask<long>(bytesRead);
            }

            return ReadAwaitedAsync(bytes, cancellationToken);
        }

        private async ValueTask<long> ReadAwaitedAsync(Memory<byte> bytes, CancellationToken cancellationToken)
        {
            while (true)
            {
                var result = await Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                if (TryReadConsume(result, bytes.Span, out var bytesRead))
                {
                    return bytesRead;
                }
            }
        }

        /// <summary>
        /// Attempts to read the specified bytes from the reader and advances the reader if successful.
        /// If the end of the pipe is reached then the available bytes is read and returned, if any.
        /// <para>
        /// Returns <c>true</c> if any bytes was read or the reader was completed.
        /// </para>
        /// </summary>
        /// <param name="result">The read result from the pipe read operation.</param>
        /// <param name="bytes">The bytes to read.</param>
        /// <param name="bytesRead">The number of bytes read.</param>
        /// <returns></returns>
        protected virtual bool TryReadConsume(ReadResult result, in Span<byte> bytes, out long bytesRead)
        {
            if (result.IsCanceled) throw new InvalidOperationException("Read operation cancelled.");

            var buffer = result.Buffer;

            // Check if enough bytes have been read
            if (buffer.Length >= bytes.Length)
            {
                // Copy requested amount of bytes from buffer and advance reader
                buffer.Slice(0, bytes.Length).CopyTo(bytes);
                Position += bytes.Length;
                PreviousChar = (char) bytes[bytes.Length - 1];
                bytesRead = bytes.Length;
                Reader.AdvanceTo(buffer.GetPosition(bytes.Length));
                return true;
            }

            if (result.IsCompleted)
            {
                ReaderCompleted = true;

                if (buffer.IsEmpty)
                {
                    bytesRead = 0;
                    return true;
                }

                // End of pipe reached, less bytes available than requested
                // Copy available bytes and advance reader to the end
                buffer.CopyTo(bytes);
                Position += buffer.Length;
                PreviousChar = (char) buffer.Slice(buffer.Length - 1).First.Span[0];
                bytesRead = buffer.Length;
                Reader.AdvanceTo(buffer.End);
                return true;
            }

            // Not enough bytes read, advance reader
            Reader.AdvanceTo(buffer.Start, buffer.End);

            bytesRead = -1;
            return false; // Consume unsuccessful
        }
    }
}