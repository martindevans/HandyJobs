using NUnit.Framework;
using me.martindevans.handyjobs.Extensions;

namespace Tests.Extensions
{
    public class IComparableExtensionsTests
    {
        #region IsLessThan

        [Test]
        public void IsLessThan_SmallerValue_ReturnsTrue()
        {
            Assert.IsTrue(1.IsLessThan(2));
        }

        [Test]
        public void IsLessThan_EqualValues_ReturnsFalse()
        {
            Assert.IsFalse(2.IsLessThan(2));
        }

        [Test]
        public void IsLessThan_LargerValue_ReturnsFalse()
        {
            Assert.IsFalse(3.IsLessThan(2));
        }

        [Test]
        public void IsLessThan_NegativeFloatValues()
        {
            Assert.IsTrue((-5f).IsLessThan(-2f));
            Assert.IsFalse((-2f).IsLessThan(-5f));
        }

        [Test]
        public void IsLessThan_StringValues()
        {
            Assert.IsTrue("apple".IsLessThan("banana"));
            Assert.IsFalse("banana".IsLessThan("apple"));
        }

        #endregion

        #region IsLessThanOrEqualTo

        [Test]
        public void IsLessThanOrEqualTo_SmallerValue_ReturnsTrue()
        {
            Assert.IsTrue(1.IsLessThanOrEqualTo(2));
        }

        [Test]
        public void IsLessThanOrEqualTo_EqualValues_ReturnsTrue()
        {
            Assert.IsTrue(2.IsLessThanOrEqualTo(2));
        }

        [Test]
        public void IsLessThanOrEqualTo_LargerValue_ReturnsFalse()
        {
            Assert.IsFalse(3.IsLessThanOrEqualTo(2));
        }

        [Test]
        public void IsLessThanOrEqualTo_StringValues()
        {
            Assert.IsTrue("apple".IsLessThanOrEqualTo("banana"));
            Assert.IsTrue("apple".IsLessThanOrEqualTo("apple"));
            Assert.IsFalse("banana".IsLessThanOrEqualTo("apple"));
        }

        #endregion

        #region IsGreaterThan

        [Test]
        public void IsGreaterThan_SmallerValue_ReturnsFalse()
        {
            Assert.IsFalse(1.IsGreaterThan(2));
        }

        [Test]
        public void IsGreaterThan_EqualValues_ReturnsFalse()
        {
            Assert.IsFalse(2.IsGreaterThan(2));
        }

        [Test]
        public void IsGreaterThan_LargerValue_ReturnsTrue()
        {
            Assert.IsTrue(3.IsGreaterThan(2));
        }

        [Test]
        public void IsGreaterThan_NegativeFloatValues()
        {
            Assert.IsTrue((-2f).IsGreaterThan(-5f));
            Assert.IsFalse((-5f).IsGreaterThan(-2f));
        }

        [Test]
        public void IsGreaterThan_StringValues()
        {
            Assert.IsTrue("banana".IsGreaterThan("apple"));
            Assert.IsFalse("apple".IsGreaterThan("banana"));
        }

        #endregion

        #region IsGreaterThanOrEqualTo

        [Test]
        public void IsGreaterThanOrEqualTo_SmallerValue_ReturnsFalse()
        {
            Assert.IsFalse(1.IsGreaterThanOrEqualTo(2));
        }

        [Test]
        public void IsGreaterThanOrEqualTo_EqualValues_ReturnsTrue()
        {
            Assert.IsTrue(2.IsGreaterThanOrEqualTo(2));
        }

        [Test]
        public void IsGreaterThanOrEqualTo_LargerValue_ReturnsTrue()
        {
            Assert.IsTrue(3.IsGreaterThanOrEqualTo(2));
        }

        [Test]
        public void IsGreaterThanOrEqualTo_StringValues()
        {
            Assert.IsTrue("banana".IsGreaterThanOrEqualTo("apple"));
            Assert.IsTrue("apple".IsGreaterThanOrEqualTo("apple"));
            Assert.IsFalse("apple".IsGreaterThanOrEqualTo("banana"));
        }

        #endregion
    }
}
