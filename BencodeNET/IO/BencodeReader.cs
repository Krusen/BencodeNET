using System;
using System.IO;

namespace BencodeNET.IO
{
    /// <summary>
    /// Reads bencode from a stream.
    /// </summary>
    public class BencodeReader : IDisposable
    {
        private readonly byte[] _tinyBuffer = new byte[1];

        private readonly Stream _stream;
        private readonly bool _leaveOpen;
        private readonly bool _supportsLength;

        private bool _hasPeeked;
        private char _peekedChar;

        /// <summary>
        /// The previously read/consumed char (does not include peeked char).
        /// </summary>
        public char PreviousChar { get; private set; }

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
        /// This is true if either <see cref="Position"/> is greater than <see cref="Length"/> or if next char is <c>default(char)</c>.
        /// </summary>
        public bool EndOfStream => Position > Length || PeekChar() == default;

        /// <summary>
        /// Creates a new <see cref="BencodeReader"/> for the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        public BencodeReader(Stream stream)
            : this(stream, leaveOpen: false)
        {
        }

        /// <summary>
        /// Creates a new <see cref="BencodeReader"/> for the specified <see cref="Stream"/>
        /// using the default buffer size of 40,960 bytes and the option of leaving the stream open after disposing of this instance.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="leaveOpen">Indicates if the stream should be left open when this <see cref="BencodeReader"/> is disposed.</param>
        public BencodeReader(Stream stream, bool leaveOpen)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
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
        /// Peeks at the next character in the stream, or <c>default(char)</c> if the end of the stream has been reached.
        /// </summary>
        public char PeekChar()
        {
            if (_hasPeeked)
                return _peekedChar;

            var read = _stream.Read(_tinyBuffer, 0, 1);

            _peekedChar = read == 0 ? default : (char)_tinyBuffer[0];
            _hasPeeked = true;

            return _peekedChar;
        }

        /// <summary>
        /// Reads the next character from the stream.
        /// Returns <c>default(char)</c> if the end of the stream has been reached.
        /// </summary>
        public char ReadChar()
        {
            if (_hasPeeked)
            {
                _hasPeeked = _peekedChar == default; // If null then EOS so don't reset peek as peeking again will just be EOS again
                return _peekedChar;
            }

            var read = _stream.Read(_tinyBuffer, 0, 1);

            PreviousChar = read == 0
                ? default
                : (char) _tinyBuffer[0];

            return PreviousChar;
        }

        /// <summary>
        /// Reads into the <paramref name="buffer"/> by reading from the stream.
        /// Returns the number of bytes actually read from the stream.
        /// </summary>
        /// <param name="buffer">The buffer to read into.</param>
        /// <returns>The number of bytes actually read from the stream and filled into the buffer.</returns>
        public int Read(byte[] buffer)
        {
            var totalRead = 0;
            if (_hasPeeked && _peekedChar != default)
            {
                buffer[0] = (byte) _peekedChar;
                totalRead = 1;
                _hasPeeked = false;

                // Just return right away if only reading this 1 byte
                if (buffer.Length == 1)
                {
                    return 1;
                }
            }

            int read = -1;
            while (read != 0 && totalRead < buffer.Length)
            {
                read = _stream.Read(buffer, totalRead, buffer.Length - totalRead);
                totalRead += read;
            }

            if (totalRead > 0)
                PreviousChar = (char) buffer[totalRead - 1];

            return totalRead;
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
