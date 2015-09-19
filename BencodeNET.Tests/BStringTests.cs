using System;
using System.IO;
using System.Text;
using BencodeNET.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BencodeNET.Tests
{
    [TestClass]
    public class BStringTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorWithNullValue()
        {
            var bstring = new BString((string)null);
        }

        [TestMethod]
        public void EqualsBString()
        {
            var bstring1 = new BString("Test String");
            var bstring2 = new BString("Test String");
            var bstring3 = new BString("Another string");

            Assert.AreEqual(bstring1, bstring2);
            Assert.AreNotEqual(bstring1, bstring3);
            Assert.AreNotEqual(bstring2, bstring3);
        }

        [TestMethod]
        public void EqualsBStringWithEqualsOperator()
        {
            var bstring1 = new BString("Test String");
            var bstring2 = new BString("Test String");
            var bstring3 = new BString("Another string");

            Assert.IsTrue(bstring1 == bstring2);
            Assert.IsTrue(bstring1 != bstring3);
            Assert.IsTrue(bstring2 != bstring3);
        }

        [TestMethod]
        public void EqualsStringWithEqualsOperator()
        {
            var bstring = new BString("Test String");
            Assert.IsTrue("Test String" == bstring);
        }

        [TestMethod]
        public void HashCodesAreEqual()
        {
            var bstring1 = new BString("Test String");
            var bstring2 = new BString("Test String");

            var expected = bstring1.GetHashCode();
            var actual = bstring2.GetHashCode();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void HashCodesAreNotEqual()
        {
            var bstring = new BString("Test String");

            Assert.AreNotEqual(bstring.GetHashCode(), new BString("Test Strin").GetHashCode());
            Assert.AreNotEqual(bstring.GetHashCode(), new BString("Test Strin ").GetHashCode());
            Assert.AreNotEqual(bstring.GetHashCode(), new BString("Test String ").GetHashCode());
            Assert.AreNotEqual(bstring.GetHashCode(), new BString("Test String2").GetHashCode());
            Assert.AreNotEqual(bstring.GetHashCode(), new BString("Test StrinG").GetHashCode());
            Assert.AreNotEqual(bstring.GetHashCode(), new BString("test string").GetHashCode());
            Assert.AreNotEqual(bstring.GetHashCode(), new BString("TestString").GetHashCode());
            Assert.AreNotEqual(bstring.GetHashCode(), new BString("teststring").GetHashCode());
            Assert.AreNotEqual(bstring.GetHashCode(), new BString("TøstStrøng").GetHashCode());
        }

        [TestMethod]
        public void Encode_Simple()
        {
            var bstring = new BString("Test String");
            Assert.AreEqual("11:Test String", bstring.Encode());
        }

        [TestMethod]
        public void Encode_EmptyString()
        {
            var bstring = new BString("");
            Assert.AreEqual("0:", bstring.Encode());
        }

        [TestMethod]
        public void Encode_UTF8()
        {
            var bstring = new BString("æøå äö èéê ñ");
            Assert.AreEqual("21:æøå äö èéê ñ", bstring.Encode());
            Assert.AreEqual("21:æøå äö èéê ñ", bstring.Encode(Encoding.UTF8));
        }

        [TestMethod]
        public void Encode_ISO88591()
        {
            var encoding = Encoding.GetEncoding("ISO-8859-1");
            var bstring = new BString("æøå äö èéê ñ", encoding);
            Assert.AreEqual("12:æøå äö èéê ñ", bstring.Encode());
            Assert.AreEqual("12:æøå äö èéê ñ", bstring.Encode(encoding));
            Assert.AreEqual(bstring.Encode(), bstring.Encode(encoding));
        }

        [TestMethod]
        public void Encode_ISO88591_WithoutSpecifyingEncoding()
        {
            var encoding = Encoding.GetEncoding("ISO-8859-1");
            var bytes = encoding.GetBytes("æøå äö èéê ñ");
            var bstring = new BString(bytes);
            Assert.AreNotEqual("12:æøå äö èéê ñ", bstring.Encode());
            Assert.AreEqual("12:æøå äö èéê ñ", bstring.Encode(encoding));
            Assert.AreNotEqual(bstring.Encode(), bstring.Encode(encoding));
        }

        [TestMethod]
        public void Encode_NumbersAndSpecialCharacters()
        {
            var bstring = new BString("123:?!#{}'|<>");
            Assert.AreEqual("13:123:?!#{}'|<>", bstring.Encode());
        }

        [TestMethod]
        public void ToString_UTF8()
        {
            var bstring = Bencode.DecodeString("21:æøå äö èéê ñ", Encoding.UTF8);
            Assert.AreEqual("æøå äö èéê ñ", bstring.ToString());
            Assert.AreEqual("æøå äö èéê ñ", bstring.ToString(Encoding.UTF8));
            Assert.AreEqual(bstring.ToString(), bstring.ToString(Encoding.UTF8));
        }

        [TestMethod]
        public void ToString_ISO88591()
        {
            var bstring = Bencode.DecodeString("12:æøå äö èéê ñ", Encoding.GetEncoding("ISO-8859-1"));
            Assert.AreEqual("æøå äö èéê ñ", bstring.ToString());
            Assert.AreEqual("æøå äö èéê ñ", bstring.ToString(Encoding.GetEncoding("ISO-8859-1")));
            Assert.AreEqual(bstring.ToString(), bstring.ToString(Encoding.GetEncoding("ISO-8859-1")));
        }

        [TestMethod]
        public void ToString_FromNonUTF8StreamWithoutEncoding()
        {
            var encoding = Encoding.GetEncoding("ISO-8859-1");
            var bytes = encoding.GetBytes("12:æøå äö èéê ñ");
            using (var ms = new MemoryStream(bytes))
            {
                var bstring = Bencode.DecodeString(ms);
                Assert.AreNotEqual("æøå äö èéê ñ", bstring.ToString());
                Assert.AreEqual("æøå äö èéê ñ", bstring.ToString(encoding));
                Assert.AreNotEqual(bstring.ToString(), bstring.ToString(encoding));
            }
        }

        [TestMethod]
        public void ToString_FromNonUTF8StreamWithEncoding()
        {
            var encoding = Encoding.GetEncoding("ISO-8859-1");
            var bytes = encoding.GetBytes("12:æøå äö èéê ñ");
            using (var ms = new MemoryStream(bytes))
            {
                var bstring = Bencode.DecodeString(ms, encoding);
                Assert.AreEqual("æøå äö èéê ñ", bstring.ToString());
                Assert.AreEqual("æøå äö èéê ñ", bstring.ToString(encoding));
                Assert.AreEqual(bstring.ToString(), bstring.ToString(encoding));
            }
        }
    }
}
