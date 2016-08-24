namespace BencodeNET.Torrents
{
    /// <summary>
    /// A reference of default torrent field names.
    /// </summary>
    public static class TorrentFields
    {
#pragma warning disable 1591
        public const string Announce = "announce";
        public const string AnnounceList = "announce-list";
        public const string CreatedBy = "created by";
        public const string CreationDate = "creation date";
        public const string Comment = "comment";
        public const string Encoding = "encoding";
        public const string Info = "info";

        // Fields in 'info' dictionary
        public const string Name = "name";
        public const string Private = "private";
        public const string PieceLength = "piece length";
        public const string Pieces = "pieces";
        public const string Length = "length";
        public const string Files = "files";
        public const string Path = "path";
        public const string Md5Sum = "md5sum";
#pragma warning restore 1591
    }
}
