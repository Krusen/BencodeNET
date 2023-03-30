using System;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BencodeNET.Objects;
using FluentAssertions;
using Xunit;

namespace BencodeNET.Tests.Objects
{
    public class BListTests
    {
        [Fact]
        public void Add_Null_ThrowsArgumentNullException()
        {
            var blist = new BList();
            Action action = () => blist.Add((IBObject) null);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void AddRange_AppendsList()
        {
            var blist1 = new BList {"item1", "item2"};
            var blist2 = new BList {"item3", "item4"};

            blist1.AddRange(blist2);

            blist1.Should().HaveCount(4);
            blist1.Should().ContainInOrder((BString) "item1", (BString) "item2", (BString) "item3", (BString) "item4");
        }

        [Fact]
        public void Indexer_Set_Null_ThrowsArgumentNullException()
        {
            var blist = new BList {0};
            Action action = () => blist[0] = null;
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void SequenceEqual_SameOrder_AreEqual()
        {
            var blist1 = new BList
            {
                "foo",
                "bar"
            };

            var blist2 = new BList
            {
                "foo",
                "bar"
            };

            blist1.SequenceEqual(blist2).Should().BeTrue();
        }

        [Fact]
        public void SequenceEqual_DifferentOrder_AreNotEqual()
        {
            var blist1 = new BList
            {
                "foo",
                "bar"
            };

            var blist2 = new BList
            {
                "bar",
                "foo"
            };

            blist1.SequenceEqual(blist2).Should().BeFalse();
        }

        #region Encode

        [Fact]
        public void CanEncode_Simple()
        {
            var blist = new BList {"hello world", 987, "foobar"};
            var bencode = blist.EncodeAsString();
            bencode.Should().Be("l11:hello worldi987e6:foobare");
        }

        [Fact]
        public void CanEncode_EmptyList()
        {
            var blist = new BList();
            var bencode = blist.EncodeAsString();
            bencode.Should().Be("le");
        }

        [Fact]
        public void CanEncode_UTF8()
        {
            var blist = new BList { "æøå äö èéê ñ" };
            var bencode = blist.EncodeAsString();
            bencode.Should().Be("l21:æøå äö èéê ñe");
        }

        [Fact]
        public void CanEncode_ISO88591()
        {
            var encoding = Encoding.GetEncoding("ISO-8859-1");
            var blist = new BList { new BString("æøå äö èéê ñ", encoding) };

            var bencode = blist.EncodeAsString(encoding);

            bencode.Should().Be("l12:æøå äö èéê ñe");
        }

        [Fact]
        public void CanEncode_Complex()
        {
            var blist = new BList
            {
                "spam",
                666,
                new BList
                {
                    "foo",
                    "bar",
                    123,
                    new BDictionary
                    {
                        {"more spam", "more eggs"}
                    }
                },
                "foobar",
                new BDictionary
                {
                    {"numbers", new BList {1, 2, 3}}
                }

            };

            var bencode = blist.EncodeAsString();

            bencode.Should().Be("l4:spami666el3:foo3:bari123ed9:more spam9:more eggsee6:foobard7:numbersli1ei2ei3eeee");
        }

        #endregion

        #region AsType/AsStrings/AsNumbers

        [Fact]
        public void AsType_ConvertsToListOfType()
        {
            var blist = new BList {1, 2, 3};
            var bnumbers = blist.AsType<BNumber>();

            bnumbers.Should<object>()
                .HaveCount(3)
                .And.ContainItemsAssignableTo<BNumber>()
                .And.ContainInOrder(blist);
        }

        [Fact]
        public void AsType_ContainingWrongType_ThrowsInvalidCastException()
        {
            var blist = new BList {1, "2", 3};
            Action action = () => blist.AsType<BNumber>();
            action.Should().Throw<InvalidCastException>();
        }

        [Fact]
        public void AsStrings_ConvertsToListOfStrings()
        {
            var blist = new BList {"a", "b", "c"};
            var strings = blist.AsStrings();

            strings.Should().HaveCount(3);
            strings.Should().ContainInOrder("a", "b", "c");
        }

        [Fact]
        public void AsStrings_ContainingNonBStringType_ThrowsInvalidCastException()
        {
            var blist = new BList {"a", "b", 3};
            Action action = () => blist.AsStrings();
            action.Should().Throw<InvalidCastException>();
        }

        [Fact]
        public void AsNumbers_ConvertsToListOfLongs()
        {
            var blist = new BList {1, 2, 3};
            var numbers = blist.AsNumbers();

            numbers.Should().HaveCount(3);
            numbers.Should().ContainInOrder(1L, 2L, 3L);
        }

        [Fact]
        public void AsNumbers_ContainingNonBNumberType_ThrowsInvalidCastException()
        {
            var blist = new BList {1, 2, "3"};
            Action action = () => blist.AsNumbers();
            action.Should().Throw<InvalidCastException>();
        }

        #endregion

        [Fact]
        public void GetSizeInBytes()
        {
            var blist = new BList{1, 2, "abc"};
            blist.GetSizeInBytes().Should().Be(13);
        }

        [Fact]
        public async Task WriteToPipeWriter()
        {
            var blist = new BList { 1, 2, "abc" };
            var (reader, writer) = new Pipe();

            blist.EncodeTo(writer);
            await writer.FlushAsync();
            reader.TryRead(out var readResult);

            var result = Encoding.UTF8.GetString(readResult.Buffer.First.Span.ToArray());
            result.Should().Be("li1ei2e3:abce");
        }

        [Fact]
        public async Task WriteToPipeWriterAsync()
        {
            var blist = new BList { 1, 2, "abc" };
            var (reader, writer) = new Pipe();

            await blist.EncodeToAsync(writer);
            reader.TryRead(out var readResult);

            var result = Encoding.UTF8.GetString(readResult.Buffer.First.Span.ToArray());
            result.Should().Be("li1ei2e3:abce");
        }

        [Fact]
        public async Task WriteToStreamAsync()
        {
            var blist = new BList { 1, 2, "abc" };

            var stream = new MemoryStream();
            await blist.EncodeToAsync(stream);

            var result = Encoding.UTF8.GetString(stream.ToArray());
            result.Should().Be("li1ei2e3:abce");
        }
    }
}
