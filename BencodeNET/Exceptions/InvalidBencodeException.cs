using System;

#pragma warning disable 1591
namespace BencodeNET.Exceptions
{
    /// <summary>
    /// Represents parse errors when encountering invalid bencode of some sort.
    /// </summary>
    /// <typeparam name="T">The type being parsed.</typeparam>
    public class InvalidBencodeException<T> : BencodeException<T>
    {
        /// <summary>
        /// The position in the stream where the error happened or
        /// the starting position of the parsed object that caused the error.
        /// </summary>
        public long StreamPosition { get; set; }

        public InvalidBencodeException()
        { }

        public InvalidBencodeException(string message)
            : base(message)
        { }

        public InvalidBencodeException(string message, Exception inner)
            : base(message, inner)
        { }

        public InvalidBencodeException(string message, Exception inner, long streamPosition)
            : base($"Failed to parse {typeof(T).Name}. {message}", inner)
        {
            StreamPosition = Math.Max(0, streamPosition);
        }

        public InvalidBencodeException(string message, long streamPosition)
            : base($"Failed to parse {typeof(T).Name}. {message}")
        {
            StreamPosition = Math.Max(0, streamPosition);
        }

        internal static InvalidBencodeException<T> InvalidBeginningChar(char? invalidChar, long streamPosition)
        {
            var message =
                $"Invalid beginning character of object. Found '{invalidChar}' at position {streamPosition}. Valid characters are: 0-9, 'i', 'l' and 'd'";
            return new InvalidBencodeException<T>(message, streamPosition);
        }

        internal static InvalidBencodeException<T> MissingEndChar(long streamPosition)
        {
            var message = "Missing end character of object. Expected 'e' but reached the end of the stream.";
            return new InvalidBencodeException<T>(message, streamPosition);
        }

        internal static InvalidBencodeException<T> BelowMinimumLength(int minimumLength, long actualLength, long streamPosition)
        {
            var message =
                $"Invalid length. Minimum valid stream length for parsing '{typeof (T).FullName}' is {minimumLength} but the actual length was only {actualLength}.";
            return new InvalidBencodeException<T>(message, streamPosition);
        }

        internal static InvalidBencodeException<T> UnexpectedChar(char expected, char? unexpected, long streamPosition)
        {
            var message = unexpected == null
                ? $"Unexpected character. Expected '{expected}' but reached end of stream."
                : $"Unexpected character. Expected '{expected}' but found '{unexpected}' at position {streamPosition}.";
            return new InvalidBencodeException<T>(message, streamPosition);
        }
    }
}