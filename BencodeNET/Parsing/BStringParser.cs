using System;
using System.Text;
using BencodeNET.Exceptions;
using BencodeNET.IO;
using BencodeNET.Objects;

namespace BencodeNET.Parsing
{
    /// <summary>
    /// A parser for bencoded byte strings.
    /// </summary>
    public class BStringParser : BObjectParser<BString>
    {
        private StringBuilder LengthString { get; } = new StringBuilder(BString.LengthMaxDigits);

        /// <summary>
        /// The minimum stream length in bytes for a valid string ('0:').
        /// </summary>
        protected const int MinimumLength = 2;

        /// <summary>
        /// Creates an instance using <see cref="System.Text.Encoding.UTF8"/> for parsing.
        /// </summary>
        public BStringParser()
            : this(Encoding.UTF8)
        { }

        /// <summary>
        /// Creates an instance using the specified encoding for parsing.
        /// </summary>
        /// <param name="encoding"></param>
        public BStringParser(Encoding encoding)
        {
            Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        /// <summary>
        /// The encoding used when creating the <see cref="BString"/> when parsing.
        /// </summary>
        public override Encoding Encoding { get; }

        /// <summary>
        /// Parses the next <see cref="BString"/> from the reader.
        /// </summary>
        /// <param name="reader">The reader to parse from.</param>
        /// <returns>The parsed <see cref="BString"/>.</returns>
        /// <exception cref="InvalidBencodeException{BString}">Invalid bencode.</exception>
        /// <exception cref="UnsupportedBencodeException{BString}">The bencode is unsupported by this library.</exception>
        public override BString Parse(BencodeReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            // Minimum valid bencode string is '0:' meaning an empty string
            if (reader.Length < MinimumLength)
                throw InvalidBencodeException<BString>.BelowMinimumLength(MinimumLength, reader.Length.Value, reader.Position);

            var startPosition = reader.Position;

            LengthString.Clear();
            for (var c = reader.ReadChar(); c != default && c.IsDigit(); c = reader.ReadChar())
            {
                // Because of memory limitations (~1-2 GB) we know for certain we cannot handle more than 10 digits (10GB)
                if (LengthString.Length >= BString.LengthMaxDigits)
                {
                    throw UnsupportedException(
                        $"Length of string is more than {BString.LengthMaxDigits} digits (>10GB) and is not supported (max is ~1-2GB).",
                        startPosition);
                }

                LengthString.Append(c);
            }

            if (reader.PreviousChar != ':')
                throw InvalidBencodeException<BString>.UnexpectedChar(':', reader.PreviousChar, reader.Position - 1);

            if (!ParseUtil.TryParseLongFast(LengthString.ToString(), out var stringLength))
                throw InvalidException($"Invalid length '{LengthString}' of string.", startPosition);

            // Int32.MaxValue is ~2GB and is the absolute maximum that can be handled in memory
            if (stringLength > int.MaxValue)
            {
                throw UnsupportedException(
                    $"Length of string is {stringLength:N0} but maximum supported length is {int.MaxValue:N0}.",
                    startPosition);
            }

            var bytes = new byte[stringLength];
            var bytesRead = reader.Read(bytes);

            // If the two don't match we've reached the end of the stream before reading the expected number of chars
            if (bytesRead != stringLength)
            {
                throw InvalidException(
                    $"Expected string to be {stringLength:N0} bytes long but could only read {bytes.Length:N0} bytes.",
                    startPosition);
            }

            return new BString(bytes, Encoding);
        }

        private static InvalidBencodeException<BString> InvalidException(string message, long startPosition)
        {
            return new InvalidBencodeException<BString>(
                $"{message} The string starts at position {startPosition}.",
                startPosition);
        }

        private static UnsupportedBencodeException<BString> UnsupportedException(string message, long startPosition)
        {
            return new UnsupportedBencodeException<BString>(
                $"{message} The string starts at position {startPosition}.",
                startPosition);
        }
    }
}
