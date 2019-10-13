using System;

#pragma warning disable 1591
namespace BencodeNET.Exceptions
{
    /// <summary>
    /// Represents generic errors in this bencode library.
    /// </summary>
    public class BencodeException : Exception
    {
        public BencodeException()
        { }

        public BencodeException(string message)
            : base(message)
        { }

        public BencodeException(string message, Exception inner)
            : base(message, inner)
        { }
    }

    /// <summary>
    /// Represents generic errors in this bencode library related to a specific <see cref="Type"/>.
    /// </summary>
    /// <typeparam name="T">The related type.</typeparam>
    public class BencodeException<T> : BencodeException
    {
        /// <summary>
        /// The type related to this error. Usually the type being parsed.
        /// </summary>
        public Type RelatedType { get; } = typeof(T);

        public BencodeException()
        { }

        public BencodeException(string message)
            : base(message)

        { }

        public BencodeException(string message, Exception inner)
            : base(message, inner)
        { }
    }
}