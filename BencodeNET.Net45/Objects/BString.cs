using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BencodeNET.IO;

namespace BencodeNET.Objects
{
    public sealed class BString : BObject<byte[]>, IComparable<BString>
    {
        /// <summary>
        /// The maximum number of digits that can be handled as the length part of a bencoded string.
        /// </summary>
        internal const int LengthMaxDigits = 10;

        public override byte[] Value { get; }

        private Encoding _encoding;
        /// <summary>
        /// Gets or sets the encoding used as the default with <c>ToString()</c>.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public Encoding Encoding
        {
            get { return _encoding; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value), "Encoding may not be set to null");
                _encoding = value;
            }
        }

        public BString(IEnumerable<byte> bytes)
            : this(bytes, Encoding.UTF8)
        { }

        public BString(IEnumerable<byte> bytes, Encoding encoding)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            _encoding = encoding;
            Value = bytes as byte[] ?? bytes.ToArray();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BString"/> class using
        /// <c>Encoding.UTF8</c> to convert the string to bytes.
        /// </summary>
        /// <param name="str"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public BString(string str)
            : this(str, Encoding.UTF8)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BString"/> class using
        /// the specified encoding to convert the string to bytes.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="encoding">The encoding used to convert the string to bytes.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public BString(string str, Encoding encoding)
        {
            if (str == null) throw new ArgumentNullException(nameof(str));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            _encoding = encoding;
            Value = encoding.GetBytes(str);
        }

        /// <summary>
        /// Gets the length in bytes of the string.
        /// </summary>
        public int Length => Value.Length;

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

        /// <summary>
        /// Encodes the object and returns the result as a string using
        /// the current value of the Encoding property.
        /// </summary>
        /// <returns>
        /// The object bencoded and converted to a string using
        /// the current value of the Encoding property.
        /// </returns>
        public override string Encode()
        {
            return Encode(_encoding);
        }

        /// <summary>
        /// Encodes the object to the specified stream and returns a reference to the stream.
        /// </summary>
        /// <typeparam name="TStream">The type of stream.</typeparam>
        /// <param name="stream">The stream to encode the object to.</param>
        /// <returns>The supplied stream.</returns>
        public override TStream EncodeToStream<TStream>(TStream stream)
        {
            using (var bstream = new BencodeStream(stream, leaveOpen:true))
            {
                bstream.Write(Length);
                bstream.Write(':');
                bstream.Write(Value);
                return stream;
            }
        }

        public override async Task<TStream> EncodeToStreamAsync<TStream>(TStream stream)
        {
            using (var bstream = new BencodeStream(stream, leaveOpen:true))
            {
                await bstream.WriteAsync(Length).ConfigureAwait(false);
                await bstream.WriteAsync(':').ConfigureAwait(false);
                await bstream.WriteAsync(Value).ConfigureAwait(false);
                return stream;
            }
        }
    }
}
