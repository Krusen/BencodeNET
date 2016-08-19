using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BencodeNET.IO
{
    public class BencodeStream : IDisposable
    {
        private static readonly byte[] _emptyByteArray = new byte[0];

        private bool _hasPeeked;
        private int _peekedByte;

        public BencodeStream(string str) : this(str, Encoding.UTF8)
        { }

        public BencodeStream(string str, Encoding encoding) : this(encoding.GetBytes(str))
        { }

        public BencodeStream(byte[] bytes) : this(new MemoryStream(bytes), false)
        { }

        public BencodeStream(Stream stream) : this(stream, false)
        { }

        public BencodeStream(Stream stream, bool leaveOpen)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            InnerStream = stream;
            LeaveOpen = leaveOpen;
        }

        public Stream InnerStream { get; protected set; }

        public bool LeaveOpen { get; }

        public long Length => InnerStream.Length;

        public long Position
        {
            get { return InnerStream.Position; }
            set
            {
                _hasPeeked = false;
                InnerStream.Position = value;
            }
        }

        public bool EndOfStream => Position >= Length;

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
            if (bytesToRead == 0) return _emptyByteArray;

            var bytes = new byte[bytesToRead];

            var offset = 0;

            if (_hasPeeked)
            {
                if (_peekedByte == -1)
                    return _emptyByteArray;

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
            if (bytesToRead == 0) return _emptyByteArray;

            var bytes = new byte[bytesToRead];

            var offset = 0;

            if (_hasPeeked)
            {
                if (_peekedByte == -1)
                    return _emptyByteArray;

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

        public void Flush()
        {
            InnerStream.Flush();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (InnerStream != null && !LeaveOpen)
                    InnerStream.Dispose();
                InnerStream = null;
            }
        }

        public static implicit operator BencodeStream(Stream stream)
        {
            return new BencodeStream(stream);
        }
    }
}
