using System;
using BencodeNET.Exceptions;
using BencodeNET.Objects;
using BencodeNET.Parsing;
using FluentAssertions;
using Xunit;

namespace BencodeNET.Tests.Parsing
{
    public class BNumberParserTests
    {
        private BNumberParser Parser { get; }

        public BNumberParserTests()
        {
            Parser = new BNumberParser();
        }

        [Theory]
        [InlineData("i1e", 1)]
        [InlineData("i2e", 2)]
        [InlineData("i3e", 3)]
        [InlineData("i42e", 42)]
        [InlineData("i100e", 100)]
        [InlineData("i1234567890e", 1234567890)]
        public void CanParsePositive(string bencode, int value)
        {
            var bnumber = Parser.ParseString(bencode);
            bnumber.Should().Be(value);
        }

        [Fact]
        public void CanParseZero()
        {
            var bnumber = Parser.ParseString("i0e");
            bnumber.Should().Be(0);
        }

        [Theory]
        [InlineData("i-1e", -1)]
        [InlineData("i-2e", -2)]
        [InlineData("i-3e", -3)]
        [InlineData("i-42e", -42)]
        [InlineData("i-100e", -100)]
        [InlineData("i-1234567890e", -1234567890)]
        public void CanParseNegative(string bencode, int value)
        {
            var bnumber = Parser.ParseString(bencode);
            bnumber.Should().Be(value);
        }

        [Theory]
        [InlineData("i9223372036854775807e", 9223372036854775807)]
        [InlineData("i-9223372036854775808e", -9223372036854775808)]
        public void CanParseInt64(string bencode, long value)
        {
            var bnumber = Parser.ParseString(bencode);
            bnumber.Should().Be(value);
        }

        [Theory]
        [InlineData("i01e")]
        [InlineData("i012e")]
        [InlineData("i01234567890e")]
        [InlineData("i00001e")]
        public void LeadingZeros_ThrowsInvalidBencodeException(string bencode)
        {
            Action action = () => Parser.ParseString(bencode);
            action.Should().Throw<InvalidBencodeException<BNumber>>();
        }

        [Fact]
        public void MinusZero_ThrowsInvalidBencodeException()
        {
            Action action = () => Parser.ParseString("i-0e");
            action.Should().Throw<InvalidBencodeException<BNumber>>();
        }

        [Theory]
        [InlineData("i")]
        [InlineData("i1")]
        [InlineData("i2")]
        [InlineData("i123")]
        public void MissingEndChar_ThrowsInvalidBencodeException(string bencode)
        {
            Action action = () => Parser.ParseString(bencode);
            action.Should().Throw<InvalidBencodeException<BNumber>>();
        }

        [Theory]
        [InlineData("1e")]
        [InlineData("42e")]
        [InlineData("100e")]
        [InlineData("1234567890e")]
        public void InvalidFirstChar_ThrowsInvalidBencodeException(string bencode)
        {
            Action action = () => Parser.ParseString(bencode);
            action.Should().Throw<InvalidBencodeException<BNumber>>();
        }

        [Fact]
        public void JustNegativeSign_ThrowsInvalidBencodeException()
        {
            Action action = () => Parser.ParseString("i-e");
            action.Should().Throw<InvalidBencodeException<BNumber>>();
        }

        [Theory]
        [InlineData("i--1e")]
        [InlineData("i--42e")]
        [InlineData("i---100e")]
        [InlineData("i----1234567890e")]
        public void MoreThanOneNegativeSign_ThrowsInvalidBencodeException(string bencode)
        {
            Action action = () => Parser.ParseString(bencode);
            action.Should().Throw<InvalidBencodeException<BNumber>>();
        }

        [Theory]
        [InlineData("i-e")]
        [InlineData("iasdfe")]
        [InlineData("i!#¤%&e")]
        [InlineData("i.e")]
        [InlineData("i42.e")]
        [InlineData("i42ae")]
        public void NonDigit_ThrowsInvalidBencodeException(string bencode)
        {
            Action action = () => Parser.ParseString(bencode);
            action.Should().Throw<InvalidBencodeException<BNumber>>();
        }

        [Fact]
        public void BelowMinimumLength_ThrowsInvalidBencodeException()
        {
            Action action = () => Parser.ParseString("ie");
            action.Should().Throw<InvalidBencodeException<BNumber>>();
        }

        [Theory]
        [InlineData("i9223372036854775808e")]
        [InlineData("i-9223372036854775809e")]
        public void LargerThanInt64_ThrowsUnsupportedException(string bencode)
        {
            Action action = () => Parser.ParseString(bencode);
            action.Should().Throw<UnsupportedBencodeException<BNumber>>();
        }

        [Theory]
        [InlineData("i12345678901234567890e")]
        [InlineData("i123456789012345678901e")]
        [InlineData("i123456789012345678901234567890e")]
        public void LongerThanMaxDigits19_ThrowsUnsupportedException(string bencode)
        {
            Action action = () => Parser.ParseString(bencode);
            action.Should().Throw<UnsupportedBencodeException<BNumber>>();
        }
    }
}
