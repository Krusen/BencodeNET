using System;
using System.IO;
using System.Text;
using BencodeNET.Objects;

namespace BencodeNET
{
    public static class Bencode
    {
        private static Encoding _defaultEncoding;

        public static Encoding DefaultEncoding
        {
            get { return _defaultEncoding; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value", "DefaultEncoding may not be set to null");

                _defaultEncoding = value;
            }
        }

        static Bencode()
        {
            DefaultEncoding = Encoding.UTF8;
        }

        public static IBObject Decode(string bencodedString)
        {
            return Decode(bencodedString, DefaultEncoding);
        }

        public static IBObject Decode(string bencodedString, Encoding encoding)
        {
            if (bencodedString == null) throw new ArgumentNullException("bencodedString");
            if (encoding == null) throw new ArgumentNullException("encoding");

            using (var ms = new MemoryStream(encoding.GetBytes(bencodedString)))
            {
                return Decode(ms, encoding);
            }
        }

        public static IBObject Decode(Stream stream)
        {
            return Decode(stream, DefaultEncoding);
        }

        public static IBObject Decode(Stream stream, Encoding encoding)
        {
            using (var reader = new BinaryReader(stream, encoding, leaveOpen:true)) { 
                var nextChar = reader.PeekChar();

                switch (nextChar)
                {
                    case 'i': return BNumber.Decode(stream, encoding);
                    case 'l': return BList.Decode(stream, encoding);
                    case 'd': return BDictionary.Decode(stream, encoding);
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9': return BString.Decode(stream, encoding);
                }

                return null;
            }
        }
    }
}
