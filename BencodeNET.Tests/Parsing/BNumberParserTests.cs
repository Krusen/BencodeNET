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
            action.Should().Throw<InvalidBencodeException<BNumber>>()
                .WithMessage("*Leading '0's are not valid.*")
                .Which.StreamPosition.Should().Be(0);
        }

        [Fact]
        public void MinusZero_ThrowsInvalidBencodeException()
        {
            Action action = () => Parser.ParseString("i-0e");
            action.Should().Throw<InvalidBencodeException<BNumber>>()
                .WithMessage("*'-0' is not a valid number.*")
                .Which.StreamPosition.Should().Be(0);
        }

        [Theory]
        [InlineData("i12")]
        [InlineData("i123")]
        public void MissingEndChar_ThrowsInvalidBencodeException(string bencode)
        {
            Action action = () => Parser.ParseString(bencode);
            action.Should().Throw<InvalidBencodeException<BNumber>>()
                .WithMessage("*Missing end character of object.*")
                .Which.StreamPosition.Should().Be(0);
        }

        [Theory]
        [InlineData("42e")]
        [InlineData("a42e")]
        [InlineData("d42e")]
        [InlineData("l42e")]
        [InlineData("100e")]
        [InlineData("1234567890e")]
        public void InvalidFirstChar_ThrowsInvalidBencodeException(string bencode)
        {
            Action action = () => Parser.ParseString(bencode);
            action.Should().Throw<InvalidBencodeException<BNumber>>()
                .WithMessage("*Unexpected character. Expected 'i'*")
                .Which.StreamPosition.Should().Be(0);
        }

        [Fact]
        public void JustNegativeSign_ThrowsInvalidBencodeException()
        {
            Action action = () => Parser.ParseString("i-e");
            action.Should().Throw<InvalidBencodeException<BNumber>>()
                .WithMessage("*It contains no digits.*")
                .Which.StreamPosition.Should().Be(0);
        }

        [Theory]
        [InlineData("i--1e")]
        [InlineData("i--42e")]
        [InlineData("i---100e")]
        [InlineData("i----1234567890e")]
        public void MoreThanOneNegativeSign_ThrowsInvalidBencodeException(string bencode)
        {
            Action action = () => Parser.ParseString(bencode);
            action.Should().Throw<InvalidBencodeException<BNumber>>()
                .WithMessage("*The value '*' is not a valid number.*")
                .Which.StreamPosition.Should().Be(0);
        }

        [Theory]
        [InlineData("iasdfe")]
        [InlineData("i!#¤%&e")]
        [InlineData("i.e")]
        [InlineData("i42.e")]
        [InlineData("i42ae")]
        public void NonDigit_ThrowsInvalidBencodeException(string bencode)
        {
            Action action = () => Parser.ParseString(bencode);
            action.Should().Throw<InvalidBencodeException<BNumber>>()
                .WithMessage("*The value '*' is not a valid number.*")
                .Which.StreamPosition.Should().Be(0);
        }


        [Theory]
        [InlineData("")]
        [InlineData("i")]
        [InlineData("ie")]
        public void BelowMinimumLength_ThrowsInvalidBencodeException(string bencode)
        {
            Action action = () => Parser.ParseString(bencode);
            action.Should().Throw<InvalidBencodeException<BNumber>>()
                .WithMessage("*Invalid length.*")
                .Which.StreamPosition.Should().Be(0);
        }

        [Theory]
        [InlineData("")]
        [InlineData("i")]
        [InlineData("ie")]
        public void BelowMinimumLength_WhenStreamWithoutLengthSupport_ThrowsInvalidException(string bencode)
        {
            var stream = new LengthNotSupportedStream(bencode);
            Action action = () => Parser.Parse(stream);
            action.Should().Throw<InvalidBencodeException<BNumber>>()
                .Which.StreamPosition.Should().Be(0);
        }

        [Theory]
        [InlineData("i9223372036854775808e")]
        [InlineData("i-9223372036854775809e")]
        public void LargerThanInt64_ThrowsUnsupportedException(string bencode)
        {
            Action action = () => Parser.ParseString(bencode);
            action.Should().Throw<UnsupportedBencodeException<BNumber>>()
                .WithMessage("*The value '*' is not a valid long (Int64)*")
                .Which.StreamPosition.Should().Be(0);
        }

        [Theory]
        [InlineData("i12345678901234567890e")]
        [InlineData("i123456789012345678901e")]
        [InlineData("i123456789012345678901234567890e")]
        public void LongerThanMaxDigits19_ThrowsUnsupportedException(string bencode)
        {
            Action action = () => Parser.ParseString(bencode);
            action.Should().Throw<UnsupportedBencodeException<BNumber>>()
                .WithMessage("*The number '*' has more than 19 digits and cannot be stored as a long*")
                .Which.StreamPosition.Should().Be(0);
        }
    }
}
