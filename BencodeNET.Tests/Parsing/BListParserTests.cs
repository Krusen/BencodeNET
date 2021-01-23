using System;
using BencodeNET.Exceptions;
using BencodeNET.IO;
using BencodeNET.Objects;
using BencodeNET.Parsing;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace BencodeNET.Tests.Parsing
{
    public partial class BListParserTests
    {
        [Theory]
        [AutoMockedData("l-something-e")]
        [AutoMockedData("l4:spame")]
        [AutoMockedData("l4:spami42ee")]
        public void CanParseSimple(string bencode, IBencodeParser bparser)
        {
            // Arrange
            var bstring = new BString("test");
            bparser.Parse(Arg.Any<BencodeReader>())
                .Returns(bstring)
                .AndSkipsAhead(bencode.Length - 2);

            // Act
            var parser = new BListParser(bparser);
            var blist = parser.ParseString(bencode);

            // Assert
            blist.Count.Should().Be(1);
            blist[0].Should().BeOfType<BString>();
            blist[0].Should().BeSameAs(bstring);
            bparser.Received(1).Parse(Arg.Any<BencodeReader>());
        }

        [Theory]
        [AutoMockedData("le")]
        public void CanParseEmptyList(string bencode, IBencodeParser bparser)
        {
            var parser = new BListParser(bparser);
            var blist = parser.ParseString(bencode);

            blist.Count.Should().Be(0);
            bparser.DidNotReceive().Parse(Arg.Any<BencodeReader>());
        }

        [Theory]
        [AutoMockedData("")]
        [AutoMockedData("l")]
        public void BelowMinimumLength2_ThrowsInvalidBencodeException(string bencode, IBencodeParser bparser)
        {
            var parser = new BListParser(bparser);
            Action action = () => parser.ParseString(bencode);

            action.Should().Throw<InvalidBencodeException<BList>>().WithMessage("*Invalid length*");
        }

        [Theory]
        [AutoMockedData("4")]
        [AutoMockedData("a")]
        [AutoMockedData(":")]
        [AutoMockedData("-")]
        [AutoMockedData(".")]
        [AutoMockedData("e")]
        public void BelowMinimumLength2_WhenStreamLengthNotSupported_ThrowsInvalidBencodeException(string bencode, IBencodeParser bparser)
        {
            var stream = new LengthNotSupportedStream(bencode);

            var parser = new BListParser(bparser);
            Action action = () => parser.Parse(stream);

            action.Should().Throw<InvalidBencodeException<BList>>().WithMessage("*Unexpected character*");
        }

        [Theory]
        [AutoMockedData("4e")]
        [AutoMockedData("ae")]
        [AutoMockedData(":e")]
        [AutoMockedData("-e")]
        [AutoMockedData(".e")]
        [AutoMockedData("ee")]
        public void InvalidFirstChar_ThrowsInvalidBencodeException(string bencode, IBencodeParser bparser)
        {
            var parser = new BListParser(bparser);
            Action action = () => parser.ParseString(bencode);

            action.Should().Throw<InvalidBencodeException<BList>>().WithMessage("*Unexpected character*");
        }

        [Theory]
        [AutoMockedData("l4:spam")]
        [AutoMockedData("l ")]
        [AutoMockedData("l:")]
        public void MissingEndChar_ThrowsInvalidBencodeException(string bencode, IBencodeParser bparser, IBObject something)
        {
            // Arrange
            bparser.Parse(Arg.Any<BencodeReader>())
                .Returns(something)
                .AndSkipsAhead(bencode.Length - 1);

            // Act
            var parser = new BListParser(bparser);
            Action action = () => parser.ParseString(bencode);

            // Assert
            action.Should().Throw<InvalidBencodeException<BList>>().WithMessage("*Missing end character of object*");
        }
    }
}
