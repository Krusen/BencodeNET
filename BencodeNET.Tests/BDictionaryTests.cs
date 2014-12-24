using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using BencodeNET.Exceptions;
using BencodeNET.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BencodeNET.Tests
{
    [TestClass]
    public class BDictionaryTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddNullValue()
        {
            var dict = new BDictionary();
            dict.Add("key", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddNullObjectValue()
        {
            var dict = new BDictionary();
            dict.Add("key", (IBObject)null);

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddKeyValuePairWithNullValue()
        {
            var dict = new BDictionary();
            dict.Add(new KeyValuePair<BString, IBObject>("key", null));

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetNullValue()
        {
            var dict = new BDictionary();
            dict.Add("key", "value");
            dict["key"] = null;
        }

        [TestMethod]
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
            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Encode_EmptyDictionary()
        {
            var bdict = new BDictionary();
            Assert.AreEqual("de", bdict.Encode());
        }

        [TestMethod]
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
            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Decode_Simple()
        {
            var bdict = BDictionary.Decode("d4:spam3:egg3:fooi42ee");
            Assert.AreEqual(2, bdict.Count);
            
            Assert.IsInstanceOfType(bdict["spam"], typeof(BString));
            Assert.IsTrue(bdict["spam"] as BString == "egg");

            Assert.IsInstanceOfType(bdict["foo"], typeof(BNumber));
            Assert.IsTrue(bdict["foo"] as BNumber == 42);
        } 
        
        [TestMethod]
        public void Decode_Complex()
        {
            var bdict = BDictionary.Decode("d6:A Listl3:foo3:bari123ed9:more spam9:more eggsee6:foobard7:numbersli1ei2ei3eee4:spam3:egge");

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

            Assert.AreEqual(3, bdict.Count);
            Assert.AreEqual(bdictExpected.Count, bdict.Count);
            Assert.AreEqual(bdictExpected.Encode(), bdict.Encode());
            Assert.IsInstanceOfType(bdict["spam"], typeof(BString));
            Assert.IsInstanceOfType(bdict["A List"], typeof(BList));
            Assert.IsInstanceOfType(bdict["foobar"], typeof(BDictionary));
        }

        [TestMethod]
        public void Decode_EmptyDictionary()
        {
            var bdict = BDictionary.Decode("de");
            Assert.AreEqual(0, bdict.Count);
        }

        #region Decode: Invalid Exceptions

        [TestMethod]
        [ExpectedException(typeof(InvalidBencodeException))]
        public void Decode_Invalid_InputMinimumLength2()
        {
            BDictionary.Decode("d");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidBencodeException))]
        public void Decode_Invalid_NonStringKey()
        {
            BDictionary.Decode("di42e4:spame");
        }
        
        [TestMethod]
        [ExpectedException(typeof(InvalidBencodeException))]
        public void Decode_Invalid_InvalidKeyObject()
        {
            BDictionary.Decode("da:spam3:egge");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidBencodeException))]
        public void Decode_Invalid_WrongBeginChar()
        {
            BDictionary.Decode("l4:spam3:egg3:fooi42ee");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidBencodeException))]
        public void Decode_Invalid_MissingEndChar()
        {
            BDictionary.Decode("d4:spam3:egg3:fooi42e");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidBencodeException))]
        public void Decode_Invalid_MissingKeyValueOrInvalidValueObject()
        {
            BDictionary.Decode("d4:spame");
        }

        #endregion
    }
}
