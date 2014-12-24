using System;
using System.Text;
using BencodeNET.Exceptions;
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
            var bstring = new BString(null);
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
        public void HashCodeEqualsStringHashCode()
        {
            var bstring = new BString("Test String");

            var expected = "Test String".GetHashCode();
            var actual = bstring.GetHashCode();

            Assert.AreEqual(expected, actual);
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
            Assert.AreEqual("6:æøåéöñ", bstring.Encode());
        }

        [TestMethod]
        public void Encode_NumbersAndSpecialCharacters()
        {
            var bstring = new BString("123:?!#{}'|<>");
            Assert.AreEqual("13:123:?!#{}'|<>", bstring.Encode());
        }

        [TestMethod]
        public void Decode_Simple()
        {
            var bstring = BString.Decode("4:spam");
            Assert.AreEqual("spam", bstring.Value);
        }

        [TestMethod]
        public void Decode_UTF8()
        {
            var bstring = BString.Decode("6:æøåéöñ", Encoding.UTF8);
            Assert.AreEqual("æøåéöñ", bstring.Value);
        }

        [TestMethod]
        public void Decode_EmptyString()
        {
            var bstring = BString.Decode("0:", Encoding.UTF8);
            Assert.AreEqual("", bstring.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidBencodeException))]
        public void Decode_Invalid_LessCharsThanSpecified()
        {
            BString.Decode("5:spam", Encoding.UTF8);
        } 

        [TestMethod]
        [ExpectedException(typeof(InvalidBencodeException))]
        public void Decode_Invalid_NoDelimiter()
        {
            BString.Decode("4spam", Encoding.UTF8);
        } 

        [TestMethod]
        [ExpectedException(typeof(InvalidBencodeException))]
        public void Decode_Invalid_NonDigitFirstChar()
        {
            BString.Decode("spam", Encoding.UTF8);
        } 

        [TestMethod]
        [ExpectedException(typeof(InvalidBencodeException))]
        public void Decode_Invalid_InputMinimumLength2()
        {
            BString.Decode("4", Encoding.UTF8);
        } 

        [TestMethod]
        [ExpectedException(typeof(InvalidBencodeException))]
        public void Decode_Invalid_MissingStringLength()
        {
            BString.Decode(":spam", Encoding.UTF8);
        } 

        [TestMethod]
        [ExpectedException(typeof(UnsupportedBencodeException))]
        public void Decode_Unsupported_StringLengthAboveMaxDigits10()
        {
            BString.Decode("12345678901:spam", Encoding.UTF8);
        } 

        [TestMethod]
        [ExpectedException(typeof(UnsupportedBencodeException))]
        public void Decode_Unsupported_StringLengthAboveInt32MaxValue()
        {
            BString.Decode("2147483648:spam", Encoding.UTF8);
        } 
    }
}
