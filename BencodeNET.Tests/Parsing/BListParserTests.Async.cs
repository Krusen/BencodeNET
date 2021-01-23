using System;
using System.Threading.Tasks;
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
        public async Task CanParseSimpleAsync(string bencode, IBencodeParser bparser)
        {
            // Arrange
            var bstring = new BString("test");
            bparser.ParseAsync(Arg.Any<PipeBencodeReader>())
                .Returns(bstring)
                .AndSkipsAheadAsync(bencode.Length - 2);

            // Act
            var parser = new BListParser(bparser);
            var blist = await parser.ParseStringAsync(bencode);

            // Assert
            blist.Count.Should().Be(1);
            blist[0].Should().BeOfType<BString>();
            blist[0].Should().BeSameAs(bstring);
            await bparser.Received(1).ParseAsync(Arg.Any<PipeBencodeReader>());
        }

        [Theory]
        [AutoMockedData("le")]
        public async Task CanParseEmptyListAsync(string bencode, IBencodeParser bparser)
        {
            var parser = new BListParser(bparser);
            var blist = await parser.ParseStringAsync(bencode);

            blist.Count.Should().Be(0);
            await bparser.DidNotReceive().ParseAsync(Arg.Any<PipeBencodeReader>());
        }

        [Theory]
        [AutoMockedData("")]
        [AutoMockedData("l")]
        public void BelowMinimumLength2_ThrowsInvalidBencodeExceptionAsync(string bencode, IBencodeParser bparser)
        {
            var parser = new BListParser(bparser);
            Func<Task> action = async () => await parser.ParseStringAsync(bencode);

            action.Should().Throw<InvalidBencodeException<BList>>().WithMessage("*reached end of stream*");
        }

        [Theory]
        [AutoMockedData("4e")]
        [AutoMockedData("ae")]
        [AutoMockedData(":e")]
        [AutoMockedData("-e")]
        [AutoMockedData(".e")]
        [AutoMockedData("ee")]
        public void InvalidFirstChar_ThrowsInvalidBencodeExceptionAsync(string bencode, IBencodeParser bparser)
        {
            var parser = new BListParser(bparser);
            Func<Task> action = async () => await parser.ParseStringAsync(bencode);

            action.Should().Throw<InvalidBencodeException<BList>>().WithMessage("*Unexpected character*");
        }

        [Theory]
        [AutoMockedData("l4:spam")]
        [AutoMockedData("l ")]
        [AutoMockedData("l:")]
        public void MissingEndChar_ThrowsInvalidBencodeExceptionAsync(string bencode, IBencodeParser bparser, IBObject something)
        {
            // Arrange
            bparser.ParseAsync(Arg.Any<PipeBencodeReader>())
                .Returns(something)
                .AndSkipsAheadAsync(bencode.Length - 1);

            // Act
            var parser = new BListParser(bparser);
            Func<Task> action = async () => await parser.ParseStringAsync(bencode);

            // Assert
            action.Should().Throw<InvalidBencodeException<BList>>().WithMessage("*Missing end character of object*");
        }
    }
}
