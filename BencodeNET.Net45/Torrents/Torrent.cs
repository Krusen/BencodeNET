using BencodeNET.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BencodeNET.Torrents
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    // TODO: Equals comparison
    public class Torrent : BObject
    {
        // Announce + Announce-List
        // The announce list is actually a list of lists of trackers, but we don't support that for now.
        public IList<string> Trackers { get; set; } = new List<string>();

        // Info - Single file
        public TorrentSingleFileInfo File { get; set; }

        // Info - Multi file
        public TorrentMultiFileInfoList Files { get; set; }

        // TODO: Make dynamic? Check if File != null or Files != null etc.?
        public virtual TorrentFileMode FileMode { get; private set; }

        /// <summary>
        /// The creation date of the .torrent file [optional]
        /// </summary>
        public DateTime? CreationDate { get; set; }

        /// <summary>
        /// The comment of the .torrent file [optional]
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// The name and version of the program used to create the .torrent [optional]
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// The encoding used by the client that created the .torrent file [optional]
        /// </summary>
        public string Encoding { get; set; }

        public long PieceSize { get; set; }
        public BString Pieces { get; internal set; }
        public bool IsPrivate { get; set; }

        public long TotalSize
        {
            get
            {
                if (FileMode == TorrentFileMode.Single)
                    return File?.FileSize ?? 0;

                if (FileMode == TorrentFileMode.Multi)
                    return Files?.Sum(x => x.FileSize) ?? 0;

                return 0;
            }
        }

        public int NumberOfPieces => (int)Math.Ceiling((double)TotalSize / PieceSize);

        // TODO: Move these to a TorrentFactory?
        public static Torrent FromFile(string path)
        {
            return FromFile(path, Bencode.DefaultEncoding);
        }

        public static Torrent FromFile(string path, Encoding encoding)
        {
            var data = Bencode.DecodeDictionaryFromFile(path, encoding);
            return FromBDictionary(data);
        }

        public static Torrent FromBDictionary(BDictionary data)
        {
            var info = data.Get<BDictionary>(Fields.Info);

            var torrent = new Torrent
            {
                IsPrivate = info.Get<BNumber>(Fields.Private) == 1,
                PieceSize = info.Get<BNumber>(Fields.PieceLength),
                Pieces = info.Get<BString>(Fields.Pieces),

                Comment = data.Get<BString>(Fields.Comment)?.ToString(),
                CreatedBy = data.Get<BString>(Fields.CreatedBy)?.ToString(),
                Encoding = data.Get<BString>(Fields.Encoding)?.ToString(),
                CreationDate = data.Get<BNumber>(Fields.CreationDate),

                // TODO: More foolproof detection, look at more keys
                FileMode = info.ContainsKey(Fields.Files) ? TorrentFileMode.Multi : TorrentFileMode.Single,

                Trackers = LoadTrackers(data)
            };

            if (torrent.FileMode == TorrentFileMode.Single)
                torrent.File = LoadSingleFileInfo(info);

            if (torrent.FileMode == TorrentFileMode.Multi)
                torrent.Files = LoadMultiFileInfoList(info);

            return torrent;
        }

        protected static TorrentSingleFileInfo LoadSingleFileInfo(BDictionary info)
        {
            return new TorrentSingleFileInfo
            {
                FileName = info.Get<BString>(Fields.Name)?.ToString(),
                FileSize = info.Get<BNumber>(Fields.Length),
                Md5Sum = info.Get<BString>(Fields.Md5Sum)?.ToString()
            };
        }

        protected static TorrentMultiFileInfoList LoadMultiFileInfoList(BDictionary info)
        {
            var list = new TorrentMultiFileInfoList
            {
                DirectoryName = info.Get<BString>(Fields.Name).ToString(),
            };

            var fileInfos = info.Get<BList>(Fields.Files).Cast<BDictionary>()
                .Select(x => new TorrentMultiFileInfo
                {
                    FileSize = x.Get<BNumber>(Fields.Length),
                    Path = x.Get<BList>(Fields.Path)?.AsStrings().ToList(),
                    Md5Sum = x.Get<BString>(Fields.Md5Sum)?.ToString()
                });

            list.AddRange(fileInfos);

            return list;
        }

        private static IList<string> LoadTrackers(BDictionary data)
        {
            var trackers = new List<string>();
            var announce = data.Get<BString>(Fields.Announce)?.ToString();
            if (!string.IsNullOrEmpty(announce))
            {
                trackers.Add(announce);
            }

            var announceLists = data.Get<BList>(Fields.AnnounceList)?.As<BList>();
            if (announceLists != null)
            {
                trackers.AddRange(announceLists.SelectMany(list => list.AsStrings()));

                trackers = trackers.Distinct().ToList();
            }

            return trackers;
        }

        public override T EncodeToStream<T>(T stream)
        {
            return EncodeToStream(stream, Bencode.DefaultEncoding);
        }

        // TODO: Split into smaller parts - maybe a TorrentFactory
        // TODO: Some sort of error handling?
        public virtual T EncodeToStream<T>(T stream, Encoding encoding) where T : Stream
        {
            var torrent = new BDictionary();

            if (Trackers?.Count > 0)
            {
                torrent[Fields.Announce] = new BString(Trackers.First(), encoding);
            }

            if (Trackers?.Count > 1)
            {
                torrent[Fields.AnnounceList] = new BList
                {
                    new BList<BString>(Trackers.Select(x => (IBObject) new BString(x, encoding)))
                };
            }

            if (Encoding != null)
                torrent[Fields.Encoding] = new BString(Encoding, encoding);

            if (Comment != null)
                torrent[Fields.Comment] = new BString(Comment, encoding);

            if (CreatedBy != null)
                torrent[Fields.CreatedBy] = new BString(CreatedBy, encoding);

            if (CreationDate != null)
            {
                torrent[Fields.CreationDate] = (BNumber)CreationDate;
            }

            torrent[Fields.Info] = CreateInfo(encoding);

            return torrent.EncodeToStream(stream);
        }

        public virtual void EncodeToFile(string path, Encoding encoding)
        {
            using (var stream = System.IO.File.OpenWrite(path))
            {
                EncodeToStream(stream);
            }
        }

        private BDictionary CreateInfo(Encoding encoding)
        {
            var info = new BDictionary
            {
                [Fields.PieceLength] = (BNumber)PieceSize,
                [Fields.Pieces] = Pieces,
            };

            if (IsPrivate)
                info[Fields.Private] = (BNumber)1;

            if (FileMode == TorrentFileMode.Single)
            {
                info[Fields.Name] = new BString(File.FileName, encoding);
                info[Fields.Length] = (BNumber)File.FileSize;

                if (File.Md5Sum != null)
                    info[Fields.Md5Sum] = new BString(File.Md5Sum, encoding);

            }
            else if (FileMode == TorrentFileMode.Multi)
            {
                info[Fields.Name] = new BString(Files.DirectoryName, encoding);

                var files = new BList<BDictionary>();
                foreach (var file in Files)
                {
                    var fileDictionary = new BDictionary
                    {
                        [Fields.Length] = (BNumber)file.FileSize,
                        [Fields.Path] = new BList(file.Path)
                    };

                    if (file.Md5Sum != null)
                        fileDictionary[Fields.Md5Sum] = new BString(file.Md5Sum, encoding);

                    files.Add(fileDictionary);
                }

                info[Fields.Files] = files;
            }

            return info;
        }

        public virtual string CalculateInfoHash()
        {
            return CalculateInfoHash(Bencode.DefaultEncoding);
        }

        public virtual string CalculateInfoHash(Encoding encoding)
        {
            var info = CreateInfo(encoding);
            return TorrentUtil.CalculateInfoHash(info);
        }

        public virtual byte[] CalculateInfoHashBytes()
        {
            return CalculateInfoHashBytes(Bencode.DefaultEncoding);
        }

        public virtual byte[] CalculateInfoHashBytes(Encoding encoding)
        {
            var info = CreateInfo(encoding);
            return TorrentUtil.CalculateInfoHashBytes(info);
        }

        public static bool operator ==(Torrent first, Torrent second)
        {
            if (ReferenceEquals(first, null))
                return ReferenceEquals(second, null);

            return first.Equals(second);
        }

        public static bool operator !=(Torrent first, Torrent second)
        {
            return !(first == second);
        }

        public override bool Equals(object other)
        {
            var obj = other as Torrent;
            if (obj == null)
                return false;

            using (var ms1 = EncodeToStream(new MemoryStream()))
            using (var ms2 = obj.EncodeToStream(new MemoryStream()))
            {
                var bytes1 = ms1.ToArray();
                var bytes2 = ms2.ToArray();

                return bytes1.SequenceEqual(bytes2);
            }
        }

        // TODO: Maybe astract this away and change to dependency and put in constructor?
        public static class Fields
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
}
