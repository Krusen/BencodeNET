using System;

namespace BencodeNET.Parsing
{
    /// <summary>
    /// A collection of helper methods for parsing bencode.
    /// </summary>
    public static class ParseUtil
    {
        private const int Int64MaxDigits = 19;

        /// <summary>
        /// A faster implementation than <see cref="long.TryParse(string, out long)"/>
        /// because we skip some checks that are not needed.
        /// </summary>
        public static bool TryParseLongFast(string value, out long result)
            => TryParseLongFast(value.AsSpan(), out result);

        /// <summary>
        /// A faster implementation than <see cref="long.TryParse(string, out long)"/>
        /// because we skip some checks that are not needed.
        /// </summary>
        public static bool TryParseLongFast(ReadOnlySpan<char> value, out long result)
        {
            result = 0;

            if (value == null)
                return false;

            var length = value.Length;

            // Cannot parse empty string
            if (length == 0)
                return false;

            var isNegative = value[0] == '-';
            var startIndex = isNegative ? 1 : 0;

            // Cannot parse just '-'
            if (isNegative && length == 1)
                return false;

            // Cannot parse string longer than long.MaxValue
            if (length - startIndex > Int64MaxDigits)
                return false;

            long parsedLong = 0;
            for (var i = startIndex; i < length; i++)
            {
                var character = value[i];
                if (!character.IsDigit())
                    return false;

                var digit = character - '0';

                if (isNegative)
                    parsedLong = 10 * parsedLong - digit;
                else
                    parsedLong = 10 * parsedLong + digit;
            }

            // Negative - should be less than zero (Int64.MinValue overflow)
            if (isNegative && parsedLong >= 0)
                return false;

            // Positive - should be equal to or greater than zero (Int64.MaxValue overflow)
            if (!isNegative && parsedLong < 0)
                return false;

            result = parsedLong;
            return true;
        }
    }
}
