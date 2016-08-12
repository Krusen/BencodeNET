using System;
#if !NETSTANDARD
using System.Runtime.Serialization;
#endif
using BencodeNET.Objects;

namespace BencodeNET.Exceptions
{
    // TODO: Rename to InvalidBencodeException?
    // TODO: More specific exceptions, e.g. InvalidFirstChar, MissingEndChar, WrongLength, BelowMinimumLength etc.
    public class BencodeParsingException : Exception
    {
        public long StreamPosition { get; set; }

        public BencodeParsingException()
        { }

        public BencodeParsingException(string message)
            : base(message)
        { }

        public BencodeParsingException(string message, long streamPosition)
            : base(message)
        {
            StreamPosition = streamPosition;
        }

        protected static string CreateMessage(string message, long streamPosition)
        {
            if (streamPosition > -1)
            {
                message = message.Trim();

                if (!message.EndsWith("."))
                    message += ".";

                message += " Parsing failed at position " + streamPosition;
            }

            return message;
        }

        protected static string CreateMessage<T>(string message, long streamPosition)
        {
            var output = "";
            if (typeof(T) == typeof(BString))
                output = "Invalid bencoded string. ";
            if (typeof(T) == typeof(BNumber))
                output = "Invalid bencoded number. ";
            if (typeof(T) == typeof(BList))
                output = "Invalid bencoded list. ";
            if (typeof(T) == typeof(BDictionary))
                output = "Invalid bencoded dictionary. ";

            output += message;

            return CreateMessage(output, streamPosition);
        }

#if !NETSTANDARD
        protected BencodeParsingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info == null)
                return;

            StreamPosition = info.GetInt64("StreamPosition");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("StreamPosition", StreamPosition);
        }
#endif
    }

#if !NETSTANDARD
    [Serializable]
#endif
    public class BencodeParsingException<T> : BencodeParsingException
    {
        public BencodeParsingException()
        { }

        public BencodeParsingException(string message)
            : base(message)
        { }

        public BencodeParsingException(string message, long streamPosition)
            : base(CreateMessage<T>(message, streamPosition), streamPosition)
        { }
    }
}
