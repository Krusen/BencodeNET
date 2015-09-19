using BencodeNET.Exceptions;
using BencodeNET.Objects;
using Xunit;

namespace BencodeNET.Tests
{
    public class BencodeDecodeListTests
    {
        [Fact]
        public void DecodeList_Simple()
        {
            var blist = Bencode.DecodeList("l4:spam3:fooi42ee");
            Assert.Equal(3, blist.Count);

            Assert.IsType<BString>(blist[0]);
            Assert.True(blist[0] as BString == "spam");

            Assert.IsType<BString>(blist[1]);
            Assert.True(blist[1] as BString == "foo");

            Assert.IsType<BNumber>(blist[2]);
            Assert.True(blist[2] as BNumber == 42);
        }

        [Fact]
        public void DecodeList_Complex()
        {
            var blist = Bencode.DecodeList("ll4:spami1ei2ei3eed3:foo3:barei42e6:foobare");
            Assert.Equal(4, blist.Count);

            Assert.IsType<BList>(blist[0]);
            Assert.Equal(4, (blist[0] as BList).Count);

            Assert.IsType<BDictionary>(blist[1]);
            Assert.Equal(1, (blist[1] as BDictionary).Count);

            Assert.IsType<BNumber>(blist[2]);
            Assert.True(blist[2] as BNumber == 42);

            Assert.IsType<BString>(blist[3]);
            Assert.True(blist[3] as BString == "foobar");
        }

        [Fact]
        public void DecodeList_EmptyList()
        {
            var blist = Bencode.DecodeList("le");
            Assert.Equal(0, blist.Count);
        }

        [Fact]
        public void DecodeList_Invalid_InputMinimumLength2()
        {
            Assert.Throws<BencodeDecodingException<BList>>(() => Bencode.DecodeList("l"));
        }

        [Fact]
        public void DecodeList_Invalid_WrongBeginChar()
        {
            Assert.Throws<BencodeDecodingException<BList>>(() => Bencode.DecodeList("4:spam3:fooi42ee"));
        }

        [Fact]
        public void DecodeList_Invalid_MissingEndChar()
        {
            Assert.Throws<BencodeDecodingException<BList>>(() => Bencode.DecodeList("l4:spam3:fooi42e"));
        }

        [Fact]
        public void DecodeList_Invalid_InvalidObjectInList()
        {
            Assert.Throws<BencodeDecodingException<BList>>(() => Bencode.DecodeList("l4:spamse"));
        }
    }
}
