using BencodeNET.Exceptions;
using BencodeNET.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BencodeNET.Tests
{
    [TestClass]
    public class BencodeDecodeListTests
    {
        [TestMethod]
        public void DecodeList_Simple()
        {
            var blist = Bencode.DecodeList("l4:spam3:fooi42ee");
            Assert.AreEqual(3, blist.Count);

            Assert.IsInstanceOfType(blist[0], typeof(BString));
            Assert.IsTrue(blist[0] as BString == "spam");

            Assert.IsInstanceOfType(blist[1], typeof(BString));
            Assert.IsTrue(blist[1] as BString == "foo");

            Assert.IsInstanceOfType(blist[2], typeof(BNumber));
            Assert.IsTrue(blist[2] as BNumber == 42);
        }

        [TestMethod]
        public void DecodeList_Complex()
        {
            var blist = Bencode.DecodeList("ll4:spami1ei2ei3eed3:foo3:barei42e6:foobare");
            Assert.AreEqual(4, blist.Count);

            Assert.IsInstanceOfType(blist[0], typeof(BList));
            Assert.AreEqual(4, (blist[0] as BList).Count);

            Assert.IsInstanceOfType(blist[1], typeof(BDictionary));
            Assert.AreEqual(1, (blist[1] as BDictionary).Count);

            Assert.IsInstanceOfType(blist[2], typeof(BNumber));
            Assert.IsTrue(blist[2] as BNumber == 42);

            Assert.IsInstanceOfType(blist[3], typeof(BString));
            Assert.IsTrue(blist[3] as BString == "foobar");
        }

        [TestMethod]
        public void DecodeList_EmptyList()
        {
            var blist = Bencode.DecodeList("le");
            Assert.AreEqual(0, blist.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(BencodeDecodingException<BList>))]
        public void DecodeList_Invalid_InputMinimumLength2()
        {
            Bencode.DecodeList("l");
        }

        [TestMethod]
        [ExpectedException(typeof(BencodeDecodingException<BList>))]
        public void DecodeList_Invalid_WrongBeginChar()
        {
            Bencode.DecodeList("4:spam3:fooi42ee");
        }

        [TestMethod]
        [ExpectedException(typeof(BencodeDecodingException<BList>))]
        public void DecodeList_Invalid_MissingEndChar()
        {
            Bencode.DecodeList("l4:spam3:fooi42e");
        }

        [TestMethod]
        [ExpectedException(typeof(BencodeDecodingException<BList>))]
        public void DecodeList_Invalid_InvalidObjectInList()
        {
            Bencode.DecodeList("l4:spamse");
        }
    }
}
