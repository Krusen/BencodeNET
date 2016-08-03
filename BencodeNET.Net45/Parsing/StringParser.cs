using System;
using System.Text;
using System.Threading.Tasks;
using BencodeNET.Exceptions;
using BencodeNET.IO;
using BencodeNET.Objects;

namespace BencodeNET.Parsing
{
    public class StringParser : BObjectParser<BString>
    {
        protected const int MinimumLength = 2;

        public StringParser(IBencodeParser bencodeParser)
        {
            if (bencodeParser == null) throw new ArgumentNullException(nameof(bencodeParser));

            BencodeParser = bencodeParser;
        }

        protected IBencodeParser BencodeParser { get; set; }

        public override BString Parse(BencodeStream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            // Minimum valid bencode string is '0:' meaning an empty string
            if (stream.Length < MinimumLength)
                throw new BencodeParsingException<BString>($"Minimum valid stream length is {MinimumLength} (an empty string: '0:')", stream.Position);

            var startPosition = stream.Position;

            var lengthString = new StringBuilder();
            for (var c = stream.ReadChar(); c != ':' && c != default(char); c = stream.ReadChar())
            {
                // Because of memory limitations (~1-2 GB) we know for certain we cannot handle more than 10 digits (10GB)
                if (lengthString.Length >= BString.LengthMaxDigits)
                {
                    throw new UnsupportedBencodeException(
                        $"Length of string is more than {BString.LengthMaxDigits} digits (>10GB) and is not supported (max is ~1-2GB).",
                        stream.Position);
                }

                lengthString.Append(c);
            }

            long stringLength;
            if (!ParseUtil.TryParseLongFast(lengthString.ToString(), out stringLength))
            {
                throw new BencodeParsingException<BString>($"Invalid length of string '{lengthString}'", startPosition);
            }

            // Int32.MaxValue is ~2GB and is the absolute maximum that can be handled in memory
            if (stringLength > int.MaxValue)
            {
                throw new UnsupportedBencodeException(
                    $"Length of string is {stringLength:N0} but maximum supported length is {int.MaxValue:N0}.",
                    stream.Position);
            }

            var bytes = stream.Read((int)stringLength);

            // If the two don't match we've reached the end of the stream before reading the expected number of chars
            if (bytes.Length != stringLength)
            {
                throw new BencodeParsingException<BString>(
                    $"Expected string to be {stringLength:N0} bytes long but could only read {bytes.Length:N0} bytes.",
                    stream.Position);
            }

            return new BString(bytes, BencodeParser.Encoding);
        }

        public override async Task<BString> ParseAsync(BencodeStream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            // Minimum valid bencode string is '0:' meaning an empty string
            if (stream.Length < MinimumLength)
                throw new BencodeParsingException<BString>($"Minimum valid stream length is {MinimumLength} (an empty string: '0:')", stream.Position);

            var startPosition = stream.Position;

            var lengthString = new StringBuilder();
            for (var c = await stream.ReadCharAsync().ConfigureAwait(false);
                c != ':' && c != default(char);
                c = await stream.ReadCharAsync().ConfigureAwait(false))
            {
                // Because of memory limitations (~1-2 GB) we know for certain we cannot handle more than 10 digits (10GB)
                if (lengthString.Length >= BString.LengthMaxDigits)
                {
                    throw new UnsupportedBencodeException(
                        $"Length of string is more than {BString.LengthMaxDigits} digits (>10GB) and is not supported (max is ~1-2GB).",
                        stream.Position);
                }

                lengthString.Append(c);
            }

            long stringLength;
            if (!ParseUtil.TryParseLongFast(lengthString.ToString(), out stringLength))
            {
                throw new BencodeParsingException<BString>($"Invalid length of string '{lengthString}'", startPosition);
            }

            // Int32.MaxValue is ~2GB and is the absolute maximum that can be handled in memory
            if (stringLength > int.MaxValue)
            {
                throw new UnsupportedBencodeException(
                    $"Length of string is {stringLength:N0} but maximum supported length is {int.MaxValue:N0}.",
                    stream.Position);
            }

            var bytes = await stream.ReadAsync((int)stringLength).ConfigureAwait(false);

            // If the two don't match we've reached the end of the stream before reading the expected number of chars
            if (bytes.Length != stringLength)
            {
                throw new BencodeParsingException<BString>(
                    $"Expected string to be {stringLength:N0} bytes long but could only read {bytes.Length:N0} bytes.",
                    stream.Position);
            }

            return new BString(bytes, BencodeParser.Encoding);
        }
    }
}
