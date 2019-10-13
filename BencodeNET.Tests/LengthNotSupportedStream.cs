using System;
using System.IO;
using System.Text;

namespace BencodeNET.Tests
{
    internal class LengthNotSupportedStream : MemoryStream
    {
        public LengthNotSupportedStream(string str)
            : this(str, Encoding.UTF8)
        {
        }

        public LengthNotSupportedStream(string str, Encoding encoding)
            : base(encoding.GetBytes(str))
        {
        }

        public override long Length => throw new NotSupportedException();
    }
}