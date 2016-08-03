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
        BString ParseString(BencodeStream stream);
        BNumber ParseNumber(BencodeStream stream);
        BList ParseList(BencodeStream stream);
        BList<T> ParseList<T>(BencodeStream stream) where T : IBObject;
        BDictionary ParseDictionary(BencodeStream stream);

        Torrent ParseTorrent(Stream stream);
        Torrent ParseTorrent(BencodeStream stream);
        Torrent ParseTorrentFromFile(string path);

        Task<IBObject> ParseAsync(string bencodedString);
        Task<IBObject> ParseAsync(Stream stream);
        Task<IBObject> ParseAsync(BencodeStream stream);
        Task<IBObject> ParseFromFileAsync(string path);
        Task<BString> ParseStringAsync(BencodeStream stream);
        Task<BNumber> ParseNumberAsync(BencodeStream stream);
        Task<BList> ParseListAsync(BencodeStream stream);
        Task<BList<T>> ParseListAsync<T>(BencodeStream stream) where T : IBObject;
        Task<BDictionary> ParseDictionaryAsync(BencodeStream stream);

        Task<Torrent> ParseTorrentAsync(Stream stream);
        Task<Torrent> ParseTorrentAsync(BencodeStream stream);
        Task<Torrent> ParseTorrentFromFileAsync(string path);
    }
}