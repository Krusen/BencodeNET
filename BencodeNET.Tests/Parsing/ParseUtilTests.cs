using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BencodeNET.Parsing;
using FluentAssertions;
using Xunit;

namespace BencodeNET.Tests.Parsing
{
    public class ParseUtilTests
    {
        public void asd()
        {
            long value;
            ParseUtil.TryParseLongFast("123", out value);

            value.Should().Be(123);
        }

        [Fact]
        public void TryParseLongFast_NullReturnsFalse()
        {
            long value;
            var result = ParseUtil.TryParseLongFast(null, out value);
            result.Should().BeFalse();
        }

        [Theory]
        [InlineAutoMockedData("")]
        [InlineAutoMockedData("-")]
        public void TryParseLongFast_ZeroLengthInputReturnsFalse(string input)
        {
            long value;
            var result = ParseUtil.TryParseLongFast(input, out value);
            result.Should().BeFalse();
        }

        [Theory]
        [InlineAutoMockedData("12345678901234567890")]
        [InlineAutoMockedData("-12345678901234567890")]
        public void TryParseLongFast_InputLongerThanInt64MaxValueReturnsFalse(string input)
        {
            long value;
            var result = ParseUtil.TryParseLongFast(input, out value);
            result.Should().BeFalse();
        }

        [Fact]
        public void TryParseLongFast_InputBiggerThanInt64MaxValueReturnsFalse()
        {
            long value;
            var result = ParseUtil.TryParseLongFast("9223372036854775808", out value);
            result.Should().BeFalse();
        }

        [Fact]
        public void TryParseLongFast_InputSmallerThanInt64MinValueReturnsFalse()
        {
            long value;
            var result = ParseUtil.TryParseLongFast("-9223372036854775809", out value);
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
            long value;
            var result = ParseUtil.TryParseLongFast(input, out value);
            result.Should().BeFalse();
        }

        [Theory]
        [InlineAutoMockedData("0", 0)]
        [InlineAutoMockedData("1", 1)]
        [InlineAutoMockedData("123", 123)]
        [InlineAutoMockedData("-1", -1)]
        [InlineAutoMockedData("9223372036854775807", 9223372036854775807)]
        [InlineAutoMockedData("-9223372036854775808", -9223372036854775808)]
        public void TryParseLongFast_ValidInputRetursTrueAndCorrectValue(string input, long expected)
        {
            long value;
            var result = ParseUtil.TryParseLongFast(input, out value);

            result.Should().BeTrue();
            value.Should().Be(expected);
        }
    }
}
