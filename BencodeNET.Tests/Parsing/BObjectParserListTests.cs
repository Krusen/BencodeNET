using System;
using BencodeNET.Objects;
using BencodeNET.Parsing;
using FluentAssertions;
using Xunit;

namespace BencodeNET.Tests.Parsing
{
    public class BObjectParserListTests
    {
        [Theory]
        [InlineAutoMockedData]
        public void Add_GenericParser_ContainsOnlyThatParser(IBObjectParser<IBObject> parser)
        {
            var list = new BObjectParserList();
            list.Add(parser);

            list.Should().HaveCount(1);
            list.Should().ContainSingle(x => x.Value == parser);
        }

        [Theory]
        [InlineAutoMockedData]
        public void Add_GenericParser_AddedWithGenericTypeAsKey(IBObjectParser<IBObject> parser)
        {
            var list = new BObjectParserList();
            list.Add(parser);

            list.Should().HaveCount(1);
            list.Should().ContainSingle(x => x.Key == typeof(IBObject));
        }

        [Theory]
        [InlineAutoMockedData]
        public void Add_GenericParser_ReplacesExistingOfSameGenericType(IBObjectParser<IBObject> parser)
        {
            var list = new BObjectParserList();
            list.Add(parser);
            list.Add(parser);

            list.Should().HaveCount(1);
            list.Should().ContainSingle(x => x.Value == parser);
        }

        [Theory]
        [InlineAutoMockedData()]
        public void Add_ParserWithType_ReplacesExistingOfSameGenericType(IBObjectParser parser)
        {
            var list = new BObjectParserList();
            list.Add(typeof(BString), parser);
            list.Add(typeof(BString), parser);

            list.Should().HaveCount(1);
            list.Should().ContainSingle(x => x.Value == parser);
        }

        [Theory]
        [InlineAutoMockedData(typeof(object))]
        [InlineAutoMockedData(typeof(string))]
        [InlineAutoMockedData(typeof(int))]
        public void Add_ParserWithNonIBObjectType_ThrowsArgumentException(Type type, IBObjectParser parser)
        {
            var list = new BObjectParserList();
            Action action = () => list.Add(type, parser);

            action.ShouldThrow<ArgumentException>("because only IBObject types are allowed");
        }

        [Theory]
        [InlineAutoMockedData]
        public void Add_WithMultipleTypes_AddsParserForEachType(IBObjectParser parser)
        {
            var types = new[] {typeof (BString), typeof (BNumber), typeof (BList)};

            var list = new BObjectParserList();
            list.Add(types, parser);

            list.Should().HaveCount(3);
            list.Should().OnlyContain(x => x.Value == parser);
        }

        [Theory]
        [InlineAutoMockedData]
        public void Clear_EmptiesList(IBObjectParser<BString> parser1, IBObjectParser<BNumber> parser2)
        {
            var list = new BObjectParserList {parser1, parser2};
            list.Clear();

            list.Should().BeEmpty();
        }

        [Fact]
        public void Indexer_Get_ReturnsNullIfKeyMissing()
        {
            var list = new BObjectParserList();

            var parser = list[typeof (object)];

            parser.Should().BeNull();
        }

        [Fact]
        public void Indexer_Get_ReturnsMatchingParserForType()
        {
            var stringParser = new StringParser();
            var list = new BObjectParserList {stringParser};

            var parser = list[typeof(BString)];

            parser.Should().BeSameAs(stringParser);
        }

        [Fact]
        public void Indexer_Set_AddsParserForType()
        {
            var stringParser = new StringParser();

            var list = new BObjectParserList();
            list[typeof (BString)] = stringParser;

            list.Should().HaveCount(1);
            list[typeof (BString)].Should().BeSameAs(stringParser);
        }

        [Fact]
        public void Indexer_Set_ReplacesExistingParserForType()
        {
            var stringParser1 = new StringParser();
            var stringParser2 = new StringParser();
            var list = new BObjectParserList { stringParser1 };

            list[typeof (BString)] = stringParser2;

            list.Should().HaveCount(1);
            list[typeof (BString)].Should().BeSameAs(stringParser2);
        }

        [Fact]
        public void Get_Generic_ReturnsMatchingParser()
        {
            var stringParser = new StringParser();
            var list = new BObjectParserList {stringParser};

            var parser = list.Get<BString>();

            parser.Should().BeSameAs(stringParser);
        }

        [Fact]
        public void Get_Generic_ReturnsNullIfNoMatchingParser()
        {
            var list = new BObjectParserList();

            var parser = list.Get<BString>();

            parser.Should().BeNull();
        }
    }
}
