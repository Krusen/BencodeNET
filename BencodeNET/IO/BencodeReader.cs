using System;
using System.Buffers;
using System.IO;

namespace BencodeNET.IO
{
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

        public char? PreviousChar { get; private set; }

        public long Position => _hasPeeked ? _stream.Position - 1 : _stream.Position;

        public long? Length => _supportsLength ? _stream.Length : (long?) null;

        public bool EndOfStream => Position > Length || PeekChar() == null;

        public BencodeReader(Stream stream)
            : this(stream, DefaultBufferSize)
        {
        }

        public BencodeReader(Stream stream, int bufferSize)
            : this(stream, bufferSize, leaveOpen: false)
        {

        }

        public BencodeReader(Stream stream, bool leaveOpen)
            : this(stream, DefaultBufferSize, leaveOpen)
        {

        }

        public BencodeReader(Stream stream, int bufferSize,  bool leaveOpen)
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

        public char? PeekChar()
        {
            if (_hasPeeked)
                return _peekedChar;

            var read = _stream.Read(_tinyBuffer, 0, 1);

            _peekedChar = read == 0 ? null : (char?)_tinyBuffer[0];
            _hasPeeked = true;

            return _peekedChar;
        }

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

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            if (_stream != null && !_leaveOpen)
                _stream.Dispose();
        }
    }
}
