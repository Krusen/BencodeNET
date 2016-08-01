using System;
using System.Collections.Generic;
using System.Linq;

namespace BencodeNET.Torrents
{
    public class TorrentMultiFileInfo
    {
        // Last entry in Path
        public string FileName => Path.LastOrDefault();

        // Length
        public long FileSize { get; set; }

        // [optional]
        public string Md5Sum { get; set; }

        public IList<string> Path { get; set; } = new List<string>();

        public string FullPath
        {
            get
            {
                return string.Join(System.IO.Path.DirectorySeparatorChar.ToString(), Path);
            }
            set
            {
                Path = value.Split(new[] { System.IO.Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            }
        }
    }
}
