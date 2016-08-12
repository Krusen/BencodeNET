using System;
using System.Collections.Generic;
using System.Linq;

namespace BencodeNET.Torrents
{
    /// <summary>
    /// File info for files in a multi-file torrents.
    /// This
    /// </summary>
    /// <remarks>
    /// Corresponds to an entry in the 'info.files' list field in a torrent.
    /// </remarks>
    public class TorrentMultiFileInfo
    {
        /// <summary>
        /// The file name. It just returns the last part of <see cref="Path"/>.
        /// </summary>
        public string FileName => Path.LastOrDefault();

        /// <summary>
        /// The file size in bytes.
        /// </summary>
        /// <remarks>
        /// Corresponds to the 'length' field.
        /// </remarks>
        public long FileSize { get; set; }

        /// <summary>
        /// [optional] 32-character hexadecimal string corresponding to the MD5 sum of the file. Rarely used.
        /// </summary>
        public string Md5Sum { get; set; }

        /// <summary>
        /// A list of file path elements.
        /// </summary>
        public IList<string> Path { get; set; } = new List<string>();

        /// <summary>
        /// The full path of the file including file name.
        /// </summary>
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
