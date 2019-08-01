using System;
using BencodeNET.Exceptions;
using BencodeNET.IO;
using BencodeNET.Objects;
using BencodeNET.Parsing;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace BencodeNET.Tests.Parsing
{
    public class BDictionaryParserTests
    {
        [Theory]
        [InlineAutoMockedData("d4:spam3:egge")]
        public void CanParseSimple(string bencode, IBencodeParser bparser)
        {
            // Arange
            var key = new BString("key");
            var value = new BString("value");

            bparser.Parse<BString>(Arg.Any<BencodeReader>())
                .Returns(key);

            bparser.Parse(Arg.Any<BencodeReader>())
                .Returns(value)
                .AndSkipsAhead(bencode.Length - 2);

            // Act
            var parser = new BDictionaryParser(bparser);
            var bdictionary = parser.ParseString(bencode);

            // Assert
            bdictionary.Count.Should().Be(1);
            bdictionary.Should().ContainKey(key);
            bdictionary[key].Should().BeSameAs(value);
        }

        [Theory]
        [InlineAutoMockedData("de")]
        public void CanParseEmptyDictionary(string bencode, IBencodeParser bparser)
        {
            var parser = new BDictionaryParser(bparser);
            var bdictionary = parser.ParseString(bencode);

            bdictionary.Count.Should().Be(0);
        }

        [Theory]
        [InlineAutoMockedData("")]
        [InlineAutoMockedData("d")]
        public void BelowMinimumLength2_ThrowsInvalidBencodeException(string bencode, IBencodeParser bparser)
        {
            var parser = new BDictionaryParser(bparser);
            Action action = () => parser.ParseString(bencode);

            action.Should().Throw<InvalidBencodeException<BDictionary>>().WithMessage("*Invalid length*");
        }

        [Theory]
        [InlineAutoMockedData("")]
        [InlineAutoMockedData("d")]
        public void BelowMinimumLength2_WhenStreamLengthNotSupported_ThrowsInvalidBencodeException(string bencode, IBencodeParser bparser)
        {
            var stream = new LengthNotSupportedStream(bencode);

            var parser = new BDictionaryParser(bparser);
            Action action = () => parser.Parse(stream);

            action.Should().Throw<InvalidBencodeException<BDictionary>>();
        }

        [Theory]
        [InlineAutoMockedData("ade")]
        [InlineAutoMockedData(":de")]
        [InlineAutoMockedData("-de")]
        [InlineAutoMockedData("1de")]
        public void InvalidFirstChar_ThrowsInvalidBencodeException(string bencode, IBencodeParser bparser)
        {
            var parser = new BDictionaryParser(bparser);
            Action action = () => parser.ParseString(bencode);

            action.Should().Throw<InvalidBencodeException<BDictionary>>().WithMessage("*Unexpected character*");
        }

        [Theory]
        [InlineAutoMockedData("da")]
        [InlineAutoMockedData("d4:spam3:egg")]
        [InlineAutoMockedData("d ")]
        public void MissingEndChar_ThrowsInvalidBencodeException(string bencode, IBencodeParser bparser, BString someKey, IBObject someValue)
        {
            // Arrange
            bparser.Parse<BString>(Arg.Any<BencodeReader>())
                .Returns(someKey);

            bparser.Parse(Arg.Any<BencodeReader>())
                .Returns(someValue)
                .AndSkipsAhead(bencode.Length - 1);

            // Act
            var parser = new BDictionaryParser(bparser);
            Action action = () => parser.ParseString(bencode);

            // Assert
            action.Should().Throw<InvalidBencodeException<BDictionary>>().WithMessage("*Missing end character of object*");
        }

        [Theory]
        [InlineAutoMockedData]
        public void InvalidKey_ThrowsInvalidBencodeException(IBencodeParser bparser)
        {
            bparser.Parse<BString>(Arg.Any<BencodeReader>()).Throws<BencodeException>();

            var parser = new BDictionaryParser(bparser);

            Action action = () => parser.ParseString("di42ee");

            action.Should().Throw<InvalidBencodeException<BDictionary>>().WithMessage("*Could not parse dictionary key*");
        }

        [Theory]
        [InlineAutoMockedData]
        public void InvalidValue_ThrowsInvalidBencodeException(IBencodeParser bparser, BString someKey)
        {
            bparser.Parse<BString>(Arg.Any<BencodeReader>()).Returns(someKey);
            bparser.Parse(Arg.Any<BencodeReader>()).Throws<BencodeException>();

            var parser = new BDictionaryParser(bparser);

            Action action = () => parser.ParseString("di42ee");

            action.Should().Throw<InvalidBencodeException<BDictionary>>().WithMessage("*Could not parse dictionary value*");
        }

        [Theory]
        [InlineAutoMockedData]
        public void DuplicateKey_ThrowsInvalidBencodeException(IBencodeParser bparser, BString someKey, BString someValue)
        {
            bparser.Parse<BString>(Arg.Any<BencodeReader>()).Returns(someKey, someKey);
            bparser.Parse(Arg.Any<BencodeReader>()).Returns(someValue);

            var parser = new BDictionaryParser(bparser);

            Action action = () => parser.ParseString("di42ee");

            action.Should().Throw<InvalidBencodeException<BDictionary>>().WithMessage("*The dictionary already contains the key*");
        }
    }
}
