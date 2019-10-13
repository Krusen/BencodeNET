using System;

#pragma warning disable 1591
namespace BencodeNET.Exceptions
{
    /// <summary>
    /// Represents parse errors for when encountering bencode that is potentially valid but not supported by this library.
    /// Usually numbers larger than <see cref="long.MaxValue"/> or strings longer than that.
    /// </summary>
    /// <typeparam name="T"></typeparam>
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
    }
}
