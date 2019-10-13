using System;
using System.Buffers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        /// <summary>
        /// The encoding used when creating the <see cref="BString"/> when parsing.
        /// </summary>
        public override Encoding Encoding => _encoding;
        private Encoding _encoding;

        /// <summary>
        /// Changes the encoding used for parsing.
        /// </summary>
        /// <param name="encoding">The new encoding to use.</param>
        public void ChangeEncoding(Encoding encoding)
        {
            _encoding = encoding;
        }

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

            var buffer = ArrayPool<char>.Shared.Rent(BString.LengthMaxDigits);
            try
            {
                var lengthString = buffer.AsSpan();
                var lengthStringCount = 0;
                for (var c = reader.ReadChar(); c != default && c.IsDigit(); c = reader.ReadChar())
                {
                    EnsureLengthStringBelowMaxLength(lengthStringCount, startPosition);

                    lengthString[lengthStringCount++] = c;
                }

                EnsurePreviousCharIsColon(reader.PreviousChar, reader.Position);

                var stringLength = ParseStringLength(lengthString, lengthStringCount, startPosition);
                var bytes = new byte[stringLength];
                var bytesRead = reader.Read(bytes);

                EnsureExpectedBytesRead(bytesRead, stringLength, startPosition);

                return new BString(bytes, Encoding);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Parses the next <see cref="BString"/> from the reader.
        /// </summary>
        /// <param name="reader">The reader to parse from.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The parsed <see cref="BString"/>.</returns>
        /// <exception cref="InvalidBencodeException{BString}">Invalid bencode.</exception>
        /// <exception cref="UnsupportedBencodeException{BString}">The bencode is unsupported by this library.</exception>
        public override async ValueTask<BString> ParseAsync(PipeBencodeReader reader, CancellationToken cancellationToken = default)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            var startPosition = reader.Position;

            using (var memoryOwner = MemoryPool<char>.Shared.Rent(BString.LengthMaxDigits))
            {
                var lengthString = memoryOwner.Memory;
                var lengthStringCount = 0;
                for (var c = await reader.ReadCharAsync(cancellationToken).ConfigureAwait(false);
                    c != default && c.IsDigit();
                    c = await reader.ReadCharAsync(cancellationToken).ConfigureAwait(false))
                {
                    EnsureLengthStringBelowMaxLength(lengthStringCount, startPosition);

                    lengthString.Span[lengthStringCount++] = c;
                }

                EnsurePreviousCharIsColon(reader.PreviousChar, reader.Position);

                var stringLength = ParseStringLength(lengthString.Span, lengthStringCount, startPosition);
                var bytes = new byte[stringLength];
                var bytesRead = await reader.ReadAsync(bytes, cancellationToken).ConfigureAwait(false);

                EnsureExpectedBytesRead(bytesRead, stringLength, startPosition);

                return new BString(bytes, Encoding);
            }
        }

        /// <summary>
        /// Ensures that the length (number of digits) of the string-length part is not above <see cref="BString.LengthMaxDigits"/>
        /// as that would equal 10 GB of data, which we cannot handle.
        /// </summary>
        private void EnsureLengthStringBelowMaxLength(int lengthStringCount, long startPosition)
        {
            // Because of memory limitations (~1-2 GB) we know for certain we cannot handle more than 10 digits (10GB)
            if (lengthStringCount >= BString.LengthMaxDigits)
            {
                throw UnsupportedException(
                    $"Length of string is more than {BString.LengthMaxDigits} digits (>10GB) and is not supported (max is ~1-2GB).",
                    startPosition);
            }
        }

        /// <summary>
        /// Ensure that the previously read char is a colon (:),
        /// separating the string-length part and the actual string value.
        /// </summary>
        private void EnsurePreviousCharIsColon(char previousChar, long position)
        {
            if (previousChar != ':') throw InvalidBencodeException<BString>.UnexpectedChar(':', previousChar, position - 1);
        }

        /// <summary>
        /// Parses the string-length <see cref="string"/> into a <see cref="long"/>.
        /// </summary>
        private long ParseStringLength(Span<char> lengthString, int lengthStringCount, long startPosition)
        {
            lengthString = lengthString.Slice(0, lengthStringCount);

            if (!ParseUtil.TryParseLongFast(lengthString, out var stringLength))
                throw InvalidException($"Invalid length '{lengthString.AsString()}' of string.", startPosition);

            // Int32.MaxValue is ~2GB and is the absolute maximum that can be handled in memory
            if (stringLength > int.MaxValue)
            {
                throw UnsupportedException(
                    $"Length of string is {stringLength:N0} but maximum supported length is {int.MaxValue:N0}.",
                    startPosition);
            }

            return stringLength;
        }

        /// <summary>
        /// Ensures that number of bytes read matches the expected number parsed from the string-length part.
        /// </summary>
        private void EnsureExpectedBytesRead(long bytesRead, long stringLength, long startPosition)
        {
            // If the two don't match we've reached the end of the stream before reading the expected number of chars
            if (bytesRead == stringLength) return;

            throw InvalidException(
                $"Expected string to be {stringLength:N0} bytes long but could only read {bytesRead:N0} bytes.",
                startPosition);
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
