namespace BencodeNET.Torrents
{
    /// <summary>
    /// Determines how strict to be when parsing torrent files.
    /// </summary>
    public enum TorrentParserMode
    {
        /// <summary>
        /// The parser will throw an exception if some parts of the torrent specification is not followed.
        /// </summary>
        Strict,

        /// <summary>
        ///  The parser will ignore stuff that doesn't follow the torrent specifications.
        /// </summary>
        Tolerant
    }
}
