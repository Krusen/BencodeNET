using System;
using System.Threading.Tasks;
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
    public partial class BDictionaryParserTests
    {
        [Theory]
        [AutoMockedData("d4:spam3:egge")]
        public async Task CanParseSimpleAsync(string bencode, IBencodeParser bparser)
        {
            // Arrange
            var key = new BString("key");
            var value = new BString("value");

            bparser.ParseAsync<BString>(Arg.Any<PipeBencodeReader>())
                .Returns(key);

            bparser.ParseAsync(Arg.Any<PipeBencodeReader>())
                .Returns(value)
                .AndSkipsAheadAsync(bencode.Length - 2);

            // Act
            var parser = new BDictionaryParser(bparser);
            var bdictionary = await parser.ParseStringAsync(bencode);

            // Assert
            bdictionary.Count.Should().Be(1);
            bdictionary.Should().ContainKey(key);
            bdictionary[key].Should().BeSameAs(value);
        }

        [Theory]
        [AutoMockedData("de")]
        public async Task CanParseEmptyDictionaryAsync(string bencode, IBencodeParser bparser)
        {
            var parser = new BDictionaryParser(bparser);
            var bdictionary = await parser.ParseStringAsync(bencode);

            bdictionary.Count.Should().Be(0);
        }

        [Theory]
        [AutoMockedData("")]
        [AutoMockedData("d")]
        public void BelowMinimumLength2_ThrowsInvalidBencodeExceptionAsync(string bencode, IBencodeParser bparser)
        {
            var parser = new BDictionaryParser(bparser);
            Func<Task> action = async () => await parser.ParseStringAsync(bencode);

            action.Should().Throw<InvalidBencodeException<BDictionary>>().WithMessage("*reached end of stream*");
        }

        [Theory]
        [AutoMockedData("ade")]
        [AutoMockedData(":de")]
        [AutoMockedData("-de")]
        [AutoMockedData("1de")]
        public void InvalidFirstChar_ThrowsInvalidBencodeExceptionAsync(string bencode, IBencodeParser bparser)
        {
            var parser = new BDictionaryParser(bparser);
            Func<Task> action = async () => await parser.ParseStringAsync(bencode);

            action.Should().Throw<InvalidBencodeException<BDictionary>>().WithMessage("*Unexpected character*");
        }

        [Theory]
        [AutoMockedData("da")]
        [AutoMockedData("d4:spam3:egg")]
        [AutoMockedData("d ")]
        public void MissingEndChar_ThrowsInvalidBencodeExceptionAsync(string bencode, IBencodeParser bparser, BString someKey, IBObject someValue)
        {
            // Arrange
            bparser.ParseAsync<BString>(Arg.Any<PipeBencodeReader>())
                .Returns(someKey);

            bparser.ParseAsync(Arg.Any<PipeBencodeReader>())
                .Returns(someValue)
                .AndSkipsAheadAsync(bencode.Length - 1);

            // Act
            var parser = new BDictionaryParser(bparser);
            Func<Task> action = async () => await parser.ParseStringAsync(bencode);

            // Assert
            action.Should().Throw<InvalidBencodeException<BDictionary>>().WithMessage("*Missing end character of object*");
        }

        [Theory]
        [AutoMockedData]
        public void InvalidKey_ThrowsInvalidBencodeExceptionAsync(IBencodeParser bparser)
        {
            bparser.ParseAsync<BString>(Arg.Any<PipeBencodeReader>()).Throws<BencodeException>();

            var parser = new BDictionaryParser(bparser);

            Func<Task> action = async () => await parser.ParseStringAsync("di42ee");

            action.Should().Throw<InvalidBencodeException<BDictionary>>().WithMessage("*Could not parse dictionary key*");
        }

        [Theory]
        [AutoMockedData]
        public void InvalidValue_ThrowsInvalidBencodeExceptionAsync(IBencodeParser bparser, BString someKey)
        {
            bparser.ParseAsync<BString>(Arg.Any<PipeBencodeReader>()).Returns(someKey);
            bparser.ParseAsync(Arg.Any<PipeBencodeReader>()).Throws<BencodeException>();

            var parser = new BDictionaryParser(bparser);

            Func<Task> action = async () => await parser.ParseStringAsync("di42ee");

            action.Should().Throw<InvalidBencodeException<BDictionary>>().WithMessage("*Could not parse dictionary value*");
        }

        [Theory]
        [AutoMockedData]
        public void DuplicateKey_ThrowsInvalidBencodeExceptionAsync(IBencodeParser bparser, BString someKey, BString someValue)
        {
            bparser.ParseAsync<BString>(Arg.Any<PipeBencodeReader>()).Returns(someKey, someKey);
            bparser.ParseAsync(Arg.Any<PipeBencodeReader>()).Returns(someValue);

            var parser = new BDictionaryParser(bparser);

            Func<Task> action = async () => await parser.ParseStringAsync("di42ee");

            action.Should().Throw<InvalidBencodeException<BDictionary>>().WithMessage("*The dictionary already contains the key*");
        }
    }
}
