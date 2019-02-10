using System;
using System.IO;
using BencodeNET.Objects;
using FluentAssertions;
using Xunit;

namespace BencodeNET.Tests.Objects
{
    public class BNumberTests
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        [Fact]
        public void Constructor_DateTime_ValueIsUnixFormat()
        {
            var bnumber = new BNumber(new DateTime(2016, 1, 1));
            bnumber.Value.Should().Be(1451606400);
        }

        [Fact]
        public void Constructor_DateTime_NullParameter_ValueIsZero()
        {
            var bnumber = new BNumber((DateTime?)null);
            bnumber.Value.Should().Be(0);
        }

        [Theory]
        [InlineAutoMockedData(0, 0)]
        [InlineAutoMockedData(42, 42)]
        [InlineAutoMockedData(-1, -1)]
        public void Equals_SameNumbersShouldBeEqual(long num1, long num2)
        {
            var bnumber1 = new BNumber(num1);
            var bnumber2 = new BNumber(num2);

            bnumber1.Equals(bnumber2).Should().BeTrue();
        }

        [Theory]
        [InlineAutoMockedData(1, 2)]
        [InlineAutoMockedData(10, 20)]
        [InlineAutoMockedData(-1, 1)]
        public void Equals_DifferentNumbersShouldNotBeEqual(long num1, long num2)
        {
            var bnumber1 = new BNumber(num1);
            var bnumber2 = new BNumber(num2);

            bnumber1.Equals(bnumber2).Should().BeFalse();
        }

        [Theory]
        [InlineAutoMockedData(0, 0)]
        [InlineAutoMockedData(42, 42)]
        [InlineAutoMockedData(-1, -1)]
        public void EqualsOperator_Integer_SameNumberShouldBeEqual(long num1, long num2)
        {
            var bnumber = new BNumber(num1);
            (bnumber == num2).Should().BeTrue();
        }

        [Theory]
        [InlineAutoMockedData(1)]
        [InlineAutoMockedData(10)]
        [InlineAutoMockedData(42)]
        [InlineAutoMockedData(long.MaxValue)]
        public void GetHashCode_SameAsInt64HashCode(long number)
        {
            var bnumber = new BNumber(number);

            var hash1 = bnumber.GetHashCode();
            var hash2 = number.GetHashCode();

            hash1.Should().Be(hash2);
        }

        [Theory]
        [InlineAutoMockedData(1)]
        [InlineAutoMockedData(10)]
        [InlineAutoMockedData(42)]
        [InlineAutoMockedData(123)]
        [InlineAutoMockedData(123456789)]
        public void CanEncode(long value)
        {
            var bnumber = new BNumber(value);
            var bencode = bnumber.EncodeAsString();
            bencode.Should().Be($"i{value}e");
        }

        [Theory]
        [InlineAutoMockedData(-1)]
        [InlineAutoMockedData(-10)]
        [InlineAutoMockedData(-42)]
        [InlineAutoMockedData(-123)]
        [InlineAutoMockedData(-123456789)]
        public void CanEncode_Negative(long value)
        {
            var bnumber = new BNumber(value);
            var bencode = bnumber.EncodeAsString();
            bencode.Should().Be($"i{value}e");
        }

        [Fact]
        public void CanEncode_Zero()
        {
            var bnumber = new BNumber(0);
            var bencode = bnumber.EncodeAsString();
            bencode.Should().Be("i0e");
        }

        [Fact]
        public void CanEncode_Int64MinValue()
        {
            var bnumber = new BNumber(-9223372036854775808);
            var bencode = bnumber.EncodeAsString();
            bencode.Should().Be("i-9223372036854775808e");
        }

        [Fact]
        public void CanEncode_Int64MaxValue()
        {
            var bnumber = new BNumber(9223372036854775807);
            var bencode = bnumber.EncodeAsString();
            bencode.Should().Be("i9223372036854775807e");
        }

        [Fact]
        public void CanEnodeToStream()
        {
            var bnumber = new BNumber(42);

            using (var stream = new MemoryStream())
            {
                bnumber.EncodeTo(stream);

                stream.Length.Should().Be(4);
                stream.AsString().Should().Be("i42e");
            }
        }

        [Fact]
        public void ToString_SameAsLong()
        {
            var bnumber = new BNumber(42);

            var str1 = bnumber.ToString();
            var str2 = 42L.ToString();

            str1.Should().Be(str2);
        }

        [Fact]
        public void ToString_WithFormat_SameAsLong()
        {
            var bnumber = new BNumber(42);

            var str1 = bnumber.ToString("N2");
            var str2 = 42L.ToString("N2");

            str1.Should().Be(str2);
        }

        [Fact]
        public void CanCastFromInt()
        {
            int number = 12345;
            var bnumber = (BNumber)number;
            bnumber.Should().Be(12345);
        }

        [Fact]
        public void CanCastFromLong()
        {
            long number = 12345;
            var bnumber = (BNumber)number;
            bnumber.Should().Be(12345);
        }

        [Fact]
        public void CanCastFromNullableInt()
        {
            int? number = null;
            var bnumber = (BNumber)number;
            bnumber.Should().BeNull();
        }

        [Fact]
        public void CanCastFromNullableLong()
        {
            long? number = null;
            var bnumber = (BNumber)number;
            bnumber.Should().BeNull();
        }


        [Fact]
        public void CanCastFromDateTime()
        {
            var bnumber = (BNumber) new DateTime(2016, 1, 1);
            bnumber.Should().Be(1451606400);
        }

        [Fact]
        public void CanCastToDateTime()
        {
            var bnumber = new BNumber(1451606400);
            var datetime = (DateTime) bnumber;
            datetime.Should().Be(new DateTime(2016, 1, 1));
        }

        [Fact]
        public void CastingFromBool_False_IsZero()
        {
            var bnumber = (BNumber) false;
            bnumber.Should().Be(0);
        }

        [Fact]
        public void CastingFromBool_True_IsOne()
        {
            var bnumber = (BNumber)true;
            bnumber.Should().Be(1);
        }

        [Theory]
        [InlineAutoMockedData(0)]
        [InlineAutoMockedData(-1)]
        [InlineAutoMockedData(-42)]
        [InlineAutoMockedData(-123456)]
        public void CastingToBool_BelowOrEqualToZero_ShouldBeFalse(int number)
        {
            var boolean = (bool) new BNumber(number);
            boolean.Should().BeFalse();
        }

        [Theory]
        [InlineAutoMockedData(1)]
        [InlineAutoMockedData(10)]
        [InlineAutoMockedData(42)]
        [InlineAutoMockedData(123456)]
        public void CastingToBool_AboveZero_ShouldBeTrue(int number)
        {
            var boolean = (bool) new BNumber(number);
            boolean.Should().BeTrue();
        }

        [Fact]
        public void CastingToBool_Null_ThrowsInvalidCastException()
        {
            BNumber bnumber = null;
            Action action = () => { var b = (bool) bnumber; };
            action.ShouldThrow<InvalidCastException>();
        }
    }
}
