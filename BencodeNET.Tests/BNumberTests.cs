using BencodeNET.Objects;
using Xunit;

namespace BencodeNET.Tests
{
    public class BNumberTests
    {
        [Fact]
        public void EqualsBNumber()
        {
            var bnumber1 = new BNumber(42);
            var bnumber2 = new BNumber(42);
            var bnumber3 = new BNumber(666);

            Assert.Equal(bnumber1, bnumber2);
            Assert.NotEqual(bnumber1, bnumber3);
            Assert.NotEqual(bnumber2, bnumber3);
        }

        [Fact]
        public void EqualsBNumberWithEqualsOperator()
        {
            var bnumber1 = new BNumber(42);
            var bnumber2 = new BNumber(42);
            var bnumber3 = new BNumber(666);

            Assert.True(bnumber1 == bnumber2);
            Assert.True(bnumber1 != bnumber3);
            Assert.True(bnumber2 != bnumber3);
        }

        [Fact]
        public void EqualsIntegerWithEqualsOperator()
        {
            var bnumber = new BNumber(42);
            Assert.True(42 == bnumber);
        }

        [Fact]
        public void HashCodeEqualsLongHashCode()
        {
            var bnumber = new BNumber(42);

            var expected = 42L.GetHashCode();
            var actual = bnumber.GetHashCode();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Encode_Simple()
        {
            var bnumber = new BNumber(42);
            Assert.Equal("i42e", bnumber.Encode());
        }

        [Fact]
        public void Encode_SimpleNegative()
        {
            var bnumber = new BNumber(-42);
            Assert.Equal("i-42e", bnumber.Encode());
        }

        [Fact]
        public void Encode_Zero()
        {
            var bnumber = new BNumber(0);
            Assert.Equal("i0e", bnumber.Encode());
        }

        [Fact]
        public void Encode_Int64MinValue()
        {
            var bnumber = new BNumber(-9223372036854775808);
            Assert.Equal("i-9223372036854775808e", bnumber.Encode());
        }

        [Fact]
        public void Encode_Int64MaxValue()
        {
            var bnumber = new BNumber(9223372036854775807);
            Assert.Equal("i9223372036854775807e", bnumber.Encode());
        }
    }
}
