using BencodeNET.Exceptions;
using BencodeNET.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BencodeNET.Tests
{
    [TestClass]
    public class BencodeDecodeDictionaryTests
    {
        [TestMethod]
        public void DecodeDictionary_Simple()
        {
            var bdict = Bencode.DecodeDictionary("d4:spam3:egg3:fooi42ee");
            Assert.AreEqual(2, bdict.Count);

            Assert.IsInstanceOfType(bdict["spam"], typeof(BString));
            Assert.IsTrue(bdict["spam"] as BString == "egg");

            Assert.IsInstanceOfType(bdict["foo"], typeof(BNumber));
            Assert.IsTrue(bdict["foo"] as BNumber == 42);
        }

        [TestMethod]
        public void DecodeDictionary_Complex()
        {
            var bdict = Bencode.DecodeDictionary("d6:A Listl3:foo3:bari123ed9:more spam9:more eggsee6:foobard7:numbersli1ei2ei3eee4:spam3:egge");

            var bdictExpected = new BDictionary
            {
                {"spam", "egg"},
                {
                    "A List", new BList
                    {
                        "foo",
                        "bar",
                        123,
                        new BDictionary
                        {
                            {"more spam", "more eggs"}
                        }
                    }
                },
                {
                    "foobar", new BDictionary
                    {
                        {"numbers", new BList {1, 2, 3}}
                    }
                }
            };

            Assert.AreEqual(3, bdict.Count);
            Assert.AreEqual(bdictExpected.Count, bdict.Count);
            Assert.AreEqual(bdictExpected.Encode(), bdict.Encode());
            Assert.IsInstanceOfType(bdict["spam"], typeof(BString));
            Assert.IsInstanceOfType(bdict["A List"], typeof(BList));
            Assert.IsInstanceOfType(bdict["foobar"], typeof(BDictionary));
        }

        [TestMethod]
        public void DecodeDictionary_EmptyDictionary()
        {
            var bdict = Bencode.DecodeDictionary("de");
            Assert.AreEqual(0, bdict.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(BencodeDecodingException<BDictionary>))]
        public void DecodeDictionary_Invalid_InputMinimumLength2()
        {
            Bencode.DecodeDictionary("d");
        }

        [TestMethod]
        [ExpectedException(typeof(BencodeDecodingException<BDictionary>))]
        public void DecodeDictionary_Invalid_NonStringKey()
        {
            Bencode.DecodeDictionary("di42e4:spame");
        }

        [TestMethod]
        [ExpectedException(typeof(BencodeDecodingException<BDictionary>))]
        public void DecodeDictionary_Invalid_InvalidKeyObject()
        {
            Bencode.DecodeDictionary("da:spam3:egge");
        }

        [TestMethod]
        [ExpectedException(typeof(BencodeDecodingException<BDictionary>))]
        public void DecodeDictionary_Invalid_WrongBeginChar()
        {
            Bencode.DecodeDictionary("l4:spam3:egg3:fooi42ee");
        }

        [TestMethod]
        [ExpectedException(typeof(BencodeDecodingException<BDictionary>))]
        public void DecodeDictionary_Invalid_MissingEndChar()
        {
            Bencode.DecodeDictionary("d4:spam3:egg3:fooi42e");
        }

        [TestMethod]
        [ExpectedException(typeof(BencodeDecodingException<BDictionary>))]
        public void DecodeDictionary_Invalid_MissingKeyValueOrInvalidValueObject()
        {
            Bencode.DecodeDictionary("d4:spame");
        }
    }
}
