using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BencodeNET.IO
{
    /// <summary>
    /// A wrapper for <see cref="Stream"/> that makes it easier and faster to work
    /// with bencode  and to read/write one byte at a time. Also has methods for peeking
    /// at the next byte (caching the read) and for reading the previous byte in stream.
    /// </summary>
    public class BencodeStream : IDisposable
    {
        private static readonly byte[] EmptyByteArray = new byte[0];

        private bool _hasPeeked;
        private int _peekedByte;

        public BencodeStream(string str) : this(str, Encoding.UTF8)
        { }

        public BencodeStream(string str, Encoding encoding) : this(encoding.GetBytes(str))
        { }

        /// <summary>
        /// Creates a new <see cref="BencodeStream"/> from the specified bytes
        /// using a <see cref="MemoryStream"/> as the <see cref="InnerStream"/>.
        /// </summary>
        /// <param name="bytes"></param>
        public BencodeStream(byte[] bytes) : this(new MemoryStream(bytes), false)
        { }

        /// <summary>
        /// Creates a new <see cref="BencodeStream"/> using the specified stream.
        /// </summary>
        /// <param name="stream">The underlying stream to use.</param>
        /// <param name="leaveOpen">Indicates if the specified stream should be left open when this <see cref="BencodeStream"/> is disposed.</param>
        public BencodeStream(Stream stream, bool leaveOpen = false)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            InnerStream = stream;
            LeaveOpen = leaveOpen;
        }

        /// <summary>
        /// The inner stream that this <see cref="BencodeStream"/> is working with.
        /// </summary>
        public Stream InnerStream { get; protected set; }

        /// <summary>
        /// If true <see cref="InnerStream"/> will not be disposed when this <see cref="BencodeStream"/> is disposed.
        /// </summary>
        public bool LeaveOpen { get; }

        /// <summary>
        /// Gets the lenght in bytes of the stream.
        /// </summary>
        public long Length => InnerStream.Length;

        /// <summary>
        /// Gets or sets the position within the stream.
        /// </summary>
        public long Position
        {
            get { return InnerStream.Position; }
            set
            {
                _hasPeeked = false;
                InnerStream.Position = value;
            }
        }

        /// <summary>
        /// Indicates if the current position is at or after the end of the stream.
        /// </summary>
        public bool EndOfStream => Position >= Length;

        /// <summary>
        /// Sets the position within the stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter.</param>
        /// <param name="origin">A value indicating the reference point used to obtain the new position.</param>
        /// <returns></returns>
        public long Seek(long offset, SeekOrigin origin)
        {
            _hasPeeked = false;
            return InnerStream.Seek(offset, origin);
        }

        #region Read

        // TODO: Documentation - this is cheap to call several times, only reads the first time until next Read()
        public int Peek()
        {
            if (_hasPeeked)
                return _peekedByte;

            _peekedByte = InnerStream.ReadByte();
            _hasPeeked = true;

            // Only seek backwards if not at end of stream
            if (_peekedByte > -1)
                InnerStream.Seek(-1, SeekOrigin.Current);

            return _peekedByte;
        }

        public char PeekChar()
        {
            if (Peek() == -1)
                return default(char);
            return (char) Peek();
        }

        public int Read()
        {
            if (!_hasPeeked)
                return InnerStream.ReadByte();

            if (_peekedByte == -1)
                return _peekedByte;

            _hasPeeked = false;
            InnerStream.Seek(1, SeekOrigin.Current);
            return _peekedByte;
        }

        public char ReadChar()
        {
            var value = Read();
            if (value == -1)
                return default(char);
            return (char) value;
        }

        public byte[] Read(int bytesToRead)
        {
            if (bytesToRead < 0) throw new ArgumentOutOfRangeException(nameof(bytesToRead));
            if (bytesToRead == 0) return EmptyByteArray;

            var bytes = new byte[bytesToRead];

            var offset = 0;

            if (_hasPeeked)
            {
                if (_peekedByte == -1)
                    return EmptyByteArray;

                bytes[0] = (byte)_peekedByte;
                offset = 1;
            }

            _hasPeeked = false;

            if (offset > 0)
                InnerStream.Seek(offset, SeekOrigin.Current);

            var readBytes = InnerStream.Read(bytes, offset, bytesToRead-offset) + offset;
            if (readBytes != bytesToRead)
                Array.Resize(ref bytes, readBytes);

            return bytes;
        }

        public int ReadPrevious()
        {
            if (InnerStream.Position == 0)
                return -1;

            _hasPeeked = false;

            InnerStream.Position -= 1;

            var bytes = new byte[1];

            var readBytes = InnerStream.Read(bytes, 0, 1);
            if (readBytes == 0)
                return -1;

            return bytes[0];
        }

        public char ReadPreviousChar()
        {
            var value = ReadPrevious();
            if (value == -1)
                return default(char);
            return (char)value;
        }

        public async Task<int> PeekAsync()
        {
            if (_hasPeeked)
                return _peekedByte;

            _peekedByte = await InnerStream.ReadByteAsync().ConfigureAwait(false);
            _hasPeeked = true;

            // Only seek backwards if not at end of stream
            if (_peekedByte > -1)
                InnerStream.Seek(-1, SeekOrigin.Current);

            return _peekedByte;
        }

        public async Task<char> PeekCharAsync()
        {
            return (char)await PeekAsync().ConfigureAwait(false);
        }

        public Task<int> ReadAsync()
        {
            if (!_hasPeeked)
                return InnerStream.ReadByteAsync();

            if (_peekedByte == -1)
                return Task.FromResult(_peekedByte);

            _hasPeeked = false;
            InnerStream.Seek(1, SeekOrigin.Current);

            return Task.FromResult(_peekedByte);
        }

        public async Task<char> ReadCharAsync()
        {
            var value = await ReadAsync().ConfigureAwait(false);
            if (value == -1)
                return default(char);
            return (char)value;
        }

        public async Task<byte[]> ReadAsync(int bytesToRead)
        {
            if (bytesToRead < 0) throw new ArgumentOutOfRangeException(nameof(bytesToRead));
            if (bytesToRead == 0) return EmptyByteArray;

            var bytes = new byte[bytesToRead];

            var offset = 0;

            if (_hasPeeked)
            {
                if (_peekedByte == -1)
                    return EmptyByteArray;

                bytes[0] = (byte)_peekedByte;
                offset = 1;
            }

            _hasPeeked = false;

            if (offset > 0)
                InnerStream.Seek(offset, SeekOrigin.Current);

            var readBytes = offset + await InnerStream.ReadAsync(bytes, offset, bytesToRead - offset).ConfigureAwait(false);
            if (readBytes != bytesToRead)
                Array.Resize(ref bytes, readBytes);

            return bytes;
        }

        public async Task<int> ReadPreviousAsync()
        {
            if (InnerStream.Position == 0)
                return -1;

            _hasPeeked = false;

            InnerStream.Position -= 1;

            var bytes = new byte[1];

            var readBytes = await InnerStream.ReadAsync(bytes, 0, 1).ConfigureAwait(false);
            if (readBytes == 0)
                return -1;

            return bytes[0];
        }

        public async Task<char> ReadPreviousCharAsync()
        {
            var value = await ReadPreviousAsync().ConfigureAwait(false);
            if (value == -1)
                return default(char);
            return (char)value;
        }

        #endregion

        #region Write

        public void Write(int number)
        {
            var bytes = Encoding.ASCII.GetBytes(number.ToString());
            Write(bytes);
        }

        public void Write(long number)
        {
            var bytes = Encoding.ASCII.GetBytes(number.ToString());
            Write(bytes);
        }

        public void Write(char c)
        {
            InnerStream.Write(c);
        }

        public void Write(byte[] bytes)
        {
            InnerStream.Write(bytes, 0, bytes.Length);
        }

        public Task WriteAsync(int number)
        {
            var bytes = Encoding.ASCII.GetBytes(number.ToString());
            return WriteAsync(bytes);
        }

        public Task WriteAsync(long number)
        {
            var bytes = Encoding.ASCII.GetBytes(number.ToString());
            return WriteAsync(bytes);
        }

        public Task WriteAsync(char c)
        {
            return InnerStream.WriteAsync(c);
        }

        public Task WriteAsync(byte[] bytes)
        {
            return InnerStream.WriteAsync(bytes, 0, bytes.Length);
        }

        #endregion

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written.
        /// </summary>
        public void Flush()
        {
            InnerStream.Flush();
        }

        /// <summary>
        /// Releases all resources used by the <see cref="BencodeStream"/>.
        /// <see cref="InnerStream"/> is also disposed unless <see cref="LeaveOpen"/> is true.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Disposes of <see cref="InnerStream"/> unless <see cref="LeaveOpen"/> is true.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (InnerStream != null && !LeaveOpen)
                    InnerStream.Dispose();
                InnerStream = null;
            }
        }

#pragma warning disable 1591
        public static implicit operator BencodeStream(Stream stream)
        {
            return new BencodeStream(stream);
        }
#pragma warning restore 1591
    }
}
