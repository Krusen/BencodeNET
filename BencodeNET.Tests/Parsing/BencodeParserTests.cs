using System;
using BencodeNET.Exceptions;
using BencodeNET.Objects;
using BencodeNET.Parsing;
using FluentAssertions;
using Xunit;

namespace BencodeNET.Tests.Parsing
{
    // TODO: "Integration" tests? Full decode tests
    public class BencodeParserTests
    {
        [Theory]
        #region Alphabet...
        [AutoMockedData("a")]
        [AutoMockedData("b")]
        [AutoMockedData("c")]
        [AutoMockedData("e")]
        [AutoMockedData("f")]
        [AutoMockedData("g")]
        [AutoMockedData("h")]
        [AutoMockedData("j")]
        [AutoMockedData("k")]
        [AutoMockedData("m")]
        [AutoMockedData("n")]
        [AutoMockedData("o")]
        [AutoMockedData("p")]
        [AutoMockedData("q")]
        [AutoMockedData("r")]
        [AutoMockedData("s")]
        [AutoMockedData("t")]
        [AutoMockedData("u")]
        [AutoMockedData("v")]
        [AutoMockedData("w")]
        [AutoMockedData("x")]
        [AutoMockedData("y")]
        [AutoMockedData("z")]
        [AutoMockedData("A")]
        [AutoMockedData("B")]
        [AutoMockedData("C")]
        [AutoMockedData("D")]
        [AutoMockedData("E")]
        [AutoMockedData("F")]
        [AutoMockedData("G")]
        [AutoMockedData("H")]
        [AutoMockedData("I")]
        [AutoMockedData("J")]
        [AutoMockedData("K")]
        [AutoMockedData("L")]
        [AutoMockedData("M")]
        [AutoMockedData("N")]
        [AutoMockedData("O")]
        [AutoMockedData("P")]
        [AutoMockedData("Q")]
        [AutoMockedData("R")]
        [AutoMockedData("S")]
        [AutoMockedData("T")]
        [AutoMockedData("U")]
        [AutoMockedData("V")]
        [AutoMockedData("W")]
        [AutoMockedData("X")]
        [AutoMockedData("Y")]
        [AutoMockedData("Z")]
        #endregion
        public void InvalidFirstChars_ThrowsInvalidBencodeException(string bencode)
        {
            var bparser = new BencodeParser();
            Action action = () => bparser.ParseString(bencode);

            action.Should().Throw<InvalidBencodeException<IBObject>>();
        }

        [Fact]
        public void EmptyString_ReturnsNull()
        {
            var bparser = new BencodeParser();
            var result = bparser.ParseString("");
            result.Should().BeNull();
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
            bdictionary["spam"].Should().BeOfType(typeof(BString));
            bdictionary["spam"].Should().Be((BString)"egg");
            bdictionary["foo"].Should().BeOfType(typeof(BNumber));
            bdictionary["foo"].Should().Be((BNumber)42);
        }
    }
}
