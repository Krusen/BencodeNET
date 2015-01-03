using BencodeNET.Exceptions;
using BencodeNET.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BencodeNET.Tests
{
    [TestClass]
    public class BencodeDecodeNumberTests
    {
        [TestMethod]
        public void DecodeNumber_Simple()
        {
            var bnumber = Bencode.DecodeNumber("i42e");
            Assert.AreEqual(42, bnumber.Value);
        }

        [TestMethod]
        public void DecodeNumber_Zero()
        {
            var bnumber = Bencode.DecodeNumber("i0e");
            Assert.AreEqual(0, bnumber.Value);
        }

        [TestMethod]
        public void DecodeNumber_Negative()
        {
            var bnumber = Bencode.DecodeNumber("i-42e");
            Assert.AreEqual(-42, bnumber.Value);
        }

        [TestMethod]
        public void DecodeNumber_Int64()
        {
            var bnumber = Bencode.DecodeNumber("i9223372036854775807e");
            Assert.AreEqual(9223372036854775807, bnumber.Value);
        }

        [TestMethod]
        public void DecodeNumber_NegativeInt64()
        {
            var bnumber = Bencode.DecodeNumber("i-9223372036854775808e");
            Assert.AreEqual(-9223372036854775808, bnumber.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(BencodeDecodingException<BNumber>))]
        public void DecodeNumber_Invalid_LeadingZeros()
        {
            Bencode.DecodeNumber("i012345e");
        }

        [TestMethod]
        [ExpectedException(typeof(BencodeDecodingException<BNumber>))]
        public void DecodeNumber_Invalid_MinusZero()
        {
            Bencode.DecodeNumber("i-0e");
        }

        [TestMethod]
        [ExpectedException(typeof(BencodeDecodingException<BNumber>))]
        public void DecodeNumber_Invalid_MissingEndChar()
        {
            Bencode.DecodeNumber("i42");
        }

        [TestMethod]
        [ExpectedException(typeof(BencodeDecodingException<BNumber>))]
        public void DecodeNumber_Invalid_WrongBeginChar()
        {
            Bencode.DecodeNumber("42e");
        }

        [TestMethod]
        [ExpectedException(typeof(BencodeDecodingException<BNumber>))]
        public void DecodeNumber_Invalid_NoDigits()
        {
            Bencode.DecodeNumber("i-e");
        }

        [TestMethod]
        [ExpectedException(typeof(BencodeDecodingException<BNumber>))]
        public void DecodeNumber_Invalid_InputMinimumLength3()
        {
            Bencode.DecodeNumber("ie");
        }

        [TestMethod]
        [ExpectedException(typeof(BencodeDecodingException<BNumber>))]
        public void DecodeNumber_Invalid_InvalidChars()
        {
            Bencode.DecodeNumber("i12abe");
        }

        [TestMethod]
        [ExpectedException(typeof(BencodeDecodingException<BNumber>))]
        public void DecodeNumber_Invalid_DoubleNegativeSign()
        {
            Bencode.DecodeNumber("i--123e");
        }

        [TestMethod]
        [ExpectedException(typeof(BencodeDecodingException<BNumber>))]
        public void DecodeNumber_Invalid_NegativeSignInsideNumber()
        {
            Bencode.DecodeNumber("i12-3e");
        }

        [TestMethod]
        [ExpectedException(typeof(UnsupportedBencodeException))]
        public void DecodeNumber_Unsupported_GreaterThanMaxDigits19()
        {
            Bencode.DecodeNumber("i12345678901234567890e");
        }

        [TestMethod]
        [ExpectedException(typeof(UnsupportedBencodeException))]
        public void DecodeNumber_Unsupported_BiggerThanInt64MaxValue()
        {
            Bencode.DecodeNumber("i9223372036854775808e");
        }

        [TestMethod]
        [ExpectedException(typeof(UnsupportedBencodeException))]
        public void DecodeNumber_Unsupported_SmallerThanInt64MinValue()
        {
            Bencode.DecodeNumber("i-9223372036854775809e");
        }
    }
}
