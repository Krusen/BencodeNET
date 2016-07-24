namespace BencodeNET.Torrents
{
    public class TorrentSingleFileInfo
    {
        // Name
        public string FileName { get; set; }

        // Length
        public long FileSize { get; set; }

        // [optional]
        public string Md5Sum { get; set; }
    }
}
