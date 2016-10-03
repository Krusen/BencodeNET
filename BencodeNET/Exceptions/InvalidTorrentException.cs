using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BencodeNET.Exceptions
{
    public class InvalidTorrentException : BencodeException
    {
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

        public string InvalidField { get; set; }
    }
}
