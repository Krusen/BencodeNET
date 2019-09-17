using BencodeNET.Parsing;
using FluentAssertions;
using Xunit;

namespace BencodeNET.Tests.Parsing
{
    public class ParseUtilTests
    {
        [Fact]
        public void TryParseLongFast_CanParseSimple()
        {
            ParseUtil.TryParseLongFast("123", out var value);
            value.Should().Be(123);
        }

        [Fact]
        public void TryParseLongFast_NullReturnsFalse()
        {
            var result = ParseUtil.TryParseLongFast((string) null, out _);
            result.Should().BeFalse();
        }

        [Theory]
        [InlineAutoMockedData("")]
        [InlineAutoMockedData("-")]
        public void TryParseLongFast_ZeroLengthInputReturnsFalse(string input)
        {
            var result = ParseUtil.TryParseLongFast(input, out _);
            result.Should().BeFalse();
        }

        [Theory]
        [InlineAutoMockedData("12345678901234567890")]
        [InlineAutoMockedData("-12345678901234567890")]
        public void TryParseLongFast_InputLongerThanInt64MaxValueReturnsFalse(string input)
        {
            var result = ParseUtil.TryParseLongFast(input, out _);
            result.Should().BeFalse();
        }

        [Fact]
        public void TryParseLongFast_InputBiggerThanInt64MaxValueReturnsFalse()
        {
            var result = ParseUtil.TryParseLongFast("9223372036854775808", out _);
            result.Should().BeFalse();
        }

        [Fact]
        public void TryParseLongFast_InputSmallerThanInt64MinValueReturnsFalse()
        {
            var result = ParseUtil.TryParseLongFast("-9223372036854775809", out _);
            result.Should().BeFalse();
        }

        [Theory]
        [InlineAutoMockedData("1.23")]
        [InlineAutoMockedData("-1.23")]
        [InlineAutoMockedData("1,23")]
        [InlineAutoMockedData("-1,23")]
        [InlineAutoMockedData("-1-23")]
        [InlineAutoMockedData("-1-")]
        [InlineAutoMockedData("-1.")]
        [InlineAutoMockedData("-1a23")]
        [InlineAutoMockedData("-1+23")]
        [InlineAutoMockedData("+123")]
        [InlineAutoMockedData("123a")]
        [InlineAutoMockedData("a")]
        public void TryParseLongFast_InputContainingNonDigitReturnsFalse(string input)
        {
            var result = ParseUtil.TryParseLongFast(input, out _);
            result.Should().BeFalse();
        }

        [Theory]
        [InlineAutoMockedData("0", 0)]
        [InlineAutoMockedData("1", 1)]
        [InlineAutoMockedData("123", 123)]
        [InlineAutoMockedData("-1", -1)]
        [InlineAutoMockedData("9223372036854775807", 9223372036854775807)]
        [InlineAutoMockedData("-9223372036854775808", -9223372036854775808)]
        public void TryParseLongFast_ValidInputReturnsTrueAndCorrectValue(string input, long expected)
        {
            var result = ParseUtil.TryParseLongFast(input, out var value);

            result.Should().BeTrue();
            value.Should().Be(expected);
        }
    }
}
