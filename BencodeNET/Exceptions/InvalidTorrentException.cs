using System;
#if !NETSTANDARD
using System.Runtime.Serialization;
#endif

#pragma warning disable 1591
namespace BencodeNET.Exceptions
{
    /// <summary>
    /// Represents parse errors when parsing torrents.
    /// </summary>
#if !NETSTANDARD
    [Serializable]
#endif
    public class InvalidTorrentException : BencodeException
    {
        public string InvalidField { get; set; }

        public InvalidTorrentException()
        { }

        public InvalidTorrentException(string message)
            : base(message)
        { }

        public InvalidTorrentException(string message, string invalidField)
            : base(message)
        {
            InvalidField = invalidField;
        }

        public InvalidTorrentException(string message, Exception inner)
            : base(message, inner)
        { }

#if !NETSTANDARD
        protected InvalidTorrentException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info == null) return;
            InvalidField = info.GetString(nameof(InvalidField));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(InvalidField), InvalidField);
        }
#endif
    }
}
#pragma warning restore 1591