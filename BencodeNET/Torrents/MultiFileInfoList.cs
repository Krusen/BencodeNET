using System.Collections.Generic;

namespace BencodeNET.Torrents
{
    /// <summary>
    /// A list of file info for the files included in a multi-file torrent.
    /// </summary>
    /// <remarks>
    /// Corresponds to the 'info' field in a multi-file torrent.
    /// </remarks>
    public class MultiFileInfoList : List<MultiFileInfo>
    {
        /// <summary>
        /// The name of the directory in which to store all the files. This is purely advisory.
        /// </summary>
        /// <remarks>
        /// Corresponds to the 'name' field.
        /// </remarks>
        public string DirectoryName { get; set; }
    }
}
