using System.IO;
using System.Threading.Tasks;
using BencodeNET.IO;
using BencodeNET.Objects;

namespace BencodeNET.Parsing
{
    public abstract class BObjectParser<T> : IBObjectParser<T> where T : IBObject
    {
        public T Parse(string bencodedString)
        {
            using (var ms = new MemoryStream())
            {
                return Parse(ms);
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