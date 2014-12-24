using System;
using System.Collections.Generic;
using BencodeNET.Exceptions;
using BencodeNET.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BencodeNET.Tests
{
    [TestClass]
    public class BListTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddNullValue()
        {
            var blist = new BList();
            blist.Add((IBObject)null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetNullValue()
        {
            var blist = new BList();
            blist.Add(0);
            blist[0] = null;
        }

        [TestMethod]
        public void Encode_Simple()
        {
            var blist = new BList {"hello world", 987, "foobar"};

            var expected = "l11:hello worldi987e6:foobare";
            var actual = blist.Encode();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Encode_EmptyList()
        {
            var blist = new BList();
            Assert.AreEqual("le", blist.Encode());
        }

        [TestMethod]
        public void Encode_Complex()
        {
            var blist = new BList
            {
                "spam",
                666,
                new BList
                {
                    "foo",
                    "bar",
                    123,
                    new BDictionary
                    {
                        {"more spam", "more eggs"}
                    }
                },
                "foobar",
                new BDictionary
                {
                    {"numbers", new BList {1, 2, 3}}
                }

            };

            var expected = "l4:spami666el3:foo3:bari123ed9:more spam9:more eggsee6:foobard7:numbersli1ei2ei3eeee";
            var actual = blist.Encode();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Decode_Simple()
        {
            var blist = BList.Decode("l4:spam3:fooi42ee");
            Assert.AreEqual(3, blist.Count);

            Assert.IsInstanceOfType(blist[0], typeof(BString));
            Assert.IsTrue(blist[0] as BString == "spam");

            Assert.IsInstanceOfType(blist[1], typeof(BString));
            Assert.IsTrue(blist[1] as BString == "foo");

            Assert.IsInstanceOfType(blist[2], typeof(BNumber));
            Assert.IsTrue(blist[2] as BNumber == 42);
        }

        [TestMethod]
        public void Decode_Complex()
        {
            var blist = BList.Decode("ll4:spami1ei2ei3eed3:foo3:barei42e6:foobare");
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
        public void Decode_EmptyList()
        {
            var blist = BList.Decode("le");
            Assert.AreEqual(0, blist.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidBencodeException))]
        public void Decode_Invalid_InputMinimumLength2()
        {
            BList.Decode("l");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidBencodeException))]
        public void Decode_Invalid_WrongBeginChar()
        {
            BList.Decode("4:spam3:fooi42ee");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidBencodeException))]
        public void Decode_Invalid_MissingEndChar()
        {
            BList.Decode("l4:spam3:fooi42e");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidBencodeException))]
        public void Decode_Invalid_InvalidObjectInList()
        {
            BList.Decode("l4:spamse");
        }
    }
}
