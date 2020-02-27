using BencodeNET.Objects;

#pragma warning disable 1591
namespace BencodeNET.Torrents
{
    /// <summary>
    /// A reference of default torrent field names.
    /// </summary>
    public static class TorrentFields
    {
        public const string Announce = "announce";
        public const string AnnounceList = "announce-list";
        public const string CreatedBy = "created by";
        public const string CreationDate = "creation date";
        public const string Comment = "comment";
        public const string Encoding = "encoding";
        public const string Info = "info";

        public static readonly BString[] Keys =
        {
            Announce,
            AnnounceList,
            Comment,
            CreatedBy,
            CreationDate,
            Encoding,
            Info
        };
    }

    /// <summary>
    /// A reference of default torrent fields names in the 'info'-dictionary.
    /// </summary>
    public static class TorrentInfoFields
    {
        public const string Name = "name";
        public const string NameUtf8 = "name.utf-8";
        public const string Private = "private";
        public const string PieceLength = "piece length";
        public const string Pieces = "pieces";
        public const string Length = "length";
        public const string Md5Sum = "md5sum";
        public const string Files = "files";

        public static readonly BString[] Keys =
        {
            Name,
            NameUtf8,
            Private,
            PieceLength,
            Pieces,
            Length,
            Md5Sum,
            Files
        };
    }

    /// <summary>
    /// A reference of default torrent fields in the dictionaries in the 'files'-list in the 'info'-dictionary.s
    /// </summary>
    public static class TorrentFilesFields
    {
        public const string Length = "length";
        public const string Path = "path";
        public const string PathUtf8 = "path.utf-8";
        public const string Md5Sum = "md5sum";

        public static readonly BString[] Keys =
        {
            Length,
            Path,
            PathUtf8,
            Md5Sum
        };
    }
}
#pragma warning restore 1591