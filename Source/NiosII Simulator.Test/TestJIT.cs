using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiosII_Simulator.Core;
using NiosII_Simulator.Core.Assembler;
using NiosII_Simulator.Core.JIT;
using System.Diagnostics;
using System.Reflection;

namespace NiosII_Simulator.Test
{
    /// <summary>
    /// Tests the JIT compiler
    /// </summary>
    [TestClass]
    public class TestJIT
    {
        [TestMethod]
        public void TestBenchmark()
        {
            VirtualMachine virtualMachine = new VirtualMachine();
			PartialJITCompiler jitCompiler = new PartialJITCompiler(virtualMachine);
			FullJITCompiler fullJitCompiler = new FullJITCompiler(virtualMachine);

			int count = 2000;

			Instruction[] methodBody = new Instruction[]
			{
				new IFormatInstruction(OperationCodes.Addi.Code(), 1, 1, -1).AsInstruction(),
				new IFormatInstruction(OperationCodes.Addi.Code(), 2, 2, 1).AsInstruction(),
				new IFormatInstruction(OperationCodes.Bne.Code(), 0, 1, -3*4).AsInstruction()
			};

			var jittedMethod = jitCompiler.GenerateMethod("", methodBody);

			TimeSpan jitTime = new TimeSpan();
			TimeSpan vmTime = new TimeSpan();

			//Partial jitter
			virtualMachine.SetRegisterValue(Registers.R1, count);
			Stopwatch timer = Stopwatch.StartNew();
			jitCompiler.InvokeJitedMethod(jittedMethod);
			timer.Stop();

			jitTime = timer.Elapsed;

			//VM
			virtualMachine.SetRegisterValue(Registers.R1, count);

			timer = Stopwatch.StartNew();
			for (int i = 0; i < methodBody.Length; i++)
			{
				virtualMachine.ExecuteInstruction(methodBody[i].Data);
			}
			timer.Stop();

			vmTime = timer.Elapsed;
        }

		/// <summary>
		/// Tests when the Jitter is enabled
		/// </summary>
		[TestMethod]
		public void TestWithJittedMethod()
		{
			VirtualMachine virtualMachine = new VirtualMachine();

			virtualMachine.SetRegisterValue(Registers.R2, 30000);

			Program testProgram = NiosAssembler.New().AssembleFromLines(
				"loop: call decr2",
				"bne r0, r2, loop",
				"br end",
				"decr2:",
				"addi r4, r4, 1",
				"addi r5, r5, 2",
				"addi r6, r6, 3",
				"subi r2, r2, 1",
				"ret",
				"end:");

			virtualMachine.Run(testProgram);
			//var jitCompiler = new FullJITCompiler(virtualMachine);
			//var jittedProgram = jitCompiler.GenerateProgram("", testProgram.GetInstructions(), testProgram.FunctionTable);
			//jitCompiler.RunJittedProgram(jittedProgram);
		}

		/// <summary>
		/// Tests jitted instructions
		/// </summary>
		[TestMethod]
		public void TestInstructions()
		{
			VirtualMachine virtualMachine = new VirtualMachine();
			PartialJITCompiler jitCompiler = new PartialJITCompiler(virtualMachine);

			//Sub
			virtualMachine.SetRegisterValue(Registers.R1, 12);
			virtualMachine.SetRegisterValue(Registers.R2, 5);

			Instruction[] methodBody = new Instruction[]
			{
				new RFormatInstruction(OperationCodes.Sub.Code(), OperationXCodes.Sub, 1, 2, 3).AsInstruction()
			};

			var jittedMethod = jitCompiler.GenerateMethod("", methodBody);
			jitCompiler.InvokeJitedMethod(jittedMethod);
			Assert.AreEqual(virtualMachine.GetRegisterValue(Registers.R3), 7);
		}

		/// <summary>
		/// Tests a jitted function
		/// </summary>
		[TestMethod]
		public void TestFunction()
		{
			VirtualMachine virtualMachine = new VirtualMachine();
			PartialJITCompiler jitCompiler = new PartialJITCompiler(virtualMachine);

			virtualMachine.SetRegisterValue(Registers.R1, 10);
			virtualMachine.SetRegisterValue(Registers.RA, 1337);

			Instruction[] methodBody = new Instruction[]
			{
				new IFormatInstruction(OperationCodes.Addi.Code(), 1, 1, -1).AsInstruction(),
				new RFormatInstruction(OperationCodes.Ret.Code(), OperationXCodes.Ret, 0, 0, 0).AsInstruction()
			};

			var jittedMethod = jitCompiler.GenerateMethod("", methodBody, true);
			jitCompiler.InvokeJitedMethod(jittedMethod);
			Assert.AreEqual(9, virtualMachine.GetRegisterValue(Registers.R1));
			Assert.AreEqual((uint)1337, virtualMachine.ProgramCounter);
		}

		/// <summary>
		/// Tests the full jitter
		/// </summary>
		[TestMethod]
		public void TestFullJit()
		{
			VirtualMachine virtualMachine = new VirtualMachine();
			int value = 1000;
			virtualMachine.SetRegisterValue(Registers.R1, value);

			Program testProgram = NiosAssembler.New().AssembleFromLines(
				"start: call dec",
				"addi r2, r2, 1",
				"bne r0, r1, start",
				"dec: addi r1, r1, -1",
				"ret");

			FullJITCompiler jitCompiler = new FullJITCompiler(virtualMachine);

			Instruction[] funcBody = testProgram.GetInstructions();

			var jittedProgram = jitCompiler.GenerateProgram(
				"test_program",
				funcBody,
				testProgram.FunctionTable);

			jitCompiler.RunJittedProgram(jittedProgram);
			Assert.AreEqual(virtualMachine.GetRegisterValue(Registers.R1), 0);
			Assert.AreEqual(virtualMachine.GetRegisterValue(Registers.R2), value);
		}

		/// <summary>
		/// Tests the full jitter
		/// </summary>
		[TestMethod]
		public void TestFullJit2()
		{
			VirtualMachine virtualMachine = new VirtualMachine();
			int value = 15464;
			int value2 = 435351;
			uint memAddr = 1024;

			virtualMachine.SetRegisterValue(Registers.R1, (int)memAddr);
			virtualMachine.WriteWordToMemory(memAddr, value);
			virtualMachine.SetRegisterValue(Registers.R3, value2);

			Program testProgram = NiosAssembler.New().AssembleFromLines(
				"ldw r2, 0(r1)",
				"stw r3, 0(r1)");

			FullJITCompiler jitCompiler = new FullJITCompiler(virtualMachine);

			Instruction[] funcBody = testProgram.GetInstructions();

			var jittedProgram = jitCompiler.GenerateProgram(
				"test_program",
				funcBody,
				testProgram.FunctionTable);

			jitCompiler.RunJittedProgram(jittedProgram);
			Assert.AreEqual(value, virtualMachine.GetRegisterValue(Registers.R2));
			Assert.AreEqual(value2, virtualMachine.ReadWordFromMemory(memAddr));
		}

		/// <summary>
		/// Tests the full jitter
		/// </summary>
		[TestMethod]
		public void TestFullJit3()
		{
			VirtualMachine virtualMachine = new VirtualMachine();
			int value = 1000;
			virtualMachine.SetRegisterValue(Registers.R1, value);

			Program testProgram = NiosAssembler.New().AssembleFromLines(
				"start: call dec",
				"call body",
				"bne r0, r1, start",
				"body: addi r2, r2, 1",
				"ret",
				"dec: addi r1, r1, -1",
				"ret");

			FullJITCompiler jitCompiler = new FullJITCompiler(virtualMachine);

			Instruction[] funcBody = testProgram.GetInstructions();

			var jittedProgram = jitCompiler.GenerateProgram(
				"test_program",
				funcBody,
				testProgram.FunctionTable);

			jitCompiler.RunJittedProgram(jittedProgram);
			Assert.AreEqual(virtualMachine.GetRegisterValue(Registers.R1), 0);
			Assert.AreEqual(virtualMachine.GetRegisterValue(Registers.R2), value);
		}
	}
}
