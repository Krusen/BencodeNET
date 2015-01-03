using System.Text;
using BencodeNET.Exceptions;
using BencodeNET.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BencodeNET.Tests
{
    [TestClass]
    public class BencodeDecodeStringTests
    {
        [TestMethod]
        public void DecodeString_Simple()
        {
            var bstring = Bencode.DecodeString("4:spam");
            Assert.AreEqual("spam", bstring.ToString());
        }

        [TestMethod]
        public void DecodeString_UTF8()
        {
            var bstring = Bencode.DecodeString("12:æøåéöñ", Encoding.UTF8);
            Assert.AreEqual("æøåéöñ", bstring.ToString(Encoding.UTF8));
        }

        [TestMethod]
        public void DecodeString_EmptyString()
        {
            var bstring = Bencode.DecodeString("0:");
            Assert.AreEqual("", bstring.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(BencodeDecodingException<BString>))]
        public void DecodeString_Invalid_LessCharsThanSpecified()
        {
            Bencode.DecodeString("5:spam");
        }

        [TestMethod]
        [ExpectedException(typeof(BencodeDecodingException<BString>))]
        public void DecodeString_Invalid_NoDelimiter()
        {
            Bencode.DecodeString("4spam");
        }

        [TestMethod]
        [ExpectedException(typeof(BencodeDecodingException<BString>))]
        public void DecodeString_Invalid_NonDigitFirstChar()
        {
            Bencode.DecodeString("spam");
        }

        [TestMethod]
        [ExpectedException(typeof(BencodeDecodingException<BString>))]
        public void DecodeString_Invalid_InputMinimumLength2()
        {
            Bencode.DecodeString("4");
        }

        [TestMethod]
        [ExpectedException(typeof(BencodeDecodingException<BString>))]
        public void DecodeString_Invalid_MissingStringLength()
        {
            Bencode.DecodeString(":spam");
        }

        [TestMethod]
        [ExpectedException(typeof(UnsupportedBencodeException))]
        public void DecodeString_Unsupported_StringLengthAboveMaxDigits10()
        {
            Bencode.DecodeString("12345678901:spam");
        }

        [TestMethod]
        [ExpectedException(typeof(UnsupportedBencodeException))]
        public void DecodeString_Unsupported_StringLengthAboveInt32MaxValue()
        {
            Bencode.DecodeString("2147483648:spam");
        }
    }
}
