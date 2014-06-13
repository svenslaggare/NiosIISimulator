using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiosII_Simulator.Core;

namespace NiosII_Simulator.Test
{
    /// <summary>
    /// Tests the BitHelpers class
    /// </summary>
    [TestClass]
    public class TestBitHelpers
    {
        /// <summary>
        /// Tests the IsAlinged method
        /// </summary>
        [TestMethod]
        public void TestIsAlinged()
        {
            int x = 4;
            Assert.AreEqual(true, BitHelpers.IsAligned(x, 2));

            x = 8;
            Assert.AreEqual(true, BitHelpers.IsAligned(x, 2));

            x = 9;
            Assert.AreEqual(false, BitHelpers.IsAligned(x, 2));

            x = 1024;
            Assert.AreEqual(true, BitHelpers.IsAligned(x, 2));
        }

        /// <summary>
        /// Tests the ToBinary method
        /// </summary>
        [TestMethod]
        public void TestToBinary()
        {
            Assert.AreEqual("0", BitHelpers.ToBinary(0));
            Assert.AreEqual("1", BitHelpers.ToBinary(1));
            Assert.AreEqual("1000", BitHelpers.ToBinary(8));
            Assert.AreEqual("1001", BitHelpers.ToBinary(9));
            Assert.AreEqual("1101", BitHelpers.ToBinary(13));
            Assert.AreEqual("101000100101", BitHelpers.ToBinary(2597));
        }

        /// <summary>
        /// Test the FromBitPattern method
        /// </summary>
        [TestMethod]
        public void TestFromBitPattern()
        {
            Assert.AreEqual(0, BitHelpers.FromBitPattern("0"));
            Assert.AreEqual(2, BitHelpers.FromBitPattern("10"));
            Assert.AreEqual(5, BitHelpers.FromBitPattern("101"));
            Assert.AreEqual(15, BitHelpers.FromBitPattern("1111"));
            Assert.AreEqual(18, BitHelpers.FromBitPattern("10010"));
            Assert.AreEqual(517245692, BitHelpers.FromBitPattern("011110110101001000101011111100"));
            Assert.AreEqual(-1, BitHelpers.FromBitPattern("11111111111111111111111111111111"));
        }
    }
}
