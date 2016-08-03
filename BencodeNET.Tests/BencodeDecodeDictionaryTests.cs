using BencodeNET.Exceptions;
using BencodeNET.Objects;
using Xunit;
using Assert = Xunit.Assert;

namespace BencodeNET.Tests
{
    public class BencodeDecodeDictionaryTests
    {
        [Fact]
        public void DecodeDictionary_Simple()
        {
            var bdict = Bencode.DecodeDictionary("d4:spam3:egg3:fooi42ee");
            Assert.Equal(2, bdict.Count);

            Assert.IsType<BString>(bdict["spam"]);
            Assert.True(bdict["spam"] as BString == "egg");

            Assert.IsType<BNumber>(bdict["foo"]);
            Assert.True(bdict["foo"] as BNumber == 42);
        }

        [Fact]
        public void DecodeDictionary_Complex()
        {
            var bdict = Bencode.DecodeDictionary("d6:A Listl3:foo3:bari123ed9:more spam9:more eggsee6:foobard7:numbersli1ei2ei3eee4:spam3:egge");

            var bdictExpected = new BDictionary
            {
                {"spam", "egg"},
                {
                    "A List", new BList
                    {
                        "foo",
                        "bar",
                        123,
                        new BDictionary
                        {
                            {"more spam", "more eggs"}
                        }
                    }
                },
                {
                    "foobar", new BDictionary
                    {
                        {"numbers", new BList {1, 2, 3}}
                    }
                }
            };

            Assert.Equal(3, bdict.Count);
            Assert.Equal(bdictExpected.Count, bdict.Count);
            Assert.Equal(bdictExpected.Encode(), bdict.Encode());
            Assert.IsType<BString>(bdict["spam"]);
            Assert.IsType<BList>(bdict["A List"]);
            Assert.IsType<BDictionary>(bdict["foobar"]);
        }

        [Fact]
        public void DecodeDictionary_EmptyDictionary()
        {
            var bdict = Bencode.DecodeDictionary("de");
            Assert.Equal(0, bdict.Count);
        }

        [Fact]
        public void DecodeDictionary_Invalid_InputMinimumLength2()
        {
            Assert.Throws<BencodeParsingException<BDictionary>>(() => Bencode.DecodeDictionary("d"));
        }

        [Fact]
        public void DecodeDictionary_Invalid_NonStringKey()
        {
            Assert.Throws<BencodeParsingException<BDictionary>>(() => Bencode.DecodeDictionary("di42e4:spame"));
        }

        [Fact]
        public void DecodeDictionary_Invalid_InvalidKeyObject()
        {
            Assert.Throws<BencodeParsingException<BDictionary>>(() => Bencode.DecodeDictionary("da:spam3:egge"));
        }

        [Fact]
        public void DecodeDictionary_Invalid_WrongBeginChar()
        {
            Assert.Throws<BencodeParsingException<BDictionary>>(() => Bencode.DecodeDictionary("l4:spam3:egg3:fooi42ee"));
        }

        [Fact]
        public void DecodeDictionary_Invalid_MissingEndChar()
        {
            Assert.Throws<BencodeParsingException<BDictionary>>(() => Bencode.DecodeDictionary("d4:spam3:egg3:fooi42e"));
        }

        [Fact]
        public void DecodeDictionary_Invalid_MissingKeyValueOrInvalidValueObject()
        {
            Assert.Throws<BencodeParsingException<BDictionary>>(() => Bencode.DecodeDictionary("d4:spame"));
        }
    }
}
