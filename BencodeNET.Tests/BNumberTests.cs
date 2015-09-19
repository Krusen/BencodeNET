using BencodeNET.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BencodeNET.Tests
{
    [TestClass]
    public class BNumberTests
    {
        [TestMethod]
        public void EqualsBNumber()
        {
            var bnumber1 = new BNumber(42);
            var bnumber2 = new BNumber(42);
            var bnumber3 = new BNumber(666);

            Assert.AreEqual(bnumber1, bnumber2);
            Assert.AreNotEqual(bnumber1, bnumber3);
            Assert.AreNotEqual(bnumber2, bnumber3);
        }

        [TestMethod]
        public void EqualsBNumberWithEqualsOperator()
        {
            var bnumber1 = new BNumber(42);
            var bnumber2 = new BNumber(42);
            var bnumber3 = new BNumber(666);

            Assert.IsTrue(bnumber1 == bnumber2);
            Assert.IsTrue(bnumber1 != bnumber3);
            Assert.IsTrue(bnumber2 != bnumber3);
        }

        [TestMethod]
        public void EqualsIntegerWithEqualsOperator()
        {
            var bnumber = new BNumber(42);
            Assert.IsTrue(42 == bnumber);
        }

        [TestMethod]
        public void HashCodeEqualsLongHashCode()
        {
            var bnumber = new BNumber(42);

            var expected = 42L.GetHashCode();
            var actual = bnumber.GetHashCode();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Encode_Simple()
        {
            var bnumber = new BNumber(42);
            Assert.AreEqual("i42e", bnumber.Encode());
        }

        [TestMethod]
        public void Encode_SimpleNegative()
        {
            var bnumber = new BNumber(-42);
            Assert.AreEqual("i-42e", bnumber.Encode());
        }

        [TestMethod]
        public void Encode_Zero()
        {
            var bnumber = new BNumber(0);
            Assert.AreEqual("i0e", bnumber.Encode());
        }

        [TestMethod]
        public void Encode_Int64MinValue()
        {
            var bnumber = new BNumber(-9223372036854775808);
            Assert.AreEqual("i-9223372036854775808e", bnumber.Encode());
        }

        [TestMethod]
        public void Encode_Int64MaxValue()
        {
            var bnumber = new BNumber(9223372036854775807);
            Assert.AreEqual("i9223372036854775807e", bnumber.Encode());
        }
    }
}
