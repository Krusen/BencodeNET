using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using BencodeNET.Exceptions;
using BencodeNET.Objects;
using BencodeNET.Parsing;
using FluentAssertions;
using Xunit;

namespace BencodeNET.Tests.Parsing
{
    public partial class BStringParserTests
    {
        [Theory]
        [InlineData("4:spam")]
        [InlineData("8:spameggs")]
        [InlineData("9:spam eggs")]
        [InlineData("9:spam:eggs")]
        [InlineData("14:!@#¤%&/()=?$|")]
        public async Task CanParseSimpleAsync(string bencode)
        {
            var parts = bencode.Split(new[] {':'}, 2);
            var length = int.Parse(parts[0]);
            var value = parts[1];

            var bstring = await Parser.ParseStringAsync(bencode);

            bstring.Length.Should().Be(length);
            bstring.Should().Be(value);
        }

        [Fact]
        public async Task CanParse_EmptyStringAsync()
        {
            var bstring = await Parser.ParseStringAsync("0:");

            bstring.Length.Should().Be(0);
            bstring.Should().Be("");
        }

        [Theory]
        [InlineData("5:spam")]
        [InlineData("6:spam")]
        [InlineData("100:spam")]
        public async Task LessCharsThanSpecified_ThrowsInvalidBencodeExceptionAsync(string bencode)
        {
            var action = async () => await Parser.ParseStringAsync(bencode);
            (await action.Should().ThrowAsync<InvalidBencodeException<BString>>())
                .WithMessage("*but could only read * bytes*")
                .Which.StreamPosition.Should().Be(0);
        }

        [Theory]
        [InlineData("4spam", 1)]
        [InlineData("10spam", 2)]
        [InlineData("4-spam", 1)]
        [InlineData("4.spam", 1)]
        [InlineData("4;spam", 1)]
        [InlineData("4,spam", 1)]
        [InlineData("4|spam", 1)]
        public async Task MissingDelimiter_ThrowsInvalidBencodeExceptionAsync(string bencode, int errorIndex)
        {
            var action = async () => await Parser.ParseStringAsync(bencode);
            (await action.Should().ThrowAsync<InvalidBencodeException<BString>>())
                .WithMessage("*Unexpected character. Expected ':'*")
                .Which.StreamPosition.Should().Be(errorIndex);
        }

        [Theory]
        [InlineData("spam")]
        [InlineData("-spam")]
        [InlineData(".spam")]
        [InlineData(",spam")]
        [InlineData(";spam")]
        [InlineData("?spam")]
        [InlineData("!spam")]
        [InlineData("#spam")]
        public async Task NonDigitFirstChar_ThrowsInvalidBencodeExceptionAsync(string bencode)
        {
            var action = async () => await Parser.ParseStringAsync(bencode);
            (await action.Should().ThrowAsync<InvalidBencodeException<BString>>())
                .WithMessage($"*Unexpected character. Expected ':' but found '{bencode[0]}'*")
                .Which.StreamPosition.Should().Be(0);
        }

        [Theory]
        [InlineData("12345678901:spam")]
        [InlineData("123456789012:spam")]
        [InlineData("1234567890123:spam")]
        [InlineData("12345678901234:spam")]
        public async Task LengthAboveMaxDigits10_ThrowsUnsupportedExceptionAsync(string bencode)
        {
            var action = async () => await Parser.ParseStringAsync(bencode);
            (await action.Should().ThrowAsync<UnsupportedBencodeException<BString>>())
                .WithMessage("*Length of string is more than * digits*")
                .Which.StreamPosition.Should().Be(0);
        }

        [Theory]
        [InlineData("1:spam")]
        [InlineData("12:spam")]
        [InlineData("123:spam")]
        [InlineData("1234:spam")]
        [InlineData("12345:spam")]
        [InlineData("123456:spam")]
        [InlineData("1234567:spam")]
        [InlineData("12345678:spam")]
        [InlineData("123456789:spam")]
        [InlineData("1234567890:spam")]
        public async Task LengthAtOrBelowMaxDigits10_DoesNotThrowUnsupportedExceptionAsync(string bencode)
        {
            var action = async () => await Parser.ParseStringAsync(bencode);
            await action.Should().NotThrowAsync<UnsupportedBencodeException<BString>>();
        }

        [Fact]
        public async Task LengthAboveInt32MaxValue_ThrowsUnsupportedExceptionAsync()
        {
            var bencode = "2147483648:spam";
            var action = async () => await Parser.ParseStringAsync(bencode);
            (await action.Should().ThrowAsync<UnsupportedBencodeException<BString>>())
                .WithMessage("*Length of string is * but maximum supported length is *")
                .Which.StreamPosition.Should().Be(0);
        }

        [Fact]
        public async Task LengthBelowInt32MaxValue_DoesNotThrowUnsupportedExceptionAsync()
        {
            var bencode = "2147483647:spam";
            var action = async () => await Parser.ParseStringAsync(bencode);
            await action.Should().NotThrowAsync<UnsupportedBencodeException<BString>>();
        }

        [Fact]
        public async Task CanParseEncodedAsLatin1Async()
        {
            var encoding = Encoding.GetEncoding("LATIN1");
            var expected = new BString("æøå", encoding);
            var parser = new BStringParser(encoding);

            // "3:æøå"
            var bytes = new byte[] {51, 58, 230, 248, 229};
            var (reader, writer) = new Pipe();
            await writer.WriteAsync(bytes);

            var bstring = await parser.ParseAsync(reader);

            bstring.Should().Be(expected);
            bstring.GetSizeInBytes().Should().Be(5);
        }

        [Theory]
        [InlineData("1-:a", 1)]
        [InlineData("1abc:a", 1)]
        [InlineData("123?:asdf", 3)]
        [InlineData("3abc:abc", 1)]
        public async Task InvalidLengthString_ThrowsInvalidExceptionAsync(string bencode, int errorIndex)
        {
            var action = async () => await Parser.ParseStringAsync(bencode);
            (await action.Should().ThrowAsync<InvalidBencodeException<BString>>())
                .WithMessage("*Unexpected character. Expected ':'*")
                .Which.StreamPosition.Should().Be(errorIndex);
        }
    }
}
