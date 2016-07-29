using System.IO;
using System.Text;

namespace BencodeNET.Objects
{
    public interface IBObject
    {
        string Encode();
        string Encode(Encoding encoding);
        T EncodeToStream<T>(T stream) where T : Stream;
        void EncodeToFile(string path);
    }
}
