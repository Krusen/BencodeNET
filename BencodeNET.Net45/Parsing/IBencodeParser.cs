using System.IO;
using System.Text;
using System.Threading.Tasks;
using BencodeNET.IO;
using BencodeNET.Objects;
using BencodeNET.Torrents;

namespace BencodeNET.Parsing
{
    public interface IBencodeParser
    {
        Encoding Encoding { get; set; }

        IBObject Parse(string bencodedString);
        IBObject Parse(Stream stream);
        IBObject Parse(BencodeStream stream);
        IBObject ParseFromFile(string path);

        Task<IBObject> ParseAsync(Stream stream);
        Task<IBObject> ParseAsync(BencodeStream stream);
        Task<IBObject> ParseFromFileAsync(string path);

        T Parse<T>(string bencodedString) where T : class, IBObject;
        T Parse<T>(Stream stream) where T : class, IBObject;
        T Parse<T>(BencodeStream stream) where T : class, IBObject;
        T ParseFromFile<T>(string path) where T : class, IBObject;

        Task<T> ParseAsync<T>(Stream stream) where T : class, IBObject;
        Task<T> ParseAsync<T>(BencodeStream stream) where T : class, IBObject;
        Task<T> ParseFromFileAsync<T>(string path) where T : class, IBObject;

    }
}