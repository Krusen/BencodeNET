using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BencodeNET.Exceptions;

namespace BencodeNET.Objects
{
    public class BString : BObject<byte[]>, IComparable<BString>
    {
        private const int LengthMaxDigits = 10;

        private readonly Encoding _encoding;

        public int Length
        {
            get { return Value.Length; }
        }

        public BString(IEnumerable<byte> bytes) : this(bytes, Bencode.DefaultEncoding)
        { }

        public BString(IEnumerable<byte> bytes, Encoding encoding)
        {
            if (bytes == null) throw new ArgumentNullException("bytes");
            if (encoding == null) throw new ArgumentNullException("encoding");

            _encoding = encoding;
            Value = bytes.ToArray();
        }

        public BString(string str) : this(str, Bencode.DefaultEncoding)
        { }

        public BString(string str, Encoding encoding)
        {
            if (str == null) throw new ArgumentNullException("str");
            if (encoding == null) throw new ArgumentNullException("encoding");

            _encoding = encoding;
            Value = encoding.GetBytes(str);
        }

        public override T EncodeToStream<T>(T stream, Encoding encoding)
        {
            using (var writer = new BinaryWriter(stream, encoding, leaveOpen:true))
            {
                writer.WriteAsString(Length);
                writer.Write(':');
                writer.Write(Value);
                return stream;
            }
        }

        public static BString Decode(string bencodedString)
        {
            return Decode(bencodedString, Bencode.DefaultEncoding);
        }

        public static BString Decode(string bencodedString, Encoding encoding)
        {
            if (bencodedString == null) throw new ArgumentNullException("bencodedString");
            if (encoding == null) throw new ArgumentNullException("encoding");

            using (var ms = new MemoryStream(encoding.GetBytes(bencodedString)))
            {
                return Decode(ms, encoding);
            }
        }

        public static BString Decode(Stream stream)
        {
            return Decode(stream, Bencode.DefaultEncoding);
        }

        public static BString Decode(Stream stream, Encoding encoding)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (encoding == null) throw new ArgumentNullException("encoding");

            // Minimum valid bencode string is '0:' meaning an empty string
            if (stream.Length < 2)
                throw InvalidException("Minimum valid length is 2 (an empty string: '0:')", stream.Position);

            var lengthChars = new List<char>();

            while (!stream.EndOfStream())
            {
                var c = (char)stream.ReadByte();

                // Break when we reach ':' if it is not the first character found
                if (lengthChars.Count > 0 && c == ':')
                    break;

                // Character then must be a digit
                if (!char.IsDigit(c))
                {
                    if (lengthChars.Count == 0)
                        throw InvalidException(string.Format("Must begin with an integer but began with '{0}'", c), stream.Position);

                    // We have found some digits but this is neither a digit nor a ':' as expected
                    throw InvalidException("Delimiter ':' was not found.", stream.Position);
                }

                // Because of memory limitations (~1-2 GB) we know for certain we cannot handle more than 10 digits (10GB)
                if (lengthChars.Count >= LengthMaxDigits)
                {
                    throw UnsupportedBencodeException.New(
                        string.Format("Length of string is more than {0} digits (>10GB) and is not supported (max is ~1-2GB).", LengthMaxDigits),
                        stream.Position);
                }

                lengthChars.Add(c);
            }

            var stringLength = long.Parse(lengthChars.AsString());

            // Int32.MaxValue is ~2GB and is the absolute maximum that can be handled in memory
            if (stringLength > int.MaxValue)
            {
                throw UnsupportedBencodeException.New(
                    string.Format("Length of string is {0:N0} but maximum supported length is {1:N0}.", stringLength, int.MaxValue),
                    stream.Position);
            }

            // TODO: Catch possible OutOfMemoryException when stringLength is close to Int32.MaxValue ?
            var bytes = new List<byte>();
            while (bytes.Count < stringLength && !stream.EndOfStream())
            {
                bytes.Add((byte)stream.ReadByte());
            }

            // If the two don't match we've reached the end of the stream before reading the expected number of chars
            if (bytes.Count != stringLength)
            {
                throw InvalidException(
                    string.Format("Expected string to be {0:N0} characters long but could only read {1:N0} characters.", stringLength, bytes.Count),
                    stream.Position);
            }

            return new BString(bytes, encoding);
        }

        private static InvalidBencodeException InvalidException(string message, long streamPosition)
        {
            return InvalidBencodeException.New("Invalid bencode string. " + message, streamPosition);
        }

        #region Operators and comparison

        public static implicit operator BString(string value)
        {
            return new BString(value);
        }

        public static bool operator ==(BString first, BString second)
        {
            if (ReferenceEquals(first, null))
                return ReferenceEquals(second, null);

            return first.Equals(second);
        }

        public static bool operator !=(BString first, BString second)
        {
            return !(first == second);
        }

        public int CompareTo(BString other)
        {
            if (other == null)
                return 1;

            var maxLength = Math.Max(this.Length, other.Length);

            for (var i = 0; i < maxLength; i++)
            {
                // This is shorter and thereby this is "less than" the other
                if (i >= this.Length)
                    return -1;

                // The other is shorter and thereby this is "greater than" the other
                if (i >= other.Length)
                    return 1;

                if (this.Value[i] > other.Value[i])
                    return 1;

                if (this.Value[i] < other.Value[i])
                    return -1;
            }

            return 0;
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return _encoding.GetString(Value.ToArray());
        }

        public string ToString(Encoding encoding)
        {
            return encoding.GetString(Value.ToArray());
        }

        public override bool Equals(object other)
        {
            if (other == null)
                return false;

            var bstr = other as BString;
            if (bstr != null)
                return Value.SequenceEqual(bstr.Value);

            return false;
        }

        public override int GetHashCode()
        {
            var bytesToHash = Math.Min(Value.Length, 32);
            
            long hashValue = 0;
            for (var i = 0; i < bytesToHash; i++)
            {
                hashValue = (37 * hashValue + Value[i]) % int.MaxValue;
            }

            return (int)hashValue;
        }

        #endregion
    }
}
