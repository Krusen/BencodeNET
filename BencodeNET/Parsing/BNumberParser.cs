using System;
using System.Linq;
using System.Text;
using BencodeNET.Exceptions;
using BencodeNET.IO;
using BencodeNET.Objects;

namespace BencodeNET.Parsing
{
    /// <summary>
    /// A parser for bencoded numbers.
    /// </summary>
    public class BNumberParser : BObjectParser<BNumber>
    {
        private StringBuilder Digits { get; } = new StringBuilder(BNumber.MaxDigits);

        /// <summary>
        /// The minimum stream length in bytes for a valid number ('i0e').
        /// </summary>
        protected const int MinimumLength = 3;

        /// <summary>
        /// The encoding used for parsing.
        /// </summary>
        public override Encoding Encoding => Encoding.UTF8;

        /// <summary>
        /// Parses the next <see cref="BNumber"/> from the reader.
        /// </summary>
        /// <param name="reader">The reader to parse from.</param>
        /// <returns>The parsed <see cref="BNumber"/>.</returns>
        /// <exception cref="InvalidBencodeException{BNumber}">Invalid bencode.</exception>
        /// <exception cref="UnsupportedBencodeException{BNumber}">The bencode is unsupported by this library.</exception>
        public override BNumber Parse(BencodeReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            if (reader.Length < MinimumLength)
                throw InvalidBencodeException<BNumber>.BelowMinimumLength(MinimumLength, reader.Length.Value, reader.Position);

            var startPosition = reader.Position;

            // Numbers must start with 'i'
            if (reader.ReadChar() != 'i')
                throw InvalidBencodeException<BNumber>.UnexpectedChar('i', reader.PreviousChar, startPosition);

            Digits.Clear();
            for (var c = reader.ReadChar(); c != default && c != 'e'; c = reader.ReadChar())
            {
                Digits.Append(c);
            }

            if (Digits.Length == 0)
                throw NoDigitsException(startPosition);

            // Last read character should be 'e'
            if (reader.PreviousChar != 'e')
                throw InvalidBencodeException<BNumber>.MissingEndChar(startPosition);

            var isNegative = Digits[0] == '-';
            var numberOfDigits = isNegative ? Digits.Length - 1 : Digits.Length;

            // We do not support numbers that cannot be stored as a long (Int64)
            if (numberOfDigits > BNumber.MaxDigits)
            {
                throw UnsupportedException(
                    $"The number '{Digits}' has more than 19 digits and cannot be stored as a long (Int64) and therefore is not supported.",
                    startPosition);
            }

            // We need at least one digit
            if (numberOfDigits < 1)
                throw NoDigitsException(startPosition);

            var firstDigit = isNegative ? Digits[1] : Digits[0];

            // Leading zeros are not valid
            if (firstDigit == '0' && numberOfDigits > 1)
                throw InvalidException($"Leading '0's are not valid. Found value '{Digits}'.", startPosition);

            // '-0' is not valid either
            if (firstDigit == '0' && numberOfDigits == 1 && isNegative)
                throw InvalidException("'-0' is not a valid number.", startPosition);

            if (!ParseUtil.TryParseLongFast(Digits.ToString(), out var number))
            {
                var nonSignChars = isNegative ? Digits.ToString(1, Digits.Length - 1) : Digits.ToString();
                if (nonSignChars.Any(x => !x.IsDigit()))
                    throw InvalidException($"The value '{Digits}' is not a valid number.", startPosition);

                throw UnsupportedException(
                    $"The value '{Digits}' is not a valid long (Int64). Supported values range from '{long.MinValue:N0}' to '{long.MaxValue:N0}'.",
                    startPosition);
            }

            return new BNumber(number);
        }

        private static InvalidBencodeException<BNumber> NoDigitsException(long startPosition)
        {
            return new InvalidBencodeException<BNumber>(
                $"It contains no digits. The number starts at position {startPosition}.",
                startPosition);
        }

        private static InvalidBencodeException<BNumber> InvalidException(string message, long startPosition)
        {
            return new InvalidBencodeException<BNumber>(
                $"{message} The number starts at position {startPosition}.",
                startPosition);
        }

        private static UnsupportedBencodeException<BNumber> UnsupportedException(string message, long startPosition)
        {
            return new UnsupportedBencodeException<BNumber>(
                $"{message} The number starts at position {startPosition}.",
                startPosition);
        }
    }
}
