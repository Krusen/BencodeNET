using System;
using System.Buffers;
using System.IO;

namespace BencodeNET.IO
{
    /// <summary>
    /// Reads bencode from a stream.
    /// </summary>
    public class BencodeReader : IDisposable
    {
        private const int DefaultBufferSize = 40960;

        private readonly byte[] _tinyBuffer = new byte[1];

        private readonly Stream _stream;
        private readonly int _chunkBufferSize;
        private readonly bool _leaveOpen;
        private readonly bool _supportsLength;

        private bool _hasPeeked;
        private char? _peekedChar;

        /// <summary>
        /// The previously read/consumed char (does not include peeked char).
        /// </summary>
        public char? PreviousChar { get; private set; }

        /// <summary>
        /// The position in the stream (does not included peeked char).
        /// </summary>
        public long Position => _hasPeeked ? _stream.Position - 1 : _stream.Position;

        /// <summary>
        /// The length of the stream, or <c>null</c> if the stream doesn't support the feature.
        /// </summary>
        public long? Length => _supportsLength ? _stream.Length : (long?) null;

        /// <summary>
        /// Returns true if the end of the stream has been reached.
        /// This is true if either <see cref="Position"/> is greater than <see cref="Length"/> or if next char is <c>null</c>.
        /// </summary>
        public bool EndOfStream => Position > Length || PeekChar() == null;

        /// <summary>
        /// Creates a new <see cref="BencodeReader"/> for the specified <see cref="Stream"/>
        /// using the default buffer size of 40,960 bytes.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        public BencodeReader(Stream stream)
            : this(stream, DefaultBufferSize)
        {
        }

        /// <summary>
        /// Creates a new <see cref="BencodeReader"/> for the specified <see cref="Stream"/>
        /// using the specified <paramref name="bufferSize"/>.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="bufferSize">The buffer size used when reading more than a single character from the stream. </param>
        public BencodeReader(Stream stream, int bufferSize)
            : this(stream, bufferSize, leaveOpen: false)
        {

        }

        /// <summary>
        /// Creates a new <see cref="BencodeReader"/> for the specified <see cref="Stream"/>
        /// using the default buffer size of 40,960 bytes and the option of leaving the stream open after disposing of this instance.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="leaveOpen">Indicates if the stream should be left open when this <see cref="BencodeReader"/> is disposed.</param>
        public BencodeReader(Stream stream, bool leaveOpen)
            : this(stream, DefaultBufferSize, leaveOpen)
        {

        }

        /// <summary>
        /// Creates a new <see cref="BencodeReader"/> for the specified <see cref="Stream"/>
        /// using the specified <paramref name="bufferSize"/> and the option of leaving the stream open after disposing of this instance.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="bufferSize">The buffer size used when reading more than a single character from the stream. </param>
        /// <param name="leaveOpen">Indicates if the stream should be left open when this <see cref="BencodeReader"/> is disposed.</param>
        public BencodeReader(Stream stream, int bufferSize, bool leaveOpen)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _chunkBufferSize = bufferSize;
            _leaveOpen = leaveOpen;
            try
            {
                _ = stream.Length;
                _supportsLength = true;
            }
            catch
            {
                _supportsLength = false;
            }

            if (!_stream.CanRead) throw new ArgumentException("The stream is not readable.", nameof(stream));
        }

        /// <summary>
        /// Peeks at the next character in the stream, or <c>null</c> if the end of the stream has been reached.
        /// </summary>
        public char? PeekChar()
        {
            if (_hasPeeked)
                return _peekedChar;

            var read = _stream.Read(_tinyBuffer, 0, 1);

            _peekedChar = read == 0 ? null : (char?)_tinyBuffer[0];
            _hasPeeked = true;

            return _peekedChar;
        }

        /// <summary>
        /// Reads the next character from the stream.
        /// Returns <c>null</c> if the end of the stream has been reached.
        /// </summary>
        public char? ReadChar()
        {
            if (_hasPeeked)
            {
                _hasPeeked = _peekedChar == null; // If null then EOS so don't reset peek as peeking again will just be EOS again
                return _peekedChar;
            }

            var read = _stream.Read(_tinyBuffer, 0, 1);

            PreviousChar = read == 0
                ? null
                : (char?) _tinyBuffer[0];

            return PreviousChar;
        }

        // TODO: Create version using Span<byte>?
        /// <summary>
        /// Reads into the <paramref name="buffer"/> by reading from the stream.
        /// Returns the number of bytes actually read from the stream.
        /// </summary>
        /// <param name="buffer">The buffer to read into.</param>
        /// <returns>The number of bytes actually read from the stream and filled into the buffer.</returns>
        public int Read(byte[] buffer)
        {
            var index = 0;
            var length = buffer.Length;
            if (_hasPeeked && _peekedChar != null)
            {
                buffer[0] = (byte) _peekedChar.Value;
                index = 1;
                length--;
                _hasPeeked = false;

                // Just return right away if only reading this 1 byte
                if (buffer.Length == 1)
                {
                    return 1;
                }
            }

            using (var ms = new MemoryStream(buffer, index, length))
            {
                var totalRead = index;
                var chunkBuffer = ArrayPool<byte>.Shared.Rent(_chunkBufferSize);
                try
                {

                    int count;
                    while ((count = _stream.Read(chunkBuffer, 0, Math.Min(chunkBuffer.Length, buffer.Length - totalRead))) != 0)
                    {
                        totalRead += count;
                        ms.Write(chunkBuffer, 0, count);
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(chunkBuffer);
                }

                if (totalRead > 0)
                {
                    PreviousChar = (char)buffer[totalRead - 1];
                }

                return totalRead;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <inheritdoc cref="Dispose()"/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            if (_stream != null && !_leaveOpen)
                _stream.Dispose();
        }
    }
}
