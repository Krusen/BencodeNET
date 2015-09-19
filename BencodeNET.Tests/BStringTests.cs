using System;
using System.IO;
using System.Text;
using BencodeNET.Objects;
using Xunit;

namespace BencodeNET.Tests
{
    public class BStringTests
    {
        [Fact]
        public void ConstructorWithNullValue()
        {
            Assert.Throws<ArgumentNullException>(() => new BString((string) null));
        }

        [Fact]
        public void EqualsBString()
        {
            var bstring1 = new BString("Test String");
            var bstring2 = new BString("Test String");
            var bstring3 = new BString("Another string");

            Assert.Equal(bstring1, bstring2);
            Assert.NotEqual(bstring1, bstring3);
            Assert.NotEqual(bstring2, bstring3);
        }

        [Fact]
        public void EqualsBStringWithEqualsOperator()
        {
            var bstring1 = new BString("Test String");
            var bstring2 = new BString("Test String");
            var bstring3 = new BString("Another string");

            Assert.True(bstring1 == bstring2);
            Assert.True(bstring1 != bstring3);
            Assert.True(bstring2 != bstring3);
        }

        [Fact]
        public void EqualsStringWithEqualsOperator()
        {
            var bstring = new BString("Test String");
            Assert.True("Test String" == bstring);
        }

        [Fact]
        public void HashCodesEqual()
        {
            var bstring1 = new BString("Test String");
            var bstring2 = new BString("Test String");

            var expected = bstring1.GetHashCode();
            var actual = bstring2.GetHashCode();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void HashCodesNotEqual()
        {
            var bstring = new BString("Test String");

            Assert.NotEqual(bstring.GetHashCode(), new BString("Test Strin").GetHashCode());
            Assert.NotEqual(bstring.GetHashCode(), new BString("Test Strin ").GetHashCode());
            Assert.NotEqual(bstring.GetHashCode(), new BString("Test String ").GetHashCode());
            Assert.NotEqual(bstring.GetHashCode(), new BString("Test String2").GetHashCode());
            Assert.NotEqual(bstring.GetHashCode(), new BString("Test StrinG").GetHashCode());
            Assert.NotEqual(bstring.GetHashCode(), new BString("test string").GetHashCode());
            Assert.NotEqual(bstring.GetHashCode(), new BString("TestString").GetHashCode());
            Assert.NotEqual(bstring.GetHashCode(), new BString("teststring").GetHashCode());
            Assert.NotEqual(bstring.GetHashCode(), new BString("TøstStrøng").GetHashCode());
        }

        [Fact]
        public void Encode_Simple()
        {
            var bstring = new BString("Test String");
            Assert.Equal("11:Test String", bstring.Encode());
        }

        [Fact]
        public void Encode_EmptyString()
        {
            var bstring = new BString("");
            Assert.Equal("0:", bstring.Encode());
        }

        [Fact]
        public void Encode_UTF8()
        {
            var bstring = new BString("æøå äö èéê ñ");
            Assert.Equal("21:æøå äö èéê ñ", bstring.Encode());
            Assert.Equal("21:æøå äö èéê ñ", bstring.Encode(Encoding.UTF8));
        }

        [Fact]
        public void Encode_ISO88591()
        {
            var encoding = Encoding.GetEncoding("ISO-8859-1");
            var bstring = new BString("æøå äö èéê ñ", encoding);
            Assert.Equal("12:æøå äö èéê ñ", bstring.Encode());
            Assert.Equal("12:æøå äö èéê ñ", bstring.Encode(encoding));
            Assert.Equal(bstring.Encode(), bstring.Encode(encoding));
        }

        [Fact]
        public void Encode_ISO88591_WithoutSpecifyingEncoding()
        {
            var encoding = Encoding.GetEncoding("ISO-8859-1");
            var bytes = encoding.GetBytes("æøå äö èéê ñ");
            var bstring = new BString(bytes);
            Assert.NotEqual("12:æøå äö èéê ñ", bstring.Encode());
            Assert.Equal("12:æøå äö èéê ñ", bstring.Encode(encoding));
            Assert.NotEqual(bstring.Encode(), bstring.Encode(encoding));
        }

        [Fact]
        public void Encode_NumbersAndSpecialCharacters()
        {
            var bstring = new BString("123:?!#{}'|<>");
            Assert.Equal("13:123:?!#{}'|<>", bstring.Encode());
        }

        [Fact]
        public void ToString_UTF8()
        {
            var bstring = Bencode.DecodeString("21:æøå äö èéê ñ", Encoding.UTF8);
            Assert.Equal("æøå äö èéê ñ", bstring.ToString());
            Assert.Equal("æøå äö èéê ñ", bstring.ToString(Encoding.UTF8));
            Assert.Equal(bstring.ToString(), bstring.ToString(Encoding.UTF8));
        }

        [Fact]
        public void ToString_ISO88591()
        {
            var bstring = Bencode.DecodeString("12:æøå äö èéê ñ", Encoding.GetEncoding("ISO-8859-1"));
            Assert.Equal("æøå äö èéê ñ", bstring.ToString());
            Assert.Equal("æøå äö èéê ñ", bstring.ToString(Encoding.GetEncoding("ISO-8859-1")));
            Assert.Equal(bstring.ToString(), bstring.ToString(Encoding.GetEncoding("ISO-8859-1")));
        }

        [Fact]
        public void ToString_FromNonUTF8StreamWithoutEncoding()
        {
            var encoding = Encoding.GetEncoding("ISO-8859-1");
            var bytes = encoding.GetBytes("12:æøå äö èéê ñ");
            using (var ms = new MemoryStream(bytes))
            {
                var bstring = Bencode.DecodeString(ms);
                Assert.NotEqual("æøå äö èéê ñ", bstring.ToString());
                Assert.Equal("æøå äö èéê ñ", bstring.ToString(encoding));
                Assert.NotEqual(bstring.ToString(), bstring.ToString(encoding));
            }
        }

        [Fact]
        public void ToString_FromNonUTF8StreamWithEncoding()
        {
            var encoding = Encoding.GetEncoding("ISO-8859-1");
            var bytes = encoding.GetBytes("12:æøå äö èéê ñ");
            using (var ms = new MemoryStream(bytes))
            {
                var bstring = Bencode.DecodeString(ms, encoding);
                Assert.Equal("æøå äö èéê ñ", bstring.ToString());
                Assert.Equal("æøå äö èéê ñ", bstring.ToString(encoding));
                Assert.Equal(bstring.ToString(), bstring.ToString(encoding));
            }
        }
    }
}
