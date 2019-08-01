using System;
using System.Text;
using BencodeNET.Exceptions;
using BencodeNET.Objects;
using BencodeNET.Parsing;
using FluentAssertions;
using Xunit;

namespace BencodeNET.Tests.Parsing
{
    public class BStringParserTests
    {
        private BStringParser Parser { get; }

        public BStringParserTests()
        {
            Parser = new BStringParser();
        }

        [Theory]
        [InlineData("4:spam")]
        [InlineData("8:spameggs")]
        [InlineData("9:spam eggs")]
        [InlineData("9:spam:eggs")]
        [InlineData("14:!@#¤%&/()=?$|")]
        public void CanParseSimple(string bencode)
        {
            var parts = bencode.Split(new[] {':'}, 2);
            var length = int.Parse(parts[0]);
            var value = parts[1];

            var bstring = Parser.ParseString(bencode);

            bstring.Length.Should().Be(length);
            bstring.Should().Be(value);
        }

        [Fact]
        public void CanParse_EmptyString()
        {
            var bstring = Parser.ParseString("0:");

            bstring.Length.Should().Be(0);
            bstring.Should().Be("");
        }

        [Theory]
        [InlineData("5:spam")]
        [InlineData("6:spam")]
        [InlineData("100:spam")]
        public void LessCharsThanSpecified_ThrowsInvalidBencodeException(string bencode)
        {
            Action action = () => Parser.ParseString(bencode);
            action.Should().Throw<InvalidBencodeException<BString>>()
                .WithMessage("*but could only read * bytes*")
                .Which.StreamPosition.Should().Be(0);
        }

        [Theory]
        [InlineData("4spam", 1)]
        [InlineData("10spam", 2)]
        [InlineData("4-spam", 1)]
        [InlineData("4.spam", 1)]
        [InlineData("4;spam", 1)]
        [InlineData("4,spam", 1)]
        [InlineData("4|spam", 1)]
        public void MissingDelimiter_ThrowsInvalidBencodeException(string bencode, int errorIndex)
        {
            Action action = () => Parser.ParseString(bencode);
            action.Should().Throw<InvalidBencodeException<BString>>()
                .WithMessage("*Unexpected character. Expected ':'*")
                .Which.StreamPosition.Should().Be(errorIndex);
        }

        [Theory]
        [InlineData("spam")]
        [InlineData("-spam")]
        [InlineData(".spam")]
        [InlineData(",spam")]
        [InlineData(";spam")]
        [InlineData("?spam")]
        [InlineData("!spam")]
        [InlineData("#spam")]
        public void NonDigitFirstChar_ThrowsInvalidBencodeException(string bencode)
        {
            Action action = () => Parser.ParseString(bencode);
            action.Should().Throw<InvalidBencodeException<BString>>()
                .WithMessage($"*Unexpected character. Expected ':' but found '{bencode[0]}'*")
                .Which.StreamPosition.Should().Be(0);
        }

        [Theory]
        [InlineData("0")]
        [InlineData("4")]
        public void LessThanMinimumLength2_ThrowsInvalidBencodeException(string bencode)
        {
            Action action = () => Parser.ParseString(bencode);
            action.Should().Throw<InvalidBencodeException<BString>>()
                .WithMessage("*Invalid length*")
                .Which.StreamPosition.Should().Be(0);
        }

        [Theory]
        [InlineData("12345678901:spam")]
        [InlineData("123456789012:spam")]
        [InlineData("1234567890123:spam")]
        [InlineData("12345678901234:spam")]
        public void LengthAboveMaxDigits10_ThrowsUnsupportedException(string bencode)
        {
            Action action = () => Parser.ParseString(bencode);
            action.Should().Throw<UnsupportedBencodeException<BString>>()
                .WithMessage("*Length of string is more than * digits*")
                .Which.StreamPosition.Should().Be(0);
        }

        [Theory]
        [InlineData("1:spam")]
        [InlineData("12:spam")]
        [InlineData("123:spam")]
        [InlineData("1234:spam")]
        [InlineData("12345:spam")]
        [InlineData("123456:spam")]
        [InlineData("1234567:spam")]
        [InlineData("12345678:spam")]
        [InlineData("123456789:spam")]
        [InlineData("1234567890:spam")]
        public void LengthAtOrBelowMaxDigits10_DoesNotThrowUnsupportedException(string bencode)
        {
            Action action = () => Parser.ParseString(bencode);
            action.Should().NotThrow<UnsupportedBencodeException<BString>>();
        }

        [Fact]
        public void LengthAboveInt32MaxValue_ThrowsUnsupportedException()
        {
            var bencode = "2147483648:spam";
            Action action = () => Parser.ParseString(bencode);
            action.Should().Throw<UnsupportedBencodeException<BString>>()
                .WithMessage("*Length of string is * but maximum supported length is *")
                .Which.StreamPosition.Should().Be(0);
        }

        [Fact]
        public void LengthBelowInt32MaxValue_DoesNotThrowUnsupportedException()
        {
            var bencode = "2147483647:spam";
            Action action = () => Parser.ParseString(bencode);
            action.Should().NotThrow<UnsupportedBencodeException<BString>>();
        }

        [Fact]
        public void CanParseEncodedAsLatin1()
        {
            var encoding = Encoding.GetEncoding("LATIN1");
            var expected = new BString("æøå", encoding);
            var parser = new BStringParser(encoding);

            // "3:æøå"
            var bytes = new byte[] {51, 58, 230, 248, 229};
            var bstring = parser.Parse(bytes);

            bstring.Should().Be(expected);
        }

        [Theory]
        [InlineData("1-:a", 1)]
        [InlineData("1abc:a", 1)]
        [InlineData("123?:asdf", 3)]
        [InlineData("3abc:abc", 1)]
        public void InvalidLengthString_ThrowsInvalidException(string bencode, int errorIndex)
        {
            Action action = () => Parser.ParseString(bencode);
            action.Should().Throw<InvalidBencodeException<BString>>()
                .WithMessage("*Unexpected character. Expected ':'*")
                .Which.StreamPosition.Should().Be(errorIndex);
        }

        [Theory]
        [InlineData("")]
        [InlineData("0")]
        public void BelowMinimumLength_WhenStreamWithoutLengthSupport_ThrowsInvalidException(string bencode)
        {
            var stream = new LengthNotSupportedStream(bencode);
            Action action = () => Parser.Parse(stream);
            action.Should().Throw<InvalidBencodeException<BString>>()
                .WithMessage("*Unexpected character. Expected ':' but reached end of stream*")
                .Which.StreamPosition.Should().Be(0);
        }
    }
}
