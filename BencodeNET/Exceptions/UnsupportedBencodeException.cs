using System;
using System.Runtime.Serialization;

namespace BencodeNET.Exceptions
{
    [Serializable]
    public class UnsupportedBencodeException : Exception
    {
        public long StreamPosition { get; set; }

        public UnsupportedBencodeException() 
            : base()
        { }

        public UnsupportedBencodeException(string message)
            : base(message)
        { }

        public UnsupportedBencodeException(string message, long streamPosition) 
            : base(message)
        {
            StreamPosition = streamPosition;
        }

        public UnsupportedBencodeException(string message, Exception innerException)
            : base(message, innerException)
        { }

        protected UnsupportedBencodeException(SerializationInfo info, StreamingContext context)
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

        public static UnsupportedBencodeException New(string message)
        {
            return New(message, -1);
        }

        public static UnsupportedBencodeException New(string message, long streamPosition)
        {
            if (streamPosition > -1)
            {
                if (!message.EndsWith("."))
                    message += ".";

                message += " Object decoding began at position " + streamPosition;
            }

            return new UnsupportedBencodeException(message, streamPosition);
        }
    }
}
