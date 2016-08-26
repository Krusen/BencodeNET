using System;
using System.Linq;
using System.Text;
using BencodeNET.Exceptions;
using BencodeNET.IO;
using BencodeNET.Objects;

namespace BencodeNET.Parsing
{
    public class BNumberParser : BObjectParser<BNumber>
    {
        protected const int MinimumLength = 3;

        protected override Encoding Encoding => Encoding.UTF8;

        public override BNumber Parse(BencodeStream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            if (stream.Length < MinimumLength)
                throw InvalidBencodeException<BNumber>.BelowMinimumLength(MinimumLength, stream.Length, stream.Position);

            var startPosition = stream.Position;

            // Numbers must start with 'i'
            if (stream.ReadChar() != 'i')
                throw InvalidBencodeException<BNumber>.UnexpectedChar('i', stream.ReadPreviousChar(), stream.Position);

            var digits = new StringBuilder();
            char c;
            for (c = stream.ReadChar(); c != 'e' && c != default(char); c = stream.ReadChar())
            {
                digits.Append(c);
            }

            // Last read character should be 'e'
            if (c != 'e')
                throw InvalidBencodeException<BNumber>.InvalidEndChar(c, stream.Position);

            var isNegative = digits[0] == '-';
            var numberOfDigits = isNegative ? digits.Length - 1 : digits.Length;

            // We do not support numbers that cannot be stored as a long (Int64)
            if (numberOfDigits > BNumber.MaxDigits)
            {
                throw new UnsupportedBencodeException<BNumber>(
                    $"The number '{digits}' has more than 19 digits and cannot be stored as a long (Int64) and therefore is not supported.",
                    stream.Position);
            }

            // We need at least one digit
            if (numberOfDigits < 1)
            {
                throw new InvalidBencodeException<BNumber>(
                    $"It contains no digits. Number starts at position {startPosition}.", startPosition);
            }

            var firstDigit = isNegative ? digits[1] : digits[0];

            // Leading zeros are not valid
            if (firstDigit == '0' && numberOfDigits > 1)
            {
                throw new InvalidBencodeException<BNumber>(
                    $"Leading '0's are not valid. Found value '{digits}'. Number starts at position {startPosition}.",
                    startPosition);
            }

            // '-0' is not valid either
            if (firstDigit == '0' && numberOfDigits == 1 && isNegative)
            {
                throw new InvalidBencodeException<BNumber>(
                    $"'-0' is not a valid number. Number starts at position {startPosition}.", startPosition);
            }

            long number;
            if (!ParseUtil.TryParseLongFast(digits.ToString(), out number))
            {
                var nonSignChars = isNegative ? digits.ToString(1, digits.Length - 1) : digits.ToString();
                if (nonSignChars.Any(x => !x.IsDigit()))
                {
                    throw new InvalidBencodeException<BNumber>(
                        $"The value '{digits}' is not a valid number. Number starts at position {startPosition}.");
                }

                throw new UnsupportedBencodeException<BNumber>(
                    $"The value '{digits}' is not a valid long (Int64). Supported values range from '{long.MinValue:N0}' to '{long.MaxValue:N0}'. Number starts at position {startPosition}.",
                    startPosition);
            }

            return new BNumber(number);
        }

        // TODO: Helper method for throwing exception with "Number starts at position ..." message appended
    }
}
