using System.IO;
using System.Threading.Tasks;
using BencodeNET.IO;
using BencodeNET.Objects;

namespace BencodeNET.Parsing
{
    public interface IBObjectParser
    {
        IBObject Parse(string bencodedString);
        IBObject Parse(Stream stream);
        IBObject Parse(BencodeStream stream);

        Task<IBObject> ParseAsync(Stream stream);
        Task<IBObject> ParseAsync(BencodeStream stream);
    }

    public interface IBObjectParser<T> : IBObjectParser where T : IBObject
    {
        new T Parse(string bencodedString);
        new T Parse(Stream stream);
        new T Parse(BencodeStream stream);

        new Task<T> ParseAsync(Stream stream);
        new Task<T> ParseAsync(BencodeStream stream);
    }
}
