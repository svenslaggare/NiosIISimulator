using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;
using System.Reflection;

namespace NiosII_Simulator.Core.JIT
{
    /// <summary>
    /// Represents a full JIT compiler.
	/// This JIT compiler compiles entire programs.
    /// </summary>
    public class FullJITCompiler : BaseJITCompiler
    {

        #region Fields
		private FieldInfo pcField;																							//The program counter field in VMData
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new JIT compiler
        /// </summary>
        /// <param name="virtualMachine">The virtual machine</param>
		public FullJITCompiler(VirtualMachine virtualMachine)
			: base(virtualMachine)
        {
			this.pcField = typeof(InternalVMData).GetField("ProgramCounter",
				BindingFlags.Instance | BindingFlags.Public);

			#region Emiters

			#region Call/Return
			//The call instruction
			this.AddJFormatEmiter(OperationCodes.Call, (i, inst, genData) =>
			{
				var gen = genData.ILGenerator;
				
				//Load vm the data
				this.LoadVMData(gen);

				//int label = i + inst.Immediate / 4;
				int label = inst.Immediate;

				//Emit the call
				gen.EmitCall(OpCodes.Call, genData.FunctionTable[label], null);
			});

			//The ret instruction
			this.AddRFormatEmiter(OperationCodes.Ret, OperationXCodes.Ret, (i, inst, genData) =>
			{
				var gen = genData.ILGenerator;
				
				//Emit the return
				gen.Emit(OpCodes.Ret);
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
		/// Emites a push pc instructions
		/// </summary>
		/// <param name="generator">The Il generator</param>
		private void EmitPushPC(ILGenerator generator)
		{
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldfld, this.pcField);
		}
		#endregion

		#region JIT Methods
		/// <summary>
		/// Generates a jitted program for the given instructions
		/// </summary>
		/// <param name="name">The name of the program</param>
		/// <param name="instructions">The instructions</param>
		/// <param name="functionTable">The function entry points and their sizes</param>
		/// <returns>A reference to the jitted method or null if not jitted</returns>
		public MethodInfo GenerateProgram(string name, Instruction[] instructions, IDictionary<uint, int> functionTable)
		{
			var jittedProgram = new DynamicMethod(
				name,
				typeof(void),
				new Type[] { typeof(InternalVMData) },
				typeof(FullJITCompiler));

			var generatorData = new MethodGeneratorData(jittedProgram.GetILGenerator(), instructions.Length);
			
			//Generate func ref for all functions
			foreach (int currentFunc in functionTable.Keys)
			{
				//Create the new function
				var funcRef = new DynamicMethod(
					"func_at_" + currentFunc,
					typeof(void),
					new Type[] { typeof(InternalVMData) },
					typeof(FullJITCompiler));

				generatorData.FunctionTable.Add(currentFunc, funcRef);
			}

			for (int i = 0; i < instructions.Length; i++)
			{
				uint currentInstNum = (uint)i;

				//Check if function
				if (functionTable.ContainsKey(currentInstNum))
				{
					int funcSize = functionTable[currentInstNum];

					//Get the func ref and jit its body
					var funcRef = (DynamicMethod)generatorData.FunctionTable[i];

					Instruction[] funcBody = new Instruction[funcSize];
					Array.Copy(instructions, i, funcBody, 0, funcSize);
					this.GenerateFunctionBody(funcRef, funcBody);
				
					i += funcSize - 1;
				}
				else
				{
					generatorData.ILGenerator.MarkLabel(generatorData.GetLabel(i));
					this.EmitInstruction(i, instructions[i].Data, generatorData);
				}
			}

			jittedProgram.GetILGenerator().Emit(OpCodes.Ret);

			return jittedProgram;
		}

		/// <summary>
		/// Generates the body for the given function
		/// </summary>
		/// <param name="funcRef">The function reference</param>
		/// <param name="funcBody">The function body</param>
		private void GenerateFunctionBody(DynamicMethod funcRef, Instruction[] funcBody)
		{
			var generatorData = new MethodGeneratorData(funcRef.GetILGenerator(), funcBody.Length);
			var gen = generatorData.ILGenerator;

			for (int i = 0; i < funcBody.Length; i++)
			{
				generatorData.ILGenerator.MarkLabel(generatorData.GetLabel(i));
				this.EmitInstruction(i, funcBody[i].Data, generatorData);
			}
		}

		/// <summary>
		/// Runs the given jitted program
		/// </summary>
		/// <param name="program">The program</param>
		public void RunJittedProgram(MethodInfo program)
		{
			program.Invoke(null, new object[] { this.vmData });
		}
		#endregion

		#endregion

	}
}
