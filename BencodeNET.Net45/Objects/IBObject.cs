using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BencodeNET.Objects
{
    public interface IBObject
    {
        string Encode();
        string Encode(Encoding encoding);
        T EncodeToStream<T>(T stream) where T : Stream;
        void EncodeToFile(string path);
        Task<T> EncodeToStreamAsync<T>(T stream) where T : Stream;
        Task EncodeToFileAsync(string path);
    }
}
