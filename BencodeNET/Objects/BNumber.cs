using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BencodeNET.Exceptions;
using BencodeNET.IO;

namespace BencodeNET.Objects
{
    public class BNumber : BObject<long>, IComparable<BNumber>
    {
        private const int MaxDigits = 19;

        public BNumber(long value)
        {
            Value = value;
        }

        public override T EncodeToStream<T>(T stream, Encoding encoding)
        {
            using (var bstream = new BencodeStream(stream, leaveOpen:true))
            {
                bstream.Write('i');
                bstream.Write(Value);
                bstream.Write('e');
                return stream;
            }
        }

        public static BNumber Decode(string bencodedString)
        {
            if (bencodedString == null) throw new ArgumentNullException("bencodedString");

            return Decode(bencodedString, Bencode.DefaultEncoding);
        }

        public static BNumber Decode(string bencodedString, Encoding encoding)
        {
            if (bencodedString == null) throw new ArgumentNullException("bencodedString");
            if (encoding == null) throw new ArgumentNullException("encoding");

            using (var ms = new MemoryStream(encoding.GetBytes(bencodedString)))
            {
                return Decode(ms, encoding);
            }
        }

        public static BNumber Decode(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            return Decode(stream, Bencode.DefaultEncoding);
        }

        public static BNumber Decode(Stream stream, Encoding encoding)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (encoding == null) throw new ArgumentNullException("encoding");

            return Decode(new BencodeStream(stream, leaveOpen: true), encoding);
        }

        public static BNumber Decode(BencodeStream stream, Encoding encoding)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (encoding == null) throw new ArgumentNullException("encoding");

            if (stream.Length < 3)
                throw InvalidBencodeException.New("Minimum valid length of stream is 3 ('i0e').", stream.Position);

            var isNegative = false;
            var endCharFound = false;

            var digits = new List<char>();

            // Numbers must start with 'i'
            var firstChar = stream.ReadChar();
            if (firstChar != 'i')
                throw InvalidException(string.Format("Must begin with 'i' but began with '{0}'.", firstChar), stream.Position);

            while (!stream.EndOfStream)
            {
                var c = stream.ReadChar();

                if (c == 'e')
                {
                    endCharFound = true;
                    break;
                }

                // We do not support numbers that cannot be stored as a long (Int64)
                if (digits.Count >= MaxDigits)
                {
                    throw UnsupportedBencodeException.New(
                        string.Format(
                            "The number '{0}' has more than 19 digits and cannot be stored as a long (Int64) and therefore is not supported.",
                            digits.AsString()),
                        stream.Position);
                }

                // There may be only one '-'
                if (c == '-' && !isNegative)
                {
                    // '-' must be the first char after the beginning 'i'
                    if (digits.Count > 0)
                        throw InvalidException("A '-' must be before any digits.", stream.Position);

                    isNegative = true;
                    continue;
                }

                // If it is not 'e', not '-' and not a digit then it is invalid
                if (!char.IsDigit(c))
                    throw InvalidException("Must only contain digits and a single prefixed '-'.", stream.Position);

                digits.Add(c);
            }

            // We need at least one digit
            if (digits.Count < 1)
                throw InvalidException("It contains no digits.", stream.Position);

            if (!endCharFound)
                throw InvalidException("Missing end character 'e'.", stream.Position);

            // Leading zeros are not valid
            if (digits[0] == '0' && digits.Count > 1)
                throw InvalidException("Leading '0's are not valid.", stream.Position);

            // '-0' is not valid either
            if (digits[0] == '0' && digits.Count == 1 && isNegative)
                throw InvalidException("'-0' is not a valid number.", stream.Position);

            if (isNegative)
                digits.Insert(0, '-');

            long number;
            if (!long.TryParse(digits.AsString(), out number))
            {
                // This should only happen if the number is bigger than 9,223,372,036,854,775,807 (or smaller than the negative version)
                throw UnsupportedBencodeException.New(
                    string.Format(
                        "The value {0} cannot be stored as a long (Int64) and is therefore not supported. The supported values range from {1:N0} to {2:N0}",
                        digits.AsString(), long.MinValue, long.MaxValue),
                    stream.Position);
            }

            return new BNumber(number);
        }

        private static InvalidBencodeException InvalidException(string message, long streamPosition)
        {
            return InvalidBencodeException.New("Invalid bencode number. " + message, streamPosition);
        }

        #region Operators and comparison

        public static implicit operator int(BNumber bint)
        {
            return (int)bint.Value;
        }

        public static implicit operator long(BNumber bint)
        {
            return bint.Value;
        }

        public static implicit operator BNumber(int value)
        {
            return new BNumber(value);
        }

        public static implicit operator BNumber(long value)
        {
            return new BNumber(value);
        }

        public static bool operator ==(BNumber bnumber, BNumber other)
        {
            if (ReferenceEquals(bnumber, null) && ReferenceEquals(other, null)) return true;
            if (ReferenceEquals(bnumber, null) || ReferenceEquals(other, null)) return false;
            return bnumber.Value == other.Value;
        }

        public static bool operator !=(BNumber bnumber, BNumber other)
        {
            return !(bnumber == other);
        }

        public int CompareTo(BNumber other)
        {
            if (other == null)
                return 1;

            return Value.CompareTo(other.Value);
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return Value.ToString();
        }

        public string ToString(string format)
        {
            return Value.ToString(format);
        }

        public string ToString(IFormatProvider formatProvider)
        {
            return Value.ToString(formatProvider);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return Value.ToString(format, formatProvider);
        }

        public override bool Equals(object other)
        {
            if (other == null)
                return false;

            var bnumber = other as BNumber;
            if (bnumber != null)
                return Value == bnumber.Value;

            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        #endregion
    }
}
