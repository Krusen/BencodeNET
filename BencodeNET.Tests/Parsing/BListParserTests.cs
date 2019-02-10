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
    public class BListParserTests
    {
        [Theory]
        [InlineAutoMockedData("l-something-e")]
        [InlineAutoMockedData("l4:spame")]
        [InlineAutoMockedData("l4:spami42ee")]
        public void CanParseSimple(string bencode, IBencodeParser bparser)
        {
            // Arrange
            var bstring = new BString("test");
            SetupBencodeParser(bparser, bencode, bstring, hasEndChar: true);

            // Act
            var parser = new BListParser(bparser);
            var blist = parser.ParseString(bencode);

            // Assert
            blist.Count.Should().Be(1);
            blist[0].Should().BeOfType<BString>();
            blist[0].Should().BeSameAs(bstring);
            bparser.Received(1).Parse(Arg.Any<BencodeStream>());
        }

        [Theory]
        [InlineAutoMockedData("le")]
        public void CanParseEmptyList(string bencode, IBencodeParser bparser)
        {
            var parser = new BListParser(bparser);
            var blist = parser.ParseString(bencode);

            blist.Count.Should().Be(0);
            bparser.Received(0).Parse(Arg.Any<BencodeStream>());
        }

        [Theory]
        [InlineAutoMockedData("l")]
        public void BelowMinimumLength2_ThrowsInvalidBencodeException(string bencode, IBencodeParser bparser)
        {
            var parser = new BListParser(bparser);
            Action action = () => parser.ParseString(bencode);

            action.Should().Throw<InvalidBencodeException<BList>>();
        }

        [Theory]
        [InlineAutoMockedData("4")]
        [InlineAutoMockedData("a")]
        [InlineAutoMockedData(":")]
        [InlineAutoMockedData("-")]
        [InlineAutoMockedData(".")]
        [InlineAutoMockedData("e")]
        public void InvalidFirstChar_ThrowsInvalidBencodeException(string bencode, IBencodeParser bparser)
        {
            var parser = new BListParser(bparser);
            Action action = () => parser.ParseString(bencode);

            action.Should().Throw<InvalidBencodeException<BList>>();
        }

        [Theory]
        [InlineAutoMockedData("l")]
        [InlineAutoMockedData("l4:spam")]
        [InlineAutoMockedData("l ")]
        [InlineAutoMockedData("l:")]
        public void MissingEndChar_ThrowsInvalidBencodeException(string bencode, IBencodeParser bparser)
        {
            // Arrange
            var bstring = new BString("test");
            SetupBencodeParser(bparser, bencode, bstring, hasEndChar:false);

            // Act
            var parser = new BListParser(bparser);
            Action action = () => parser.ParseString(bencode);

            // Assert
            action.Should().Throw<InvalidBencodeException<BList>>();
        }

        private static void SetupBencodeParser(IBencodeParser bparser, string bencode, IBObject obj, bool hasEndChar)
        {
            bparser.Parse(Arg.Any<BencodeStream>())
                .Returns(obj)
                .AndDoes(x =>
                {
                    // Set stream position to end of list, skipping all "parsed" content
                    var stream = x.Arg<BencodeStream>();
                    stream.Position += Math.Max(1, bencode.Length - 1);

                    if (hasEndChar) stream.Position--;
                });
        }
    }
}
