using System;
using System.IO;
using System.Linq;
using BencodeNET.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BencodeNET.Tests
{
    [TestClass]
    public class BencodeTests
    {
        [TestMethod]
        [DeploymentItem("BencodeNET.Tests\\Files\\ubuntu-14.10-desktop-amd64.iso.torrent", "Files")]
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

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DefaultEncodingCannotBeSetToNull()
        {
            Bencode.DefaultEncoding = null;
        }

        [TestMethod]
        public void Decode_String()
        {
            var obj = Bencode.Decode("4:spam");
            Assert.IsInstanceOfType(obj, typeof(BString));
            Assert.AreEqual("spam", obj.ToString());
        }

        [TestMethod]
        public void Decode_Number()
        {
            var obj = Bencode.Decode("i666e");
            Assert.IsInstanceOfType(obj, typeof(BNumber));
            Assert.IsTrue(666 == obj as BNumber);
            Assert.AreEqual("666", obj.ToString());
        }

        [TestMethod]
        public void Decode_SimpleList()
        {
            var obj = Bencode.Decode("l5:hello5:worldi123ee");
            Assert.IsInstanceOfType(obj, typeof(BList));
            
            var blist = obj as BList;
            Assert.IsNotNull(blist);
            Assert.AreEqual(3, blist.Count);

            Assert.IsInstanceOfType(blist[0], typeof(BString));
            Assert.AreEqual("hello", blist[0].ToString());
            
            Assert.IsInstanceOfType(blist[1], typeof(BString));
            Assert.AreEqual("world", blist[1].ToString());
            
            Assert.IsInstanceOfType(blist[2], typeof(BNumber));
            Assert.AreEqual("123", blist[2].ToString());
        }

        [TestMethod]
        public void Decode_SimpleDictionary()
        {
            var obj = Bencode.Decode("d5:hello5:world3:fooi234ee");
            Assert.IsInstanceOfType(obj, typeof(BDictionary));

            var bdict = obj as BDictionary;
            Assert.IsNotNull(bdict);
            Assert.AreEqual(2, bdict.Count);

            Assert.IsTrue(bdict.ContainsKey("hello"));
            Assert.IsInstanceOfType(bdict["hello"], typeof(BString));
            Assert.AreEqual("world", bdict["hello"].ToString());
            
            Assert.IsTrue(bdict.ContainsKey("foo"));
            Assert.IsInstanceOfType(bdict["foo"], typeof(BNumber));
            Assert.AreEqual("234", bdict["foo"].ToString());
        }
    }
}