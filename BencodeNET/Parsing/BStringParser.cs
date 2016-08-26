using System;
using System.Text;
using BencodeNET.Exceptions;
using BencodeNET.IO;
using BencodeNET.Objects;

namespace BencodeNET.Parsing
{
    public class BStringParser : BObjectParser<BString>
    {
        protected const int MinimumLength = 2;

        public BStringParser()
            : this(Encoding.UTF8)
        { }

        public BStringParser(Encoding encoding)
        {
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            Encoding = encoding;
        }

        protected override Encoding Encoding { get; }

        public override BString Parse(BencodeStream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            // Minimum valid bencode string is '0:' meaning an empty string
            if (stream.Length < MinimumLength)
                throw InvalidBencodeException<BString>.BelowMinimumLength(MinimumLength, stream.Length, stream.Position);

            var startPosition = stream.Position;

            var lengthString = new StringBuilder();
            for (var c = stream.ReadChar(); c != ':' && c != default(char); c = stream.ReadChar())
            {
                // Because of memory limitations (~1-2 GB) we know for certain we cannot handle more than 10 digits (10GB)
                if (lengthString.Length >= BString.LengthMaxDigits)
                {
                    throw new UnsupportedBencodeException<BString>(
                        $"Length of string is more than {BString.LengthMaxDigits} digits (>10GB) and is not supported (max is ~1-2GB). String starts at position {startPosition}.",
                        stream.Position);
                }

                lengthString.Append(c);
            }

            long stringLength;
            if (!ParseUtil.TryParseLongFast(lengthString.ToString(), out stringLength))
            {
                throw new InvalidBencodeException<BString>(
                    $"Invalid length '{lengthString}' of string. String starts at position {startPosition}.",
                    startPosition);
            }

            // Int32.MaxValue is ~2GB and is the absolute maximum that can be handled in memory
            if (stringLength > int.MaxValue)
            {
                throw new UnsupportedBencodeException<BString>(
                    $"Length of string is {stringLength:N0} but maximum supported length is {int.MaxValue:N0}. String starts at position {startPosition}.",
                    stream.Position);
            }

            var bytes = stream.Read((int)stringLength);

            // If the two don't match we've reached the end of the stream before reading the expected number of chars
            if (bytes.Length != stringLength)
            {
                throw new InvalidBencodeException<BString>(
                    $"Expected string to be {stringLength:N0} bytes long but could only read {bytes.Length:N0} bytes. String starts at position {startPosition}.",
                    stream.Position);
            }

            return new BString(bytes, Encoding);
        }
    }
}
