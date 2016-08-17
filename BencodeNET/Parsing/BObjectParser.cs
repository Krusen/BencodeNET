using System.IO;
using System.Text;
using System.Threading.Tasks;
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

        IBObject IBObjectParser.Parse(Stream stream)
        {
            return Parse(stream);
        }

        IBObject IBObjectParser.Parse(BencodeStream stream)
        {
            return Parse(stream);
        }

        Task<IBObject> IBObjectParser.ParseAsync(Stream stream)
        {
            return ParseAsync(stream).FromDerived<IBObject, T>();
        }

        Task<IBObject> IBObjectParser.ParseAsync(BencodeStream stream)
        {
            return ParseAsync(stream).FromDerived<IBObject, T>();
        }

        public T Parse(string bencodedString)
        {
            using (var stream = bencodedString.AsStream(Encoding))
            {
                return Parse(stream);
            }
        }

        public T Parse(Stream stream)
        {
            return Parse(new BencodeStream(stream));
        }

        public abstract T Parse(BencodeStream stream);

        public Task<T> ParseAsync(Stream stream)
        {
            return ParseAsync(new BencodeStream(stream));
        }

        public abstract Task<T> ParseAsync(BencodeStream stream);
    }
}