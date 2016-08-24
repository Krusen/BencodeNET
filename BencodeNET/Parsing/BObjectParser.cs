using System.IO;
using System.Text;
using BencodeNET.IO;
using BencodeNET.Objects;

namespace BencodeNET.Parsing
{
    public abstract class BObjectParser<T> : IBObjectParser<T> where T : IBObject
    {
        protected abstract Encoding Encoding { get; }

        IBObject IBObjectParser.Parse(string bencodedString)
        {
            return Parse(bencodedString);
        }

        IBObject IBObjectParser.Parse(byte[] bytes)
        {
            return Parse(bytes);
        }

        IBObject IBObjectParser.Parse(Stream stream)
        {
            return Parse(stream);
        }

        IBObject IBObjectParser.Parse(BencodeStream stream)
        {
            return Parse(stream);
        }

        public virtual T Parse(string bencodedString)
        {
            using (var stream = bencodedString.AsStream(Encoding))
            {
                return Parse(stream);
            }
        }

        public virtual T Parse(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                return Parse(stream);
            }
        }

        public virtual T Parse(Stream stream)
        {
            return Parse(new BencodeStream(stream));
        }

        public abstract T Parse(BencodeStream stream);
    }
}