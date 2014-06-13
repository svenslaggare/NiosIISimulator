using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiosII_Simulator.Core;

namespace NiosII_Simulator.Test
{
    /// <summary>
    /// Tests the instruction set
    /// </summary>
    [TestClass]
    public class TestInstructionSet
    {
        private VirtualMachine virtualMachine;

        [TestInitialize]
        public void Initialize()
        {
            this.virtualMachine = new VirtualMachine();
        }

        /// <summary>
        /// Tests the ADD instruction
        /// </summary>
        [TestMethod]
        public void TestAdd()
        {
            this.virtualMachine.SetRegisterValue(Registers.R8, 2);
            this.virtualMachine.SetRegisterValue(Registers.R9, 8);

            RFormatInstruction slliInstruction = new RFormatInstruction(
                OperationCodes.Add.Code(),
                OperationXCodes.Add,
                Registers.R8.Number(),
                Registers.R9.Number(),
                Registers.R10.Number());

            this.virtualMachine.ExecuteInstruction(slliInstruction.Encode());
            Assert.AreEqual(10, this.virtualMachine.GetRegisterValue(Registers.R10));
        }

        /// <summary>
        /// Tests the ADDI instruction
        /// </summary>
        [TestMethod]
        public void TestAddi()
        {
            this.virtualMachine.SetRegisterValue(Registers.R8, 2);

            IFormatInstruction slliInstruction = new IFormatInstruction(
                OperationCodes.Addi.Code(),
                Registers.R8.Number(),
                Registers.R9.Number(),
                15);

            this.virtualMachine.ExecuteInstruction(slliInstruction.Encode());
            Assert.AreEqual(17, this.virtualMachine.GetRegisterValue(Registers.R9));
        }

        /// <summary>
        /// Tests the SUB instruction
        /// </summary>
        [TestMethod]
        public void TestSub()
        {
            this.virtualMachine.SetRegisterValue(Registers.R8, 2);
            this.virtualMachine.SetRegisterValue(Registers.R9, 8);

            RFormatInstruction slliInstruction = new RFormatInstruction(
                OperationCodes.Sub.Code(),
                OperationXCodes.Sub,
                Registers.R8.Number(),
                Registers.R9.Number(),
                Registers.R10.Number());

            this.virtualMachine.ExecuteInstruction(slliInstruction.Encode());
            Assert.AreEqual(-6, this.virtualMachine.GetRegisterValue(Registers.R10));
        }

        /// <summary>
        /// Tests the SLLI instruction
        /// </summary>
        [TestMethod]
        public void TestSlli()
        {
            this.virtualMachine.SetRegisterValue(Registers.R8, 2);

            RFormatInstruction slliInstruction = new RFormatInstruction(
                OperationCodes.Slli.Code(),
                OperationXCodes.Slli | 2,
                Registers.R8.Number(),
                0,
                Registers.R9.Number());

            this.virtualMachine.ExecuteInstruction(slliInstruction.Encode());
            Assert.AreEqual(8, this.virtualMachine.GetRegisterValue(Registers.R9));
        }

        /// <summary>
        /// Tests the SRLI instruction
        /// </summary>
        [TestMethod]
        public void TestSrli()
        {
            this.virtualMachine.SetRegisterValue(Registers.R8, 8);

            RFormatInstruction slliInstruction = new RFormatInstruction(
                OperationCodes.Srli.Code(),
                OperationXCodes.Srli | 2,
                Registers.R8.Number(),
                0,
                Registers.R9.Number());

            this.virtualMachine.ExecuteInstruction(slliInstruction.Encode());
            Assert.AreEqual(2, this.virtualMachine.GetRegisterValue(Registers.R9));
        }

		/// <summary>
		/// Tests the LDB instruction
		/// </summary>
		[TestMethod]
		public void TestLdb()
		{
			uint addr = 1024;
			byte value = 54;

			this.virtualMachine.SetRegisterValue(Registers.R1, (int)addr);
			this.virtualMachine.WriteByteToMemory(addr, value);

			IFormatInstruction ldbInstruction = new IFormatInstruction(
				OperationCodes.Ldb.Code(),
				1,
				2,
				0);

			this.virtualMachine.ExecuteInstruction(ldbInstruction.Encode());
			Assert.AreEqual(value, this.virtualMachine.GetRegisterValue(Registers.R2));
		}

		/// <summary>
		/// Tests the LDBU instruction
		/// </summary>
		[TestMethod]
		public void TestLdbu()
		{
			uint addr = 1024;
			byte value = 255;

			this.virtualMachine.SetRegisterValue(Registers.R1, (int)addr);
			this.virtualMachine.WriteByteToMemory(addr, value);

			IFormatInstruction ldbInstruction = new IFormatInstruction(
				OperationCodes.Ldbu.Code(),
				1,
				2,
				0);

			this.virtualMachine.ExecuteInstruction(ldbInstruction.Encode());
			Assert.AreEqual(value, this.virtualMachine.GetRegisterValue(Registers.R2));
		}

		/// <summary>
		/// Tests the STB instruction
		/// </summary>
		[TestMethod]
		public void TestStb()
		{
			uint addr = 1024;
			byte value = 54;

			this.virtualMachine.SetRegisterValue(Registers.R1, (int)addr);
			this.virtualMachine.SetRegisterValue(Registers.R2, value);

			IFormatInstruction ldbInstruction = new IFormatInstruction(
				OperationCodes.Stb.Code(),
				1,
				2,
				0);

			this.virtualMachine.ExecuteInstruction(ldbInstruction.Encode());
			Assert.AreEqual(value, this.virtualMachine.ReadByteFromMemory(addr));
		}
    }
}
