using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace BencodeNET.Objects
{
    public class TorrentFile
    {
        private readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private readonly BDictionary _data = new BDictionary();

        public BDictionary Info
        {
            get { return (BDictionary) _data[Fields.Info]; }
            set { _data[Fields.Info] = value; }
        }

        public string Announce
        {
            get
            {
                if (!_data.ContainsKey(Fields.Announce))
                    return null;
                return _data[Fields.Announce].ToString();
            }
            set { _data[Fields.Announce] = new BString(value); }
        }

        public BList AnnounceList
        {
            get { return (BList) _data[Fields.AnnounceList] ?? new BList(); }
            set { _data[Fields.AnnounceList] = value; }
        }

        public DateTime CreationDate
        {
            get
            {
                var unixTime = (BNumber) _data[Fields.CreationDate];
                return _epoch.AddSeconds(unixTime);
            }
            set
            {
                var unixTime = value.Subtract(_epoch).TotalSeconds.ToString();
                _data[Fields.CreationDate] = new BString(unixTime);
            }
        }

        public string Comment
        {
            get
            {
                if (!_data.ContainsKey(Fields.Comment))
                    return null;
                return _data[Fields.Comment].ToString();
            }
            set { _data[Fields.Comment] = new BString(value); }
        }

        public string CreatedBy
        {
            get
            {
                if (!_data.ContainsKey(Fields.CreatedBy))
                    return null;
                return _data[Fields.CreatedBy].ToString();
            }
            set { _data[Fields.CreatedBy] = new BString(value); }
        }

        public string Encoding
        {
            get
            {
                if (!_data.ContainsKey(Fields.Encoding))
                    return null;
                return _data[Fields.Encoding].ToString();
            }
            set { _data[Fields.Encoding] = new BString(value); }
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
            get { return _data[key]; }
            set { _data[key] = value; }
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

        public TorrentFile()
        { }

        public TorrentFile(BDictionary torrent)
        {
            _data = torrent;
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
