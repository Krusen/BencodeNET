using System.IO;
using BencodeNET.IO;
using BencodeNET.Objects;

namespace BencodeNET.Parsing
{
    public interface IBObjectParser
    {
        IBObject Parse(string bencodedString);
        IBObject Parse(byte[] bytes);
        IBObject Parse(Stream stream);
        IBObject Parse(BencodeStream stream);
    }

    public interface IBObjectParser<T> : IBObjectParser where T : IBObject
    {
        new T Parse(string bencodedString);
        new T Parse(byte[] bytes);
        new T Parse(Stream stream);
        new T Parse(BencodeStream stream);
    }
}
