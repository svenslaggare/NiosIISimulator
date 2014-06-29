using System;
using System.Collections.Generic;
using System.Reflection;
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
		private VirtualMachine virtualMachine;
		private FullJITCompiler jitCompiler;

		[TestInitialize]
		public void Initialize()
		{
			this.virtualMachine = new VirtualMachine();
			this.jitCompiler = new FullJITCompiler(this.virtualMachine);
		}

		private MethodInfo CreateProgram(params Instruction[] funcBody)
		{
			return jitCompiler.GenerateProgram(
				"test_program",
				funcBody,
				new Dictionary<uint, int>());
		}

		/// <summary>
		/// Tests the ldb instruction
		/// </summary>
		[TestMethod]
		public void TestLdb()
		{
			int value = 125;
			uint memAddr = 1024;

			virtualMachine.SetRegisterValue(Registers.R1, (int)memAddr);
			virtualMachine.WriteWordToMemory(memAddr, value);

			var jittedProgram = CreateProgram(
				new IFormatInstruction(OperationCodes.Ldb.Code(), 1, 2, 0).AsInstruction());

			jitCompiler.RunJittedProgram(jittedProgram);
			Assert.AreEqual(value, virtualMachine.GetRegisterValue(Registers.R2));
		}

		/// <summary>
		/// Tests the stb instruction
		/// </summary>
		[TestMethod]
		public void TestStb()
		{
			int value = 125;
			uint memAddr = 1024;

			virtualMachine.SetRegisterValue(Registers.R1, (int)memAddr);
			virtualMachine.SetRegisterValue(Registers.R2, value);

			var jittedProgram = CreateProgram(
				new IFormatInstruction(OperationCodes.Stb.Code(), 1, 2, 0).AsInstruction());

			jitCompiler.RunJittedProgram(jittedProgram);
			Assert.AreEqual(value, virtualMachine.ReadWordFromMemory(memAddr));
		}

		/// <summary>
		/// Tests the slli instruction
		/// </summary>
		[TestMethod]
		public void TestSlli()
		{
			this.virtualMachine.SetRegisterValue(Registers.R8, 2);

			var jittedProgram = CreateProgram(
				new RFormatInstruction(
					OperationCodes.Slli.Code(),
					OperationXCodes.Slli | 2,
					Registers.R8.Number(),
					0,
					Registers.R9.Number()).AsInstruction());

			jitCompiler.RunJittedProgram(jittedProgram);

			Assert.AreEqual(8, this.virtualMachine.GetRegisterValue(Registers.R9));
		}

		/// <summary>
		/// Tests the srli instruction
		/// </summary>
		[TestMethod]
		public void TestSrli()
		{
			this.virtualMachine.SetRegisterValue(Registers.R8, 8);

			var jittedProgram = CreateProgram(
				new RFormatInstruction(
					OperationCodes.Srli.Code(),
					OperationXCodes.Srli | 2,
					Registers.R8.Number(),
					0,
					Registers.R9.Number()).AsInstruction());

			jitCompiler.RunJittedProgram(jittedProgram);

			Assert.AreEqual(2, this.virtualMachine.GetRegisterValue(Registers.R9));
		}

		/// <summary>
		/// Tests the cmpeq instruction
		/// </summary>
		[TestMethod]
		public void TestCmpeq()
		{
			this.virtualMachine.SetRegisterValue(Registers.R1, 2);
			this.virtualMachine.SetRegisterValue(Registers.R2, 4);

			var jittedProgram = CreateProgram(
				new RFormatInstruction(
					OperationCodes.Cmpeq.Code(),
					OperationXCodes.Cmpeq,
					Registers.R1.Number(),
					Registers.R2.Number(),
					Registers.R3.Number()).AsInstruction());

			jitCompiler.RunJittedProgram(jittedProgram);

			Assert.AreEqual(0, this.virtualMachine.GetRegisterValue(Registers.R3));

			this.virtualMachine.SetRegisterValue(Registers.R1, 4);
			this.virtualMachine.SetRegisterValue(Registers.R2, 4);

			jittedProgram = CreateProgram(
				new RFormatInstruction(
					OperationCodes.Cmpeq.Code(),
					OperationXCodes.Cmpeq,
					Registers.R1.Number(),
					Registers.R2.Number(),
					Registers.R3.Number()).AsInstruction());

			jitCompiler.RunJittedProgram(jittedProgram);

			Assert.AreEqual(1, this.virtualMachine.GetRegisterValue(Registers.R3));
		}

		/// <summary>
		/// Tests the cmpne instruction
		/// </summary>
		[TestMethod]
		public void TestCmpne()
		{
			this.virtualMachine.SetRegisterValue(Registers.R1, 2);
			this.virtualMachine.SetRegisterValue(Registers.R2, 4);

			var jittedProgram = CreateProgram(
				new RFormatInstruction(
					OperationCodes.Cmpne.Code(),
					OperationXCodes.Cmpne,
					Registers.R1.Number(),
					Registers.R2.Number(),
					Registers.R3.Number()).AsInstruction());

			jitCompiler.RunJittedProgram(jittedProgram);

			Assert.AreEqual(1, this.virtualMachine.GetRegisterValue(Registers.R3));

			this.virtualMachine.SetRegisterValue(Registers.R1, 4);
			this.virtualMachine.SetRegisterValue(Registers.R2, 4);

			jittedProgram = CreateProgram(
				new RFormatInstruction(
					OperationCodes.Cmpne.Code(),
					OperationXCodes.Cmpne,
					Registers.R1.Number(),
					Registers.R2.Number(),
					Registers.R3.Number()).AsInstruction());

			jitCompiler.RunJittedProgram(jittedProgram);

			Assert.AreEqual(0, this.virtualMachine.GetRegisterValue(Registers.R3));
		}

		/// <summary>
		/// Tests the cmpge instruction
		/// </summary>
		[TestMethod]
		public void TestCmpge()
		{
			this.virtualMachine.SetRegisterValue(Registers.R1, 4);
			this.virtualMachine.SetRegisterValue(Registers.R2, 2);

			var jittedProgram = CreateProgram(
				new RFormatInstruction(
					OperationCodes.Cmpge.Code(),
					OperationXCodes.Cmpge,
					Registers.R1.Number(),
					Registers.R2.Number(),
					Registers.R3.Number()).AsInstruction());

			jitCompiler.RunJittedProgram(jittedProgram);

			Assert.AreEqual(1, this.virtualMachine.GetRegisterValue(Registers.R3));

			this.virtualMachine.SetRegisterValue(Registers.R1, 4);
			this.virtualMachine.SetRegisterValue(Registers.R2, 4);

			jittedProgram = CreateProgram(
				new RFormatInstruction(
					OperationCodes.Cmpge.Code(),
					OperationXCodes.Cmpge,
					Registers.R1.Number(),
					Registers.R2.Number(),
					Registers.R3.Number()).AsInstruction());

			jitCompiler.RunJittedProgram(jittedProgram);

			Assert.AreEqual(1, this.virtualMachine.GetRegisterValue(Registers.R3));

			this.virtualMachine.SetRegisterValue(Registers.R1, 2);
			this.virtualMachine.SetRegisterValue(Registers.R2, 4);

			jittedProgram = CreateProgram(
				new RFormatInstruction(
					OperationCodes.Cmpge.Code(),
					OperationXCodes.Cmpge,
					Registers.R1.Number(),
					Registers.R2.Number(),
					Registers.R3.Number()).AsInstruction());

			jitCompiler.RunJittedProgram(jittedProgram);

			Assert.AreEqual(0, this.virtualMachine.GetRegisterValue(Registers.R3));
		}

		/// <summary>
		/// Tests the cmplt instruction
		/// </summary>
		[TestMethod]
		public void TestCmplt()
		{
			this.virtualMachine.SetRegisterValue(Registers.R1, 2);
			this.virtualMachine.SetRegisterValue(Registers.R2, 4);

			var jittedProgram = CreateProgram(
				new RFormatInstruction(
					OperationCodes.Cmplt.Code(),
					OperationXCodes.Cmplt,
					Registers.R1.Number(),
					Registers.R2.Number(),
					Registers.R3.Number()).AsInstruction());

			jitCompiler.RunJittedProgram(jittedProgram);

			Assert.AreEqual(1, this.virtualMachine.GetRegisterValue(Registers.R3));

			this.virtualMachine.SetRegisterValue(Registers.R1, 6);
			this.virtualMachine.SetRegisterValue(Registers.R2, 4);

			jittedProgram = CreateProgram(
				new RFormatInstruction(
					OperationCodes.Cmplt.Code(),
					OperationXCodes.Cmplt,
					Registers.R1.Number(),
					Registers.R2.Number(),
					Registers.R3.Number()).AsInstruction());

			jitCompiler.RunJittedProgram(jittedProgram);

			Assert.AreEqual(0, this.virtualMachine.GetRegisterValue(Registers.R3));

			this.virtualMachine.SetRegisterValue(Registers.R1, 4);
			this.virtualMachine.SetRegisterValue(Registers.R2, 4);

			jittedProgram = CreateProgram(
				new RFormatInstruction(
					OperationCodes.Cmplt.Code(),
					OperationXCodes.Cmplt,
					Registers.R1.Number(),
					Registers.R2.Number(),
					Registers.R3.Number()).AsInstruction());

			jitCompiler.RunJittedProgram(jittedProgram);

			Assert.AreEqual(0, this.virtualMachine.GetRegisterValue(Registers.R3));
		}
	}
}
