using System;
using System.Buffers;
using System.IO;
using System.Text;

namespace BencodeNET.IO
{
    /// <summary>
    /// A wrapper for <see cref="Stream"/> that makes it easier and faster to work
    /// with bencode and to read/write one byte at a time. Also has methods for peeking
    /// at the next byte (caching the read) and for reading the previous byte in stream.
    /// </summary>
    public class BencodeStream : IDisposable
    {
        private static readonly byte[] EmptyByteArray = new byte[0];

        private readonly byte[] _singleByteBuffer = new byte[1];

        private bool _hasPeeked;
        private int _peekedByte;

        /// <summary>
        /// Creates a new <see cref="BencodeStream"/> by converting the string
        /// to bytes using <see cref="Encoding.UTF8"/> and storing them in a <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="str"></param>
        public BencodeStream(string str)
            : this(str, Encoding.UTF8)
        { }

        /// <summary>
        /// Creates a new <see cref="BencodeStream"/> by converting the string
        /// to bytes using the specified encoding and storing them in a <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="encoding"></param>
        public BencodeStream(string str, Encoding encoding)
            : this(encoding.GetBytes(str))
        { }

        /// <summary>
        /// Creates a new <see cref="BencodeStream"/> from the specified bytes
        /// using a <see cref="MemoryStream"/> as the <see cref="InnerStream"/>.
        /// </summary>
        /// <param name="bytes"></param>
        public BencodeStream(byte[] bytes)
            : this(new MemoryStream(bytes))
        { }

        /// <summary>
        /// Creates a new <see cref="BencodeStream"/> using the specified stream.
        /// </summary>
        /// <param name="stream">The underlying stream to use.</param>
        /// <param name="leaveOpen">Indicates if the specified stream should be left open when this <see cref="BencodeStream"/> is disposed.</param>
        public BencodeStream(Stream stream, bool leaveOpen = false)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (!stream.CanSeek) throw new ArgumentException("Only seekable streams are supported.", nameof(stream));

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
        /// Gets the length of the stream in bytes.
        /// </summary>
        public long Length => InnerStream.Length;

        /// <summary>
        /// Gets or sets the position within the stream.
        /// </summary>
        public long Position
        {
            get => InnerStream.Position;
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

        /// <summary>
        /// Reads the next byte in the stream without advancing the position.
        /// This can safely be called multiple times as the read byte is cached until the position
        /// in the stream is changed or a read operation is performed.
        /// </summary>
        /// <returns>The next byte in the stream.</returns>
        public int Peek()
        {
            if (_hasPeeked)
                return _peekedByte;

            var position = InnerStream.Position;
            _peekedByte = InnerStream.ReadByte();
            _hasPeeked = true;
            InnerStream.Position = position;

            return _peekedByte;
        }

        /// <summary>
        /// Reads the next char in the stream without advancing the position.
        /// This can safely be called multiple times as the read char is cached until the position
        /// in the stream is changed or a read operation is performed.
        /// </summary>
        /// <returns>The next char in the stream.</returns>
        public char PeekChar()
        {
            if (Peek() == -1)
                return default(char);
            return (char) Peek();
        }

        /// <summary>
        /// Reads the next byte in the stream.
        /// If a <see cref="Peek"/> or a <see cref="PeekChar"/> has been performed
        /// the peeked value is returned and the position is incremented by 1.
        /// </summary>
        /// <returns>The next býte in the stream.</returns>
        public int Read()
        {
            if (!_hasPeeked)
                return InnerStream.ReadByte();

            if (_peekedByte == -1)
                return _peekedByte;

            _hasPeeked = false;
            InnerStream.Position += 1;
            return _peekedByte;
        }

        /// <summary>
        /// Reads the next char in the stream.
        /// If a <see cref="Peek"/> or a <see cref="PeekChar"/> has been performed
        /// the peeked value is returned and the position is incremented by 1.
        /// </summary>
        /// <returns>The next char in the stream.</returns>
        public char ReadChar()
        {
            var value = Read();
            if (value == -1)
                return default(char);
            return (char) value;
        }

        /// <summary>
        /// Reads the specified amount of bytes from the stream.
        /// </summary>
        /// <param name="bytesToRead">The number of bytes to read.</param>
        /// <returns>The read bytes.</returns>
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
                InnerStream.Position += offset;

            var readBytes = InnerStream.Read(bytes, offset, bytesToRead-offset) + offset;
            if (readBytes != bytesToRead)
                Array.Resize(ref bytes, readBytes);

            return bytes;
        }

        /// <summary>
        /// Reads the previous byte in the stream and decrements the position by 1.
        /// </summary>
        /// <returns>The previous byte in stream.</returns>
        public int ReadPrevious()
        {
            if (InnerStream.Position == 0)
                return -1;

            _hasPeeked = false;

            InnerStream.Position -= 1;

            var readBytes = InnerStream.Read(_singleByteBuffer, 0, 1);
            if (readBytes == 0)
                return -1;

            return _singleByteBuffer[0];
        }

        /// <summary>
        /// Reads the previous char in the stream and decrements the position by 1.
        /// </summary>
        /// <returns>The previous char in the stream.</returns>
        public char ReadPreviousChar()
        {
            var value = ReadPrevious();
            if (value == -1)
                return default;
            return (char)value;
        }

        /// <summary>
        /// Writes a number to the stream.
        /// </summary>
        /// <param name="number">The number to write to the stream.</param>
        public void Write(int number)
        {
            var bytes = Encoding.ASCII.GetBytes(number.ToString());
            Write(bytes);
        }

        /// <summary>
        /// Writes the number to the stream.
        /// </summary>
        /// <param name="number">The number to write to the stream.</param>
        public void Write(long number)
        {
            var bytes = Encoding.ASCII.GetBytes(number.ToString());
            Write(bytes);
        }

        /// <summary>
        /// Writes a char to the stream.
        /// </summary>
        /// <param name="c">The char to write to the stream.</param>
        public void Write(char c)
        {
            InnerStream.WriteByte((byte) c);
        }

        /// <summary>
        /// Writes an array of bytes to the stream.
        /// </summary>
        /// <param name="bytes">The bytes to write to the stream.</param>
        public void Write(byte[] bytes)
        {
            Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Writes a sequence of bytes to the stream and advances the position by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes to copy from.</param>
        /// <param name="offset">The zero-based offset in <paramref name="buffer"/> to start copying from to the stream.</param>
        /// <param name="count">The number of bytes to be written to the stream</param>
        public void Write(byte[] buffer, int offset, int count)
        {
            InnerStream.Write(buffer, offset, count);
        }

        /// <summary>
        /// Attempts to set the length of the stream to the specified <paramref name="length"/> if supported.
        /// </summary>
        /// <param name="length"></param>
        public bool TrySetLength(long length)
        {
            if (!InnerStream.CanWrite || !InnerStream.CanSeek)
                return false;

            try
            {
                if (InnerStream.Length >= length)
                    return false;

                InnerStream.SetLength(length);
                return true;
            }
            catch
            {
                return false;
            }
        }

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
