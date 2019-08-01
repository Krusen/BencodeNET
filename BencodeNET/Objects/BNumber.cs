using System;
using System.IO;

namespace BencodeNET.Objects
{
    /// <summary>
    /// Represents a bencoded number (integer).
    /// </summary>
    /// <remarks>
    /// The underlying value is a <see cref="long"/>.
    /// </remarks>
    public sealed class BNumber : BObject<long>, IComparable<BNumber>
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// The string-length of long.MaxValue. Longer strings cannot be parsed.
        /// </summary>
        internal const int MaxDigits = 19;

        /// <summary>
        /// The underlying value.
        /// </summary>
        public override long Value { get; }

        /// <summary>
        /// Create a <see cref="BNumber"/> from a <see cref="long"/>.
        /// </summary>
        public BNumber(long value)
        {
            Value = value;
        }

        /// <summary>
        /// Create a <see cref="BNumber"/> from a <see cref="DateTime"/>.
        /// </summary>
        /// <remarks>
        /// Bencode dates are stored in unix format (seconds since epoch).
        /// </remarks>
        public BNumber(DateTime? datetime)
        {
            Value = datetime?.Subtract(Epoch).Ticks / TimeSpan.TicksPerSecond ?? 0;
        }

        /// <inheritdoc/>
        public override int GetSizeInBytes() => Value.DigitCount() + 2;

        /// <inheritdoc/>
        protected override void EncodeObject(Stream stream)
        {
            stream.Write('i');
            stream.Write(Value);
            stream.Write('e');
        }

#pragma warning disable 1591
        public static implicit operator int?(BNumber bint)
        {
            if (bint == null) return null;
            return (int)bint.Value;
        }

        public static implicit operator long?(BNumber bint)
        {
            if (bint == null) return null;
            return bint.Value;
        }

        public static implicit operator int(BNumber bint)
        {
            if (bint == null) throw new InvalidCastException();
            return (int)bint.Value;
        }

        public static implicit operator long(BNumber bint)
        {
            if (bint == null) throw new InvalidCastException();
            return bint.Value;
        }

        public static implicit operator bool(BNumber bint)
        {
            if (bint == null) throw new InvalidCastException();
            return bint.Value > 0;
        }

        public static implicit operator DateTime?(BNumber number)
        {
            if (number == null) return null;

            if (number.Value > int.MaxValue)
            {
                try
                {
                    return Epoch.AddMilliseconds(number);
                }
                catch (ArgumentOutOfRangeException)
                {
                    return Epoch;
                }
            }

            return Epoch.AddSeconds(number);
        }

        public static implicit operator BNumber(int value) => new BNumber(value);

        public static implicit operator BNumber(long value) => new BNumber(value);

        public static implicit operator BNumber(bool value) => new BNumber(value ? 1 : 0);

        public static implicit operator BNumber(DateTime? datetime) => new BNumber(datetime);

        public static bool operator ==(BNumber bnumber, BNumber other)
        {
            if (ReferenceEquals(bnumber, null) && ReferenceEquals(other, null)) return true;
            if (ReferenceEquals(bnumber, null) || ReferenceEquals(other, null)) return false;
            return bnumber.Value == other.Value;
        }

        public static bool operator !=(BNumber bnumber, BNumber other) => !(bnumber == other);

        public override bool Equals(object other)
        {
            var bnumber = other as BNumber;
            return Value == bnumber?.Value;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => Value.GetHashCode();

        public int CompareTo(BNumber other)
        {
            if (other == null)
                return 1;

            return Value.CompareTo(other.Value);
        }

        public override string ToString() => Value.ToString();

        public string ToString(string format) => Value.ToString(format);

        public string ToString(IFormatProvider formatProvider) => Value.ToString(formatProvider);

        public string ToString(string format, IFormatProvider formatProvider) => Value.ToString(format, formatProvider);
#pragma warning restore 1591
    }
}
