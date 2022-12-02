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
            var result = ParseUtil.TryParseLongFast((string)null, out _);
            result.Should().BeFalse();
        }

        [Theory]
        [AutoMockedData("")]
        [AutoMockedData("-")]
        public void TryParseLongFast_ZeroLengthInputReturnsFalse(string input)
        {
            var result = ParseUtil.TryParseLongFast(input, out _);
            result.Should().BeFalse();
        }

        [Theory]
        [AutoMockedData("12345678901234567890")]
        [AutoMockedData("-12345678901234567890")]
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
        [AutoMockedData("1.23")]
        [AutoMockedData("-1.23")]
        [AutoMockedData("1,23")]
        [AutoMockedData("-1,23")]
        [AutoMockedData("-1-23")]
        [AutoMockedData("-1-")]
        [AutoMockedData("-1.")]
        [AutoMockedData("-1a23")]
        [AutoMockedData("-1+23")]
        [AutoMockedData("+123")]
        [AutoMockedData("123a")]
        [AutoMockedData("a")]
        public void TryParseLongFast_InputContainingNonDigitReturnsFalse(string input)
        {
            var result = ParseUtil.TryParseLongFast(input, out _);
            result.Should().BeFalse();
        }

        [Theory]
        [AutoMockedData("0", 0)]
        [AutoMockedData("1", 1)]
        [AutoMockedData("123", 123)]
        [AutoMockedData("-1", -1)]
        [AutoMockedData("9223372036854775807", 9223372036854775807)]
        [AutoMockedData("-9223372036854775808", -9223372036854775808)]
        public void TryParseLongFast_ValidInputReturnsTrueAndCorrectValue(string input, long expected)
        {
            var result = ParseUtil.TryParseLongFast(input, out var value);

            result.Should().BeTrue();
            value.Should().Be(expected);
        }
    }
}
