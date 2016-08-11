using System.IO;
using System.Threading.Tasks;
using BencodeNET.Objects;
using FluentAssertions;
using Xunit;

namespace BencodeNET.Tests.Objects
{
    public class BNumberTests
    {
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
            var bencode = bnumber.Encode();
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
            var bencode = bnumber.Encode();
            bencode.Should().Be($"i{value}e");
        }

        [Fact]
        public void CanEncode_Zero()
        {
            var bnumber = new BNumber(0);
            var bencode = bnumber.Encode();
            bencode.Should().Be("i0e");
        }

        [Fact]
        public void CanEncode_Int64MinValue()
        {
            var bnumber = new BNumber(-9223372036854775808);
            var bencode = bnumber.Encode();
            bencode.Should().Be("i-9223372036854775808e");
        }

        [Fact]
        public void CanEncode_Int64MaxValue()
        {
            var bnumber = new BNumber(9223372036854775807);
            var bencode = bnumber.Encode();
            bencode.Should().Be("i9223372036854775807e");
        }

        [Fact]
        public void CanEnodeToStream()
        {
            var bnumber = new BNumber(42);

            using (var stream = new MemoryStream())
            {
                bnumber.EncodeToStream(stream);

                stream.Length.Should().Be(4);
                stream.AsString().Should().Be("i42e");
            }
        }

        [Fact]
        public async Task CanEnodeToStreamAsync()
        {
            var bnumber = new BNumber(42);

            using (var stream = new MemoryStream())
            {
                await bnumber.EncodeToStreamAsync(stream);

                stream.Length.Should().Be(4);
                stream.AsString().Should().Be("i42e");
            }
        }

        // TODO: ToString methods
    }
}
