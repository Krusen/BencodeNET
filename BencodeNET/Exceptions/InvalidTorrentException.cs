using System;

#pragma warning disable 1591
namespace BencodeNET.Exceptions
{
    /// <summary>
    /// Represents parse errors when parsing torrents.
    /// </summary>
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
    }
}
#pragma warning restore 1591