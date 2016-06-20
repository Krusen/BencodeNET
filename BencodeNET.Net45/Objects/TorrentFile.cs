using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace BencodeNET.Objects
{
    public class TorrentFile : BObject<BDictionary>
    {
        private readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// The dictionary that describes the file(s) of the torrent
        /// </summary>
        public BDictionary Info
        {
            get { return (BDictionary) Value[Fields.Info]; }
            set { Value[Fields.Info] = value; }
        }

        /// <summary>
        /// The announce URL of the tracker
        /// </summary>
        public string Announce
        {
            get
            {
                if (!Value.ContainsKey(Fields.Announce))
                    return null;
                return Value[Fields.Announce].ToString();
            }
            set { Value[Fields.Announce] = new BString(value); }
        }

        /// <summary>
        /// The announce URLs list of the tracker [optional]
        /// </summary>
        public BList AnnounceList
        {
            get { return (BList) Value[Fields.AnnounceList] ?? new BList(); }
            set { Value[Fields.AnnounceList] = value; }
        }

        /// <summary>
        /// The creation date of the .torrent file [optional]
        /// </summary>
        public DateTime CreationDate
        {
            get
            {
                var unixTime = (BNumber) Value[Fields.CreationDate] ?? 0;
                return _epoch.AddSeconds(unixTime);
            }
            set
            {
                var unixTime = (value.Subtract(_epoch).Ticks/TimeSpan.TicksPerSecond).ToString();
                Value[Fields.CreationDate] = new BString(unixTime);
            }
        }

        /// <summary>
        /// The comment of the .torrent file [optional]
        /// </summary>
        public string Comment
        {
            get
            {
                if (!Value.ContainsKey(Fields.Comment))
                    return null;
                return Value[Fields.Comment].ToString();
            }
            set { Value[Fields.Comment] = new BString(value); }
        }

        /// <summary>
        /// The name and version of the program used to create the .torrent [optional]
        /// </summary>
        public string CreatedBy
        {
            get
            {
                if (!Value.ContainsKey(Fields.CreatedBy))
                    return null;
                return Value[Fields.CreatedBy].ToString();
            }
            set { Value[Fields.CreatedBy] = new BString(value); }
        }

        /// <summary>
        /// The encoding used by the client that created the .torrent file [optional]
        /// </summary>
        public string Encoding
        {
            get
            {
                if (!Value.ContainsKey(Fields.Encoding))
                    return null;
                return Value[Fields.Encoding].ToString();
            }
            set { Value[Fields.Encoding] = new BString(value); }
        }

        public string CalculateInfoHash()
        {
            return CalculateInfoHash(Info);
        }

        public byte[] CalculateInfoHashBytes()
        {
            return CalculateInfoHashBytes(Info);
        }

        public static string CalculateInfoHash(BDictionary info)
        {
            var hashBytes = CalculateInfoHashBytes(info);

            return BitConverter.ToString(hashBytes).Replace("-", "");
        }

        public static byte[] CalculateInfoHashBytes(BDictionary info)
        {
            using (var sha1 = new SHA1Managed())
            using (var ms = new MemoryStream())
            {
                info.EncodeToStream(ms);
                ms.Position = 0;

                return sha1.ComputeHash(ms);
            }
        }

        public IBObject this[BString key]
        {
            get { return Value[key]; }
            set { Value[key] = value; }
        }

        public static bool operator ==(TorrentFile first, TorrentFile second)
        {
            if (ReferenceEquals(first, null))
                return ReferenceEquals(second, null);

            return first.Equals(second);
        }

        public static bool operator !=(TorrentFile first, TorrentFile second)
        {
            return !(first == second);
        }

        public override TStream EncodeToStream<TStream>(TStream stream)
        {
            return Value.EncodeToStream(stream);
        }

        public override bool Equals(object other)
        {
            var torrent = other as TorrentFile;
            if (torrent == null)
                return false;

            var comparisons = new List<bool>
            {
                Info == torrent.Info,
                Announce == torrent.Announce,
                AnnounceList == torrent.AnnounceList,
                CreationDate == torrent.CreationDate,
                CreatedBy == torrent.CreatedBy,
                Comment == torrent.Comment,
                Encoding == torrent.Encoding,
                CalculateInfoHash() == torrent.CalculateInfoHash()
            };

            return !comparisons.Contains(false);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public TorrentFile() : this(new BDictionary())
        { }

        public TorrentFile(BDictionary torrent)
        {
            Value = torrent;
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
        }
    }
}
