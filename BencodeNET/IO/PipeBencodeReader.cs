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
        private readonly PipeReader _reader;

        private bool _endOfStream;

        /// <summary>
        /// The position in the pipe (number of read bytes/characters) (does not included peeked char).
        /// </summary>
        public long Position { get; set; }

        /// <summary>
        /// The previously read/consumed char (does not include peeked char).
        /// </summary>
        public char PreviousChar { get; protected set; }

        /// <summary>
        /// Creates a <see cref="PipeBencodeReader"/> that reads from the specified <see cref="PipeReader"/>.
        /// </summary>
        /// <param name="reader"></param>
        public PipeBencodeReader(PipeReader reader)
        {
            _reader = reader;
        }

        /// <summary>
        /// Peek at the next char in the pipe, without advancing the reader.
        /// </summary>
        public ValueTask<char> PeekCharAsync(CancellationToken cancellationToken = default)
            => ReadCharAsync(peek: true, cancellationToken);

        /// <summary>
        /// Read the next char in the pipe and advance the reader.
        /// </summary>
        public ValueTask<char> ReadCharAsync(CancellationToken cancellationToken = default)
            => ReadCharAsync(peek: false, cancellationToken);

        private ValueTask<char> ReadCharAsync(bool peek = false, CancellationToken cancellationToken = default)
        {
            if (_endOfStream)
                return new ValueTask<char>(default(char));

            if (_reader.TryRead(out var result))
                return new ValueTask<char>(ReadCharConsume(result.Buffer, peek));

            return ReadCharAsyncAwaited(peek, cancellationToken);
        }

        private async ValueTask<char> ReadCharAsyncAwaited(bool peek, CancellationToken cancellationToken)
        {
            var result = await _reader.ReadAsync(cancellationToken).ConfigureAwait(false);
            return ReadCharConsume(result.Buffer, peek);
        }

        /// <summary>
        /// Reads the next char in the pipe and consumes it (advances the reader),
        /// unless <paramref name="peek"/> is <c>true</c>.
        /// </summary>
        private char ReadCharConsume(in ReadOnlySequence<byte> buffer, bool peek)
        {
            if (buffer.IsEmpty)
            {
                _endOfStream = true;
                return default;
            }

            var c = (char) buffer.First.Span[0];

            if (peek)
            {
                // Advance reader to start (i.e. don't advance)
                _reader.AdvanceTo(buffer.Start);
                return c;
            }

            // Consume char by advancing reader
            Position++;
            PreviousChar = c;
            _reader.AdvanceTo(buffer.GetPosition(1));
            return c;
        }

        /// <summary>
        /// Read bytes from the pipe.
        /// Returns the number of bytes actually read.
        /// </summary>
        /// <param name="bytes">The amount of bytes to read.</param>
        /// <param name="cancellationToken"></param>
        public ValueTask<long> ReadAsync(Memory<byte> bytes, CancellationToken cancellationToken = default)
        {
            if (bytes.Length == 0 || _endOfStream)
                return new ValueTask<long>(0);

            if (_reader.TryRead(out var result) && TryReadConsume(result, bytes.Span, out var bytesRead))
            {
                return new ValueTask<long>(bytesRead);
            }

            return ReadAsyncAwaited(bytes, cancellationToken);
        }

        private async ValueTask<long> ReadAsyncAwaited(Memory<byte> bytes, CancellationToken cancellationToken)
        {
            while (true)
            {
                var result = await _reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                if (TryReadConsume(result, bytes.Span, out var bytesRead))
                {
                    return bytesRead;
                }
            }
        }

        private bool TryReadConsume(ReadResult result, Span<byte> bytes, out long bytesRead)
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
                _reader.AdvanceTo(buffer.GetPosition(bytes.Length));
                return true;
            }

            if (result.IsCompleted)
            {
                _endOfStream = true;

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
                _reader.AdvanceTo(buffer.End);
                return true;
            }

            // Not enough bytes read, advance reader
            _reader.AdvanceTo(buffer.Start, buffer.End);

            bytesRead = -1;
            return false; // Consume unsuccessful
        }
    }
}