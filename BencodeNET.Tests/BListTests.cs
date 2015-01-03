using System;
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
    }
}
