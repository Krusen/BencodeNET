using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BencodeNET.IO;

namespace BencodeNET.Objects
{
    /// <summary>
    /// A class representing a bencoded string, i.e. a byte-string.
    /// The underlying value is a <see cref="byte"/> array.
    /// </summary>
    public sealed class BString : BObject<IReadOnlyList<byte>>, IComparable<BString>
    {
        /// <summary>
        /// The maximum number of digits that can be handled as the length part of a bencoded string.
        /// </summary>
        internal const int LengthMaxDigits = 10;

        /// <summary>
        /// The underlying bytes of the string.
        /// </summary>
        public override IReadOnlyList<byte> Value => _value;
        private readonly byte[] _value;

        /// <summary>
        /// Gets the length of the string in bytes.
        /// </summary>
        public int Length => _value.Length;

        private static readonly Encoding DefaultEncoding = Encoding.UTF8;

        /// <summary>
        /// Gets or sets the encoding used as the default with <c>ToString()</c>.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public Encoding Encoding
        {
            get { return _encoding; }
            set { _encoding = value ?? DefaultEncoding; }
        }
        private Encoding _encoding;

        /// <summary>
        /// Creates a <see cref="BString"/> from bytes with the specified encoding.
        /// </summary>
        /// <param name="bytes">The bytes representing the data.</param>
        /// <param name="encoding">The encoding of the bytes. Defaults to <see cref="System.Text.Encoding.UTF8"/>.</param>
        public BString(IEnumerable<byte> bytes, Encoding encoding = null)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            _encoding = encoding ?? DefaultEncoding;
            _value = bytes as byte[] ?? bytes.ToArray();
        }

        /// <summary>
        /// Creates a <see cref="BString"/> using the specified encoding to convert the string to bytes.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="encoding">The encoding used to convert the string to bytes.</param>
        /// <exception cref="ArgumentNullException">If </exception>
        public BString(string str, Encoding encoding = null)
        {
            if (str == null) throw new ArgumentNullException(nameof(str));

            _encoding = encoding ?? DefaultEncoding;
            _value = _encoding.GetBytes(str);
        }

        /// <summary>
        /// Encodes the string and returns the result as a string using
        /// the current value of the <see cref="Encoding"/> property.
        /// </summary>
        /// <returns>
        /// The object bencoded and converted to a string using
        /// the current value of the <see cref="Encoding"/> property.
        /// </returns>
        public override string Encode()
        {
            return Encode(_encoding);
        }

        /// <summary>
        /// Encodes the byte-string to the specified stream and returns a reference to the stream.
        /// </summary>
        /// <typeparam name="TStream">The type of stream.</typeparam>
        /// <param name="stream">The stream to encode the byte-string to.</param>
        /// <returns>The supplied stream.</returns>
        public override TStream EncodeToStream<TStream>(TStream stream)
        {
            using (var bstream = new BencodeStream(stream, leaveOpen: true))
            {
                bstream.Write(_value.Length);
                bstream.Write(':');
                bstream.Write(_value);
                return stream;
            }
        }

        /// <summary>
        /// Encodes the byte-string to the specified stream and returns a reference to the stream.
        /// </summary>
        /// <typeparam name="TStream">The type of stream.</typeparam>
        /// <param name="stream">The stream to encode the byte-string to.</param>
        /// <returns>The supplied stream.</returns>
        public override async Task<TStream> EncodeToStreamAsync<TStream>(TStream stream)
        {
            using (var bstream = new BencodeStream(stream, leaveOpen: true))
            {
                await bstream.WriteAsync(_value.Length).ConfigureAwait(false);
                await bstream.WriteAsync(':').ConfigureAwait(false);
                await bstream.WriteAsync(_value).ConfigureAwait(false);
                return stream;
            }
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
            var bstring = other as BString;
            if (bstring != null)
                return Value.SequenceEqual(bstring.Value);

            return false;
        }

        public bool Equals(BString bstring)
        {
            if (bstring == null)
                return false;
            return Value.SequenceEqual(bstring.Value);
        }

        public override int GetHashCode()
        {
            var bytesToHash = Math.Min(Value.Count, 32);

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

        /// <summary>
        /// Converts the underlying bytes to a string representation using the current value of the Encoding property.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return _encoding.GetString(Value.ToArray());
        }

        /// <summary>
        /// Converts the underlying bytes to a string representation using the specified encoding.
        /// </summary>
        /// <param name="encoding">The encoding to use to convert the underlying byte array to a <see cref="System.String" />.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public string ToString(Encoding encoding)
        {
            return encoding.GetString(Value.ToArray());
        }
    }
}
