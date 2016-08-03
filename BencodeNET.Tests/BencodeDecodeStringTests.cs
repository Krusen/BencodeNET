using System.Text;
using BencodeNET.Exceptions;
using BencodeNET.Objects;
using Xunit;

namespace BencodeNET.Tests
{
    public class BencodeDecodeStringTests
    {
        [Fact]
        public void DecodeString_Simple()
        {
            var bstring = Bencode.DecodeString("4:spam");
            Assert.Equal("spam", bstring.ToString());
        }

        [Fact]
        public void DecodeString_EmptyString()
        {
            var bstring = Bencode.DecodeString("0:");
            Assert.Equal("", bstring.ToString());
        }

        [Fact]
        public void DecodeString_Invalid_LessCharsThanSpecified()
        {
            Assert.Throws<BencodeParsingException<BString>>(() => Bencode.DecodeString("5:spam"));
        }

        [Fact]
        public void DecodeString_Invalid_NoDelimiter()
        {
            Assert.Throws<BencodeParsingException<BString>>(() => Bencode.DecodeString("4spam"));
        }

        [Fact]
        public void DecodeString_Invalid_NonDigitFirstChar()
        {
            Assert.Throws<BencodeParsingException<BString>>(() => Bencode.DecodeString("spam"));
        }

        [Fact]
        public void DecodeString_Invalid_InputMinimumLength2()
        {
            Assert.Throws<BencodeParsingException<BString>>(() => Bencode.DecodeString("4"));
        }

        [Fact]
        public void DecodeString_Invalid_MissingStringLength()
        {
            Assert.Throws<BencodeParsingException<BString>>(() => Bencode.DecodeString(":spam"));
        }

        [Fact]
        public void DecodeString_Unsupported_StringLengthAboveMaxDigits10()
        {
            Assert.Throws<UnsupportedBencodeException>(() => Bencode.DecodeString("12345678901:spam"));
        }

        [Fact]
        public void DecodeString_Unsupported_StringLengthAboveInt32MaxValue()
        {
            Assert.Throws<UnsupportedBencodeException>(() => Bencode.DecodeString("2147483648:spam"));
        }
    }
}
