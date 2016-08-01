using System.IO;
using System.Text;
#if !NET35
using System.Threading.Tasks;
#endif

namespace BencodeNET.Objects
{
    public interface IBObject
    {
        string Encode();
        string Encode(Encoding encoding);
        T EncodeToStream<T>(T stream) where T : Stream;
        void EncodeToFile(string path);
#if !NET35
        Task<T> EncodeToStreamAsync<T>(T stream) where T : Stream;
        Task EncodeToFileAsync(string path);
#endif
    }
}
