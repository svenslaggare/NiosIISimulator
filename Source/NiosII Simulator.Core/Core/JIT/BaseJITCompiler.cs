using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace NiosII_Simulator.Core.JIT
{
	/// <summary>
	/// Emits an jitted instruction for the given generator
	/// </summary>
	/// <param name="index">The index of the instruction</param>
	/// <param name="instruction">The instruction</param>
	/// <param name="generator">The generator data</param>
	public delegate void IFormatInstructionEmiter(int index, IFormatInstruction instruction, MethodGeneratorData generatorData);

	/// <summary>
	/// Emits an jitted instruction for the given generator
	/// </summary>
	/// <param name="index">The index of the instruction</param>
	/// <param name="instruction">The instruction</param>
	/// <param name="generator">The generator data</param>
	public delegate void RFormatInstructionEmiter(int index, RFormatInstruction instruction, MethodGeneratorData generatorData);

	/// <summary>
	/// Emits an jitted instruction for the given generator
	/// </summary>
	/// <param name="index">The index of the instruction</param>
	/// <param name="instruction">The instruction</param>
	/// <param name="generator">The generator data</param>
	public delegate void JFormatInstructionEmiter(int index, JFormatInstruction instruction, MethodGeneratorData generatorData);

	/// <summary>
	/// Represents an instruction emiter
	/// </summary>
	internal class InstructionEmiter
	{
		/// <summary>
		/// The instruction format
		/// </summary>
		public InstructionFormat InstructionFormat { get; set; }

		/// <summary>
		/// The I-format emiter
		/// </summary>
		public IFormatInstructionEmiter IFormatInstructionEmiter { get; set; }

		/// <summary>
		/// The R-format emiter (because a R-format instructions can have some op-codes)
		/// </summary>
		public Dictionary<int, RFormatInstructionEmiter> RFormatInstructionEmiters { get; set; }

		/// <summary>
		/// The R-format opx maskers (for shift instructions)
		/// </summary>
		public List<int> RFormatOpxMaskers { get; set; }

		/// <summary>
		/// The J-format emiter
		/// </summary>
		public JFormatInstructionEmiter JFormatInstructionEmiter { get; set; }
	}

	/// <summary>
	/// Contains data for a method generator
	/// </summary>
	public class MethodGeneratorData
	{
		/// <summary>
		/// The IL generator
		/// </summary>
		public ILGenerator ILGenerator { get; private set; }

		private readonly IDictionary<int, Label> labels;
		private readonly IDictionary<int, MethodInfo> functionTable;

		/// <summary>
		/// Creates a new method generator data container
		/// </summary>
		/// <param name="generator">The IL generator</param>
		/// <param name="numInstructions">The number of instructions</param>
		public MethodGeneratorData(ILGenerator generator, int numInstructions)
		{
			this.ILGenerator = generator;
			this.labels = new Dictionary<int, Label>();
			this.functionTable = new Dictionary<int, MethodInfo>();

			for (int i = 0; i < numInstructions; i++)
			{
				this.labels.Add(i, generator.DefineLabel());
			}
		}

		/// <summary>
		/// The function entry points and their sizes (in instructions)
		/// </summary>
		public IDictionary<int, MethodInfo> FunctionTable
		{
			get { return this.functionTable; }
		}

		/// <summary>
		/// Returns the given label
		/// </summary>
		/// <param name="labelName">The name of the label</param>
		public Label GetLabel(int labelName)
		{
			Label label = new Label();
			bool exists = this.labels.TryGetValue(labelName, out label);

			if (!exists)
			{
				label = this.ILGenerator.DefineLabel();
				this.labels.Add(labelName, label);
			}

			return label;
		}
	}

	/// <summary>
	/// Represents a base for JIT compilers
	/// </summary>
	public abstract class BaseJITCompiler
	{

		#region Fields
		protected readonly VirtualMachine virtualMachine;                                                                   //The virtual machine
		protected readonly InternalVMData vmData;																		    //The internal VM data

		protected readonly FieldInfo registerField;																			//The register field in VMData
		protected readonly FieldInfo memoryField;																		    //the memory field in VMData

		//Helper methods for reading & writing from main memory
		protected readonly MethodInfo convertToInt;
		protected readonly MethodInfo setWordMemAddr;

		private readonly IDictionary<OperationCodes, InstructionEmiter> instructionEmiters;									//The instruction emiters
		#endregion

		#region Constructors
		/// <summary>
        /// Creates a new JIT compiler
        /// </summary>
        /// <param name="virtualMachine">The virtual machine</param>
		public BaseJITCompiler(VirtualMachine virtualMachine)
        {
            this.virtualMachine = virtualMachine;
			this.vmData = this.virtualMachine.GetInternalData();

			this.registerField = typeof(InternalVMData).GetField("Registers",
				BindingFlags.Instance | BindingFlags.Public);

			this.memoryField = typeof(InternalVMData).GetField("Memory",
				BindingFlags.Instance | BindingFlags.Public);

			this.convertToInt = typeof(BaseJITCompiler).GetMethod("ConvertToInt", BindingFlags.NonPublic | BindingFlags.Static);
			this.setWordMemAddr = typeof(BaseJITCompiler).GetMethod("SetWordMemAddr", BindingFlags.NonPublic | BindingFlags.Static);

			this.instructionEmiters = new Dictionary<OperationCodes, InstructionEmiter>();

			#region Emiters

			#region Generators
			//Creates R-format emiters
			Func<OpCode, RFormatInstructionEmiter> rFormatEmiterGenerator = opCode =>
			{
				return (i, inst, genData) =>
				{
					var gen = genData.ILGenerator;

					//Push reg C
					this.EmitRegRef(gen, inst.RegisterC);

					//Load the value of reg A
					this.EmitRegRef(gen, inst.RegisterA);
					this.EmitLoadRef(gen);

					//Load the value of reg B
					this.EmitRegRef(gen, inst.RegisterB);
					this.EmitLoadRef(gen);

					//Apply the operation
					gen.Emit(opCode);

					//Store in reg C
					this.EmitSaveRef(gen);
				};
			};

			Func<OpCode, IFormatInstructionEmiter> iFormatEmiterGenerator = opCode =>
			{
				return (i, inst, genData) =>
				{
					var gen = genData.ILGenerator;

					//Push reg B
					this.EmitRegRef(gen, inst.RegisterB);

					//Load the value of reg A
					this.EmitRegRef(gen, inst.RegisterA);
					this.EmitLoadRef(gen);

					//Push the imm value
					gen.Emit(OpCodes.Ldc_I4, inst.SignedImmediate);

					//Apply the operation
					gen.Emit(opCode);

					//Store in reg B
					this.EmitSaveRef(gen);
				};
			};
			#endregion

			#region Arithmetic
			//The addi instruction
			this.AddIFormatEmiter(OperationCodes.Addi, iFormatEmiterGenerator(OpCodes.Add));

			//The add instruction
			this.AddRFormatEmiter(OperationCodes.Add, OperationXCodes.Add, rFormatEmiterGenerator(OpCodes.Add));

			//The sub instruction
			this.AddRFormatEmiter(OperationCodes.Sub, OperationXCodes.Sub, rFormatEmiterGenerator(OpCodes.Sub));
            #endregion

			#region Logic
			//The and instruction
			this.AddRFormatEmiter(OperationCodes.And, OperationXCodes.And, rFormatEmiterGenerator(OpCodes.And));

			//The andi instruction
			this.AddIFormatEmiter(OperationCodes.Andi, iFormatEmiterGenerator(OpCodes.And));

			//The or instruction
			this.AddRFormatEmiter(OperationCodes.Or, OperationXCodes.Or, rFormatEmiterGenerator(OpCodes.Or));

			//The ori instruction
			this.AddIFormatEmiter(OperationCodes.Ori, iFormatEmiterGenerator(OpCodes.Or));

			//The xor instruction
			this.AddRFormatEmiter(OperationCodes.Xor, OperationXCodes.Xor, rFormatEmiterGenerator(OpCodes.Xor));

			//The xori instruction
			this.AddIFormatEmiter(OperationCodes.Xori, iFormatEmiterGenerator(OpCodes.Xor));

			//The nor instruction
			this.AddRFormatEmiter(OperationCodes.Nor, OperationXCodes.Nor, (i, inst, genData) =>
			{
				var gen = genData.ILGenerator;

				//Push reg C
				this.EmitRegRef(gen, inst.RegisterC);

				//Load the value of reg A
				this.EmitRegRef(gen, inst.RegisterA);
				this.EmitLoadRef(gen);

				//Load the value of reg B
				this.EmitRegRef(gen, inst.RegisterB);
				this.EmitLoadRef(gen);

				//OR them
				gen.Emit(OpCodes.Or);

				//Invert them
				gen.Emit(OpCodes.Not);

				//Store in reg C
				this.EmitSaveRef(gen);
			});
			#endregion

			#region Shift
			//The sll instruction
			this.AddRFormatEmiter(OperationCodes.Sll, OperationXCodes.Sll, rFormatEmiterGenerator(OpCodes.Shl));

			//The sll instruction
			this.AddRFormatEmiter(OperationCodes.Srl, OperationXCodes.Srl, rFormatEmiterGenerator(OpCodes.Shr));
			#endregion

			#region Branch
			//The br instruction
			this.AddIFormatEmiter(OperationCodes.Br, (i, inst, genData) =>
			{
				var gen = genData.ILGenerator;

				//Emit the jump
				int label = i + (inst.SignedImmediate / 4) + 1;
				gen.Emit(OpCodes.Br, genData.GetLabel(label));
			});
			
			//Create branch emiters
			Func<OpCode, IFormatInstructionEmiter> branchEmiterGenerator = opCode =>
			{
				return (i, inst, genData) =>
				{
					var gen = genData.ILGenerator;

					//Load the value of reg A
					this.EmitRegRef(gen, inst.RegisterA);
					this.EmitLoadRef(gen);

					//Load the value of reg B
					this.EmitRegRef(gen, inst.RegisterB);
					this.EmitLoadRef(gen);

					//Emit the jump
					int label = (i + inst.SignedImmediate / 4) + 1;
					gen.Emit(opCode, genData.GetLabel(label));
				};
			};

			//The beq instruction
			this.AddIFormatEmiter(OperationCodes.Beq, branchEmiterGenerator(OpCodes.Beq));

			//The bne instruction
			this.AddIFormatEmiter(OperationCodes.Bne, branchEmiterGenerator(OpCodes.Bne_Un));

			//The bge instruction
			this.AddIFormatEmiter(OperationCodes.Bge, branchEmiterGenerator(OpCodes.Bge));

			//The blt instruction
			this.AddIFormatEmiter(OperationCodes.Blt, branchEmiterGenerator(OpCodes.Blt));
			#endregion

			#region Memory
			//The ldw instruction
			this.AddIFormatEmiter(OperationCodes.Ldw, (i, inst, genData) =>
			{
				var gen = genData.ILGenerator;

				//Emit reference to reg B
				this.EmitRegRef(gen, inst.RegisterB);

				for (int offset = 0; offset < 4; offset++)
				{
					//Emit the memory reference
					this.EmitMemRef(gen);

					//Load the value of reg A (base address)
					this.EmitRegRef(gen, inst.RegisterA);
					this.EmitLoadRef(gen);

					//Push the imm value (offset)
					gen.Emit(OpCodes.Ldc_I4, inst.SignedImmediate + offset);

					//Compute the effective address
					gen.Emit(OpCodes.Add);

					//Load from memory
					this.EmitLoadMem(gen);
				}

				//Convert the top 4 bytes to a int
				gen.EmitCall(OpCodes.Call, this.convertToInt, null);

				//Store in reg B
				this.EmitSaveRef(gen);
			});

			//The stw instruction
			this.AddIFormatEmiter(OperationCodes.Stw, (i, inst, genData) =>
			{
				var gen = genData.ILGenerator;

				//Push the vm data
				this.LoadVMData(gen);

				//Load the value of reg A (base addr)
				this.EmitRegRef(gen, inst.RegisterA);
				this.EmitLoadRef(gen);

				//Push the imm value (offset)
				gen.Emit(OpCodes.Ldc_I4, inst.SignedImmediate);

				//Compute the effective address
				gen.Emit(OpCodes.Add);

				//Load the value of reg B (value)
				this.EmitRegRef(gen, inst.RegisterB);
				this.EmitLoadRef(gen);

				//Store in memory
				gen.EmitCall(OpCodes.Call, this.setWordMemAddr, null);
			});

			//The ldb instruction
			this.AddIFormatEmiter(OperationCodes.Ldb, (i, inst, genData) =>
			{
				var gen = genData.ILGenerator;

				//Emit reference to reg B
				this.EmitRegRef(gen, inst.RegisterB);

				//Emit the memory reference
				this.EmitMemRef(gen);

				//Load the value of reg A (base address)
				this.EmitRegRef(gen, inst.RegisterA);
				this.EmitLoadRef(gen);

				//Push the imm value (offset)
				gen.Emit(OpCodes.Ldc_I4, inst.SignedImmediate);

				//Compute the effective address
				gen.Emit(OpCodes.Add);

				//Load from memory
				this.EmitLoadMem(gen);

				//Store in reg B
				this.EmitSaveRef(gen);
			});

			//The stb instruction
			this.AddIFormatEmiter(OperationCodes.Stb, (i, inst, genData) =>
			{
				var gen = genData.ILGenerator;

				//Emit the reference to the memory field
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldfld, this.memoryField);

				//Load the value of reg A (base addr)
				this.EmitRegRef(gen, inst.RegisterA);
				this.EmitLoadRef(gen);

				//Push the imm value (offset)
				gen.Emit(OpCodes.Ldc_I4, inst.SignedImmediate);

				//Compute the effective address
				gen.Emit(OpCodes.Add);

				//Load the value of reg B (value)
				this.EmitRegRef(gen, inst.RegisterB);
				this.EmitLoadRef(gen);

				//Store in memory
				gen.Emit(OpCodes.Stelem_I1);
			});
			#endregion

			#endregion
		}
		#endregion

		#region Properties
		
		#endregion

		#region Methods

		#region Emit Helpers
		/// <summary>
		/// Emites a load VM data instruction
		/// </summary>
		/// <param name="generator">The IL generator</param>
		protected void LoadVMData(ILGenerator generator)
		{
			generator.Emit(OpCodes.Ldarg_0);
		}

		/// <summary>
		/// Emites a reference for the given register
		/// </summary>
		/// <param name="generator">The IL generator</param>
		/// <param name="regNum">The reg num</param>
		protected void EmitRegRef(ILGenerator generator, int regNum)
		{
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldfld, this.registerField);
			generator.Emit(OpCodes.Ldc_I4, regNum);
		}

		/// <summary>
		/// Emites a save ref instruction
		/// </summary>
		/// <param name="generator">The IL generator</param>
		protected void EmitSaveRef(ILGenerator generator)
		{
			generator.Emit(OpCodes.Stelem_I4);
		}

		/// <summary>
		/// Emites a load ref instruction
		/// </summary>
		/// <param name="generator">The IL generator</param>
		protected void EmitLoadRef(ILGenerator generator)
		{
			generator.Emit(OpCodes.Ldelem_I4);
		}

		/// <summary>
		/// Emites a reference to the memory
		/// </summary>
		/// <param name="generator">The IL generator</param>
		protected void EmitMemRef(ILGenerator generator)
		{
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldfld, this.memoryField);
		}

		/// <summary>
		/// Emites a store memory instruction
		/// </summary>
		/// <param name="generator">The IL generator</param>
		protected void EmitStoreMem(ILGenerator generator)
		{
			generator.Emit(OpCodes.Stelem_I4);
		}

		/// <summary>
		/// Emites a load memory instruction
		/// </summary>
		/// <param name="generator">The IL generator</param>
		protected void EmitLoadMem(ILGenerator generator)
		{
			generator.Emit(OpCodes.Ldelem_I1);
		}

		/// <summary>
		/// Converts the given byts to an int
		/// </summary>
		protected static int ConvertToInt(byte byte1, byte byte2, byte byte3, byte byte4)
		{
			return byte1 | (byte2 << 8) | (byte3 << 16) | (byte4 << 24);
		}

		/// <summary>
		/// Sets the mem address to the given int (word)
		/// </summary>
		protected static void SetWordMemAddr(InternalVMData vmData, int address, int value)
		{
			vmData.Memory[address + 3] = (byte)(value >> 24);
			vmData.Memory[address + 2] = (byte)(value >> 16);
			vmData.Memory[address + 1] = (byte)(value >> 8);
			vmData.Memory[address] = (byte)(value);
		}
		#endregion

		#region Emiters
		/// <summary>
		/// Adds the given I-format emiter to the list of instruction executors
		/// </summary>
		/// <param name="opCode">The op code</param>
		/// <param name="emiter">The emiter</param>
		public void AddIFormatEmiter(OperationCodes opCode, IFormatInstructionEmiter emiter)
		{
			if (!this.instructionEmiters.ContainsKey(opCode))
			{
				//Create the emiter
				InstructionEmiter newExecutor = new InstructionEmiter()
				{
					InstructionFormat = InstructionFormat.IFormat,
					IFormatInstructionEmiter = emiter,
				};

				//Add it
				this.instructionEmiters.Add(opCode, newExecutor);
			}
		}

		/// <summary>
		/// Adds the given R-format emiter to the list of instruction emiter
		/// </summary>
		/// <param name="opCode">The op code</param>
		/// <param name="opxCode">The opx code</param>
		/// <param name="emiter">The emiter</param>
		/// <param name="opxMask">The opx mask</param>
		public void AddRFormatEmiter(OperationCodes opCode, int opxCode, RFormatInstructionEmiter emiter, int opxMask = -1)
		{
			InstructionEmiter newEmiter = null;

			//Check if an executor exist
			if (this.instructionEmiters.ContainsKey(opCode))
			{
				//Get it
				newEmiter = this.instructionEmiters[opCode];
			}
			else
			{
				//Create an new one
				newEmiter = new InstructionEmiter()
				{
					InstructionFormat = InstructionFormat.RFormat,
					RFormatInstructionEmiters = new Dictionary<int, RFormatInstructionEmiter>(),
					RFormatOpxMaskers = new List<int>()
				};

				this.instructionEmiters.Add(opCode, newEmiter);
			}

			//Check if an opx code executor exists
			if (!newEmiter.RFormatInstructionEmiters.ContainsKey(opxCode))
			{
				newEmiter.RFormatInstructionEmiters.Add(opxCode, emiter);

				if (opxMask != -1)
				{
					newEmiter.RFormatOpxMaskers.Add(opxMask);
				}
			}
		}

		/// <summary>
		/// Adds the given J-format emiter to the list of instruction emiters
		/// </summary>
		/// <param name="opCode">The op code</param>
		/// <param name="emiter">The emiter</param>
		public void AddJFormatEmiter(OperationCodes opCode, JFormatInstructionEmiter emiter)
		{
			if (!this.instructionEmiters.ContainsKey(opCode))
			{
				//Create the emiter
				InstructionEmiter newEmiter = new InstructionEmiter()
				{
					InstructionFormat = InstructionFormat.JFormat,
					JFormatInstructionEmiter = emiter,
				};

				//Add it
				this.instructionEmiters.Add(opCode, newEmiter);
			}
		}

		/// <summary>
		/// Decodes and emites the given I-format instruction
		/// </summary>
		/// <param name="index">The index of the instruction</param>
		/// <param name="instructionData">The instruction data</param>
		/// <param name="generatorData">The generator data</param>
		/// <param name="emiter">The emiter</param>
		private void DecodeAndEmitIFormat(int index, int instructionData, MethodGeneratorData generatorData, InstructionEmiter emiter)
		{
			//Decode and emit it
			IFormatInstruction instruction = IFormatInstruction.Decode(instructionData);
			emiter.IFormatInstructionEmiter(index, instruction, generatorData);
		}

		/// <summary>
		/// Decodes and emits the given R-format instruction
		/// </summary>
		/// <param name="index">The index of the instruction</param>
		/// <param name="instructionData">The instruction data</param>
		/// <param name="generatorData">The method generator data</param>
		/// <param name="emiter">The executor</param>
		private void DecodeAndEmitRFormat(int index, int instructionData, MethodGeneratorData generatorData, InstructionEmiter emiter)
		{
			//Decode it
			RFormatInstruction instruction = RFormatInstruction.Decode(instructionData);

			//Find the R-format emiter
			RFormatInstructionEmiter opxEmiter = null;

			//Check if maskers exists
			if (emiter.RFormatOpxMaskers.Count == 0)
			{
				if (emiter.RFormatInstructionEmiters.TryGetValue(instruction.OpxCode, out opxEmiter))
				{
					opxEmiter(index, instruction, generatorData);
				}
			}
			else
			{
				//Loop until an executor is found
				foreach (int currentMask in emiter.RFormatOpxMaskers)
				{
					int opxCode = instruction.OpxCode & currentMask;

					if (emiter.RFormatInstructionEmiters.TryGetValue(opxCode, out opxEmiter))
					{
						opxEmiter(index, instruction, generatorData);
						break;
					}
				}
			}
		}

		/// <summary>
		/// Decodes and emites the given J-format instruction
		/// </summary>
		/// <param name="index">The index of the instruction</param>
		/// <param name="instructionData">The instruction data</param>
		/// <param name="generatorData">The generator data</param>
		/// <param name="emiter">The emiter</param>
		private void DecodeAndEmitJFormat(int index, int instructionData, MethodGeneratorData generatorData, InstructionEmiter emiter)
		{
			//Decode and execute it
			JFormatInstruction instruction = JFormatInstruction.Decode(instructionData);
			emiter.JFormatInstructionEmiter(index, instruction, generatorData);
		}

		/// <summary>
		/// Emites the given instruction
		/// </summary>
		/// <param name="index">The index of the instruction</param>
		/// <param name="instructionData">The data of the instruction</param>
		/// <param name="generatorData">The generator data</param>
		/// <returns>True if emited else false</returns>
		public bool EmitInstruction(int index, int instructionData, MethodGeneratorData generatorData)
		{
			//Read the opcode
			OperationCodes opCode = (OperationCodes)(instructionData & 0x3F);

			//Find the emiter
			InstructionEmiter emiter = null;

			if (this.instructionEmiters.TryGetValue(opCode, out emiter))
			{
				switch (emiter.InstructionFormat)
				{
					case InstructionFormat.IFormat:
						this.DecodeAndEmitIFormat(index, instructionData, generatorData, emiter);
						break;

					case InstructionFormat.RFormat:
						this.DecodeAndEmitRFormat(index, instructionData, generatorData, emiter);
						break;

					case InstructionFormat.JFormat:
						this.DecodeAndEmitJFormat(index, instructionData, generatorData, emiter);
						break;
				}

				return true;
			}
			else
			{
				return false;
			}
		}
		#endregion

		#endregion

	}
}
