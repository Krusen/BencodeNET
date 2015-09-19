using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BencodeNET.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BencodeNET.Tests
{
    [TestClass]
    [DeploymentItem("BencodeNET.Tests\\Files\\ubuntu-14.10-desktop-amd64.iso.torrent", "Files")]
    public class TorrentFileTests
    {
        private TorrentFile torrent;

        [TestInitialize]
        public void TestSetup()
        {
            const string path = @"Files\ubuntu-14.10-desktop-amd64.iso.torrent";
            torrent = Bencode.DecodeTorrentFile(path);
        }

        [TestMethod]
        public void CalculateInfoHash()
        {
            var hash = torrent.CalculateInfoHash();
            Assert.AreEqual(hash, "B415C913643E5FF49FE37D304BBB5E6E11AD5101");
        }

        [TestMethod]
        public void CalculateInfoHashBytes()
        {
            var bytes = torrent.CalculateInfoHashBytes();
            var expected = new byte[] { 180, 21, 201, 19, 100, 62, 95, 244, 159, 227, 125, 48, 75, 187, 94, 110, 17, 173, 81, 1 };
            
            CollectionAssert.AreEqual(expected, bytes);
        }

        [TestMethod]
        public void NonexistentKeyAccessReturnsNull()
        {
            Assert.IsNull(torrent["asdf"]);
        }

        [TestMethod]
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

            Assert.AreEqual(expected, torrent.Announce);
        }

        [TestMethod]
        public void Property_AnnounceList()
        {
            var expected = new BList
            {
                new BList {new BString("http://torrent.ubuntu.com:6969/announce")},
                new BList {new BString("http://ipv6.torrent.ubuntu.com:6969/announce")}
            };

            Assert.AreEqual(expected, torrent.AnnounceList);
        }

        [TestMethod]
        public void Property_Comment()
        {
            var expected = "Ubuntu CD releases.ubuntu.com";

            Assert.AreEqual(expected, torrent.Comment);
        }

        [TestMethod]
        public void Property_CreationDate()
        {
            var expected = new DateTime(1970, 1, 1).AddSeconds(1414070124);

            Assert.AreEqual(expected, torrent.CreationDate);
        }

        [TestMethod]
        public void Property_CreatedBy()
        {
            Assert.AreEqual(null, torrent.CreatedBy);
        }

        [TestMethod]
        public void Property_Info()
        {
            Assert.AreEqual(4, torrent.Info.Count);
            Assert.IsTrue(torrent.Info.ContainsKey("length"));
            Assert.IsTrue(torrent.Info.ContainsKey("name"));
            Assert.IsTrue(torrent.Info.ContainsKey("piece length"));
            Assert.IsTrue(torrent.Info.ContainsKey("pieces"));
        }
    }
}
