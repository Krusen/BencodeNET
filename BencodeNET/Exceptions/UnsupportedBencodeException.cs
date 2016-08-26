using System;
#if !NETSTANDARD
using System.Runtime.Serialization;
#endif

namespace BencodeNET.Exceptions
{
#if !NETSTANDARD
    [Serializable]
#endif
    public class UnsupportedBencodeException<T> : BencodeException<T>
    {
        public long StreamPosition { get; set; }

        public UnsupportedBencodeException()
        { }

        public UnsupportedBencodeException(string message)
            : base(message)
        { }

        public UnsupportedBencodeException(string message, Exception inner)
            : base(message, inner)
        { }

        public UnsupportedBencodeException(string message, long streamPosition)
            : base(message)
        {
            StreamPosition = streamPosition;
        }

#if !NETSTANDARD
        protected UnsupportedBencodeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info == null)
                return;

            StreamPosition = info.GetInt64(nameof(StreamPosition));
        }

        /// <summary>
        /// Sets the <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown. </param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination. </param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is a null reference. </exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(StreamPosition), StreamPosition);
        }
#endif
    }
}
