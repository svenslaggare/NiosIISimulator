using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiosII_Simulator.Core;
using NiosII_Simulator.Core.Assembler;

namespace NiosII_Simulator.Test
{
    /// <summary>
    /// Tests the assembler
    /// </summary>
    [TestClass]
    public class TestAssembler
    {
        /// <summary>
        /// Tests the RemoveComments methods
        /// </summary>
        [TestMethod]
        public void TestRemoveComments()
        {
            Assert.AreEqual("ADD r5, r6, r7", NiosAssembler.RemoveComments("ADD r5, r6, r7"));
            Assert.AreEqual("ADD r5, r6, r7 ", NiosAssembler.RemoveComments("ADD r5, r6, r7 #tja"));
            Assert.AreEqual("LDW r5, 0(r6) ", NiosAssembler.RemoveComments("LDW r5, 0(r6) #tja"));
            Assert.AreEqual("x: LDW r5, 0(r6)", NiosAssembler.RemoveComments("x: LDW r5, 0(r6)"));
        }

        /// <summary>
        /// Tests the ExtractInstructionAndOperands method
        /// </summary>
        [TestMethod]
        public void TestExtractInstructionAndOperands()
        {
            string[] instruction = NiosAssembler.ExtractInstructionAndOperands("x:    ADD   r5,   r6,    r7");
            Assert.AreEqual("x:", instruction[0]);
            Assert.AreEqual("ADD", instruction[1]);
            Assert.AreEqual("r5", instruction[2]);
            Assert.AreEqual("r6", instruction[3]);
            Assert.AreEqual("r7", instruction[4]);
        }

        /// <summary>
        /// Tests the Assemble method
        /// </summary>
        [TestMethod]
        public void TestAssemble()
		{
			Program testProgram = NiosAssembler.New().AssembleFromLines(
				"movi r6, 10",
				"movi r7, 15",
				"add r5, r6, r7");

            VirtualMachine virtualMachine = new VirtualMachine();
            virtualMachine.Run(testProgram);
            Assert.AreEqual(10, virtualMachine.GetRegisterValue(Registers.R6));
            Assert.AreEqual(15, virtualMachine.GetRegisterValue(Registers.R7));
            Assert.AreEqual(25, virtualMachine.GetRegisterValue(Registers.R5));

			Program loopProgram = NiosAssembler.New().AssembleFromLines(
				"movi r8, 50",
				"loop: addi r8, r8, -1",
				"addi r9, r9, 5",
				"bne r0, r8, loop");

            virtualMachine.Run(loopProgram);
            Assert.AreEqual(50 * 5, virtualMachine.GetRegisterValue(Registers.R9));

			Program loopWithMemProgram = NiosAssembler.New().AssembleFromLines(
				"movi r8, 50",
				"movi r9, 0",
				"loop: addi r8, r8, -1",
				"addi r9, r9, 5",
				"bne r0, r8, loop",
				"stw r9, 1024(r0)",
				"ldw r10, 1024(r0)");

            virtualMachine.Run(loopWithMemProgram);
            Assert.AreEqual(50 * 5, virtualMachine.GetRegisterValue(Registers.R9));
            Assert.AreEqual(50 * 5, virtualMachine.ReadWordFromMemory(1024));
            Assert.AreEqual(50 * 5, virtualMachine.GetRegisterValue(Registers.R10));
        }

        /// <summary>
        /// Tests the Assemble method
        /// </summary>
        [TestMethod]
        public void TestAssemble2()
        {
            VirtualMachine virtualMachine = new VirtualMachine();
			Program loopWithMemProgram = NiosAssembler.New().AssembleFromLines(
				"#Init start values",
				"movi r8, 50 #50 ok?",
				"movi r9, 0",
				"#Start loop",
				"loop: addi r8, r8, -1",
				"addi r9, r9, 5",
				"bne r0, r8, loop",
				"stw r9, 1024(r0)",
				"ldw r10, 1024(r0)");

            virtualMachine.Run(loopWithMemProgram);
            Assert.AreEqual(50 * 5, virtualMachine.GetRegisterValue(Registers.R9));
            Assert.AreEqual(50 * 5, virtualMachine.ReadWordFromMemory(1024));
            Assert.AreEqual(50 * 5, virtualMachine.GetRegisterValue(Registers.R10));
        }

        /// <summary>
        /// Tests the Assemble method
        /// </summary>
        [TestMethod]
        public void TestAssemble3()
        {
            VirtualMachine virtualMachine = new VirtualMachine();
			Program loopWithMemProgram = NiosAssembler.New().AssembleFromLines(
				"#Init start values",
				"movi r8, 50 #50 ok?",
				"movi r9, 0",
				"#Start loop",
				"loop:",
				"addi r8, r8, -1",
				"addi r9, r9, 5",
				"bne r0, r8, loop",
				"stw r9, 1024(r0)",
				"ldw r10, 1024(r0)");

            virtualMachine.Run(loopWithMemProgram);
            Assert.AreEqual(50 * 5, virtualMachine.GetRegisterValue(Registers.R9));
            Assert.AreEqual(50 * 5, virtualMachine.ReadWordFromMemory(1024));
            Assert.AreEqual(50 * 5, virtualMachine.GetRegisterValue(Registers.R10));
        }

        /// <summary>
        /// Tests the Assemble method
        /// </summary>
        [TestMethod]
        public void TestAssemble4()
        {
            VirtualMachine virtualMachine = new VirtualMachine();
			Program program = NiosAssembler.New().AssembleFromLines(
				"movi r2, 10",
				"loop: call decr2",
				"bne r0, r2, loop",
				"br end",
				"decr2:",
				"subi r2, r2, 1",
				"ret",
				"end:");

            virtualMachine.Run(program);
            Assert.AreEqual(0, virtualMachine.GetRegisterValue(Registers.R2));
        }

        /// <summary>
        /// Tests the Assemble method
        /// </summary>
        [TestMethod]
        public void TestAssemble5()
        {
			VirtualMachine virtualMachine = new VirtualMachine();
			Program program = NiosAssembler.New().Assemble(
			   new string[]
			   { 
                    "movi r2, 10",
                    "loop: call decr2",
                    "addi r4, r2, 48",
                    "call putchar",
                    "bne r0, r2, loop",
                    "br end",
                    "decr2:",
                    "subi r2, r2, 1",
                    "ret",
                    "end:"
                },
				virtualMachine.SystemCalls);

            virtualMachine.Run(program);
            Assert.AreEqual(0, virtualMachine.GetRegisterValue(Registers.R2));
        }

        /// <summary>
        /// Tests the Assemble method
        /// </summary>
        [TestMethod]
        public void TestAssemble6()
        {
            VirtualMachine virtualMachine = new VirtualMachine();
			Program program = NiosAssembler.New().Assemble(
               new string[]
			   { 
                    "decr2:",
                    "subi r2, r2, 1",
                    "ret",
                    "main:",
                    "movi r2, 10",
                    "loop: call decr2",
                    "addi r4, r2, 48",
                    "call putchar",
                    "addi r8, r8, 1",
                    "bne r0, r2, loop"
                },
                virtualMachine.SystemCalls);

            virtualMachine.Run(program);
            Assert.AreEqual(0, virtualMachine.GetRegisterValue(Registers.R2));
            Assert.AreEqual(10, virtualMachine.GetRegisterValue(Registers.R8));
        }

        /// <summary>
        /// Tests the assembling an incorrect program
        /// </summary>
        [TestMethod]
        public void TestIncorrectAssemble1()
        {
            try
            {
				Program incorrectProgram = NiosAssembler.New().AssembleFromLines(
					"movi r8, 50",
					"fcdfdf");

                Assert.Fail("Expected failure");
            }
            catch
            {

            }
        }

        /// <summary>
        /// Tests the marcos
        /// </summary>
        [TestMethod]
        public void TestMarcos()
        {
			Program testProgram = NiosAssembler.New().Assemble(
                "movi r10, 15");

            VirtualMachine virtualMachine = new VirtualMachine();
            virtualMachine.Run(testProgram);
            Assert.AreEqual(15, virtualMachine.GetRegisterValue(Registers.R10));
        }

		/// <summary>
		/// Tests the assembler marcos
		/// </summary>
		[TestMethod]
		public void TestAssemblerMarcos()
		{
			Program testProgram = NiosAssembler.New().Assemble(
				"addi r2, r2, %hi(756746)");

			VirtualMachine virtualMachine = new VirtualMachine();
			virtualMachine.Run(testProgram);
			Assert.AreEqual(11, virtualMachine.GetRegisterValue(Registers.R2));

			testProgram = NiosAssembler.New().AssembleFromLines(
				"br %lo(end)",
				"end: addi r3, r3, 1");

			virtualMachine = new VirtualMachine();
			virtualMachine.Run(testProgram);
			Assert.AreEqual(1, virtualMachine.GetRegisterValue(Registers.R3));
		}

		/// <summary>
		/// Tests the assembler macros
		/// </summary>
		[TestMethod]
		public void TestAssemblerMacros2()
		{
			VirtualMachine virtualMachine = new VirtualMachine();
			Program program = NiosAssembler.New().AssembleFromLines(
				"movi r2, 10",
				"loop: call %lo(decr2)",
				"bne r0, r2, loop",
				"br end",
				"decr2:",
				"subi r2, r2, 1",
				"ret",
				"end:");

			virtualMachine.Run(program);
			Assert.AreEqual(0, virtualMachine.GetRegisterValue(Registers.R2));
		}

		/// <summary>
		/// Tests declaring data variables
		/// </summary>
		[TestMethod]
		public void TestDataVariables()
		{
			VirtualMachine virtualMachine = new VirtualMachine();
			Program program = NiosAssembler.New().AssembleFromLines(
				".data",
				"x: .word 1337",
				".text",
				"movia r1, x",
				"ldw r2, 0(r1)");

			virtualMachine.Run(program);
			Assert.AreEqual(1337, virtualMachine.GetRegisterValue(Registers.R2));

			virtualMachine.ClearMemory();

			program = NiosAssembler.New().AssembleFromLines(
				".data",
				"x: .word 112",
				".text",
				"movia r1, x",
				"ldb r2, 0(r1)");

			virtualMachine.Run(program);
			Assert.AreEqual(112, virtualMachine.GetRegisterValue(Registers.R2));
		}
    }
}
