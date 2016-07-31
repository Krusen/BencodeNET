using System;
#if !NET35
using System.Threading.Tasks;
#endif
using BencodeNET.IO;

namespace BencodeNET.Objects
{
    public sealed class BNumber : BObject<long>, IComparable<BNumber>
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// The string-length of long.MaxValue. Longer strings cannot be parsed.
        /// </summary>
        internal const int MaxDigits = 19;

        public BNumber(long value)
        {
            Value = value;
        }

        // TODO: Doc. about expecting a unix format
        public BNumber(DateTime? datetime)
        {
            if (datetime == null)
                Value = 0;

            Value = datetime.Value.Subtract(Epoch).Ticks / TimeSpan.TicksPerSecond;
        }

        public static implicit operator int(BNumber bint)
        {
            return (int)bint.Value;
        }

        public static implicit operator long(BNumber bint)
        {
            return bint.Value;
        }

        public static implicit operator DateTime? (BNumber number)
        {
            return Epoch.AddSeconds(number);
        }

        public static implicit operator BNumber(int value)
        {
            return new BNumber(value);
        }

        public static implicit operator BNumber(long value)
        {
            return new BNumber(value);
        }

        public static implicit operator BNumber(DateTime? datetime)
        {
            return new BNumber(datetime);
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

        public int CompareTo(BNumber other)
        {
            if (other == null)
                return 1;

            return Value.CompareTo(other.Value);
        }

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

        public override T EncodeToStream<T>(T stream)
        {
            using (var bstream = new BencodeStream(stream, leaveOpen:true))
            {
                bstream.Write('i');
                bstream.Write(Value);
                bstream.Write('e');
                return stream;
            }
        }

#if !NET35
        public override async Task<TStream> EncodeToStreamAsync<TStream>(TStream stream)
        {
            using (var bstream = new BencodeStream(stream, leaveOpen: true))
            {
                await bstream.WriteAsync('i').ConfigureAwait(false);
                await bstream.WriteAsync(Value).ConfigureAwait(false);
                await bstream.WriteAsync('e').ConfigureAwait(false);
                return stream;
            }
        }
#endif
    }
}
