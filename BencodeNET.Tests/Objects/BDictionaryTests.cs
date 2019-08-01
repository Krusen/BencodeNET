using System;
using System.Collections.Generic;
using System.Linq;
using BencodeNET.Objects;
using FluentAssertions;
using Xunit;

namespace BencodeNET.Tests.Objects
{
    public class BDictionaryTests
    {
        [Fact]
        public void Add_NullStringValue_ResultsInEmptyBString()
        {
            var dict = new BDictionary {{"key", (string) null}};
            dict.Get<BString>("key").Value.Should().BeEmpty();
        }

        [Fact]
        public void Add_NullIBObjectValue_ThrowsArgumentNullException()
        {
            var dict = new BDictionary();
            Action action = () => dict.Add("key", (IBObject)null);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Add_KeyValuePairWithNullValue_ThrowsArgumentException()
        {
            var dict = new BDictionary();
            Action action = () => dict.Add(new KeyValuePair<BString, IBObject>("key", null));
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Indexer_Set_Null_ThrowsArgumentNullException()
        {
            var dict = new BDictionary {{"key", "value"}};
            Action action = () => dict["key"] = null;
            action.Should().Throw<ArgumentNullException>();
        }

        #region MergeWith

        [Fact]
        public void MergeWith_StringReplacesExistingKey()
        {
            var dict1 = new BDictionary {{"key", "value"}};
            var dict2 = new BDictionary {{"key", "replaced value"}};

            dict1.MergeWith(dict2);

            dict1.Should().HaveCount(1);
            dict1["key"].Should().Be((BString)"replaced value");
        }

        [Fact]
        public void MergeWith_StringWithNewKeyIsAdded()
        {
            var dict1 = new BDictionary {{"key", "value"}};
            var dict2 = new BDictionary {{"another key", "value"}};

            dict1.MergeWith(dict2);

            dict1.Should().HaveCount(2);
            dict1["key"].Should().Be((BString)"value");
            dict1["another key"].Should().Be((BString)"value");
        }

        [Fact]
        public void MergeWith_NumberReplacesExistingKey()
        {
            var dict1 = new BDictionary {{"key", 1}};
            var dict2 = new BDictionary {{"key", 42}};

            dict1.MergeWith(dict2);

            dict1.Should().HaveCount(1);
            dict1["key"].Should().Be((BNumber) 42);
        }

        [Fact]
        public void MergeWith_NumberWithNewKeyIsAdded()
        {
            var dict1 = new BDictionary {{"key", 1}};
            var dict2 = new BDictionary {{"another key", 42}};

            dict1.MergeWith(dict2);

            dict1.Should().HaveCount(2);
            dict1["key"].Should().Be((BNumber) 1);
            dict1["another key"].Should().Be((BNumber) 42);
        }

        [Fact]
        public void MergeWith_ExistingKeyOptionMerge_ListAppendedToExistingKeyOfSameType()
        {
            var dict1 = new BDictionary {{"key", new BList {"item1"}}};
            var dict2 = new BDictionary {{"key", new BList {"item2", "item3"}}};

            dict1.MergeWith(dict2, ExistingKeyAction.Merge);

            dict1.Should().HaveCount(1);
            dict1["key"].Should().BeOfType<BList>();
            dict1.Get<BList>("key").Should()
                .HaveCount(3)
                .And.ContainInOrder((BString) "item1", (BString) "item2", (BString) "item3");
        }

        [Fact]
        public void MergeWith_ExistingKeyOption_Replace_ListReplacesExistingKeyOfSameType()
        {
            var dict1 = new BDictionary {{"key", new BList {"item1"}}};
            var dict2 = new BDictionary {{"key", new BList {"item2", "item3"}}};

            dict1.MergeWith(dict2, ExistingKeyAction.Replace);

            dict1.Should().HaveCount(1);
            dict1["key"].Should().BeOfType<BList>();
            dict1.Get<BList>("key").Should()
                .HaveCount(2)
                .And.ContainInOrder((BString) "item2", (BString) "item3");
        }

        [Fact]
        public void MergeWith_ExistingKeyOption_Skip_ListIsSkippedForExistingKeyOfSameType()
        {
            var dict1 = new BDictionary {{"key", new BList {"item1"}}};
            var dict2 = new BDictionary {{"key", new BList {"item2", "item3"}}};

            dict1.MergeWith(dict2, ExistingKeyAction.Skip);

            dict1.Should().HaveCount(1);
            dict1["key"].Should().BeOfType<BList>();
            dict1.Get<BList>("key").Should()
                .HaveCount(1)
                .And.ContainInOrder((BString) "item1");
        }

        [Fact]
        public void MergeWith_ListReplacesExistingKeyOfDifferentType()
        {
            var dict1 = new BDictionary {{"key", "value"}};
            var dict2 = new BDictionary {{"key", new BList {"item1", "item2"}}};

            dict1.MergeWith(dict2);

            dict1.Should().HaveCount(1);
            dict1["key"].Should().BeOfType<BList>();
            dict1.Get<BList>("key").Should()
                .HaveCount(2)
                .And.ContainInOrder((BString) "item1", (BString) "item2");
        }

        [Fact]
        public void MergeWith_ListWithNewKeyIsAdded()
        {
            var list = new BList {1, 2, 3};
            var dict1 = new BDictionary {{"key", 1}};
            var dict2 = new BDictionary {{"another key", list}};

            dict1.MergeWith(dict2);

            dict1.Should().HaveCount(2);
            dict1["key"].Should().Be((BNumber) 1);
            dict1["another key"].Should().Be(list);
        }

        [Fact]
        public void MergeWith_ExistingKeyOption_Merge_DictionaryMergedWithExistingKeyOfSameType()
        {
            var dict1 = new BDictionary {{"main", new BDictionary {{"key", "value"}}}};
            var dict2 = new BDictionary {{"main", new BDictionary {{"key2", "value2"}}}};

            dict1.MergeWith(dict2, ExistingKeyAction.Merge);

            dict1.Should().HaveCount(1);
            dict1.Get<BDictionary>("main").Should().HaveCount(2).And.ContainKeys("key", "key2");
        }

        [Fact]
        public void MergeWith_ExistingKeyOption_Replace_DictionaryReplacesExistingKeyOfSameType()
        {
            var dict1 = new BDictionary {{"main", new BDictionary {{"key", "value"}}}};
            var dict2 = new BDictionary {{"main", new BDictionary {{"key2", "value2"}}}};

            dict1.MergeWith(dict2, ExistingKeyAction.Replace);

            dict1.Should().HaveCount(1);
            dict1.Get<BDictionary>("main").Should().HaveCount(1).And.ContainKey("key2");
        }

        [Fact]
        public void MergeWith_ExistingKeyOption_Skip_DictionarySkippedForExistingKeyOfSameType()
        {
            var dict1 = new BDictionary {{"main", new BDictionary {{"key", "value"}}}};
            var dict2 = new BDictionary {{"main", new BDictionary {{"key2", "value2"}}}};

            dict1.MergeWith(dict2, ExistingKeyAction.Skip);

            dict1.Should().HaveCount(1);
            dict1.Get<BDictionary>("main").Should().HaveCount(1).And.ContainKey("key");
        }

        [Fact]
        public void MergeWith_DictionaryReplacesExistingKeyOfDifferentType()
        {
            var dict1 = new BDictionary {{"main", new BList {"item1"}}};
            var dict2 = new BDictionary {{"main", new BDictionary {{"key", "value"}}}};

            dict1.MergeWith(dict2);

            dict1.Should().HaveCount(1);
            dict1.Get<BDictionary>("main").Should().HaveCount(1).And.ContainKey("key");
        }

        [Fact]
        public void MergeWith_DictionaryWithNewKeyIsAdded()
        {
            var dict1 = new BDictionary { { "main", new BDictionary { { "key", "value" } } } };
            var dict2 = new BDictionary { { "main2", new BDictionary { { "key2", "value2" } } } };

            dict1.MergeWith(dict2);

            dict1.Should().HaveCount(2);
            dict1.Get<BDictionary>("main").Should().HaveCount(1).And.ContainKeys("key");
            dict1.Get<BDictionary>("main2").Should().HaveCount(1).And.ContainKeys("key2");
        }

        #endregion

        #region SequenceEqual

        [Fact]
        public void SequenceEqual_WithKeysAddedInSameOrder_AreEqual()
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

            bdict1.SequenceEqual(bdict2).Should().BeTrue();
        }

        [Fact]
        public void SequenceEqual_WithKeysAddedInDifferentOrder_AreEqual()
        {
            var bdict1 = new BDictionary
            {
                {"foobar", "Hello World!"},
                {"number", 747},
                {"key", "value"}
            };

            var bdict2 = new BDictionary
            {
                {"key", "value"},
                {"foobar", "Hello World!"},
                {"number", 747}
            };

            bdict1.SequenceEqual(bdict2).Should().BeTrue();
        }

        [Fact]
        public void SequenceEqual_WithDifferentKeys_AreNotEqual()
        {
            var bdict1 = new BDictionary
            {
                {"foobar", "Hello World!"},
            };

            var bdict2 = new BDictionary
            {
                {"key", "Hello World!"},
            };

            bdict1.SequenceEqual(bdict2).Should().BeFalse();
        }

        [Fact]
        public void SequenceEqual_WithDifferentValues_AreNotEqual()
        {
            var bdict1 = new BDictionary
            {
                {"foobar", "Hello World!"},
            };

            var bdict2 = new BDictionary
            {
                {"foobar", "Another world..."},
            };

            bdict1.SequenceEqual(bdict2).Should().BeFalse();
        }

        #endregion

        #region Encode

        [Fact]
        public void CanEncode_Simple()
        {
            var bdict = new BDictionary
            {
                {"foobar", "Hello World!"},
                {"number", 747}
            };

            var bencode = bdict.EncodeAsString();

            bencode.Should().Be("d6:foobar12:Hello World!6:numberi747ee");
        }

        [Fact]
        public void Encode_KeyAreSortedInLexicalOrder()
        {
            var bdict = new BDictionary
            {
                {"number", 747},
                {"foobar", "Hello World!"},
                {"key", "value"}
            };

            var bencode = bdict.EncodeAsString();

            bencode.Should().Be("d6:foobar12:Hello World!3:key5:value6:numberi747ee");
        }

        [Fact]
        public void CanEncode_EmptyDictionary()
        {
            var bdict = new BDictionary();
            var bencode = bdict.EncodeAsString();
            bencode.Should().Be("de");
        }

        [Fact]
        public void CanEncode_Complex()
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

            var bencode = bdict.EncodeAsString();

            bencode.Should()
                .Be("d6:A Listl3:foo3:bari123ed9:more spam9:more eggsee6:foobard7:numbersli1ei2ei3eee4:spam3:egge");
        }

        #endregion

        [Fact]
        public void GetSizeInBytes()
        {
            var bdict = new BDictionary
            {
                // 6 + 5
                {"spam", "egg"},
                // 6 + 3 + 3 + 3 + 3 (+ 2)
                { "list", new BList { 1, 2, 3} },
                // 5 + 5
                { "str", "abc" },
                // 5 + 4
                { "num", 42 }
            }; // 2
            bdict.GetSizeInBytes().Should().Be(49);
        }
    }
}
