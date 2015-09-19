using System;
using System.IO;
using System.Linq;
using BencodeNET.Objects;
using Xunit;

namespace BencodeNET.Tests
{
    public class BencodeTests
    {
        [Fact]
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

            Assert.True(originalBytes.SequenceEqual(newBytes));
        }

        [Fact]
        public void Decode_TorrentFile_FromPath()
        {
            const string path = @"Files\ubuntu-14.10-desktop-amd64.iso.torrent";

            TorrentFile actual;
            TorrentFile expected;

            using (var stream = File.OpenRead(path))
            {
                actual = Bencode.DecodeTorrentFile(path);
                expected = new TorrentFile(Bencode.DecodeDictionary(stream));
            }

            Assert.Equal(actual, expected);
        }

        [Fact]
        public void Decode_TorrentFile_FromStream()
        {
            const string path = @"Files\ubuntu-14.10-desktop-amd64.iso.torrent";

            TorrentFile actual;
            TorrentFile expected;

            using (var stream = File.OpenRead(path))
            {
                actual = Bencode.DecodeTorrentFile(stream);
                stream.Position = 0;
                expected = new TorrentFile(Bencode.DecodeDictionary(stream));
            }

            Assert.Equal(actual, expected);
        }

        [Fact]
        public void DefaultEncodingCannotBeSetToNull()
        {
            Assert.Throws<ArgumentNullException>(() => Bencode.DefaultEncoding = null);
        }

        [Fact]
        public void Decode_String()
        {
            var obj = Bencode.Decode("4:spam");
            Assert.IsType<BString>(obj);
            Assert.Equal("spam", obj.ToString());
        }

        [Fact]
        public void Decode_Number()
        {
            var obj = Bencode.Decode("i666e");
            Assert.IsType<BNumber>(obj);
            Assert.True(666 == obj as BNumber);
            Assert.Equal("666", obj.ToString());
        }

        [Fact]
        public void Decode_SimpleList()
        {
            var obj = Bencode.Decode("l5:hello5:worldi123ee");
            Assert.IsType<BList>(obj);
            
            var blist = obj as BList;
            Assert.NotNull(blist);
            Assert.Equal(3, blist.Count);

            Assert.IsType<BString>(blist[0]);
            Assert.Equal("hello", blist[0].ToString());

            Assert.IsType<BString>(blist[1]);
            Assert.Equal("world", blist[1].ToString());

            Assert.IsType<BNumber>(blist[2]);
            Assert.Equal("123", blist[2].ToString());
        }

        [Fact]
        public void Decode_SimpleDictionary()
        {
            var obj = Bencode.Decode("d5:hello5:world3:fooi234ee");
            Assert.IsType<BDictionary>(obj);

            var bdict = obj as BDictionary;
            Assert.NotNull(bdict);
            Assert.Equal(2, bdict.Count);

            Assert.True(bdict.ContainsKey("hello"));
            Assert.IsType<BString>(bdict["hello"]);
            Assert.Equal("world", bdict["hello"].ToString());
            
            Assert.True(bdict.ContainsKey("foo"));
            Assert.IsType<BNumber>(bdict["foo"]);
            Assert.Equal("234", bdict["foo"].ToString());
        }
    }
}