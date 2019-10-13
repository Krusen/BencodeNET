using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using BencodeNET.Objects;
using FluentAssertions;
using Xunit;

namespace BencodeNET.Tests.Objects
{
    public class BStringTests
    {
        [Fact]
        public void ConstructorEmpty_ResultsInEmptyValue()
        {
            var bstring = new BString();
            bstring.Value.ToArray().Should().BeEmpty();
        }

        [Fact]
        public void ConstructorWithNullValue_ResultsInEmptyValue()
        {
            var bstring = new BString((string)null);
            bstring.Value.ToArray().Should().BeEmpty();
        }

        #region Equals

        [Theory]
        [InlineAutoMockedData("hello world", "hello world")]
        [InlineAutoMockedData("a", "a")]
        [InlineAutoMockedData(" ", " ")]
        [InlineAutoMockedData("", "")]
        public void Equals_SameContentShouldBeEqual(string str1, string str2)
        {
            var bstring1 = new BString(str1);
            var bstring2 = new BString(str2);

            bstring1.Equals(bstring2).Should().BeTrue();
            bstring2.Equals(bstring1).Should().BeTrue();
        }

        [Theory]
        [InlineAutoMockedData("hello", "world")]
        [InlineAutoMockedData(" ", "")]
        [InlineAutoMockedData("1", "2")]
        public void Equals_DifferentContentShouldNotBeEqual(string str1, string str2)
        {
            var bstring1 = new BString(str1);
            var bstring2 = new BString(str2);

            bstring1.Equals(bstring2).Should().BeFalse();
            bstring2.Equals(bstring1).Should().BeFalse();
        }

        [Fact]
        public void Equals_DifferentEncodingAreNotEqual()
        {
            var bstring1 = new BString("ø");
            var bstring2 = new BString("ø", Encoding.ASCII);

            bstring1.Equals(bstring2).Should().BeFalse();
            bstring2.Equals(bstring1).Should().BeFalse();
        }

        [Theory]
        [InlineAutoMockedData("hello world", "hello world")]
        [InlineAutoMockedData("a", "a")]
        [InlineAutoMockedData(" ", " ")]
        [InlineAutoMockedData("", "")]
        public void EqualsOperator_BString_WithSameContentShouldBeEqual(string str1, string str2)
        {
            var bstring1 = new BString(str1);
            var bstring2 = new BString(str2);

            (bstring1 == bstring2).Should().BeTrue();
        }

        [Theory]
        [InlineAutoMockedData("hello", "world")]
        [InlineAutoMockedData(" ", "")]
        [InlineAutoMockedData("1", "2")]
        public void EqualsOperator_BString_WithDifferentContentShouldNotBeEqual(string str1, string str2)
        {
            var bstring1 = new BString(str1);
            var bstring2 = new BString(str2);

            (bstring1 == bstring2).Should().BeFalse();
        }

        [Fact]
        public void EqualsOperator_BString_WithDifferentEncodingAreNotEqual()
        {
            var bstring1 = new BString("ø");
            var bstring2 = new BString("ø", Encoding.ASCII);

            (bstring1 == bstring2).Should().BeFalse();
        }

        [Theory]
        [InlineAutoMockedData("hello world", "hello world")]
        [InlineAutoMockedData("a", "a")]
        [InlineAutoMockedData(" ", " ")]
        [InlineAutoMockedData("", "")]
        public void EqualsOperator_String_WithSameContentAreEqual(string str1, string str2)
        {
            var bstring = new BString(str1);
            (bstring == str2).Should().BeTrue();
        }

        [Theory]
        [InlineAutoMockedData("hello", "world")]
        [InlineAutoMockedData(" ", "")]
        [InlineAutoMockedData("1", "2")]
        public void EqualsOperator_String_WithDifferentContentAreNotEqual(string str1, string str2)
        {
            var bstring = new BString(str1);
            (bstring == str2).Should().BeFalse();
        }

        [Theory]
        [InlineAutoMockedData("test", "test")]
        [InlineAutoMockedData("TEST", "TEST")]
        public void GetHashCode_AreEqualWithSameContent(string str1, string str2)
        {
            var bstring1 = new BString(str1);
            var bstring2 = new BString(str2);

            var expected = bstring1.GetHashCode();
            var actual = bstring2.GetHashCode();

            actual.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMockedData("Test Strin")]
        [InlineAutoMockedData("Test Strin ")]
        [InlineAutoMockedData("Test String ")]
        [InlineAutoMockedData("Test String2")]
        [InlineAutoMockedData("Test StrinG")]
        [InlineAutoMockedData("test string")]
        [InlineAutoMockedData("TestString")]
        [InlineAutoMockedData("teststring")]
        public void GetHashCode_AreNotEqualWithDifferentValues(string other)
        {
            var bstring = new BString("Test String");
            var otherBString = new BString(other);
            bstring.GetHashCode().Should().NotBe(otherBString.GetHashCode());
        }

        #endregion

        [Fact]
        public void Encoding_DefaultIsUTF8()
        {
            var bstring = new BString("foo");
            bstring.Encoding.Should().Be(Encoding.UTF8);
        }

        #region Encode

        [Theory]
        [InlineAutoMockedData("some string", 11)]
        [InlineAutoMockedData("spam", 4)]
        [InlineAutoMockedData("1234567890", 10)]
        public void CanEncode(string str, int length)
        {
            var bstring = new BString(str);
            var bencode = bstring.EncodeAsString();
            bencode.Should().Be($"{length}:{str}");
        }

        [Fact]
        public void CanEncode_NullString()
        {
            var bstring = new BString();
            var bencode = bstring.EncodeAsString();
            bencode.Should().Be("0:");
        }

        [Fact]
        public void CanEncode_EmptyString()
        {
            var bstring = new BString("");
            var bencode = bstring.EncodeAsString();
            bencode.Should().Be("0:");
        }

        [Fact]
        public void CanEncode_UTF8()
        {
            var bstring = new BString("æøå äö èéê ñ", Encoding.UTF8);
            var bencode = bstring.EncodeAsString();
            bencode.Should().Be("21:æøå äö èéê ñ");
        }

        [Fact]
        public void CanEncode_ISO88591()
        {
            var encoding = Encoding.GetEncoding("ISO-8859-1");
            var bstring = new BString("æøå äö èéê ñ", encoding);

            var bencode = bstring.EncodeAsString(encoding);

            bencode.Should().Be("12:æøå äö èéê ñ");
        }

        [Fact]
        public void CanEncode_ISO88591_WithoutSpecifyingEncoding()
        {
            var encoding = Encoding.GetEncoding("ISO-8859-1");
            var bytes = encoding.GetBytes("æøå äö èéê ñ");
            var bstring = new BString(bytes);

            var bencode = bstring.EncodeAsString(encoding);

            bencode.Should().Be("12:æøå äö èéê ñ");
        }

        [Fact]
        public void CanEncode_NumbersAndSpecialCharacters()
        {
            var bstring = new BString("123:?!#{}'|<>");
            var bencode = bstring.EncodeAsString();
            bencode.Should().Be("13:123:?!#{}'|<>");
        }

        [Fact]
        public void CanEncodeToStream()
        {
            var bstring = new BString("hello world");

            using (var stream = new MemoryStream())
            {
                bstring.EncodeTo(stream);

                stream.Length.Should().Be(14);
                stream.AsString().Should().Be("11:hello world");
            }
        }

        [Fact]
        public void CanEncodeAsBytes()
        {
            var bstring = new BString("hello world");
            var expected = Encoding.ASCII.GetBytes("11:hello world");

            var bytes = bstring.EncodeAsBytes();

            bytes.Should().BeEquivalentTo(expected);
        }

        #endregion

        [Fact]
        public void ToString_WithoutEncoding_EncodesUsingUTF8()
        {
            var bstring = new BString("æøå äö èéê ñ", Encoding.UTF8);
            var value = bstring.ToString();
            value.Should().Be("æøå äö èéê ñ");
        }

        [Fact]
        public void ToString_ISO88591()
        {
            var encoding = Encoding.GetEncoding("ISO-8859-1");
            var bstring = new BString("æøå äö èéê ñ", encoding);
            var value = bstring.ToString(encoding);
            value.Should().Be("æøå äö èéê ñ");
        }

        [Fact]
        public void GetSizeInBytes()
        {
            var bstring = new BString("abc", Encoding.UTF8);
            bstring.GetSizeInBytes().Should().Be(5);
        }

        [Fact]
        public void GetSizeInBytes_UTF8()
        {
            var bstring = new BString("æøå äö èéê ñ", Encoding.UTF8);
            bstring.GetSizeInBytes().Should().Be(24);
        }

        [Fact]
        public async Task WriteToPipeWriter()
        {
            var bstring = new BString("æøå äö èéê ñ");
            var (reader, writer) = new Pipe();

            bstring.EncodeTo(writer);
            await writer.FlushAsync();
            reader.TryRead(out var readResult);

            var result = Encoding.UTF8.GetString(readResult.Buffer.First.Span.ToArray());
            result.Should().Be("21:æøå äö èéê ñ");
        }

        [Fact]
        public async Task WriteToPipeWriterAsync()
        {
            var bstring = new BString("æøå äö èéê ñ");
            var (reader, writer) = new Pipe();

            await bstring.EncodeToAsync(writer);
            reader.TryRead(out var readResult);

            var result = Encoding.UTF8.GetString(readResult.Buffer.First.Span.ToArray());
            result.Should().Be("21:æøå äö èéê ñ");
        }

        [Fact]
        public async Task WriteToStreamAsync()
        {
            var bstring = new BString("æøå äö èéê ñ");

            var stream = new MemoryStream();
            await bstring.EncodeToAsync(stream);

            var result = Encoding.UTF8.GetString(stream.ToArray());
            result.Should().Be("21:æøå äö èéê ñ");
        }
    }
}
