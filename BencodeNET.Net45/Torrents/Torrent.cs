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
    public class Torrent : IBObject
    {
        private readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public Torrent()
        { }

        public Torrent(BDictionary torrent)
            : this(torrent, System.Text.Encoding.UTF8)
        { }

        // TODO: Split into smaller parts - maybe a TorrentFactory
        public Torrent(BDictionary torrent, Encoding encoding)
        {
            var info = torrent.GetBDictionary(Fields.Info);

            IsPrivate = info.GetBNumber(Fields.Private) == 1;
            PieceSize = info.GetBNumber(Fields.PieceLength);
            Pieces = info.GetBString(Fields.Pieces);

            // TODO: More foolproof detection, look at more keys
            FileMode = info.ContainsKey(Fields.Files) ? TorrentFileMode.Multi : TorrentFileMode.Single;

            if (FileMode == TorrentFileMode.Single)
            {
                File = new TorrentSingleFileInfo
                {
                    FileName = info.GetBString(Fields.Name)?.ToString(encoding),
                    FileSize = info.GetBNumber(Fields.Length),
                    Md5Sum = info.GetBString(Fields.Md5Sum)?.ToString(encoding)
                };
            }
            else
            {
                Files = new TorrentMultiFileInfoList
                {
                    DirectoryName = info.GetBString(Fields.Name).ToString(encoding),
                };

                var fileInfos = info.GetBList(Fields.Files).Select(x => new TorrentMultiFileInfo
                {
                    FileSize = ((BDictionary) x).GetBNumber(Fields.Length),
                    Path = ((BDictionary) x).GetBList(Fields.Path)?.AsStrings(encoding).ToList(),
                    Md5Sum = ((BDictionary) x).GetBString(Fields.Md5Sum)?.ToString(encoding)
                });

                Files.AddRange(fileInfos);
            }

            var announce = torrent.GetBString(Fields.Announce)?.ToString(encoding);
            if (!string.IsNullOrEmpty(announce))
            {
                Trackers.Add(announce);
            }

            var announceLists = torrent.GetBList(Fields.AnnounceList)?.AsBLists();
            if (announceLists != null)
            {
                foreach (var list in announceLists)
                {
                    foreach (var tracker in list.AsStrings())
                    {
                        Trackers.Add(tracker);
                    }
                }

                Trackers = Trackers.Distinct().ToList();
            }

            var unixTime = torrent.GetBNumber(Fields.CreationDate);
            CreationDate = unixTime == null ? (DateTime?)null : _epoch.AddSeconds(unixTime);

            if (torrent.ContainsKey(Fields.Comment))
            {
                Comment = torrent.GetBString(Fields.Comment).ToString(encoding);
            }

            if (torrent.ContainsKey(Fields.CreatedBy))
            {
                CreatedBy = torrent.GetBString(Fields.CreatedBy).ToString(encoding);
            }

            if (torrent.ContainsKey(Fields.Encoding))
            {
                Encoding = torrent.GetBString(Fields.Encoding).ToString(encoding);
            }
        }

        // Announce + Announce-List
        // The announce list is actually a list of lists of trackers, but we don't support that for now.
        public IList<string> Trackers { get; set; } = new List<string>();

        // Info - Single file
        public TorrentSingleFileInfo File { get; set; }

        // Info - Multi file
        public TorrentMultiFileInfoList Files { get; set; }

        // TODO: Make dynamic? Check if File != null or Files != null etc.?
        public TorrentFileMode FileMode { get; private set; }

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

        public string Encode()
        {
            return Encode(Bencode.DefaultEncoding);
        }

        public string Encode(Encoding encoding)
        {
            using (var ms = EncodeToStream(new MemoryStream()))
            {
                return encoding.GetString(ms.ToArray());
            }
        }

        public T EncodeToStream<T>(T stream) where T : Stream
        {
            return EncodeToStream(stream, Bencode.DefaultEncoding);
        }

        // TODO: EncodeToFile method, maybe add to IBObject or BObject
        // TODO: Split into smaller parts - maybe a TorrentFactory
        public T EncodeToStream<T>(T stream, Encoding encoding) where T : Stream
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
            {
                torrent[Fields.Encoding] = new BString(Encoding, encoding);
            }

            if (Comment != null)
            {
                torrent[Fields.Comment] = new BString(Comment, encoding);
            }

            if (CreationDate != null)
            {
                var unixTime = CreationDate.Value.Subtract(_epoch).Ticks/TimeSpan.TicksPerSecond;
                torrent[Fields.CreationDate] = new BNumber(unixTime);
            }

            if (CreatedBy != null)
            {
                torrent[Fields.CreatedBy] = new BString(CreatedBy, encoding);
            }

            // TODO: Check FileMode instead?
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
                info[Fields.Length] = (BNumber) File.FileSize;

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

            torrent[Fields.Info] = info;

            return torrent.EncodeToStream(stream);
        }

        public void RecalculateInfoHash()
        {

        }

        //private string CalculateInfoHash()
        //{
        //    return CalculateInfoHash(Info);
        //}

        //private byte[] CalculateInfoHashBytes()
        //{
        //    return CalculateInfoHashBytes(Info);
        //}

        private static string CalculateInfoHash(BDictionary info)
        {
            var hashBytes = CalculateInfoHashBytes(info);

            return BitConverter.ToString(hashBytes).Replace("-", "");
        }

        private static byte[] CalculateInfoHashBytes(BDictionary info)
        {
            using (var sha1 = new SHA1Managed())
            using (var ms = new MemoryStream())
            {
                info.EncodeToStream(ms);
                ms.Position = 0;

                return sha1.ComputeHash(ms);
            }
        }

        private static class Fields
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
