using System;
using System.Text;
using BencodeNET.Objects;
using Xunit;
using Assert = Xunit.Assert;

namespace BencodeNET.Tests
{
    public class BListTests
    {
        [Fact]
        public void AddNullValue()
        {
            var blist = new BList();
            Assert.Throws<ArgumentNullException>(() => blist.Add((IBObject)null));
        }

        [Fact]
        public void SetNullValue()
        {
            var blist = new BList();
            blist.Add(0);
            Assert.Throws<ArgumentNullException>(() => blist[0] = null);
        }

        [Fact]
        public void EqualsBList()
        {
            var blist1 = new BList
            {
                "asdf",
                "qwer"
            };
            var blist2 = new BList()
            {
                "asdf",
                "qwer"
            };
            var blist3 = new BList()
            {
                "qwer",
                "asdf"
            };

            Assert.Equal(blist1, blist2);
            Assert.NotEqual(blist1, blist3);
            Assert.NotEqual(blist2, blist3);
        }

        [Fact]
        public void EqualsBListWithEqualsOperator()
        {
            var blist1 = new BList
            {
                "asdf",
                "qwer"
            };
            var blist2 = new BList()
            {
                "asdf",
                "qwer"
            };
            var blist3 = new BList()
            {
                "qwer",
                "asdf"
            };

            Assert.True(blist1 == blist2);
            Assert.True(blist1 != blist3);
            Assert.True(blist2 != blist3);
        }

        [Fact]
        public void HashCodesEqual()
        {
            var blist1 = new BList
            {
                "asdf",
                "qwer"
            };
            var blist2 = new BList()
            {
                "asdf",
                "qwer"
            };

            var expected = blist1.GetHashCode();
            var actual = blist2.GetHashCode();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void HashCodesNotEqual()
        {
            var blist1 = new BList
            {
                "asdf",
                "qwer"
            };
            var blist2 = new BList()
            {
                "asdf",
                666
            };

            var expected = blist1.GetHashCode();
            var actual = blist2.GetHashCode();

            Assert.NotEqual(expected, actual);
        }

        [Fact]
        public void Encode_Simple()
        {
            var blist = new BList {"hello world", 987, "foobar"};

            var expected = "l11:hello worldi987e6:foobare";
            var actual = blist.Encode();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Encode_EmptyList()
        {
            var blist = new BList();
            Assert.Equal("le", blist.Encode());
        }

        [Fact]
        public void Encode_UTF8()
        {
            var blist = new BList { "æøå äö èéê ñ" };
            Assert.Equal("l21:æøå äö èéê ñe", blist.Encode());
            Assert.Equal("l21:æøå äö èéê ñe", blist.Encode(Encoding.UTF8));
            Assert.Equal(blist.Encode(), blist.Encode(Encoding.UTF8));
        }

        [Fact]
        public void Encode_ISO88591()
        {
            var encoding = Encoding.GetEncoding("ISO-8859-1");
            var blist = new BList { new BString("æøå äö èéê ñ", encoding) };

            Assert.NotEqual("l12:æøå äö èéê ñe", blist.Encode());
            Assert.Equal("l12:æøå äö èéê ñe", blist.Encode(encoding));
            Assert.NotEqual(blist.Encode(), blist.Encode(encoding));
        }

        [Fact]
        public void Encode_Complex()
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

            var expected = "l4:spami666el3:foo3:bari123ed9:more spam9:more eggsee6:foobard7:numbersli1ei2ei3eeee";
            var actual = blist.Encode();

            Assert.Equal(expected, actual);
        }
    }
}
