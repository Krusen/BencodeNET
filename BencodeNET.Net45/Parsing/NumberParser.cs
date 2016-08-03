using System;
using System.Text;
using System.Threading.Tasks;
using BencodeNET.Exceptions;
using BencodeNET.IO;
using BencodeNET.Objects;

namespace BencodeNET.Parsing
{
    public class NumberParser : BObjectParser<BNumber>
    {
        protected const int MinimumLength = 3;

        public override BNumber Parse(BencodeStream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            if (stream.Length < MinimumLength)
                throw new BencodeParsingException<BNumber>($"Minimum valid length of stream is {MinimumLength} ('i0e').", stream.Position);

            var startPosition = stream.Position;

            // Numbers must start with 'i'
            if (stream.ReadChar() != 'i')
                throw new BencodeParsingException<BNumber>(
                    $"Must begin with 'i' but began with '{stream.ReadPreviousChar()}'.", stream.Position);

            var digits = new StringBuilder();
            char c;
            for (c = stream.ReadChar(); c != 'e' && c != default(char); c = stream.ReadChar())
            {
                digits.Append(c);
            }

            // Last read character should be 'e'
            if (c != 'e')
                throw new BencodeParsingException<BNumber>("Missing end character 'e'.", stream.Position);

            var isNegative = digits[0] == '-';
            var numberOfDigits = isNegative ? digits.Length - 1 : digits.Length;

            // We do not support numbers that cannot be stored as a long (Int64)
            if (numberOfDigits > BNumber.MaxDigits)
            {
                throw new UnsupportedBencodeException(
                    $"The number '{digits}' has more than 19 digits and cannot be stored as a long (Int64) and therefore is not supported.",
                    stream.Position);
            }

            // We need at least one digit
            if (numberOfDigits < 1)
                throw new BencodeParsingException<BNumber>("It contains no digits.", startPosition);

            var firstDigit = isNegative ? digits[1] : digits[0];

            // Leading zeros are not valid
            if (firstDigit == '0' && numberOfDigits > 1)
                throw new BencodeParsingException<BNumber>("Leading '0's are not valid.", startPosition);

            // '-0' is not valid either
            if (firstDigit == '0' && numberOfDigits == 1 && isNegative)
                throw new BencodeParsingException<BNumber>("'-0' is not a valid number.", startPosition);

            long number;
            if (!ParseUtil.TryParseLongFast(digits.ToString(), out number))
            {
                throw new BencodeParsingException<BNumber>(
                    $"The value '{digits}' is not a valid long (Int64). Supported values range from '{long.MinValue:N0}' to '{long.MaxValue:N0}'.",
                    stream.Position);
            }

            return new BNumber(number);
        }

        public override async Task<BNumber> ParseAsync(BencodeStream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            if (stream.Length < MinimumLength)
                throw new BencodeParsingException<BNumber>($"Minimum valid length of stream is {MinimumLength} ('i0e').", stream.Position);

            var startPosition = stream.Position;

            // Numbers must start with 'i'
            if (await stream.ReadCharAsync().ConfigureAwait(false) != 'i')
                throw new BencodeParsingException<BNumber>(
                    $"Must begin with 'i' but began with '{await stream.ReadPreviousCharAsync().ConfigureAwait(false)}'.", stream.Position);

            var digits = new StringBuilder();
            char c;
            for (c = await stream.ReadCharAsync().ConfigureAwait(false);
                c != 'e' && c != default(char);
                c = await stream.ReadCharAsync().ConfigureAwait(false))
            {
                digits.Append(c);
            }

            // Last read character should be 'e'
            if (c != 'e')
                throw new BencodeParsingException<BNumber>("Missing end character 'e'.", stream.Position);

            var isNegative = digits[0] == '-';
            var numberOfDigits = isNegative ? digits.Length - 1 : digits.Length;

            // We do not support numbers that cannot be stored as a long (Int64)
            if (numberOfDigits > BNumber.MaxDigits)
            {
                throw new UnsupportedBencodeException(
                    $"The number '{digits}' has more than 19 digits and cannot be stored as a long (Int64) and therefore is not supported.",
                    stream.Position);
            }

            // We need at least one digit
            if (numberOfDigits < 1)
                throw new BencodeParsingException<BNumber>("It contains no digits.", startPosition);

            var firstDigit = isNegative ? digits[1] : digits[0];

            // Leading zeros are not valid
            if (firstDigit == '0' && numberOfDigits > 1)
                throw new BencodeParsingException<BNumber>("Leading '0's are not valid.", startPosition);

            // '-0' is not valid either
            if (firstDigit == '0' && numberOfDigits == 1 && isNegative)
                throw new BencodeParsingException<BNumber>("'-0' is not a valid number.", startPosition);

            long number;
            if (!ParseUtil.TryParseLongFast(digits.ToString(), out number))
            {
                throw new BencodeParsingException<BNumber>(
                    $"The value '{digits}' is not a valid long (Int64). Supported values range from '{long.MinValue:N0}' to '{long.MaxValue:N0}'.",
                    stream.Position);
            }

            return new BNumber(number);
        }
    }
}
