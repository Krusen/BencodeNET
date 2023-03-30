using System;
using System.Threading.Tasks;
using BencodeNET.Exceptions;
using BencodeNET.Objects;
using BencodeNET.Parsing;
using FluentAssertions;
using Xunit;

namespace BencodeNET.Tests.Parsing
{
    public partial class BNumberParserTests
    {
        [Theory]
        [InlineData("i1e", 1)]
        [InlineData("i2e", 2)]
        [InlineData("i3e", 3)]
        [InlineData("i42e", 42)]
        [InlineData("i100e", 100)]
        [InlineData("i1234567890e", 1234567890)]
        public async Task CanParsePositiveAsync(string bencode, int value)
        {
            var bnumber = await Parser.ParseStringAsync(bencode);
            bnumber.Should().Be(value);
        }

        [Fact]
        public async Task CanParseZeroAsync()
        {
            var bnumber = await Parser.ParseStringAsync("i0e");
            bnumber.Should().Be(0);
        }

        [Theory]
        [InlineData("i-1e", -1)]
        [InlineData("i-2e", -2)]
        [InlineData("i-3e", -3)]
        [InlineData("i-42e", -42)]
        [InlineData("i-100e", -100)]
        [InlineData("i-1234567890e", -1234567890)]
        public async Task CanParseNegativeAsync(string bencode, int value)
        {
            var bnumber = await Parser.ParseStringAsync(bencode);
            bnumber.Should().Be(value);
        }

        [Theory]
        [InlineData("i9223372036854775807e", 9223372036854775807)]
        [InlineData("i-9223372036854775808e", -9223372036854775808)]
        public async Task CanParseInt64Async(string bencode, long value)
        {
            var bnumber = await Parser.ParseStringAsync(bencode);
            bnumber.Should().Be(value);
        }

        [Theory]
        [InlineData("i01e")]
        [InlineData("i012e")]
        [InlineData("i01234567890e")]
        [InlineData("i00001e")]
        public async Task LeadingZeros_ThrowsInvalidBencodeExceptionAsync(string bencode)
        {
            var action = async () => await Parser.ParseStringAsync(bencode);
            (await action.Should().ThrowAsync<InvalidBencodeException<BNumber>>())
                .WithMessage("*Leading '0's are not valid.*")
                .Which.StreamPosition.Should().Be(0);
        }

        [Fact]
        public async Task MinusZero_ThrowsInvalidBencodeExceptionAsync()
        {
            var action = async () => await Parser.ParseStringAsync("i-0e");
            (await action.Should().ThrowAsync<InvalidBencodeException<BNumber>>())
                .WithMessage("*'-0' is not a valid number.*")
                .Which.StreamPosition.Should().Be(0);
        }

        [Theory]
        [InlineData("i12")]
        [InlineData("i123")]
        public async Task MissingEndChar_ThrowsInvalidBencodeExceptionAsync(string bencode)
        {
            var action = async () => await Parser.ParseStringAsync(bencode);
            (await action.Should().ThrowAsync<InvalidBencodeException<BNumber>>())
                .WithMessage("*Missing end character of object.*")
                .Which.StreamPosition.Should().Be(0);
        }

        [Theory]
        [InlineData("42e")]
        [InlineData("a42e")]
        [InlineData("d42e")]
        [InlineData("l42e")]
        [InlineData("100e")]
        [InlineData("1234567890e")]
        public async Task InvalidFirstChar_ThrowsInvalidBencodeExceptionAsync(string bencode)
        {
            var action = async () => await Parser.ParseStringAsync(bencode);
            (await action.Should().ThrowAsync<InvalidBencodeException<BNumber>>())
                .WithMessage("*Unexpected character. Expected 'i'*")
                .Which.StreamPosition.Should().Be(0);
        }

        [Fact]
        public async Task JustNegativeSign_ThrowsInvalidBencodeExceptionAsync()
        {
            var action = async () => await Parser.ParseStringAsync("i-e");
            (await action.Should().ThrowAsync<InvalidBencodeException<BNumber>>())
                .WithMessage("*It contains no digits.*")
                .Which.StreamPosition.Should().Be(0);
        }

        [Theory]
        [InlineData("i--1e")]
        [InlineData("i--42e")]
        [InlineData("i---100e")]
        [InlineData("i----1234567890e")]
        public async Task MoreThanOneNegativeSign_ThrowsInvalidBencodeExceptionAsync(string bencode)
        {
            var action = async () => await Parser.ParseStringAsync(bencode);
            (await action.Should().ThrowAsync<InvalidBencodeException<BNumber>>())
                .WithMessage("*The value '*' is not a valid number.*")
                .Which.StreamPosition.Should().Be(0);
        }

        [Theory]
        [InlineData("iasdfe")]
        [InlineData("i!#¤%&e")]
        [InlineData("i.e")]
        [InlineData("i42.e")]
        [InlineData("i42ae")]
        public async Task NonDigit_ThrowsInvalidBencodeExceptionAsync(string bencode)
        {
            var action = async () => await Parser.ParseStringAsync(bencode);
            (await action.Should().ThrowAsync<InvalidBencodeException<BNumber>>())
                .WithMessage("*The value '*' is not a valid number.*")
                .Which.StreamPosition.Should().Be(0);
        }


        [Theory]
        [InlineData("", "reached end of stream")]
        [InlineData("i", "contains no digits")]
        [InlineData("ie", "contains no digits")]
        public async Task BelowMinimumLength_ThrowsInvalidBencodeExceptionAsync(string bencode, string exceptionMessage)
        {
            var action = async () => await Parser.ParseStringAsync(bencode);
            (await action.Should().ThrowAsync<InvalidBencodeException<BNumber>>())
                .WithMessage($"*{exceptionMessage}*")
                .Which.StreamPosition.Should().Be(0);
        }

        [Theory]
        [InlineData("i9223372036854775808e")]
        [InlineData("i-9223372036854775809e")]
        public async Task LargerThanInt64_ThrowsUnsupportedExceptionAsync(string bencode)
        {
            var action = async () => await Parser.ParseStringAsync(bencode);
            (await action.Should().ThrowAsync<UnsupportedBencodeException<BNumber>>())
                .WithMessage("*The value '*' is not a valid long (Int64)*")
                .Which.StreamPosition.Should().Be(0);
        }

        [Theory]
        [InlineData("i12345678901234567890e")]
        [InlineData("i123456789012345678901e")]
        [InlineData("i123456789012345678901234567890e")]
        public async Task LongerThanMaxDigits19_ThrowsUnsupportedExceptionAsync(string bencode)
        {
            var action = async () => await Parser.ParseStringAsync(bencode);
            (await action.Should().ThrowAsync<UnsupportedBencodeException<BNumber>>())
                .WithMessage("*The number '*' has more than 19 digits and cannot be stored as a long*")
                .Which.StreamPosition.Should().Be(0);
        }
    }
}
