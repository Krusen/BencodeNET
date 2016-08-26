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
    // TODO: "Integration" tests? Full decode tests
    // TODO: stream/bencodestream methods
    public class BencodeParserTests
    {
        [Theory]
        [InlineAutoMockedData("0")]
        [InlineAutoMockedData("1")]
        [InlineAutoMockedData("2")]
        [InlineAutoMockedData("3")]
        [InlineAutoMockedData("4")]
        [InlineAutoMockedData("5")]
        [InlineAutoMockedData("6")]
        [InlineAutoMockedData("7")]
        [InlineAutoMockedData("8")]
        [InlineAutoMockedData("9")]
        public void FirstCharDigit_CallsStringParser(string bencode, IBObjectParser<BString> stringParser)
        {
            var bparser = new BencodeParser();
            bparser.Parsers.AddOrReplace(stringParser);
            bparser.ParseString(bencode);

            stringParser.Received(1).Parse(Arg.Any<BencodeStream>());
        }

        [Theory]
        [InlineAutoMockedData("i")]
        public void FirstChar_I_CallsNumberParser(string bencode, IBObjectParser<BNumber> numberParser)
        {
            var bparser = new BencodeParser();
            bparser.Parsers.AddOrReplace(numberParser);
            bparser.ParseString(bencode);

            numberParser.Received(1).Parse(Arg.Any<BencodeStream>());
        }

        [Theory]
        [InlineAutoMockedData("l")]
        public void FirstChar_L_CallsNumberParser(string bencode, IBObjectParser<BList> listParser)
        {
            var bparser = new BencodeParser();
            bparser.Parsers.AddOrReplace(listParser);
            bparser.ParseString(bencode);

            listParser.Received(1).Parse(Arg.Any<BencodeStream>());
        }

        [Theory]
        [InlineAutoMockedData("d")]
        public void FirstChar_D_CallsNumberParser(string bencode, IBObjectParser<BDictionary> dictionaryParser)
        {
            var bparser = new BencodeParser();
            bparser.Parsers.AddOrReplace(dictionaryParser);
            bparser.ParseString(bencode);

            dictionaryParser.Received(1).Parse(Arg.Any<BencodeStream>());
        }

        [Theory]
        #region Alphabet...
        [InlineAutoMockedData("a")]
        [InlineAutoMockedData("b")]
        [InlineAutoMockedData("c")]
        [InlineAutoMockedData("e")]
        [InlineAutoMockedData("f")]
        [InlineAutoMockedData("g")]
        [InlineAutoMockedData("h")]
        [InlineAutoMockedData("j")]
        [InlineAutoMockedData("k")]
        [InlineAutoMockedData("m")]
        [InlineAutoMockedData("n")]
        [InlineAutoMockedData("o")]
        [InlineAutoMockedData("p")]
        [InlineAutoMockedData("q")]
        [InlineAutoMockedData("r")]
        [InlineAutoMockedData("s")]
        [InlineAutoMockedData("t")]
        [InlineAutoMockedData("u")]
        [InlineAutoMockedData("v")]
        [InlineAutoMockedData("w")]
        [InlineAutoMockedData("x")]
        [InlineAutoMockedData("y")]
        [InlineAutoMockedData("z")]
        [InlineAutoMockedData("A")]
        [InlineAutoMockedData("B")]
        [InlineAutoMockedData("C")]
        [InlineAutoMockedData("D")]
        [InlineAutoMockedData("E")]
        [InlineAutoMockedData("F")]
        [InlineAutoMockedData("G")]
        [InlineAutoMockedData("H")]
        [InlineAutoMockedData("I")]
        [InlineAutoMockedData("J")]
        [InlineAutoMockedData("K")]
        [InlineAutoMockedData("L")]
        [InlineAutoMockedData("M")]
        [InlineAutoMockedData("N")]
        [InlineAutoMockedData("O")]
        [InlineAutoMockedData("P")]
        [InlineAutoMockedData("Q")]
        [InlineAutoMockedData("R")]
        [InlineAutoMockedData("S")]
        [InlineAutoMockedData("T")]
        [InlineAutoMockedData("U")]
        [InlineAutoMockedData("V")]
        [InlineAutoMockedData("W")]
        [InlineAutoMockedData("X")]
        [InlineAutoMockedData("Y")]
        [InlineAutoMockedData("Z")]
        #endregion
        public void InvalidFirstChars_ThrowsInvalidBencodeException(string bencode)
        {
            var bparser = new BencodeParser();
            Action action = () => bparser.ParseString(bencode);

            action.ShouldThrow<InvalidBencodeException<IBObject>>();
        }

        [Fact]
        public void CanParse_ListOfStrings()
        {
            var bencode = "l4:spam3:egge";

            var bparser = new BencodeParser();
            var blist = bparser.ParseString(bencode) as BList;

            blist.Should().HaveCount(2);
            blist[0].Should().BeOfType<BString>();
            blist[0].Should().Be((BString)"spam");
            blist[1].Should().BeOfType<BString>();
            blist[1].Should().Be((BString)"egg");
        }

        [Fact]
        public void CanParseGeneric_ListOfStrings()
        {
            var bencode = "l4:spam3:egge";

            var bparser = new BencodeParser();
            var blist = bparser.ParseString<BList>(bencode);

            blist.Should().HaveCount(2);
            blist[0].Should().BeOfType<BString>();
            blist[0].Should().Be((BString)"spam");
            blist[1].Should().BeOfType<BString>();
            blist[1].Should().Be((BString)"egg");
        }

        [Fact]
        public void CanParse_SimpleDictionary()
        {
            var bencode = "d4:spam3:egg3:fooi42ee";

            var bparser = new BencodeParser();
            var bdictionary = bparser.ParseString<BDictionary>(bencode);

            bdictionary.Should().HaveCount(2);
            bdictionary.Should().ContainKey("spam");
            bdictionary.Should().ContainKey("foo");
            bdictionary["spam"].Should().BeOfType(typeof (BString));
            bdictionary["spam"].Should().Be((BString) "egg");
            bdictionary["foo"].Should().BeOfType(typeof (BNumber));
            bdictionary["foo"].Should().Be((BNumber) 42);
        }
    }
}
