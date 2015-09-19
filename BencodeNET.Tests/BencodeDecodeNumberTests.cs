using BencodeNET.Exceptions;
using BencodeNET.Objects;
using Xunit;

namespace BencodeNET.Tests
{
    public class BencodeDecodeNumberTests
    {
        [Fact]
        public void DecodeNumber_Simple()
        {
            var bnumber = Bencode.DecodeNumber("i42e");
            Assert.Equal(42, bnumber.Value);
        }

        [Fact]
        public void DecodeNumber_Zero()
        {
            var bnumber = Bencode.DecodeNumber("i0e");
            Assert.Equal(0, bnumber.Value);
        }

        [Fact]
        public void DecodeNumber_Negative()
        {
            var bnumber = Bencode.DecodeNumber("i-42e");
            Assert.Equal(-42, bnumber.Value);
        }

        [Fact]
        public void DecodeNumber_Int64()
        {
            var bnumber = Bencode.DecodeNumber("i9223372036854775807e");
            Assert.Equal(9223372036854775807, bnumber.Value);
        }

        [Fact]
        public void DecodeNumber_NegativeInt64()
        {
            var bnumber = Bencode.DecodeNumber("i-9223372036854775808e");
            Assert.Equal(-9223372036854775808, bnumber.Value);
        }

        [Fact]
        public void DecodeNumber_Invalid_LeadingZeros()
        {
            Assert.Throws<BencodeDecodingException<BNumber>>(() => Bencode.DecodeNumber("i012345e"));
        }

        [Fact]
        public void DecodeNumber_Invalid_MinusZero()
        {
            Assert.Throws<BencodeDecodingException<BNumber>>(() => Bencode.DecodeNumber("i-0e"));
        }

        [Fact]
        public void DecodeNumber_Invalid_MissingEndChar()
        {
            Assert.Throws<BencodeDecodingException<BNumber>>(() => Bencode.DecodeNumber("i42"));
        }

        [Fact]
        public void DecodeNumber_Invalid_WrongBeginChar()
        {
            Assert.Throws<BencodeDecodingException<BNumber>>(() => Bencode.DecodeNumber("42e"));
        }

        [Fact]
        public void DecodeNumber_Invalid_NoDigits()
        {
            Assert.Throws<BencodeDecodingException<BNumber>>(() => Bencode.DecodeNumber("i-e"));
        }

        [Fact]
        public void DecodeNumber_Invalid_InputMinimumLength3()
        {
            Assert.Throws<BencodeDecodingException<BNumber>>(() => Bencode.DecodeNumber("ie"));
        }

        [Fact]
        public void DecodeNumber_Invalid_InvalidChars()
        {
            Assert.Throws<BencodeDecodingException<BNumber>>(() => Bencode.DecodeNumber("i12abe"));
        }

        [Fact]
        public void DecodeNumber_Invalid_DoubleNegativeSign()
        {
            Assert.Throws<BencodeDecodingException<BNumber>>(() => Bencode.DecodeNumber("i--123e"));
        }

        [Fact]
        public void DecodeNumber_Invalid_NegativeSignInsideNumber()
        {
            Assert.Throws<BencodeDecodingException<BNumber>>(() => Bencode.DecodeNumber("i12-3e"));
        }

        [Fact]
        public void DecodeNumber_Unsupported_GreaterThanMaxDigits19()
        {
            Assert.Throws<UnsupportedBencodeException>(() => Bencode.DecodeNumber("i12345678901234567890e"));
        }

        [Fact]
        public void DecodeNumber_Unsupported_BiggerThanInt64MaxValue()
        {
            Assert.Throws<UnsupportedBencodeException>(() => Bencode.DecodeNumber("i9223372036854775808e"));
        }

        [Fact]
        public void DecodeNumber_Unsupported_SmallerThanInt64MinValue()
        {
            Assert.Throws<UnsupportedBencodeException>(() => Bencode.DecodeNumber("i-9223372036854775809e"));
        }
    }
}
