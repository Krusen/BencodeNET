using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BencodeNET.Objects
{
    /// <summary>
    /// Represents a bencoded string, i.e. a byte-string.
    /// It isn't necessarily human-readable.
    /// </summary>
    /// <remarks>
    /// The underlying value is a <see cref="byte"/> array.
    /// </remarks>
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
            get => _encoding;
            set => _encoding = value ?? DefaultEncoding;
        }
        private Encoding _encoding;

        /// <summary>
        /// Creates an empty <see cref="BString"/> ('0:').
        /// </summary>
        public BString()
            : this((string)null)
        {
        }

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
        /// <exception cref="ArgumentNullException"></exception>
        public BString(string str, Encoding encoding = null)
        {
            _encoding = encoding ?? DefaultEncoding;
            _value = string.IsNullOrEmpty(str)
                ? Array.Empty<byte>()
                : _encoding.GetBytes(str);
        }

        /// <inheritdoc/>
        public override int GetSizeInBytes() => _value.Length + 1 + _value.Length.DigitCount();

        /// <inheritdoc/>
        protected override void EncodeObject(Stream stream)
        {
            stream.Write(_value.Length);
            stream.Write(':');
            stream.Write(_value);
        }

        /// <inheritdoc/>
        protected override ValueTask<FlushResult> EncodeObjectAsync(PipeWriter writer, CancellationToken cancellationToken = default)
        {
            // Init
            var size = GetSizeInBytes();
            var buffer = writer.GetSpan(size);

#if NETCOREAPP2_1
            // Write length
            var writtenBytes = Encoding.GetBytes(_value.Length.ToString().AsSpan(), buffer);

            // Write ':'
            buffer[writtenBytes] = (byte) ':';

            // Write value
            _value.AsSpan().CopyTo(buffer.Slice(writtenBytes + 1));
#else
            // Write length
            var lengthBytes = Encoding.GetBytes(_value.Length.ToString());
            lengthBytes.CopyTo(buffer);

            // Write ':'
            buffer[lengthBytes.Length] = (byte) ':';

            // Write value
            _value.AsSpan().CopyTo(buffer.Slice(lengthBytes.Length + 1));
#endif

            // Commit
            writer.Advance(size);
            return writer.FlushAsync(cancellationToken);
        }

#pragma warning disable 1591
        public static implicit operator BString(string value) => new BString(value);

        public static bool operator ==(BString first, BString second)
        {
            if (ReferenceEquals(first, null))
                return ReferenceEquals(second, null);

            return first.Equals(second);
        }

        public static bool operator !=(BString first, BString second) => !(first == second);

        public override bool Equals(object other) => other is BString bstring && Value.SequenceEqual(bstring.Value);

        public bool Equals(BString bstring) => bstring != null && Value.SequenceEqual(bstring.Value);

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
#pragma warning restore 1591

        /// <summary>
        /// Converts the underlying bytes to a string representation using the current value of the <see cref="Encoding"/> property.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
#if NETCOREAPP2_1
            return _encoding.GetString(_value.AsSpan());
#else
            return _encoding.GetString(_value);
#endif
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
            encoding = encoding ?? _encoding;
#if NETCOREAPP2_1
            return encoding.GetString(_value.AsSpan());
#else
            return encoding.GetString(_value);
#endif
        }
    }
}
