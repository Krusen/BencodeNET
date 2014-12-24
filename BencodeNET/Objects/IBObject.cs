using System.IO;
using System.Text;

namespace BencodeNET.Objects
{
    public interface IBObject
    {
        string Encode();
        string Encode(Encoding encoding);
        T EncodeToStream<T>(T stream) where T : Stream;
        T EncodeToStream<T>(T stream, Encoding encoding) where T : Stream;

        //IBObject Decode(string str);
        //IBObject Decode(Stream str);
        //T Decode<T>(string str) where T : IBObject;
        //T Decode<T>(Stream stream) where T : IBObject;
    }
}
