using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiosII_Simulator.Core;

namespace NiosII_Simulator.Test
{
    /// <summary>
    /// Tests the instruction formats
    /// </summary>
    [TestClass]
    public class TestInstructionFormats
    {
        /// <summary>
        /// Tests the I-format
        /// </summary>
        [TestMethod]
        public void TestIFormat()
        {
            IFormatInstruction test = new IFormatInstruction(9, 12, 16, 1343);
            IFormatInstruction decodedTest = IFormatInstruction.Decode(test.Encode());
            Assert.AreEqual(test.OpCode, decodedTest.OpCode);
            Assert.AreEqual(test.RegisterA, decodedTest.RegisterA);
            Assert.AreEqual(test.RegisterB, decodedTest.RegisterB);
            Assert.AreEqual(test.Immediate, decodedTest.Immediate);

            Assert.AreEqual(9, decodedTest.OpCode);
            Assert.AreEqual(12, decodedTest.RegisterA);
            Assert.AreEqual(16, decodedTest.RegisterB);
            Assert.AreEqual(1343, decodedTest.Immediate);

            test = new IFormatInstruction(25, 12, 16, -1);
            decodedTest = IFormatInstruction.Decode( test.Encode());
            Assert.AreEqual(test.OpCode, decodedTest.OpCode);
            Assert.AreEqual(test.RegisterA, decodedTest.RegisterA);
            Assert.AreEqual(test.RegisterB, decodedTest.RegisterB);
            Assert.AreEqual(test.SignedImmediate, decodedTest.SignedImmediate);

            Assert.AreEqual(25, decodedTest.OpCode);
            Assert.AreEqual(12, decodedTest.RegisterA);
            Assert.AreEqual(16, decodedTest.RegisterB);
            Assert.AreEqual(-1, decodedTest.SignedImmediate);

            test = new IFormatInstruction(25, 12, 16, -256);
            decodedTest = IFormatInstruction.Decode(test.Encode());
            Assert.AreEqual(test.OpCode, decodedTest.OpCode);
            Assert.AreEqual(test.RegisterA, decodedTest.RegisterA);
            Assert.AreEqual(test.RegisterB, decodedTest.RegisterB);
            Assert.AreEqual(test.SignedImmediate, decodedTest.SignedImmediate);

            Assert.AreEqual(25, decodedTest.OpCode);
            Assert.AreEqual(12, decodedTest.RegisterA);
            Assert.AreEqual(16, decodedTest.RegisterB);
            Assert.AreEqual(-256, decodedTest.SignedImmediate);

            test = new IFormatInstruction(OperationCodes.Bne.Code(), Registers.R0.Number(), Registers.R8.Number(), -8);
            decodedTest = IFormatInstruction.Decode(test.Encode());
            Assert.AreEqual(test.OpCode, decodedTest.OpCode);
            Assert.AreEqual(test.RegisterA, decodedTest.RegisterA);
            Assert.AreEqual(test.RegisterB, decodedTest.RegisterB);
            Assert.AreEqual(test.SignedImmediate, decodedTest.SignedImmediate);

            Assert.AreEqual(OperationCodes.Bne.Code(), decodedTest.OpCode);
            Assert.AreEqual(0, decodedTest.RegisterA);
            Assert.AreEqual(8, decodedTest.RegisterB);
            Assert.AreEqual(-8, decodedTest.SignedImmediate);

			test = new IFormatInstruction(25, 12, 16, 40512);
			decodedTest = IFormatInstruction.Decode(test.Encode());
			Assert.AreEqual(test.Immediate, decodedTest.Immediate);
			Assert.AreEqual(40512, decodedTest.Immediate);
        }

        /// <summary>
        /// Tests the R-format
        /// </summary>
        [TestMethod]
        public void TestRFormat()
        {
            RFormatInstruction test = new RFormatInstruction(25, 2047, 12, 15, 17);
            RFormatInstruction decodedTest = RFormatInstruction.Decode(test.Encode());
            Assert.AreEqual(test.OpCode, decodedTest.OpCode);
            Assert.AreEqual(test.OpxCode, decodedTest.OpxCode);
            Assert.AreEqual(test.RegisterA, decodedTest.RegisterA);
            Assert.AreEqual(test.RegisterB, decodedTest.RegisterB);
            Assert.AreEqual(test.RegisterC, decodedTest.RegisterC);

            Assert.AreEqual(25, decodedTest.OpCode);
            Assert.AreEqual(2047, decodedTest.OpxCode);
            Assert.AreEqual(12, decodedTest.RegisterA);
            Assert.AreEqual(15, decodedTest.RegisterB);
            Assert.AreEqual(17, decodedTest.RegisterC);
        }

        /// <summary>
        /// Tests the J-format
        /// </summary>
        [TestMethod]
        public void TestJFormat()
        {
            JFormatInstruction test = new JFormatInstruction(45, 4325357);
            JFormatInstruction decodedTest = JFormatInstruction.Decode(test.Encode());
            Assert.AreEqual(test.OpCode, decodedTest.OpCode);
            Assert.AreEqual(test.Immediate, decodedTest.Immediate);

            Assert.AreEqual(45, decodedTest.OpCode);
            Assert.AreEqual(4325357, decodedTest.Immediate);
        }

        [TestMethod]
        public void TestArith()
        {
            int x = 1345;
            int x1 = x & 0xFF;
            int x2 = (x >> 8) & 0xFF;

            int opCode = 25 & 0x3F;
            //int imm = (1345 << 5) & 0x3FFFC0;

            int imm1 = (x1 << 6);
            int imm2 = (x2 << 6 + 8);
            int imm = (imm1 | imm2);

            int regB = (12 & 0x1F) << 22;
            int regA = (14 & 0x1F) << 27;

            int inst1 = opCode;
            int inst2 = opCode | imm | regA | regB;
            //int inst2 = opCode | imm;
            Assert.AreEqual(inst1 & 0x3F, 25);
            Assert.AreEqual(inst1 & 0x3F, inst2 & 0x3F);

            int instImm = inst2 & 0x3FFFC0;
            int decodedImm1 = (inst2 >> 6) & 0xFF;
            int decodedImm2 = (inst2 >> 6 + 8) & 0xFF;

            int decodedImm = decodedImm1 | (decodedImm2 << 8);

            int decodedRegB = (inst2 >> 22) & 0x1F;
            int decodedRegA = (inst2 >> 27) & 0x1F;

            Assert.AreEqual(x1, decodedImm1);
            Assert.AreEqual(x2, decodedImm2);
            Assert.AreEqual(x, decodedImm);
            Assert.AreEqual(12, decodedRegB);
            Assert.AreEqual(14, decodedRegA);
        }
    }
}
