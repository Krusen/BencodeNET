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
    public class BDictionaryParserTests
    {
        [Theory]
        [InlineAutoMockedData("d4:spam3:egge")]
        public void CanParseSimple(string bencode, IBencodeParser bparser)
        {
            var key = new BString("key");
            var value = new BString("value");
            SetupBencodeParser(bparser, bencode, key, value, hasEndChar:true);

            var parser = new BDictionaryParser(bparser);
            var bdictionary = parser.Parse(bencode);

            bdictionary.Count.Should().Be(1);
            bdictionary.Should().ContainKey(key);
            bdictionary[key].Should().BeSameAs(value);
        }

        [Theory]
        [InlineAutoMockedData("de")]
        public void CanParseEmptyDictionary(string bencode, IBencodeParser bparser)
        {
            var parser = new BDictionaryParser(bparser);
            var bdictionary = parser.Parse(bencode);

            bdictionary.Count.Should().Be(0);
        }

        [Theory]
        [InlineAutoMockedData("d")]
        public void BelowMinimumLength2_ThrowsParsingException(string bencode, IBencodeParser bparser)
        {
            var parser = new BDictionaryParser(bparser);
            Action action = () => parser.Parse(bencode);

            action.ShouldThrow<BencodeParsingException<BDictionary>>();
        }

        [Theory]
        [InlineAutoMockedData("ade")]
        [InlineAutoMockedData(":de")]
        [InlineAutoMockedData("-de")]
        [InlineAutoMockedData("1de")]
        public void InvalidFirstChar_ThrowsParsingException(string bencode, IBencodeParser bparser)
        {
            var parser = new BDictionaryParser(bparser);
            Action action = () => parser.Parse(bencode);

            action.ShouldThrow<BencodeParsingException<BDictionary>>();
        }

        [Theory]
        [InlineAutoMockedData("da")]
        [InlineAutoMockedData("d4:spam3:egg")]
        [InlineAutoMockedData("d ")]
        public void MissingEndChar_ThrowsParsingException(string bencode, IBencodeParser bparser)
        {
            SetupBencodeParser(bparser, bencode, new BString("key"), new BString("value"), hasEndChar:false);

            var parser = new BDictionaryParser(bparser);
            Action action = () => parser.Parse(bencode);

            action.ShouldThrow<BencodeParsingException<BDictionary>>();
        }

        private static void SetupBencodeParser(IBencodeParser bparser, string bencode, BString key, IBObject value, bool hasEndChar)
        {
            bparser.Parse<BString>(Arg.Any<BencodeStream>())
                .Returns(key);

            bparser.Parse(Arg.Any<BencodeStream>())
                .Returns(value)
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
