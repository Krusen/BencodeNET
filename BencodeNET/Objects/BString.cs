using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BencodeNET.IO;

namespace BencodeNET.Objects
{
    public sealed class BString : BObject<byte[]>, IComparable<BString>
    {
        internal const int LengthMaxDigits = 10;

        private Encoding _encoding;

        public Encoding Encoding
        {
            get { return _encoding; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value", "Encoding may not be set to null");
                _encoding = value;
            }
        }

        public BString(IEnumerable<byte> bytes)
            : this(bytes, Bencode.DefaultEncoding)
        { }

        public BString(IEnumerable<byte> bytes, Encoding encoding)
        {
            if (bytes == null) throw new ArgumentNullException("bytes");
            if (encoding == null) throw new ArgumentNullException("encoding");

            _encoding = encoding;
            Value = bytes.ToArray();
        }

        public BString(string str)
            : this(str, Bencode.DefaultEncoding)
        { }

        public BString(string str, Encoding encoding)
        {
            if (str == null) throw new ArgumentNullException("str");
            if (encoding == null) throw new ArgumentNullException("encoding");

            _encoding = encoding;
            Value = encoding.GetBytes(str);
        }

        public int Length
        {
            get { return Value.Length; }
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

        public override string ToString()
        {
            return _encoding.GetString(Value.ToArray());
        }

        public string ToString(Encoding encoding)
        {
            return encoding.GetString(Value.ToArray());
        }

        public override string Encode()
        {
            return Encode(_encoding);
        }

        public override T EncodeToStream<T>(T stream)
        {
            using (var bstream = new BencodeStream(stream, leaveOpen:true))
            {
                bstream.Write(Length);
                bstream.Write(':');
                bstream.Write(Value);
                return stream;
            }
        }
    }
}
