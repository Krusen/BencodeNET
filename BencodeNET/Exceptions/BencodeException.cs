using System;
#if !NETSTANDARD
using System.Runtime.Serialization;
#endif

#pragma warning disable 1591
namespace BencodeNET.Exceptions
{
    /// <summary>
    /// Represents generic errors in this bencode library.
    /// </summary>
#if !NETSTANDARD
    [Serializable]
#endif
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

#if !NETSTANDARD
        protected BencodeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
#endif
    }

    /// <summary>
    /// Represents generic errors in this bencode library related to a specific <see cref="Type"/>.
    /// </summary>
    /// <typeparam name="T">The related type.</typeparam>
#if !NETSTANDARD
    [Serializable]
#endif
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

#if !NETSTANDARD
        protected BencodeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info == null) return;
            RelatedType = Type.GetType(info.GetString(nameof(RelatedType)), false);
        }

        /// <summary>
        /// Sets the <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown. </param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination. </param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is a null reference.</exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(RelatedType), RelatedType.AssemblyQualifiedName);
        }
#endif
    }
}