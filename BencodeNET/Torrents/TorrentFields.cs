namespace BencodeNET.Torrents
{
    // TODO: Maybe astract this away and change to dependency and put in constructor?
    public static class TorrentFields
    {
        public const string Announce = "announce";
        public const string AnnounceList = "announce-list";
        public const string CreatedBy = "created by";
        public const string CreationDate = "creation date";
        public const string Comment = "comment";
        public const string Encoding = "encoding";
        public const string Info = "info";

        public const string Name = "name";
        public const string Private = "private";
        public const string PieceLength = "piece length";
        public const string Pieces = "pieces";
        public const string Length = "length";
        public const string Files = "files";
        public const string Path = "path";
        public const string Md5Sum = "md5sum";
    }
}
