using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace BencodeNET.Objects
{
    public class TorrentFile
    {
        private readonly BDictionary _data = new BDictionary();

        public BDictionary Info
        {
            get { return (BDictionary) _data["info"]; }
        }

        public string Announce
        {
            get
            {
                if (!_data.ContainsKey("announce"))
                    return null;
                return _data["announce"].ToString();
            }
        }

        public BList AnnounceList
        {
            get { return (BList) _data["announce-list"]; }
        }

        public DateTime CreationDate
        {
            get
            {
                var unixTime = (BNumber) _data["creation date"];
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                return epoch.AddSeconds(unixTime);
            }
        }

        public string Comment
        {
            get
            {
                if (!_data.ContainsKey("comment"))
                    return null;
                return _data["comment"].ToString();
            }
        }

        public string CreatedBy
        {
            get
            {
                if (!_data.ContainsKey("created by"))
                    return null;
                return _data["created by"].ToString();
            }
        }

        public string Encoding
        {
            get
            {
                if (!_data.ContainsKey("encoding"))
                    return null;
                return _data["encoding"].ToString();
            }
        }

        public string CalculateInfoHash()
        {
            var hashBytes = CalculateInfoHashBytes();

            return BitConverter.ToString(hashBytes).Replace("-", "");
        }

        public byte[] CalculateInfoHashBytes()
        {
            using (var sha1 = new SHA1Managed())
            using (var ms = new MemoryStream())
            {
                _data["info"].EncodeToStream(ms);
                ms.Position = 0;

                return sha1.ComputeHash(ms);
            }
        }

        public IBObject this[BString key]
        {
            get { return _data[key]; }
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
    }
}
