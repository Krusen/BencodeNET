using System;
#if !NETSTANDARD
using System.Runtime.Serialization;
#endif

namespace BencodeNET.Exceptions
{
#if !NETSTANDARD
    [Serializable]
#endif
    public class InvalidBencodeException<T> : BencodeException<T>
    {
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
            StreamPosition = streamPosition;
        }

        public InvalidBencodeException(string message, long streamPosition)
            : base($"Failed to parse {typeof(T).Name}. {message}")
        {
            StreamPosition = streamPosition;
        }

#if !NETSTANDARD
        protected InvalidBencodeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info == null) return;
            StreamPosition = info.GetInt64(nameof(StreamPosition));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(StreamPosition), StreamPosition);
        }
#endif

        public static InvalidBencodeException<T> InvalidBeginningChar(char invalidChar, long streamPosition)
        {
            var message =
                $"Invalid beginning character of object. Found '{invalidChar}' at position {streamPosition}. Valid characters are: 0-9, 'i', 'l' and 'd'";
            return new InvalidBencodeException<T>(message, streamPosition);
        }

        public static InvalidBencodeException<T> InvalidEndChar(char invalidChar, long streamPosition)
        {
            var message =
                $"Invalid end character of object. Expected 'e' but found '{invalidChar}' at position {streamPosition}.";
            return new InvalidBencodeException<T>(message, streamPosition);
        }

        public static InvalidBencodeException<T> MissingEndChar()
        {
            var message = "Missing end character of object. Expected 'e' but reached the end of the stream.";
            return new InvalidBencodeException<T>(message);
        }

        public static InvalidBencodeException<T> BelowMinimumLength(int minimumLength, long actualLength, long streamPosition)
        {
            var message =
                $"Invalid length. Minimum valid stream length for parsing '{typeof (T).FullName}' is {minimumLength} but the actual length was only {actualLength}.";
            return new InvalidBencodeException<T>(message, streamPosition);
        }

        public static InvalidBencodeException<T> UnexpectedChar(char expected, char unexpected, long streamPosition)
        {
            var message = $"Unexpected character. Expected '{expected}' but found '{unexpected}' at position {streamPosition}.";
            return new InvalidBencodeException<T>(message, streamPosition);
        }
    }
}