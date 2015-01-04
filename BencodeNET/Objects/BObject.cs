using System.IO;
using System.Text;

namespace BencodeNET.Objects
{
    public abstract class BObject<TY> : IBObject
    {
        internal BObject()
        { }

        public TY Value { get; protected set; }

        public virtual string Encode()
        {
            return Encode(Bencode.DefaultEncoding);
        }

        public virtual string Encode(Encoding encoding)
        {
            var ms = new MemoryStream();
            EncodeToStream(ms).Position = 0;
            return new StreamReader(ms, encoding).ReadToEnd();
        }

        public abstract T EncodeToStream<T>(T stream) where T : Stream;
    }
}
