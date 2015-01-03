using System;
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
            Assert.AreEqual(bstring1, bstring2);
        }

        [TestMethod]
        public void EqualsBStringWithEqualsOperator()
        {
            var bstring1 = new BString("Test String");
            var bstring2 = new BString("Test String");
            Assert.IsTrue(bstring1 == bstring2);
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
            var bstring = new BString("æøåéöñ");

            var expected = string.Format("{0}:æøåéöñ", Encoding.UTF8.GetByteCount("æøåéöñ"));
            var actual = bstring.Encode(Encoding.UTF8);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Encode_NumbersAndSpecialCharacters()
        {
            var bstring = new BString("123:?!#{}'|<>");
            Assert.AreEqual("13:123:?!#{}'|<>", bstring.Encode());
        }
    }
}
