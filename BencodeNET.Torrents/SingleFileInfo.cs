namespace BencodeNET.Torrents
{
    /// <summary>
    /// File info for a file in a single-file torrent.
    /// </summary>
    /// <remarks>
    /// Corresponds to the 'info' field in a single-file torrent.
    /// </remarks>
    public class SingleFileInfo
    {
        // Name
        /// <summary>
        /// The file name. This is purely advisory.
        /// </summary>
        /// <remarks>
        /// Corresponds to the 'name' field.
        /// </remarks>
        public string FileName { get; set; }

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
    }
}
