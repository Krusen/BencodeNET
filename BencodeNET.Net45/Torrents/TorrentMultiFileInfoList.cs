using System.Collections.Generic;

namespace BencodeNET.Torrents
{
    public class TorrentMultiFileInfoList : List<TorrentMultiFileInfo>
    {
        // Name
        public string DirectoryName { get; set; }
    }
}
