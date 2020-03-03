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
        /// <summary> </summary>
        public MultiFileInfoList()
        { }

        /// <summary></summary>
        /// <param name="directoryName">Name of directory to store files in.</param>
        public MultiFileInfoList(string directoryName)
        {
            DirectoryName = directoryName;
        }

        /// <summary></summary>
        /// <param name="directoryName">Name of directory to store files in.</param>
        /// <param name="directoryNameUtf8">Name of directory to store files in.</param>
        public MultiFileInfoList(string directoryName, string directoryNameUtf8)
        {
            DirectoryName = directoryName;
            DirectoryNameUtf8 = directoryNameUtf8;
        }

        /// <summary>
        /// The name of the directory in which to store all the files. This is purely advisory.
        /// </summary>
        /// <remarks>
        /// Corresponds to the 'name' field.
        /// </remarks>
        public string DirectoryName { get; set; }

        /// <summary>
        /// The UTF-8 encoded name of the directory in which to store all the files. This is purely advisory.
        /// </summary>
        /// <remarks>
        /// Corresponds to the 'name.utf-8' field.
        /// </remarks>
        public string DirectoryNameUtf8 { get; set; }
    }
}
