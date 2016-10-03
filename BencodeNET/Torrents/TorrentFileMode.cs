namespace BencodeNET.Torrents
{
    /// <summary>
    /// Indicates the torrent file mode.
    /// Torrents are structured differently if it is either single-file or multi-file.
    /// </summary>
    public enum TorrentFileMode
    {
        /// <summary>
        /// Torrent file mode could not be determined and is most likely invalid.
        /// </summary>
        Unknown,

        /// <summary>
        /// Single-file torrent. Contains only a single file.
        /// </summary>
        Single,

        /// <summary>
        /// Multi-file torrent. Can contain multiple files and a parent directory name for all included files.
        /// </summary>
        Multi
    }
}