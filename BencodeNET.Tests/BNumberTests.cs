using System;
using BencodeNET.Exceptions;
using BencodeNET.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BencodeNET.Tests
{
    [TestClass]
    public class BNumberTests
    {
        [TestMethod]
        public void EqualsBNumber()
        {
            var bnumber1 = new BNumber(42);
            var bnumber2 = new BNumber(42);
            Assert.AreEqual(bnumber1, bnumber2);
        }

        [TestMethod]
        public void EqualsBNumberWithEqualsOperator()
        {
            var bnumber1 = new BNumber(42);
            var bnumber2 = new BNumber(42);
            Assert.IsTrue(bnumber1 == bnumber2);
        }

        [TestMethod]
        public void EqualsIntegerWithEqualsOperator()
        {
            var bnumber = new BNumber(42);
            Assert.IsTrue(42 == bnumber);
        }

        [TestMethod]
        public void HashCodeEqualsLongHashCode()
        {
            var bnumber = new BNumber(42);

            var expected = 42L.GetHashCode();
            var actual = bnumber.GetHashCode();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Encode_Simple()
        {
            var bnumber = new BNumber(42);
            Assert.AreEqual("i42e", bnumber.Encode());
        }

        [TestMethod]
        public void Encode_SimpleNegative()
        {
            var bnumber = new BNumber(-42);
            Assert.AreEqual("i-42e", bnumber.Encode());
        }

        [TestMethod]
        public void Encode_Zero()
        {
            var bnumber = new BNumber(0);
            Assert.AreEqual("i0e", bnumber.Encode());
        }

        [TestMethod]
        public void Encode_Int64MinValue()
        {
            var bnumber = new BNumber(-9223372036854775808);
            Assert.AreEqual("i-9223372036854775808e", bnumber.Encode());
        }

        [TestMethod]
        public void Encode_Int64MaxValue()
        {
            var bnumber = new BNumber(9223372036854775807);
            Assert.AreEqual("i9223372036854775807e", bnumber.Encode());
        }

        [TestMethod]
        public void Decode_Simple()
        {
            var bnumber = BNumber.Decode("i42e");
            Assert.AreEqual(42, bnumber.Value);
        }

        [TestMethod]
        public void Decode_Zero()
        {
            var bnumber = BNumber.Decode("i0e");
            Assert.AreEqual(0, bnumber.Value);
        }

        [TestMethod]
        public void Decode_Negative()
        {
            var bnumber = BNumber.Decode("i-42e");
            Assert.AreEqual(-42, bnumber.Value);
        }

        [TestMethod]
        public void Decode_Int64()
        {
            var bnumber = BNumber.Decode("i9223372036854775807e");
            Assert.AreEqual(9223372036854775807, bnumber.Value);
        }

        [TestMethod]
        public void Decode_NegativeInt64()
        {
            var bnumber = BNumber.Decode("i-9223372036854775808e");
            Assert.AreEqual(-9223372036854775808, bnumber.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidBencodeException))]
        public void Decode_Invalid_LeadingZeros()
        {
            BNumber.Decode("i012345e");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidBencodeException))]
        public void Decode_Invalid_MinusZero()
        {
            BNumber.Decode("i-0e");
        }
        
        [TestMethod]
        [ExpectedException(typeof(InvalidBencodeException))]
        public void Decode_Invalid_MissingEndChar()
        {
            BNumber.Decode("i42");
        } 
        
        [TestMethod]
        [ExpectedException(typeof(InvalidBencodeException))]
        public void Decode_Invalid_WrongBeginChar()
        {
            BNumber.Decode("42e");
        }
        
        [TestMethod]
        [ExpectedException(typeof(InvalidBencodeException))]
        public void Decode_Invalid_NoDigits()
        {
            BNumber.Decode("i-e");
        }
        
        [TestMethod]
        [ExpectedException(typeof(InvalidBencodeException))]
        public void Decode_Invalid_InputMinimumLength3()
        {
            BNumber.Decode("ie");
        }
        
        [TestMethod]
        [ExpectedException(typeof(InvalidBencodeException))]
        public void Decode_Invalid_InvalidChars()
        {
            BNumber.Decode("i12abe");
        }
        
        [TestMethod]
        [ExpectedException(typeof(InvalidBencodeException))]
        public void Decode_Invalid_DoubleNegativeSign()
        {
            BNumber.Decode("i--123e");
        }
        
        [TestMethod]
        [ExpectedException(typeof(InvalidBencodeException))]
        public void Decode_Invalid_NegativeSignInsideNumber()
        {
            BNumber.Decode("i12-3e");
        }

        [TestMethod]
        [ExpectedException(typeof(UnsupportedBencodeException))]
        public void Decode_Unsupported_GreaterThanMaxDigits19()
        {
            BNumber.Decode("i12345678901234567890e");
        }

        [TestMethod]
        [ExpectedException(typeof(UnsupportedBencodeException))]
        public void Decode_Unsupported_BiggerThanInt64MaxValue()
        {
            BNumber.Decode("i9223372036854775808e");
        }

        [TestMethod]
        [ExpectedException(typeof(UnsupportedBencodeException))]
        public void Decode_Unsupported_SmallerThanInt64MinValue()
        {
            BNumber.Decode("i-9223372036854775809e");
        }
    }
}
