using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BencodeNET.Exceptions;

namespace BencodeNET.Objects
{
    public class BString : BObject<string>, IComparable<BString>
    {
        private const int LengthMaxDigits = 10;

        public BString(string str)
        {
            if (str == null) throw new ArgumentNullException("str");

            Value = str;
        }

        public BString(IEnumerable<char> chars) : this(chars.AsString())
        { }

        public override T EncodeToStream<T>(T stream, Encoding encoding)
        {
            var bufferSize = Math.Max(1, Value.Length)*2;
            using (var writer = new StreamWriter(stream, encoding, bufferSize, leaveOpen:true))
            {
                writer.Write(Value.Length);
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

            var startPosition = stream.Position;

            // Minimum valid bencode string is '0:' meaning an empty string
            if (stream.Length < 2)
                throw InvalidException("Minimum valid length is 2 (an empty string: '0:')", startPosition);

            using (var reader = new BinaryReader(stream, encoding, leaveOpen:true)) {
                var lengthChars = new List<char>();

                while (!stream.EndOfStream())
                {
                    var c = reader.ReadChar();

                    // Break when we reach ':' but only if it is not the first char
                    if (lengthChars.Count > 0 && c == ':')
                        break;

                    // Character must be a digit
                    if (!char.IsDigit(c))
                    {
                        if (lengthChars.Count == 0)
                            throw InvalidException(string.Format("Must begin with an integer but began with '{0}'", c), startPosition);

                        // We have found some digits but this is neither a digit nor a ':' as expected
                        throw InvalidException("Delimiter ':' was not found.", startPosition);
                    }

                    // Because of memory limitations (~1-2 GB) we know for certain we cannot handle more than 10 digits (10GB)
                    if (lengthChars.Count >= LengthMaxDigits)
                    {
                        throw UnsupportedBencodeException.New(
                            string.Format("Length of string is more than {0} digits (>10GB) and is not supported (max is ~1-2GB).", LengthMaxDigits),
                            startPosition);
                    }

                    lengthChars.Add(c);
                }

                var stringLength = long.Parse(lengthChars.AsString());

                // Int32.MaxValue is ~2GB and is the absolute maximum that can be handled in memory
                if (stringLength > int.MaxValue)
                {
                    throw UnsupportedBencodeException.New(
                        string.Format("Length of string is {0:N0} but maximum supported length is {1:N0}.", stringLength, int.MaxValue),
                        startPosition);
                }

                // TODO: Catch possible OutOfMemoryException when stringLength is close to Int32.MaxValue ?
                var chars = reader.ReadChars((int) stringLength);

                // If the two don't match we've reached the end of the stream before reading the expected number of chars
                if (chars.Length != stringLength)
                {
                    throw InvalidException(
                        string.Format("Expected string to be {0:N0} characters long but could only read {1:N0} characters.", stringLength, chars.Length),
                        startPosition);
                }

                return new BString(chars);
            }
        }

        private static InvalidBencodeException InvalidException(string message, long streamPosition)
        {
            return InvalidBencodeException.New("Invalid bencode string. " + message, streamPosition);
        }

        #region Operators and comparison

        public static implicit operator string(BString value)
        {
            return value.Value;
        }

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

            return string.CompareOrdinal(Value, other.Value);
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return Value;
        }

        public override bool Equals(object other)
        {
            if (other == null)
                return false;

            var bstr = other as BString;
            if (bstr != null)
                return Value == bstr.Value;

            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        #endregion
    }
}
