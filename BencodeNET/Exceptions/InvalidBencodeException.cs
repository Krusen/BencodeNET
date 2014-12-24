using System;
using System.Runtime.Serialization;

namespace BencodeNET.Exceptions
{
    [Serializable]
    public class InvalidBencodeException : Exception
    {
        public long StreamPosition { get; set; }

        public InvalidBencodeException() 
            : base()
        { }

        public InvalidBencodeException(string message)
            : base(message)
        { }

        public InvalidBencodeException(string message, long streamPosition) 
            : base(message)
        {
            StreamPosition = streamPosition;
        }

        public InvalidBencodeException(string message, Exception innerException)
            : base(message, innerException)
        { }

        protected InvalidBencodeException(SerializationInfo info, StreamingContext context)
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

        public static InvalidBencodeException New(string message)
        {
            return New(message, -1);
        }

        public static InvalidBencodeException New(string message, long streamPosition)
        {
            if (streamPosition > -1)
            {
                if (!message.EndsWith("."))
                    message += ".";

                message += " Object decoding began at position " + streamPosition;
            }

            return new InvalidBencodeException(message, streamPosition);
        }
    }
}
