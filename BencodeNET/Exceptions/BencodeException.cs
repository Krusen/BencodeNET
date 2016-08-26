using System;
#if !NETSTANDARD
using System.Runtime.Serialization;
#endif

namespace BencodeNET.Exceptions
{
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

#if !NETSTANDARD
    [Serializable]
#endif
    public class BencodeException<T> : BencodeException
    {
        // TODO: Naming of this property
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

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(RelatedType), RelatedType.AssemblyQualifiedName);
        }
#endif
    }

}
