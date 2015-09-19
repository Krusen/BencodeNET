using System;
using BencodeNET.Objects;
using Xunit;

namespace BencodeNET.Tests
{
    public class TorrentFileTests
    {
        private TorrentFile torrent;

        public TorrentFileTests()
        {
            const string path = @"Files\ubuntu-14.10-desktop-amd64.iso.torrent";

            torrent = Bencode.DecodeTorrentFile(path);
        }

        [Fact]
        public void CalculateInfoHash()
        {
            var hash = torrent.CalculateInfoHash();
            Assert.Equal(hash, "B415C913643E5FF49FE37D304BBB5E6E11AD5101");
        }

        [Fact]
        public void CalculateInfoHashBytes()
        {
            var bytes = torrent.CalculateInfoHashBytes();
            var expected = new byte[] { 180, 21, 201, 19, 100, 62, 95, 244, 159, 227, 125, 48, 75, 187, 94, 110, 17, 173, 81, 1 };
            
            Assert.Equal(expected, bytes);
        }

        [Fact]
        public void NonexistentKeyAccessReturnsNull()
        {
            Assert.Null(torrent["asdf"]);
        }

        [Fact]
        public void Property_Announce()
        {
            var announce = "http://torrent.ubuntu.com:6969/announce";
            var announceList = new BList
            {
                new BList {new BString("http://torrent.ubuntu.com:6969/announce")},
                new BList {new BString("http://ipv6.torrent.ubuntu.com:6969/announce")}
            };
            var comment = "Ubuntu CD releases.ubuntu.com";
            var creationDate = new DateTime(1970, 1, 1).AddSeconds(1414070124);

            var expected = "http://torrent.ubuntu.com:6969/announce";

            Assert.Equal(expected, torrent.Announce);
        }

        [Fact]
        public void Property_AnnounceList()
        {
            var expected = new BList
            {
                new BList {new BString("http://torrent.ubuntu.com:6969/announce")},
                new BList {new BString("http://ipv6.torrent.ubuntu.com:6969/announce")}
            };

            Assert.Equal(expected, torrent.AnnounceList);
        }

        [Fact]
        public void Property_Comment()
        {
            var expected = "Ubuntu CD releases.ubuntu.com";

            Assert.Equal(expected, torrent.Comment);
        }

        [Fact]
        public void Property_CreationDate()
        {
            var expected = new DateTime(1970, 1, 1).AddSeconds(1414070124);

            Assert.Equal(expected, torrent.CreationDate);
        }

        [Fact]
        public void Property_CreatedBy()
        {
            Assert.Equal(null, torrent.CreatedBy);
        }

        [Fact]
        public void Property_Info()
        {
            Assert.Equal(4, torrent.Info.Count);
            Assert.True(torrent.Info.ContainsKey("length"));
            Assert.True(torrent.Info.ContainsKey("name"));
            Assert.True(torrent.Info.ContainsKey("piece length"));
            Assert.True(torrent.Info.ContainsKey("pieces"));
        }
    }
}
