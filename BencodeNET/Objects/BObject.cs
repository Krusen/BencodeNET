using System.IO;
using System.Text;

namespace BencodeNET.Objects
{
    public abstract class BObject<TY> : IBObject
    {
        public TY Value { get; protected set; }

        public string Encode()
        {
            return Encode(Bencode.DefaultEncoding);
        }

        public string Encode(Encoding encoding)
        {
            var ms = new MemoryStream();
            EncodeToStream(ms).Position = 0;
            return new StreamReader(ms, encoding).ReadToEnd();
        }

        public T EncodeToStream<T>(T stream) where T : Stream
        {
            return EncodeToStream(stream, Bencode.DefaultEncoding);
        }

        public abstract T EncodeToStream<T>(T stream, Encoding encoding) where T : Stream;
    }
}
