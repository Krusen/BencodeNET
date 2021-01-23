using System;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
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
        [AutoMockedData(0, 0)]
        [AutoMockedData(42, 42)]
        [AutoMockedData(-1, -1)]
        public void Equals_SameNumbersShouldBeEqual(long num1, long num2)
        {
            var bnumber1 = new BNumber(num1);
            var bnumber2 = new BNumber(num2);

            bnumber1.Equals(bnumber2).Should().BeTrue();
        }

        [Theory]
        [AutoMockedData(1, 2)]
        [AutoMockedData(10, 20)]
        [AutoMockedData(-1, 1)]
        public void Equals_DifferentNumbersShouldNotBeEqual(long num1, long num2)
        {
            var bnumber1 = new BNumber(num1);
            var bnumber2 = new BNumber(num2);

            bnumber1.Equals(bnumber2).Should().BeFalse();
        }

        [Theory]
        [AutoMockedData(0, 0)]
        [AutoMockedData(42, 42)]
        [AutoMockedData(-1, -1)]
        public void EqualsOperator_Integer_SameNumberShouldBeEqual(long num1, long num2)
        {
            var bnumber = new BNumber(num1);
            (bnumber == num2).Should().BeTrue();
        }

        [Theory]
        [AutoMockedData(1)]
        [AutoMockedData(10)]
        [AutoMockedData(42)]
        [AutoMockedData(long.MaxValue)]
        public void GetHashCode_SameAsInt64HashCode(long number)
        {
            var bnumber = new BNumber(number);

            var hash1 = bnumber.GetHashCode();
            var hash2 = number.GetHashCode();

            hash1.Should().Be(hash2);
        }

        #region Encode

        [Theory]
        [AutoMockedData(1)]
        [AutoMockedData(10)]
        [AutoMockedData(42)]
        [AutoMockedData(123)]
        [AutoMockedData(123456789)]
        public void CanEncode(long value)
        {
            var bnumber = new BNumber(value);
            var bencode = bnumber.EncodeAsString();
            bencode.Should().Be($"i{value}e");
        }

        [Theory]
        [AutoMockedData(-1)]
        [AutoMockedData(-10)]
        [AutoMockedData(-42)]
        [AutoMockedData(-123)]
        [AutoMockedData(-123456789)]
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
        public void CanEncodeToStream()
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
        public void CanEncodeAsBytes()
        {
            var bnumber = new BNumber(42);
            var expected = Encoding.ASCII.GetBytes("i42e");

            var bytes = bnumber.EncodeAsBytes();

            bytes.Should().BeEquivalentTo(expected);
        }

        #endregion

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

        #region Casts

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
        public void CanCastFromNullableInt_WhenNull()
        {
            int? number = null;
            var bnumber = (BNumber)number;
            bnumber.Should().BeNull();
        }

        [Fact]
        public void CanCastFromNullableLong_WhenNull()
        {
            long? number = null;
            var bnumber = (BNumber)number;
            bnumber.Should().BeNull();
        }

        [Fact]
        public void CanCastFromNullableInt_WhenNotNull()
        {
            int? number = 12345;
            var bnumber = (BNumber)number;
            bnumber.Should().Be(12345);
        }

        [Fact]
        public void CanCastFromNullableLong_WhenNotNull()
        {
            long? number = 12345;
            var bnumber = (BNumber)number;
            bnumber.Should().Be(12345);
        }

        [Fact]
        public void CanCastToInt()
        {
            BNumber bnumber = new BNumber(12345);
            int number = (int)bnumber;
            number.Should().Be(12345);
        }

        [Fact]
        public void CanCastToLong()
        {
            BNumber bnumber = new BNumber(12345);
            long number = (long)bnumber;
            number.Should().Be(12345);
        }

        [Fact]
        public void CanCastToNullableInt_WhenNull()
        {
            BNumber bnumber = null;
            var number = (int?)bnumber;
            number.Should().BeNull();
        }

        [Fact]
        public void CanCastToNullableLong_WhenNull()
        {
            BNumber bnumber = null;
            var number = (long?)bnumber;
            number.Should().BeNull();
        }

        [Fact]
        public void CanCastToNullableInt_WhenNotNull()
        {
            BNumber bnumber = 12345;
            var number = (int?)bnumber;
            number.Should().Be(12345);
        }

        [Fact]
        public void CanCastToNullableLong_WhenNotNull()
        {
            BNumber bnumber = 12345;
            var number = (long?)bnumber;
            number.Should().Be(12345);
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
        [AutoMockedData(0)]
        [AutoMockedData(-1)]
        [AutoMockedData(-42)]
        [AutoMockedData(-123456)]
        public void CastingToBool_BelowOrEqualToZero_ShouldBeFalse(int number)
        {
            var boolean = (bool) new BNumber(number);
            boolean.Should().BeFalse();
        }

        [Theory]
        [AutoMockedData(1)]
        [AutoMockedData(10)]
        [AutoMockedData(42)]
        [AutoMockedData(123456)]
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
            action.Should().Throw<InvalidCastException>();
        }

        #endregion

        [Fact]
        public void GetSizeInBytes()
        {
            var bnumber = new BNumber(42);
            bnumber.GetSizeInBytes().Should().Be(4);
        }

        [Fact]
        public async Task WriteToPipeWriter()
        {
            var bnumber = new BNumber(1234);
            var (reader, writer) = new Pipe();

            bnumber.EncodeTo(writer);
            await writer.FlushAsync();
            reader.TryRead(out var readResult);

            var result = Encoding.UTF8.GetString(readResult.Buffer.First.Span.ToArray());
            result.Should().Be("i1234e");
        }

        [Fact]
        public async Task WriteToPipeWriterAsync()
        {
            var bnumber = new BNumber(1234);
            var (reader, writer) = new Pipe();

            await bnumber.EncodeToAsync(writer);
            reader.TryRead(out var readResult);

            var result = Encoding.UTF8.GetString(readResult.Buffer.First.Span.ToArray());
            result.Should().Be("i1234e");
        }

        [Fact]
        public async Task WriteToStreamAsync()
        {
            var bnumber = new BNumber(1234);

            var ms = new MemoryStream();
            await bnumber.EncodeToAsync(ms);

            var result = Encoding.UTF8.GetString(ms.ToArray());
            result.Should().Be("i1234e");
        }
    }
}
