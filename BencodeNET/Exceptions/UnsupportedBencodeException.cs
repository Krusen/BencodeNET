using System;
#if !NETSTANDARD
using System.Runtime.Serialization;
#endif

namespace BencodeNET.Exceptions
{
#if !NETSTANDARD
    [Serializable]
#endif
    public class UnsupportedBencodeException : Exception
    {
        public long StreamPosition { get; set; }

        public UnsupportedBencodeException()
        { }

        public UnsupportedBencodeException(string message)
            : base(message)
        { }

        public UnsupportedBencodeException(string message, long streamPosition)
            : base(CreateMessage(message, streamPosition))
        {
            StreamPosition = streamPosition;
        }

        private static string CreateMessage(string message, long streamPosition)
        {
            if (streamPosition > -1)
            {
                if (!message.EndsWith("."))
                    message += ".";

                message += " Object decoding began at position " + streamPosition;
            }

            return message;
        }

#if !NETSTANDARD
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
#endif
    }
}
