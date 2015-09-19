using System;
using System.Text;
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
        public void EqualsBList()
        {
            var blist1 = new BList
            {
                "asdf",
                "qwer"
            };
            var blist2 = new BList()
            {
                "asdf",
                "qwer"
            };
            var blist3 = new BList()
            {
                "qwer",
                "asdf"
            };

            Assert.AreEqual(blist1, blist2);
            Assert.AreNotEqual(blist1, blist3);
            Assert.AreNotEqual(blist2, blist3);
        }

        [TestMethod]
        public void EqualsBListWithEqualsOperator()
        {
            var blist1 = new BList
            {
                "asdf",
                "qwer"
            };
            var blist2 = new BList()
            {
                "asdf",
                "qwer"
            };
            var blist3 = new BList()
            {
                "qwer",
                "asdf"
            };

            Assert.IsTrue(blist1 == blist2);
            Assert.IsTrue(blist1 != blist3);
            Assert.IsTrue(blist2 != blist3);
        }

        [TestMethod]
        public void HashCodesAreEqual()
        {
            var blist1 = new BList
            {
                "asdf",
                "qwer"
            };
            var blist2 = new BList()
            {
                "asdf",
                "qwer"
            };

            var expected = blist1.GetHashCode();
            var actual = blist2.GetHashCode();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void HashCodesAreNotEqual()
        {
            var blist1 = new BList
            {
                "asdf",
                "qwer"
            };
            var blist2 = new BList()
            {
                "asdf",
                666
            };

            var expected = blist1.GetHashCode();
            var actual = blist2.GetHashCode();

            Assert.AreNotEqual(expected, actual);
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
        public void Encode_UTF8()
        {
            var blist = new BList { "æøå äö èéê ñ" };
            Assert.AreEqual("l21:æøå äö èéê ñe", blist.Encode());
            Assert.AreEqual("l21:æøå äö èéê ñe", blist.Encode(Encoding.UTF8));
            Assert.AreEqual(blist.Encode(), blist.Encode(Encoding.UTF8));
        }

        [TestMethod]
        public void Encode_ISO88591()
        {
            var encoding = Encoding.GetEncoding("ISO-8859-1");
            var blist = new BList { new BString("æøå äö èéê ñ", encoding) };

            Assert.AreNotEqual("l12:æøå äö èéê ñe", blist.Encode());
            Assert.AreEqual("l12:æøå äö èéê ñe", blist.Encode(encoding));
            Assert.AreNotEqual(blist.Encode(), blist.Encode(encoding));
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
