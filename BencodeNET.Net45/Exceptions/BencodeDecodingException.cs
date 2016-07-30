using System;
using System.Runtime.Serialization;
using BencodeNET.Objects;

namespace BencodeNET.Exceptions
{
#if !NETSTANDARD
    [Serializable]
#endif
    public class BencodeDecodingException<T> : Exception
    {
        public long StreamPosition { get; set; }

        public BencodeDecodingException() 
        { }

        public BencodeDecodingException(string message) 
            : base(message)
        { }

        public BencodeDecodingException(string message, long streamPosition)
            : base(CreateMessage(message, streamPosition))
        {
            StreamPosition = streamPosition;
        }

        private static string CreateMessage(string message, long streamPosition)
        {
            var output = "";
            if (typeof(T) == typeof (BString))
                output = "Invalid bencoded string. ";
            if (typeof(T) == typeof(BNumber))
                output = "Invalid bencoded number. ";
            if (typeof(T) == typeof(BList))
                output = "Invalid bencoded list. ";
            if (typeof(T) == typeof(BDictionary))
                output = "Invalid bencoded dictionary. ";

            output += message;

            if (streamPosition > -1)
            {
                output = output.Trim();

                if (!output.EndsWith("."))
                    output += ".";

                output += " Decoding failed at position " + streamPosition;
            }

            return output;
        }

#if !NETSTANDARD
        protected BencodeDecodingException(SerializationInfo info, StreamingContext context)
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
