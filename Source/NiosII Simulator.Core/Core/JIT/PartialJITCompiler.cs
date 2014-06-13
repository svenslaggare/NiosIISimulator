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
    /// Represents a partial JIT compiler.
	/// This JIT compiler supports running along side the interpreter.
    /// </summary>
    public class PartialJITCompiler : BaseJITCompiler
    {

        #region Fields
		private readonly IDictionary<uint, JittedMethod> jittedMethods;														//The JITed methods
		private ISet<uint> failedJittedFunctions;																			//Functions that failed being jitted

		private MethodInfo setPcMethod;																						//The SetPC method in VMData
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new JIT compiler
        /// </summary>
        /// <param name="virtualMachine">The virtual machine</param>
        public PartialJITCompiler(VirtualMachine virtualMachine)
			: base(virtualMachine)
        {
			this.setPcMethod = typeof(InternalVMData).GetMethod("SetPC",
				BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Instance);

			this.jittedMethods = new Dictionary<uint, JittedMethod>();
			this.failedJittedFunctions = new HashSet<uint>();

			#region Emiters

			#region Call/Return
			//The ret instruction
			this.AddRFormatEmiter(OperationCodes.Ret, OperationXCodes.Ret, (i, inst, genData) =>
			{
				var gen = genData.ILGenerator;

				//Load the VM data
				this.LoadVMData(gen);

				//Push the value of the ra
				this.EmitRegRef(gen, Registers.RA.Number());
				this.EmitLoadRef(gen);

				//Emit the set pc
				this.EmitSetPC(gen);

				gen.Emit(OpCodes.Ret);
			});
			#endregion

			#endregion
		}
        #endregion

        #region Properties

        #endregion

		#region Help Classes
		private class JittedMethod
		{
			public MethodInfo MethodInfo { get; set; }
			public Action<InternalVMData> MethodReference { get; set; }
		}
		#endregion

		#region Methods

		#region Emit Helpers
		/// <summary>
		/// Emites the set program counter instructionns
		/// </summary>
		/// <param name="generator">The IL generator</param>
		private void EmitSetPC(ILGenerator generator)
		{
			generator.EmitCall(OpCodes.Call, this.setPcMethod, null);
		}

		/// <summary>
		/// Sets the PC to the given value
		/// </summary>
		/// <param name="value">The value</param>
		private void SetPC(int value)
		{
			this.virtualMachine.ProgramCounter = (uint)value;
		}
		#endregion

		#region JIT Methods
		/// <summary>
		/// Returns a reference to the given jitted method
		/// </summary>
		/// <param name="entryPoint">The entry point</param>
		/// <returns>The method or null</returns>
		public Action<InternalVMData> GetJittedMethod(uint entryPoint)
		{
			JittedMethod jittedMethod = null;
			bool foundMethod = this.jittedMethods.TryGetValue(entryPoint, out jittedMethod);
			
			if (foundMethod)
			{
				return jittedMethod.MethodReference;
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Creates a new jited method
		/// </summary>
		/// <param name="entryPoint">The entry point</param>
		/// <param name="methodBody">The method body</param>
		public void JitMethod(uint entryPoint, Instruction[] methodBody)
		{
			var methodInfo = this.GenerateMethod("jitted_method_at_" + entryPoint, methodBody, true);

			if (methodInfo != null)
			{
				var methodRef = (Action<InternalVMData>)methodInfo.CreateDelegate(typeof(Action<InternalVMData>));

				this.jittedMethods.Add(
					entryPoint,
					new JittedMethod()
					{
						MethodInfo = methodInfo,
						MethodReference = methodRef
					});
			}
			else
			{
				this.failedJittedFunctions.Add(entryPoint);
			}
		}

		/// <summary>
		/// Creates a new jitted method
		/// </summary>
		/// <param name="entryPoint">The entry point</param>
		public void JitMethod(uint entryPoint)
		{
			//Don't try to jit functions that already tried being jitted
			if (!this.failedJittedFunctions.Contains(entryPoint))
			{
				//Loop until a RET instruction is found
				uint currentAddr = entryPoint;
				Instruction[] methodBody = null;

				while (currentAddr < this.virtualMachine.ProgramEnd)
				{
					//Read the data
					int instructionData = this.virtualMachine.ReadWordFromMemory(currentAddr);

					//Read the opcode
					int opCode = instructionData & 0x3F;

					if (opCode == OperationCodes.Ret.Code())
					{
						//Decode it
						RFormatInstruction instruction = RFormatInstruction.Decode(instructionData);

						if (instruction.OpxCode == OperationXCodes.Ret)
						{
							//We have found a RET instruction
							uint size = currentAddr - entryPoint;

							//Convert the data in mem to instructions
							methodBody = new Instruction[(size / 4) + 1];

							for (int i = 0; i < methodBody.Length; i++)
							{
								methodBody[i] = new Instruction(
									this.virtualMachine.ReadWordFromMemory(entryPoint + (uint)i * 4));
							}
							break;
						}
					}

					currentAddr += 4;
				}

				this.JitMethod(entryPoint, methodBody);
			}
		}

		/// <summary>
		/// Genereates a jitted method for the given method body
		/// </summary>
		/// <param name="name">The name of the method</param>
		/// <param name="methodBody">The method body</param>
		/// <param name="isFunction">Indicates if the given method is a function or inline code.</param>
		/// <returns>A reference to the jited method or null if not jited</returns>
		public MethodInfo GenerateMethod(string name, Instruction[] methodBody, bool isFunction = false)
		{
			var jitedMethod = new DynamicMethod(
				name,
				typeof(void),
				new Type[] { typeof(InternalVMData) },
				typeof(PartialJITCompiler));

			var generatorData = new MethodGeneratorData(jitedMethod.GetILGenerator(), methodBody.Length);
			
			for (int i = 0; i < methodBody.Length; i++)
			{
				generatorData.ILGenerator.MarkLabel(generatorData.GetLabel(i));
				bool emited = this.EmitInstruction(i, methodBody[i].Data, generatorData);

				if (!emited)
				{
					return null;
				}
			}

			if (!isFunction)
			{
				jitedMethod.GetILGenerator().Emit(OpCodes.Ret);
			}

			return jitedMethod;
		}

		/// <summary>
		/// Invokes the given jitted method
		/// </summary>
		/// <param name="method">The method</param>
		public void InvokeJitedMethod(MethodInfo method)
		{
			method.Invoke(null, new object[] { this.vmData });
		}

		/// <summary>
		/// Invokes the given JITed method
		/// </summary>
		/// <param name="method">The method</param>
		public void InvokeJitedMethod(Action<InternalVMData> method)
		{
			method(this.vmData);
		}
		#endregion

		#endregion

	}
}
