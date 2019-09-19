using System;
using System.Buffers;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

            using (var digits = MemoryPool<char>.Shared.Rent(BNumber.MaxDigits))
            {
                var digitCount = 0;
                for (var c = reader.ReadChar(); c != default && c != 'e'; c = reader.ReadChar())
                {
                    digits.Memory.Span[digitCount++] = c;
                }

                if (digitCount == 0)
                    throw NoDigitsException(startPosition);

                // Last read character should be 'e'
                if (reader.PreviousChar != 'e')
                    throw InvalidBencodeException<BNumber>.MissingEndChar(startPosition);

                return ParseNumber(digits.Memory.Span.Slice(0, digitCount), startPosition);
            }
        }

        /// <summary>
        /// Parses the next <see cref="BNumber"/> from the reader.
        /// </summary>
        /// <param name="reader">The reader to parse from.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The parsed <see cref="BNumber"/>.</returns>
        /// <exception cref="InvalidBencodeException{BNumber}">Invalid bencode.</exception>
        /// <exception cref="UnsupportedBencodeException{BNumber}">The bencode is unsupported by this library.</exception>
        public override async ValueTask<BNumber> ParseAsync(PipeBencodeReader reader, CancellationToken cancellationToken = default)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            var startPosition = reader.Position;

            // Numbers must start with 'i'
            if (await reader.ReadCharAsync(cancellationToken).ConfigureAwait(false) != 'i')
                throw InvalidBencodeException<BNumber>.UnexpectedChar('i', reader.PreviousChar, startPosition);

            using (var memoryOwner = MemoryPool<char>.Shared.Rent(BNumber.MaxDigits))
            {
                var digits = memoryOwner.Memory;
                var digitCount = 0;
                for (var c = await reader.ReadCharAsync(cancellationToken).ConfigureAwait(false);
                    c != default && c != 'e';
                    c = await reader.ReadCharAsync(cancellationToken).ConfigureAwait(false))
                {
                    digits.Span[digitCount++] = c;
                }

                if (digitCount == 0)
                    throw NoDigitsException(startPosition);

                // Last read character should be 'e'
                if (reader.PreviousChar != 'e')
                    throw InvalidBencodeException<BNumber>.MissingEndChar(startPosition);

                return ParseNumber(digits.Span.Slice(0, digitCount), startPosition);
            }
        }

        private BNumber ParseNumber(in ReadOnlySpan<char> digits, long startPosition)
        {
            var isNegative = digits[0] == '-';
            var numberOfDigits = isNegative ? digits.Length - 1 : digits.Length;

            // We do not support numbers that cannot be stored as a long (Int64)
            if (numberOfDigits > BNumber.MaxDigits)
            {
                throw UnsupportedException(
                    $"The number '{digits.AsString()}' has more than 19 digits and cannot be stored as a long (Int64) and therefore is not supported.",
                    startPosition);
            }

            // We need at least one digit
            if (numberOfDigits < 1)
                throw NoDigitsException(startPosition);

            var firstDigit = isNegative ? digits[1] : digits[0];

            // Leading zeros are not valid
            if (firstDigit == '0' && numberOfDigits > 1)
                throw InvalidException($"Leading '0's are not valid. Found value '{digits.AsString()}'.", startPosition);

            // '-0' is not valid either
            if (firstDigit == '0' && numberOfDigits == 1 && isNegative)
                throw InvalidException("'-0' is not a valid number.", startPosition);

            if (!ParseUtil.TryParseLongFast(digits, out var number))
            {
                var nonSignChars = isNegative ? digits.Slice(1) : digits;
                if (nonSignChars.AsString().Any(x => !x.IsDigit()))
                    throw InvalidException($"The value '{digits.AsString()}' is not a valid number.", startPosition);

                throw UnsupportedException(
                    $"The value '{digits.AsString()}' is not a valid long (Int64). Supported values range from '{long.MinValue:N0}' to '{long.MaxValue:N0}'.",
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
