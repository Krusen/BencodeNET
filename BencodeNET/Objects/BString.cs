using System;
using System.IO;
using System.IO.Pipelines;
using System.Text;

namespace BencodeNET.Objects
{
    /// <summary>
    /// Represents a bencoded string, i.e. a byte-string.
    /// It isn't necessarily human-readable.
    /// </summary>
    /// <remarks>
    /// The underlying value is a <see cref="byte"/> array.
    /// </remarks>
    public sealed class BString : BObject<ReadOnlyMemory<byte>>, IComparable<BString>
    {
        /// <summary>
        /// The maximum number of digits that can be handled as the length part of a bencoded string.
        /// </summary>
        internal const int LengthMaxDigits = 10;

        /// <summary>
        /// The underlying bytes of the string.
        /// </summary>
        public override ReadOnlyMemory<byte> Value { get; }

        /// <summary>
        /// Gets the length of the string in bytes.
        /// </summary>
        public int Length => Value.Length;

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
        public BString(byte[] bytes, Encoding encoding = null)
        {
            Value = bytes ?? throw new ArgumentNullException(nameof(bytes));
            _encoding = encoding ?? DefaultEncoding;
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

            if (string.IsNullOrEmpty(str))
            {
                Value = Array.Empty<byte>();
            }
            else
            {
                var maxByteCount = _encoding.GetMaxByteCount(str.Length);
                var span = new byte[maxByteCount].AsSpan();

                var length = _encoding.GetBytes(str.AsSpan(), span);

                Value = span.Slice(0, length).ToArray();
            }
        }

        /// <inheritdoc/>
        public override int GetSizeInBytes() => Value.Length + 1 + Value.Length.DigitCount();

        /// <inheritdoc/>
        protected override void EncodeObject(Stream stream)
        {
            stream.Write(Value.Length);
            stream.Write(':');
            stream.Write(Value.Span);
        }

        /// <inheritdoc/>
        protected override void EncodeObject(PipeWriter writer)
        {
            // Init
            var size = GetSizeInBytes();
            var buffer = writer.GetSpan(size);

            // Write length
            var writtenBytes = Encoding.GetBytes(Value.Length.ToString().AsSpan(), buffer);

            // Write ':'
            buffer[writtenBytes] = (byte) ':';

            // Write value
            Value.Span.CopyTo(buffer.Slice(writtenBytes + 1));

            // Commit
            writer.Advance(size);
        }

#pragma warning disable 1591
        public static implicit operator BString(string value) => new BString(value);

        public static bool operator ==(BString first, BString second)
        {
            if (first is null)
                return second is null;

            return first.Equals(second);
        }

        public static bool operator !=(BString first, BString second) => !(first == second);

        public override bool Equals(object other) => other is BString bstring && Value.Span.SequenceEqual(bstring.Value.Span);

        public bool Equals(BString bstring) => bstring != null && Value.Span.SequenceEqual(bstring.Value.Span);

        public override int GetHashCode()
        {
            var bytesToHash = Math.Min(Value.Length, 32);

            long hashValue = 0;
            for (var i = 0; i < bytesToHash; i++)
            {
                hashValue = (37 * hashValue + Value.Span[i]) % int.MaxValue;
            }

            return (int)hashValue;
        }

        public int CompareTo(BString other)
        {
            return Value.Span.SequenceCompareTo(other.Value.Span);
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
            return _encoding.GetString(Value.Span);
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
            return encoding.GetString(Value.Span);
        }
    }
}
