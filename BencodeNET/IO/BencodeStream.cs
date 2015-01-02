using System;
using System.IO;
using System.Text;

namespace BencodeNET.IO
{
    public class BencodeStream : IDisposable
    {
        private readonly byte[] _emptyByteArray = new byte[0];

        private readonly bool _leaveOpen;

        private bool _hasPeeked;
        private int _peekedByte;

        public Stream BaseStream { get; protected set; }

        public bool EndOfStream { get { return Position >= Length; } }

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

            BaseStream = stream;
            _leaveOpen = leaveOpen;
        }

        public int Peek()
        {
            if (_hasPeeked)
                return _peekedByte;

            _peekedByte = Read();
            _hasPeeked = true;
            Position -= 1;
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
            if (_hasPeeked)
            {
                if (_peekedByte == -1)
                    return _peekedByte;

                _hasPeeked = false;
                Position += 1;
                return _peekedByte;
            }

            var bytes = new byte[1];

            var readBytes = BaseStream.Read(bytes, 0, 1);
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

            if (_hasPeeked)
            {
                if (_peekedByte == -1)
                    return _emptyByteArray;

                _hasPeeked = false;
            }

            var readBytes = BaseStream.Read(bytes, 0, bytesToRead);
            if (readBytes != bytesToRead)
            {
                Array.Resize(ref bytes, readBytes);
            }

            return bytes;
        }

        public int ReadPrevious()
        {
            if (Position == 0)
                return -1;

            Position -= 1;

            var bytes = new byte[1];

            var readBytes = BaseStream.Read(bytes, 0, 1);
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
            BaseStream.WriteByte((byte)c);
        }

        public void Write(byte[] bytes)
        {
            BaseStream.Write(bytes, 0, bytes.Length);
        }

        public void Flush()
        {
            BaseStream.Flush();   
        }

        public long Seek(long offset, SeekOrigin origin)
        {
            return BaseStream.Seek(offset, origin);
        }

        public bool CanRead
        {
            get { return BaseStream.CanRead; }
        }

        public bool CanSeek
        {
            get { return BaseStream.CanSeek; }
        }

        public bool CanWrite
        {
            get { return BaseStream.CanWrite; }
        }

        public long Length
        {
            get { return BaseStream.Length; }
        }

        public long Position
        {
            get { return BaseStream.Position; }
            set { BaseStream.Position = value; }
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
                if (BaseStream != null && !_leaveOpen)
                    BaseStream.Close();
                BaseStream = null;
            }
        }
    }
}
