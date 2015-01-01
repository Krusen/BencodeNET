using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BencodeNET.Tests
{
    [TestClass]
    [DeploymentItem("BencodeNET.Tests\\Files\\", "Files")]
    public class BencodeTests
    {
        [TestMethod]
        public void TorrentFileDecodeEncodeIsEqual()
        {
            const string originalFile = @"Files\ubuntu-14.10-desktop-amd64.iso.torrent";
            const string newFile = @"Files\ubuntu-14.10-desktop-amd64.iso.test.torrent";

            using (var streamRead = File.OpenRead(originalFile))
            {
                var bobject = Bencode.Decode(streamRead);
                using (var streamWrite = File.OpenWrite(newFile))
                {
                    bobject.EncodeToStream(streamWrite);
                }
            }

            var originalBytes = File.ReadAllBytes(originalFile);
            var newBytes = File.ReadAllBytes(newFile);

            Assert.IsTrue(originalBytes.SequenceEqual(newBytes));
        }
    }
}
