using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiosII_Simulator.Core;
using NiosII_Simulator.Core.JIT;

namespace NiosII_Simulator.Test
{
	/// <summary>
	/// Tests the instruction set for the JIT compiler
	/// </summary>
	[TestClass]
	public class TestJITInstructionSet
	{
		/// <summary>
		/// Tests the LDB instruction
		/// </summary>
		[TestMethod]
		public void TestLdb()
		{
			VirtualMachine virtualMachine = new VirtualMachine();
			int value = 125;
			uint memAddr = 1024;

			virtualMachine.SetRegisterValue(Registers.R1, (int)memAddr);
			virtualMachine.WriteWordToMemory(memAddr, value);

			FullJITCompiler jitCompiler = new FullJITCompiler(virtualMachine);

			Instruction[] funcBody = new Instruction[]
			{
				new IFormatInstruction(OperationCodes.Ldb.Code(), 1, 2, 0).AsInstruction()
			};

			var jittedProgram = jitCompiler.GenerateProgram(
				"test_program",
				funcBody,
				new Dictionary<uint, int>());

			jitCompiler.RunJittedProgram(jittedProgram);
			Assert.AreEqual(value, virtualMachine.GetRegisterValue(Registers.R2));
		}

		/// <summary>
		/// Tests the STB instruction
		/// </summary>
		[TestMethod]
		public void TestStb()
		{
			VirtualMachine virtualMachine = new VirtualMachine();
			int value = 125;
			uint memAddr = 1024;

			virtualMachine.SetRegisterValue(Registers.R1, (int)memAddr);
			virtualMachine.SetRegisterValue(Registers.R2, value);

			FullJITCompiler jitCompiler = new FullJITCompiler(virtualMachine);

			Instruction[] funcBody = new Instruction[]
			{
				new IFormatInstruction(OperationCodes.Stb.Code(), 1, 2, 0).AsInstruction()
			};

			var jittedProgram = jitCompiler.GenerateProgram(
				"test_program",
				funcBody,
				new Dictionary<uint, int>());

			jitCompiler.RunJittedProgram(jittedProgram);
			Assert.AreEqual(value, virtualMachine.ReadWordFromMemory(memAddr));
		}
	}
}
