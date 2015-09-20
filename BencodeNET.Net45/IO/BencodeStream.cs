using System;
using System.IO;
using System.Text;

namespace BencodeNET.IO
{
    public class BencodeStream : IDisposable
    {
        private readonly byte[] _emptyByteArray = new byte[0];

        private Stream _stream;
        private readonly bool _leaveOpen;

        private bool _hasPeeked;
        private long _peekPosition;
        private int _peekedByte;

        public Stream BaseStream { get { return _stream; } }

        public bool EndOfStream { get { return Position >= Length; } }

        private bool HasValidPeek
        {
            get { return _hasPeeked && _peekPosition == _stream.Position; }
        }

        public BencodeStream(string str) : this(Bencode.DefaultEncoding.GetBytes(str))
        { }

        public BencodeStream(string str, Encoding encoding) : this(encoding.GetBytes(str))
        { }

        public BencodeStream(byte[] bytes) : this(new MemoryStream(bytes), false)
        { }

        public BencodeStream(Stream stream) : this(stream, false)
        { }

        public BencodeStream(Stream stream, bool leaveOpen)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            _stream = stream;
            _leaveOpen = leaveOpen;
        }

        public int Peek()
        {
            if (HasValidPeek)
                return _peekedByte;

            _peekedByte = Read();
            _stream.Position -= 1;
            _peekPosition = _stream.Position;
            _hasPeeked = true;
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
            if (HasValidPeek)
            {
                if (_peekedByte == -1)
                    return _peekedByte;

                _hasPeeked = false;
                _stream.Position += 1;
                return _peekedByte;
            }

            var bytes = new byte[1];

            var readBytes = _stream.Read(bytes, 0, 1);
            if (readBytes == 0)
                return -1;

            return bytes[0];
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
            if (bytesToRead < 0) throw new ArgumentOutOfRangeException("bytesToRead");
            if (bytesToRead == 0) return _emptyByteArray;

            var bytes = new byte[bytesToRead];

            if (HasValidPeek)
            {
                if (_peekedByte == -1)
                    return _emptyByteArray;

                _hasPeeked = false;
            }

            var readBytes = _stream.Read(bytes, 0, bytesToRead);
            if (readBytes != bytesToRead)
                Array.Resize(ref bytes, readBytes);

            return bytes;
        }

        public int ReadPrevious()
        {
            if (_stream.Position == 0)
                return -1;

            _stream.Position -= 1;

            var bytes = new byte[1];

            var readBytes = _stream.Read(bytes, 0, 1);
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
            _stream.WriteByte((byte)c);
        }

        public void Write(byte[] bytes)
        {
            _stream.Write(bytes, 0, bytes.Length);
        }

        public void Flush()
        {
            _stream.Flush();
        }

        public long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public bool CanRead
        {
            get { return _stream.CanRead; }
        }

        public bool CanSeek
        {
            get { return _stream.CanSeek; }
        }

        public bool CanWrite
        {
            get { return _stream.CanWrite; }
        }

        public long Length
        {
            get { return _stream.Length; }
        }

        public long Position
        {
            get { return _stream.Position; }
            set { _stream.Position = value; }
        }

        public virtual void Close()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_stream != null && !_leaveOpen)
                    _stream.Close();
                _stream = null;
            }
        }
    }
}
