using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiosII_Simulator.Core;
using NiosII_Simulator.Core.Assembler;

namespace NiosII_Simulator.Test
{
    /// <summary>
    /// Tests instructions that depends on more information or pseudo instrcutions
    /// </summary>
    [TestClass]
    public class TestAssemblerInstructionSet
    {
        private VirtualMachine virtualMachine;

        [TestInitialize]
        public void Initialize()
        {
            this.virtualMachine = new VirtualMachine();
        }

        /// <summary>
        /// Tests the BEQ instruction
        /// </summary>
        [TestMethod]
        public void TestBeq()
        {
            VirtualMachine virtualMachine = new VirtualMachine();

			Program program = NiosAssembler.New().AssembleFromLines(
				"beq r0, r8, x",
				"movi r9, 1337",
				"br end",
				"x: movi r9, 4711",
				"end:");

            //Test when true
            virtualMachine.SetRegisterValue(Registers.R8, 0);
            virtualMachine.Run(program);
            Assert.AreEqual(4711, virtualMachine.GetRegisterValue(Registers.R9));

            //Test when false
            virtualMachine.SetRegisterValue(Registers.R8, 5);
            virtualMachine.Run(program);
            Assert.AreEqual(1337, virtualMachine.GetRegisterValue(Registers.R9));
        }

        /// <summary>
        /// Tests the BNE instruction
        /// </summary>
        [TestMethod]
        public void TestBne()
        {
            VirtualMachine virtualMachine = new VirtualMachine();

			Program program = NiosAssembler.New().AssembleFromLines(
				"bne r0, r8, x",
                "movi r9, 1337",
                "br end",
                "x: movi r9, 4711",
                "end:");

            //Test when true
            virtualMachine.SetRegisterValue(Registers.R8, 5);
            virtualMachine.Run(program);
            Assert.AreEqual(4711, virtualMachine.GetRegisterValue(Registers.R9));

            //Test when false
            virtualMachine.SetRegisterValue(Registers.R8, 0);
            virtualMachine.Run(program);
            Assert.AreEqual(1337, virtualMachine.GetRegisterValue(Registers.R9));
        }

        /// <summary>
        /// Tests the BGE instruction
        /// </summary>
        [TestMethod]
        public void TestBge()
        {
            VirtualMachine virtualMachine = new VirtualMachine();

			Program program = NiosAssembler.New().AssembleFromLines(
				"bge r7, r8, x",
				"movi r9, 1337",
				"br end",
				"x: movi r9, 4711",
				"end:");

            //Test when true
            virtualMachine.SetRegisterValue(Registers.R7, 5);
            virtualMachine.SetRegisterValue(Registers.R8, 5);
            virtualMachine.Run(program);
            Assert.AreEqual(4711, virtualMachine.GetRegisterValue(Registers.R9));

            //Test when false
            virtualMachine.SetRegisterValue(Registers.R7, 4);
            virtualMachine.SetRegisterValue(Registers.R8, 5);
            virtualMachine.Run(program);
            Assert.AreEqual(1337, virtualMachine.GetRegisterValue(Registers.R9));
        }

        /// <summary>
        /// Tests the BGT instruction
        /// </summary>
        [TestMethod]
        public void TestBgt()
        {
            VirtualMachine virtualMachine = new VirtualMachine();

			Program program = NiosAssembler.New().AssembleFromLines(
				"bgt r7, r8, x",
				"movi r9, 1337",
				"br end",
				"x: movi r9, 4711",
				"end:");

            //Test when true
            virtualMachine.SetRegisterValue(Registers.R7, 6);
            virtualMachine.SetRegisterValue(Registers.R8, 5);
            virtualMachine.Run(program);
            Assert.AreEqual(4711, virtualMachine.GetRegisterValue(Registers.R9));

            //Test when false
            virtualMachine.SetRegisterValue(Registers.R7, 4);
            virtualMachine.SetRegisterValue(Registers.R8, 5);
            virtualMachine.Run(program);
            Assert.AreEqual(1337, virtualMachine.GetRegisterValue(Registers.R9));
        }

        /// <summary>
        /// Tests the BLE instruction
        /// </summary>
        [TestMethod]
        public void TestBle()
        {
            VirtualMachine virtualMachine = new VirtualMachine();

			Program program = NiosAssembler.New().AssembleFromLines(
				"ble r7, r8, x",
				"movi r9, 1337",
				"br end",
				"x: movi r9, 4711",
				"end:");

            //Test when true
            virtualMachine.SetRegisterValue(Registers.R7, 5);
            virtualMachine.SetRegisterValue(Registers.R8, 5);
            virtualMachine.Run(program);
            Assert.AreEqual(4711, virtualMachine.GetRegisterValue(Registers.R9));

            //Test when false
            virtualMachine.SetRegisterValue(Registers.R7, 5);
            virtualMachine.SetRegisterValue(Registers.R8, 4);
            virtualMachine.Run(program);
            Assert.AreEqual(1337, virtualMachine.GetRegisterValue(Registers.R9));
        }

        /// <summary>
        /// Tests the BLT instruction
        /// </summary>
        [TestMethod]
        public void TestBlt()
        {
            VirtualMachine virtualMachine = new VirtualMachine();

			Program program = NiosAssembler.New().AssembleFromLines(
				"blt r7, r8, x",
				"movi r9, 1337",
				"br end",
				"x: movi r9, 4711",
				"end:");

            //Test when true
            virtualMachine.SetRegisterValue(Registers.R7, 4);
            virtualMachine.SetRegisterValue(Registers.R8, 5);
            virtualMachine.Run(program);
            Assert.AreEqual(4711, virtualMachine.GetRegisterValue(Registers.R9));

            //Test when false
            virtualMachine.SetRegisterValue(Registers.R7, 5);
            virtualMachine.SetRegisterValue(Registers.R8, 4);
            virtualMachine.Run(program);
            Assert.AreEqual(1337, virtualMachine.GetRegisterValue(Registers.R9));
        }

		/// <summary>
		/// Tests the MOVIA instruction
		/// </summary>
		[TestMethod]
		public void TestMovia()
		{
			VirtualMachine virtualMachine = new VirtualMachine();

			Program program = NiosAssembler.New().AssembleFromLines(
				"movia r1, 4351314");

			virtualMachine.Run(program);
			Assert.AreEqual(4351314, virtualMachine.GetRegisterValue(Registers.R1));
		}
    }
}
