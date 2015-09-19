using System;
using System.Collections.Generic;
using BencodeNET.Objects;
using Xunit;
using Assert = Xunit.Assert;

namespace BencodeNET.Tests
{
    public class BDictionaryTests
    {
        [Fact]
        public void AddNullValue()
        {
            var dict = new BDictionary();
            Assert.Throws<ArgumentNullException>(() => dict.Add("key", null));
        }

        [Fact]
        public void AddNullObjectValue()
        {
            var dict = new BDictionary();
            Assert.Throws<ArgumentNullException>(() => dict.Add("key", (IBObject)null));
        }

        [Fact]
        public void AddKeyValuePairWithNullValue()
        {
            var dict = new BDictionary();
            Assert.Throws<ArgumentException>(() => dict.Add(new KeyValuePair<BString, IBObject>("key", null)));
        }

        [Fact]
        public void SetNullValue()
        {
            var dict = new BDictionary();
            dict.Add("key", "value");
            Assert.Throws<ArgumentNullException>(() => dict["key"] = null);
        }

        [Fact]
        public void EqualsBDictionary()
        {
            var bdict1 = new BDictionary
            {
                {"foobar", "Hello World!"},
                {"number", 747},
                {"key", "value"}
            };

            var bdict2 = new BDictionary
            {
                {"foobar", "Hello World!"},
                {"number", 747},
                {"key", "value"}
            };

            // Different order, but keys are sorted alphabetically so it is still equal
            var bdict3 = new BDictionary
            {
                {"number", 747},
                {"foobar", "Hello World!"},
                {"key", "value"}
            };

            var bdict4 = new BDictionary
            {
                {"foobar", "Hello World!"},
                {"number", 666},
                {"key", "value"}
            };

            Assert.Equal(bdict1, bdict2);
            Assert.Equal(bdict1, bdict3);
            Assert.Equal(bdict2, bdict3);
            Assert.NotEqual(bdict1, bdict4);
            Assert.NotEqual(bdict2, bdict4);
            Assert.NotEqual(bdict3, bdict4);
        }

        [Fact]
        public void EqualsBDictionaryWithEqualsOperator()
        {
            var bdict1 = new BDictionary
            {
                {"foobar", "Hello World!"},
                {"number", 747},
                {"key", "value"}
            };

            var bdict2 = new BDictionary
            {
                {"foobar", "Hello World!"},
                {"number", 747},
                {"key", "value"}
            };

            // Different order, but keys are sorted alphabetically so it is still equal
            var bdict3 = new BDictionary
            {
                {"number", 747},
                {"foobar", "Hello World!"},
                {"key", "value"}
            };

            var bdict4 = new BDictionary
            {
                {"foobar", "Hello World!"},
                {"number", 666},
                {"key", "value"}
            };

            Assert.True(bdict1 == bdict2);
            Assert.True(bdict1 == bdict3);
            Assert.True(bdict2 == bdict3);
            Assert.True(bdict1 != bdict4);
            Assert.True(bdict2 != bdict4);
            Assert.True(bdict3 != bdict4);
        }

        [Fact]
        public void Encode_Simple()
        {
            var bdict = new BDictionary
            {
                {"foobar", "Hello World!"},
                {"number", 747},
                {"key", "value"}
            };

            // Keys should be sorted in lexical order
            var expected = "d6:foobar12:Hello World!3:key5:value6:numberi747ee";
            var actual = bdict.Encode();
            
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Encode_EmptyDictionary()
        {
            var bdict = new BDictionary();
            Assert.Equal("de", bdict.Encode());
        }

        [Fact]
        public void Encode_Complex()
        {
            var bdict = new BDictionary
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

            // Keys should be sorted in lexical order
            var expected = "d6:A Listl3:foo3:bari123ed9:more spam9:more eggsee6:foobard7:numbersli1ei2ei3eee4:spam3:egge";
            var actual = bdict.Encode();
            
            Assert.Equal(expected, actual);
        }
    }
}
