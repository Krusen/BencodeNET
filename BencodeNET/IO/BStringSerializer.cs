using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BencodeNET.Exceptions;
using BencodeNET.Objects;
using BencodeNET.Parsing;

namespace BencodeNET.IO
{
    public interface IBObjectSerializer<T> where T : IBObject
    {
        ValueTask<T> DeserializeAsync(PipeReader reader, CancellationToken cancellationToken = default);

        ValueTask<FlushResult> SerializeAsync(PipeWriter writer, T value, CancellationToken cancellationToken = default);
    }

    public class BStringSerializer : IBObjectSerializer<BString>
    {
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public ValueTask<BString> DeserializeAsync(PipeReader reader, CancellationToken cancellationToken = default)
        {
            return DeserializeAsync(new PipeBencodeReader(reader), cancellationToken);
        }

        private async ValueTask<BString> DeserializeAsync(PipeBencodeReader reader, CancellationToken cancellationToken = default)
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

        public ValueTask<FlushResult> SerializeAsync(PipeWriter writer, BString value, CancellationToken cancellationToken = default)
        {
            // Init
            var size = value.GetSizeInBytes();
            var buffer = writer.GetMemory(size);

            // Write length
            var writtenBytes = value.Encoding.GetBytes(value.Value.Length.ToString().AsSpan(), buffer.Span);

            // Write ':'
            buffer.Span[writtenBytes] = (byte) ':';

            // Write value
            value.Value.CopyTo(buffer.Slice(writtenBytes + 1));

            // Commit
            writer.Advance(size);

            return writer.FlushAsync(cancellationToken);
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
