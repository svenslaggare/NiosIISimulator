using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiosII_Simulator.Core.Exceptions;
using NiosII_Simulator.Core.JIT;

namespace NiosII_Simulator.Core
{
    /// <summary>
    /// Execute the given I-format instruction
    /// </summary>
    /// <param name="instruction">An instruction in the I-format</param>
    public delegate void IFormatInstructionExecutor(IFormatInstruction instruction);

    /// <summary>
    /// Execute the given R-format instruction
    /// </summary>
    /// <param name="instruction">An instruction in the R-format</param>
    public delegate void RFormatInstructionExecutor(RFormatInstruction instruction);

    /// <summary>
    /// Execute the given J-format instruction
    /// </summary>
    /// <param name="instruction">An instruction in the J-format</param>
    public delegate void JFormatInstructionExecutor(JFormatInstruction instruction);

	/// <summary>
	/// The instruction formats
	/// </summary>
	internal enum InstructionFormat { IFormat, RFormat, JFormat }

	/// <summary>
	/// Repesents an instruction executor
	/// </summary>
    internal class InstructionExecutor
    {
        /// <summary>
        /// The instruction format
        /// </summary>
        public InstructionFormat InstructionFormat { get; set; }

		/// <summary>
		/// The I-format executor
		/// </summary>
        public IFormatInstructionExecutor IFormatInstructionExecutor { get; set; }

        /// <summary>
		/// The R-format executors (because a R-format instructions can have multiple instructions with the same OP-code)
        /// </summary>
        public Dictionary<int, RFormatInstructionExecutor> RFormatInstructionExecutors { get; set; }

		/// <summary>
		/// The R-format opx maskers (for shift instructions)
		/// </summary>
        public List<int> RFormatOpxMaskers { get; set; }

        /// <summary>
        /// The J-format executor
        /// </summary>
        public JFormatInstructionExecutor JFormatInstructionExecutor { get; set; }
    }

	/// <summary>
	/// Represents an internal representation of the VM
	/// </summary>
	public class InternalVMData
	{
		/// <summary>
		/// The value of the program counter
		/// </summary>
		public int ProgramCounter;

		/// <summary>
		/// The registers
		/// </summary>
		public int[] Registers;

		/// <summary>
		/// The main memory
		/// </summary>
		public byte[] Memory;

		/// <summary>
		/// The VM
		/// </summary>
		public VirtualMachine VirtualMachine;

		/// <summary>
		/// Sets the PC to the given value
		/// </summary>
		/// <param name="value">The new value of the PC</param>
		public void SetPC(int value)
		{
			this.VirtualMachine.ProgramCounter = (uint)value;
		}
	}

    /// <summary>
    /// Represents a Nios II virtual machine
    /// </summary>
    public class VirtualMachine
    {

        #region Fields
        private readonly int[] registers;						                                                                //The register values
        private readonly byte[] memory;                                                                                         //The main memory

        private uint entryPoint;                                                                                                //The entry point
        private uint textArea;                                                                                                  //Where the program codes is stored
        private uint programEnd;                                                                                                //Where the program code ends

        private uint dataArea;                                                                                                  //Where the data is stored
        private readonly int heapStart;                                                                                         //Where the heap start

        private uint programCounter;                                                                                            //The program code
        private Instruction instructionRegister;                                                                                //The loaded instruction

        private IDictionary<int, InstructionExecutor> instructionExecutors;                                                     //The instruction executors

        private readonly IDictionary<uint, Action> systemCalls;                                                                 //The system calls
        private readonly IDictionary<string, uint> systemCallLinker;                                                            //The system call linker

		private PartialJITCompiler jitCompiler;																					//The JIT compiler
		private bool enableJit;																									//Indicates if the jitter is enabled
		#endregion

        #region Constructors
        /// <summary>
        /// Creates a new virtual machine
        /// </summary>
        /// <param name="memorySize">The size of the main memory in bytes</param>
        public VirtualMachine(int memorySize = 8 * 1024 * 1024)
        {
			//Init the registers
			this.registers = new int[32];
           
            //Create the main memory
            this.memory = new byte[memorySize];
            this.heapStart = (int)(0.5 * memorySize);

            //Add executors
            this.instructionExecutors = new Dictionary<int, InstructionExecutor>();

			#region Instruction Executors

			#region Arithmetic
			//The addi instruction
            this.AddIFormatExecutor(OperationCodes.Addi.Code(), inst =>
            {
                //Get the value of reg A
                int regA = this.GetRegisterValue(this.GetRegister(inst.RegisterA).Value);

                //Get the destination register
                Registers regB = this.GetRegister(inst.RegisterB).Value;

                //Add them
                int value = regA + inst.SignedImmediate;

                //Store at the register
                this.SetRegisterValue(regB, value);
            });

            //The add instruction
            this.AddRFormatExecutor(OperationCodes.Add.Code(), OperationXCodes.Add, inst =>
            {
                //Get the value of reg A & B
                int regA = this.GetRegisterValue(this.GetRegister(inst.RegisterA).Value);
                int regB = this.GetRegisterValue(this.GetRegister(inst.RegisterB).Value);

                //Get the destination register
                Registers regC = this.GetRegister(inst.RegisterC).Value;

                //Add them
                int value = regA + regB;

                //Store at the register
                this.SetRegisterValue(regC, value);
            });

            //The sub instruction
            this.AddRFormatExecutor(OperationCodes.Sub.Code(), OperationXCodes.Sub, inst =>
            {
                //Get the value of reg A & B
                int regA = this.GetRegisterValue(this.GetRegister(inst.RegisterA).Value);
                int regB = this.GetRegisterValue(this.GetRegister(inst.RegisterB).Value);

                //Get the destination register
                Registers regC = this.GetRegister(inst.RegisterC).Value;

                //Subtract them
                int value = regA - regB;

                //Store at the register
                this.SetRegisterValue(regC, value);
            });
            #endregion

            #region Logic
            //The and instruction
            this.AddRFormatExecutor(OperationCodes.And.Code(), OperationXCodes.And, inst =>
            {
                //Get the value of reg A & B
                int regA = this.GetRegisterValue(this.GetRegister(inst.RegisterA).Value);
                int regB = this.GetRegisterValue(this.GetRegister(inst.RegisterB).Value);

                //Get the destination register
                Registers regC = this.GetRegister(inst.RegisterC).Value;

				//AND them
                int value = regA & regB;

                //Store at the register
                this.SetRegisterValue(regC, value);
            });

            //The andi instruction
            this.AddIFormatExecutor(OperationCodes.Andi.Code(), inst =>
            {
                //Get the value of reg A
                int regA = this.GetRegisterValue(this.GetRegister(inst.RegisterA).Value);

                //Get the destination register
                Registers regB = this.GetRegister(inst.RegisterB).Value;

				//AND them
                int value = regA & inst.Immediate;

                //Store at the register
                this.SetRegisterValue(regB, value);
            });

            //The andhi instruction
            this.AddIFormatExecutor(OperationCodes.Andhi.Code(), inst =>
            {
                //Get the value of reg A
                int regA = this.GetRegisterValue(this.GetRegister(inst.RegisterA).Value);

                //Get the destination register
                Registers regB = this.GetRegister(inst.RegisterB).Value;

                //AND them
                int value = regA & (inst.Immediate << 16);

                //Store at the register
                this.SetRegisterValue(regB, value);
            });

            //The or instruction
            this.AddRFormatExecutor(OperationCodes.Or.Code(), OperationXCodes.Or, inst =>
            {
                //Get the value of reg A & B
                int regA = this.GetRegisterValue(this.GetRegister(inst.RegisterA).Value);
                int regB = this.GetRegisterValue(this.GetRegister(inst.RegisterB).Value);

                //Get the destination register
                Registers regC = this.GetRegister(inst.RegisterC).Value;

                //OR them
                int value = regA | regB;

                //Store at the register
                this.SetRegisterValue(regC, value);
            });

            //The ori instruction
            this.AddIFormatExecutor(OperationCodes.Ori.Code(), inst =>
            {
                //Get the value of reg A
                int regA = this.GetRegisterValue(this.GetRegister(inst.RegisterA).Value);

                //Get the destination register
                Registers regB = this.GetRegister(inst.RegisterB).Value;

                //OR them
                int value = regA | inst.Immediate;

                //Store at the register
                this.SetRegisterValue(regB, value);
            });

            //The orhi instruction
            this.AddIFormatExecutor(OperationCodes.Orhi.Code(), inst =>
            {
                //Get the value of reg A
                int regA = this.GetRegisterValue(this.GetRegister(inst.RegisterA).Value);

                //Get the destination register
                Registers regB = this.GetRegister(inst.RegisterB).Value;

                //OR them
                int value = regA | (inst.Immediate << 16);

                //Store at the register
                this.SetRegisterValue(regB, value);
            });

            //The xor instruction
            this.AddRFormatExecutor(OperationCodes.Xor.Code(), OperationXCodes.Xor, inst =>
            {
                //Get the value of reg A & B
                int regA = this.GetRegisterValue(this.GetRegister(inst.RegisterA).Value);
                int regB = this.GetRegisterValue(this.GetRegister(inst.RegisterB).Value);

                //Get the destination register
                Registers regC = this.GetRegister(inst.RegisterC).Value;

                //XOR them
                int value = regA ^ regB;

                //Store at the register
                this.SetRegisterValue(regC, value);
            });

            //The xori instruction
            this.AddIFormatExecutor(OperationCodes.Xori.Code(), inst =>
            {
                //Get the value of reg A
                int regA = this.GetRegisterValue(this.GetRegister(inst.RegisterA).Value);

                //Get the destination register
                Registers regB = this.GetRegister(inst.RegisterB).Value;

                //XOR them
                int value = regA ^ inst.Immediate;

                //Store at the register
                this.SetRegisterValue(regB, value);
            });

            //The xorhi instruction
            this.AddIFormatExecutor(OperationCodes.Xorhi.Code(), inst =>
            {
                //Get the value of reg A
                int regA = this.GetRegisterValue(this.GetRegister(inst.RegisterA).Value);

                //Get the destination register
                Registers regB = this.GetRegister(inst.RegisterB).Value;

                //XOR them
                int value = regA ^ (inst.Immediate << 16);

                //Store at the register
                this.SetRegisterValue(regB, value);
            });

            //The nor instruction
            this.AddRFormatExecutor(OperationCodes.Nor.Code(), OperationXCodes.Nor, inst =>
            {
                //Get the value of reg A & B
                int regA = this.GetRegisterValue(this.GetRegister(inst.RegisterA).Value);
                int regB = this.GetRegisterValue(this.GetRegister(inst.RegisterB).Value);

                //Get the destination register
                Registers regC = this.GetRegister(inst.RegisterC).Value;

                //NOR them
                int value = ~(regA | regB);

                //Store at the register
                this.SetRegisterValue(regC, value);
            });
            #endregion

            #region Shift
            //The sll instruction
            this.AddRFormatExecutor(OperationCodes.Sll.Code(), OperationXCodes.Sll, inst =>
            {
                //Get the value of reg A & B
                int regA = this.GetRegisterValue(this.GetRegister(inst.RegisterA).Value);
                int regB = this.GetRegisterValue(this.GetRegister(inst.RegisterB).Value);

                //Get the destination register
                Registers regC = this.GetRegister(inst.RegisterC).Value;

                //Shift them
                int value = regA << regB;

                //Store at the register
                this.SetRegisterValue(regC, value);
            });

            //The srl instruction
            this.AddRFormatExecutor(OperationCodes.Srl.Code(), OperationXCodes.Srl, inst =>
            {
                //Get the value of reg A & B
                int regA = this.GetRegisterValue(this.GetRegister(inst.RegisterA).Value);
                int regB = this.GetRegisterValue(this.GetRegister(inst.RegisterB).Value);

                //Get the destination register
                Registers regC = this.GetRegister(inst.RegisterC).Value;

                //Shift them
                int value = regA >> regB;

                //Store at the register
                this.SetRegisterValue(regC, value);
            });

            //The slli instruction
            this.AddRFormatExecutor(OperationCodes.Slli.Code(), OperationXCodes.Slli, inst =>
            {
                //Get the value of reg A
                int regA = this.GetRegisterValue(this.GetRegister(inst.RegisterA).Value);

                //Get the imm value from the OPX code
                int imm = inst.OpxCode & 0x1F;

                //Get the destination register
                Registers regC = this.GetRegister(inst.RegisterC).Value;

                //Shift them
                int value = regA << imm;

                //Store at the register
                this.SetRegisterValue(regC, value);
            }, ~0x1F);

            //The srli instruction
            this.AddRFormatExecutor(OperationCodes.Srli.Code(), OperationXCodes.Srli, inst =>
            {
                //Get the value of reg A
                int regA = this.GetRegisterValue(this.GetRegister(inst.RegisterA).Value);

                //Get the imm value from the OPX code
                int imm = inst.OpxCode & 0x1F;

                //Get the destination register
                Registers regC = this.GetRegister(inst.RegisterC).Value;

                //Shift them
                int value = regA >> imm;

                //Store at the register
                this.SetRegisterValue(regC, value);
            }, ~0x1F);
            #endregion

            #region Memory
            //The ldw instruction
            this.AddIFormatExecutor(OperationCodes.Ldw.Code(), inst =>
            {
                //Get the value of reg A
                uint baseAddress = (uint)this.GetRegisterValue(this.GetRegister(inst.RegisterA).Value);

                //Get the destination register
                Registers destinationReg = this.GetRegister(inst.RegisterB).Value;

                //Compute the effective address
                uint address = (uint)(baseAddress + inst.Immediate);

                //Check if the address is alinged to 4
                if (!BitHelpers.IsAligned(address, 2))
                {
                    throw new MisalignedAddressException("LDW", this.ProgramCounter, address);
                }

                //Load the value
                int value = this.ReadWordFromMemory(address);

                //Store at the register
                this.SetRegisterValue(destinationReg, value);
            });

            //The stw instruction
            this.AddIFormatExecutor(OperationCodes.Stw.Code(), inst =>
            {
                //Get the value of reg A & B
                uint baseAddress = (uint)this.GetRegisterValue(this.GetRegister(inst.RegisterA).Value);
                int value = this.GetRegisterValue(this.GetRegister(inst.RegisterB).Value);

                //Compute the effective address
                uint address = (uint)(baseAddress + inst.Immediate);

                //Check if the address is alinged to 4
                if (!BitHelpers.IsAligned(address, 2))
                {
                    throw new MisalignedAddressException("STW", this.ProgramCounter, address);
                }

                //Store the value
                this.WriteWordToMemory(address, value);
            });

			//The ldb instruction
			this.AddIFormatExecutor(OperationCodes.Ldb.Code(), inst =>
			{
				//Get the value of reg A
				uint baseAddress = (uint)this.GetRegisterValue(this.GetRegister(inst.RegisterA).Value);

				//Get the destination register
				Registers destinationReg = this.GetRegister(inst.RegisterB).Value;

				//Compute the effective address
				uint address = (uint)(baseAddress + inst.Immediate);

				////Check if the address is alinged to 4
				//if (!BitHelpers.IsAligned(address, 2))
				//{
				//	throw new MisalignedAddressException("LDB", this.ProgramCounter, address);
				//}

				//Load the value
				byte value = this.ReadByteFromMemory(address);

				//Store at the register
				this.SetRegisterValue(destinationReg, value);
			});

			//The ldbu instruction
			this.AddIFormatExecutor(OperationCodes.Ldbu.Code(), inst =>
			{
				//Get the value of reg A
				uint baseAddress = (uint)this.GetRegisterValue(this.GetRegister(inst.RegisterA).Value);

				//Get the destination register
				Registers destinationReg = this.GetRegister(inst.RegisterB).Value;

				//Compute the effective address
				uint address = (uint)(baseAddress + inst.Immediate);

				////Check if the address is alinged to 4
				//if (!BitHelpers.IsAligned(address, 2))
				//{
				//	throw new MisalignedAddressException("LDB", this.ProgramCounter, address);
				//}

				//Load the value
				int value = (int)((uint)this.ReadByteFromMemory(address));

				//Store at the register
				this.SetRegisterValue(destinationReg, value);
			});

			//The stb instruction
			this.AddIFormatExecutor(OperationCodes.Stb.Code(), inst =>
			{
				//Get the value of reg A & B
				uint baseAddress = (uint)this.GetRegisterValue(this.GetRegister(inst.RegisterA).Value);
				byte value = (byte)(this.GetRegisterValue(this.GetRegister(inst.RegisterB).Value) & 0xFFFF);

				//Compute the effective address
				uint address = (uint)(baseAddress + inst.Immediate);

				////Check if the address is alinged to 4
				//if (!BitHelpers.IsAligned(address, 2))
				//{
				//	throw new MisalignedAddressException("STW", this.ProgramCounter, address);
				//}

				//Store the value
				this.WriteByteToMemory(address, value);
			});
            #endregion

            #region Branch
            //The br instruction
            this.AddIFormatExecutor(OperationCodes.Br.Code(), inst =>
            {
                //Get the offset
                int offset = inst.Immediate;

                //Calculate the new value of the PC
                uint newPC = (uint)(this.ProgramCounter + offset);

                //Execute the jump
                this.JumpTo(newPC, "BR");
            });

            //The beq instruction
            this.AddIFormatExecutor(OperationCodes.Beq.Code(), inst =>
            {
                //Get the value of reg A & B
                int regA = this.GetRegisterValue(this.GetRegister(inst.RegisterA).Value);
                int regB = this.GetRegisterValue(this.GetRegister(inst.RegisterB).Value);
                
                //Check if equal
                if (regA == regB)
                {
                    //Get the offset
                    int offset = inst.Immediate;

                    //Calculate the new value of the PC
                    uint newPC = (uint)(this.ProgramCounter + offset);

                    //Execute the jump
                    this.JumpTo(newPC, "BEQ");
                }
            });

            //The bne instruction
            this.AddIFormatExecutor(OperationCodes.Bne.Code(), inst =>
            {
                //Get the value of reg A & B
                int regA = this.GetRegisterValue(this.GetRegister(inst.RegisterA).Value);
                int regB = this.GetRegisterValue(this.GetRegister(inst.RegisterB).Value);

                //Check if equal
                if (regA != regB)
                {
                    //Get the offset
                    int offset = inst.SignedImmediate;

                    //Calculate the new value of the PC
                    uint newPC = (uint)(this.ProgramCounter + offset);

                    //Execute the jump
                    this.JumpTo(newPC, "BNE");
                }
            });

            //The bge instruction
            this.AddIFormatExecutor(OperationCodes.Bge.Code(), inst =>
            {
                //Get the value of reg A & B
                int regA = this.GetRegisterValue(this.GetRegister(inst.RegisterA).Value);
                int regB = this.GetRegisterValue(this.GetRegister(inst.RegisterB).Value);

                //Check if equal
                if (regA >= regB)
                {
                    //Get the offset
                    int offset = inst.SignedImmediate;

                    //Calculate the new value of the PC
                    uint newPC = (uint)(this.ProgramCounter + offset);

                    //Execute the jump
                    this.JumpTo(newPC, "BGE");
                }
            });

            //The blt instruction
            this.AddIFormatExecutor(OperationCodes.Blt.Code(), inst =>
            {
                //Get the value of reg A & B
                int regA = this.GetRegisterValue(this.GetRegister(inst.RegisterA).Value);
                int regB = this.GetRegisterValue(this.GetRegister(inst.RegisterB).Value);

                //Check if equal
                if (regA < regB)
                {
                    //Get the offset
                    int offset = inst.SignedImmediate;

                    //Calculate the new value of the PC
                    uint newPC = (uint)(this.ProgramCounter + offset);

                    //Execute the jump
                    this.JumpTo(newPC, "BLT");
                }
            });
            #endregion

            #region Call/Return
            //The call instruction
            this.AddJFormatExecutor(OperationCodes.Call.Code(), inst =>
            {
                //Save the PC in RA register
                this.SetRegisterValue(Registers.RA, (int)this.ProgramCounter);

                //Get the 4 highest bits of the PC
                uint highPC = this.ProgramCounter & (0xF0000000);

                //Calculate the new PC
                uint newPC = highPC | (uint)(inst.Immediate * 4);

				if (this.enableJit)
				{
					//Check if jitted method
					var jittedMethod = this.jitCompiler.GetJittedMethod(newPC);

					if (jittedMethod != null)
					{
						this.jitCompiler.InvokeJitedMethod(jittedMethod);
					}
					else if (this.systemCalls.ContainsKey(newPC))
					{
						//Check if system call
						this.systemCalls[newPC]();
					}
					else
					{
						//Normal call
						this.ProgramCounter = newPC;
						this.jitCompiler.JitMethod(newPC);
					}
				}
				else
				{
					//Check if system call
					if (this.systemCalls.ContainsKey(newPC))
					{
						//Check if system call
						this.systemCalls[newPC]();
					}
					else
					{
						//Normal call
						this.ProgramCounter = newPC;
					}
				}
            });

            //The callr instruction
            this.AddRFormatExecutor(OperationCodes.Callr.Code(), OperationXCodes.Callr, inst =>
            {
                if (inst.RegisterC == 0x1f && inst.RegisterB == 0)
                {
                    //Save the PC in RA register
                    this.SetRegisterValue(Registers.RA, (int)this.ProgramCounter);

                    //Get the value of register A
                    uint regA = (uint)this.GetRegisterValue(this.GetRegister(inst.RegisterA).Value);

                    //Check if system call
                    if (this.systemCalls.ContainsKey(regA))
                    {
                        this.systemCalls[regA]();
                    }
                    else
                    {
                        //Set the value of pc
                        this.ProgramCounter = regA;
                    }
                }
            });

            //The ret instruction
            this.AddRFormatExecutor(OperationCodes.Ret.Code(), OperationXCodes.Ret, inst =>
            {
                if (inst.RegisterA == 0x1f)
                {
                    //Get the value of ra
                    uint ra = (uint)this.GetRegisterValue(Registers.RA);

                    //Set the value of pc
                    this.ProgramCounter = ra;
                }
            });
            #endregion

			#endregion

			this.systemCalls = new Dictionary<uint, Action>();
            this.systemCallLinker = new Dictionary<string,uint>();

            #region System calls
            this.AddSystemCall("putchar", 5000, () =>
            {
                //Convert from ASCII
                char charToPrint = ASCIIEncoding.ASCII.GetChars(new byte[] { (byte)this.GetRegisterValue(Registers.R4) })[0];
                Console.Write(charToPrint);
            });
            #endregion

			#region JIT Compiler
			this.jitCompiler = new PartialJITCompiler(this);
			this.enableJit = false;
			#endregion
		}
        #endregion

        #region Properties
        /// <summary>
        /// The program counter
        /// </summary>
        public uint ProgramCounter
        {
            get { return this.programCounter; }
            set { this.programCounter = value; }
        }

        /// <summary>
        /// Returns the end of the current loaded program
        /// </summary>
        public uint ProgramEnd
        {
            get { return this.programEnd; }
        }

        /// <summary>
        /// Returns the value of the instruction register, the current loaded instruction
        /// </summary>
        public Instruction InstructionRegister
        {
            get { return this.instructionRegister; }
        }

        /// <summary>
        /// Returns the system calls
        /// </summary>
        public IDictionary<string, uint> SystemCalls
        {
            get { return this.systemCallLinker; }
        }

		/// <summary>
		/// Enables the JIT compiler
		/// </summary>
		public bool EnableJit
		{
			get { return this.enableJit; }
			set { this.enableJit = value; }
		}
        #endregion

        #region Methods

        #region Register Methods
        /// <summary>
        /// Returns the given register from its number
        /// </summary>
        /// <param name="number">The number</param>
        /// <returns>The register or null</returns>
        public Registers? GetRegister(int number)
        {
            try
            {
				return (Registers)Enum.ToObject(typeof(Registers), number);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the value stored at the given register
        /// </summary>
        /// <param name="register">The register</param>
        public int GetRegisterValue(Registers register)
        {
            switch (register)
            {
                case Registers.R0:
                    return 0;
                default:
                    return this.registers[register.Number()];
            }
        }

        /// <summary>
        /// Sets the value for the given register
        /// </summary>
        /// <param name="register">The register</param>
        /// <param name="value">The value</param>
        public void SetRegisterValue(Registers register, int value)
        {
            switch (register)
            {
                case Registers.R0:
                    break;
                default:
                    this.registers[register.Number()] = value;
                    break;
            }
        }
        #endregion

        #region Memory Methods
        /// <summary>
        /// Reads an byte from memory at the given address
        /// </summary>
        /// <param name="address">The address</param>
        public byte ReadByteFromMemory(uint address)
        {
            return this.memory[address];
        }

        /// <summary>
        /// Reads an word (an int) from memory at the given address
        /// </summary>
        /// <param name="address">The address</param>
        public int ReadWordFromMemory(uint address)
        {
            byte byte1 = this.memory[address];
            byte byte2 = this.memory[address + 1];
            byte byte3 = this.memory[address + 2];
            byte byte4 = this.memory[address + 3];

            int value = byte1 | (byte2 << 8) | (byte3 << 16) | (byte4 << 24);
            return value;
        }

        /// <summary>
        /// Writes an byte to memory at the given address
        /// </summary>
        /// <param name="address">The address</param>
        /// <param name="value">The value</param>
        public void WriteByteToMemory(uint address, byte value)
        {
            this.memory[address] = value;
        }

        /// <summary>
        /// Writes an word (an int) to memory at the given address
        /// </summary>
        /// <param name="address">The address</param>
        /// <param name="value">The value</param>
        public void WriteWordToMemory(uint address, int value)
        {
            this.memory[address + 3] = (byte)(value >> 24);
            this.memory[address + 2] = (byte)(value >> 16);
            this.memory[address + 1] = (byte)(value >> 8);
            this.memory[address] = (byte)(value);
        }

        /// <summary>
        /// Clears the main memory
        /// </summary>
        public void ClearMemory()
        {
            for (int i = 0; i < this.memory.Length; i++)
            {
                this.memory[i] = 0;
            }
        }
        #endregion

        #region Executor
        /// <summary>
        /// Adds the given I-format executor to the list of instruction executors
        /// </summary>
        /// <param name="opCode">The op code</param>
        /// <param name="executor">The executor</param>
        public void AddIFormatExecutor(int opCode, IFormatInstructionExecutor executor)
        {
            if (!this.instructionExecutors.ContainsKey(opCode))
            {
                //Create the executor
                InstructionExecutor newExecutor = new InstructionExecutor()
                {
                    InstructionFormat = InstructionFormat.IFormat,
                    IFormatInstructionExecutor = executor,
                };

                //Add it
                this.instructionExecutors.Add(opCode, newExecutor);
            }
        }

        /// <summary>
        /// Adds the given R-format executor to the list of instruction executors
        /// </summary>
        /// <param name="opCode">The op code</param>
        /// <param name="opxCode">The opx code</param>
        /// <param name="executor">The executor</param>
        /// <param name="opxMask">The opx mask</param>
        public void AddRFormatExecutor(int opCode, int opxCode, RFormatInstructionExecutor executor, int opxMask = -1)
        {
            InstructionExecutor newExecutor = null;

            //Check if an executor exist
            if (this.instructionExecutors.ContainsKey(opCode))
            {
                //Get it
                newExecutor = this.instructionExecutors[opCode];
            }
            else
            {
                //Create an new one
                newExecutor = new InstructionExecutor()
                {
                    InstructionFormat = InstructionFormat.RFormat,
                    RFormatInstructionExecutors = new Dictionary<int, RFormatInstructionExecutor>(),
                    RFormatOpxMaskers = new List<int>()
                };

                this.instructionExecutors.Add(opCode, newExecutor);
            }

            //Check if an opx code executor exists
            if (!newExecutor.RFormatInstructionExecutors.ContainsKey(opxCode))
            {
                newExecutor.RFormatInstructionExecutors.Add(opxCode, executor);

                if (opxMask != -1)
                {
                    newExecutor.RFormatOpxMaskers.Add(opxMask);
                }
            }
        }

        /// <summary>
        /// Adds the given J-format executor to the list of instruction executors
        /// </summary>
        /// <param name="opCode">The op code</param>
        /// <param name="executor">The executor</param>
        public void AddJFormatExecutor(int opCode, JFormatInstructionExecutor executor)
        {
            if (!this.instructionExecutors.ContainsKey(opCode))
            {
                //Create the executor
                InstructionExecutor newExecutor = new InstructionExecutor()
                {
                    InstructionFormat = InstructionFormat.JFormat,
                    JFormatInstructionExecutor = executor,
                };

                //Add it
                this.instructionExecutors.Add(opCode, newExecutor);
            }
        }


        /// <summary>
        /// Decodes and executes the given I-format instruction
        /// </summary>
        /// <param name="instructionData">The instruction data</param>
        /// <param name="executor">The executor</param>
        private void DecodeAndExecuteIFormat(int instructionData, InstructionExecutor executor)
        {
            //Decode and execute it
            IFormatInstruction instruction = IFormatInstruction.Decode(instructionData);
            executor.IFormatInstructionExecutor(instruction);
        }

        /// <summary>
        /// Decodes and executes the given R-format instruction
        /// </summary>
        /// <param name="instructionData">The instruction data</param>
        /// <param name="executor">The executor</param>
        private void DecodeAndExecuteRFormat(int instructionData, InstructionExecutor executor)
        {
            //Decode it
            RFormatInstruction instruction = RFormatInstruction.Decode(instructionData);

            //Find the R-format executor
            RFormatInstructionExecutor opxExecutor = null;

            //Check if maskers exists
            if (executor.RFormatOpxMaskers.Count == 0)
            {
                if (executor.RFormatInstructionExecutors.TryGetValue(instruction.OpxCode, out opxExecutor))
                {
                    opxExecutor(instruction);
                }
            }
            else
            {
                //Loop until an executor is found
                foreach (int currentMask in executor.RFormatOpxMaskers)
                {
                    int opxCode = instruction.OpxCode & currentMask;

                    if (executor.RFormatInstructionExecutors.TryGetValue(opxCode, out opxExecutor))
                    {
                        opxExecutor(instruction);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Decodes and executes the given J-format instruction
        /// </summary>
        /// <param name="instructionData">The instruction data</param>
        /// <param name="executor">The executor</param>
        private void DecodeAndExecuteJFormat(int instructionData, InstructionExecutor executor)
        {
            //Decode and execute it
            JFormatInstruction instruction = JFormatInstruction.Decode(instructionData);
            executor.JFormatInstructionExecutor(instruction);
        }
        #endregion

        #region System calls Methods
        /// <summary>
        /// Adds an new system call
        /// </summary>
        /// <param name="callName">The name (function) of the call</param>
        /// <param name="callAddress">The address</param>
        /// <param name="callAction">The action</param>
        public void AddSystemCall(string callName, uint callAddress, Action callAction)
        {
            if (!this.systemCalls.ContainsKey(callAddress))
            {
                this.systemCalls.Add(callAddress, callAction);
                this.systemCallLinker.Add(callName, callAddress);
            }
        }

        /// <summary>
        /// Returns the address for the given call
        /// </summary>
        /// <param name="callName">The name of the call</param>
        /// <returns>The address or null</returns>
        public uint? GetCallAddress(string callName)
        {
            if (this.systemCallLinker.ContainsKey(callName))
            {
                return this.systemCallLinker[callName];
            }

            return null;
        }
        #endregion

        #region Instruction Helper Methods
        /// <summary>
        /// Jumps to the given adress
        /// </summary>
        /// <param name="newProgramCounter">The value to jump to</param>
        /// <param name="instructionName">The name of the instruction</param>
        private void JumpTo(uint newProgramCounter, string instructionName)
        {
            //Check if the address is alinged to 4
            if (!BitHelpers.IsAligned(newProgramCounter, 2))
            {
                throw new MisalignedAddressException(instructionName, this.ProgramCounter, newProgramCounter);
            }

            //Set the PC
            this.ProgramCounter = newProgramCounter;
        }
        #endregion

        #region Fetch/Execute Methods
        /// <summary>
        /// Fetches an new instruction and updates the program counter
        /// </summary>
        public void Fetch()
        {
            //Fetch an instruction
            this.instructionRegister = new Instruction(this.ReadWordFromMemory(this.ProgramCounter));

            //Update the PC
            this.ProgramCounter += 4;
        }

        /// <summary>
        /// Executes the instruction in the instruction register
        /// </summary>
        public void Execute()
        {
            this.ExecuteInstruction(this.instructionRegister.Data);
        }

        /// <summary>
        /// Executes the given instruction
        /// </summary>
        /// <param name="instructionData">The data of the instruction</param>
        public void ExecuteInstruction(int instructionData)
        {
            //Read the opcode
            int opCode = instructionData & 0x3F;

            //Find the executor
            InstructionExecutor executor = null;

            if (this.instructionExecutors.TryGetValue(opCode, out executor))
            {
                switch (executor.InstructionFormat)
                {
                    case InstructionFormat.IFormat:
                        this.DecodeAndExecuteIFormat(instructionData, executor);
                        break;

                    case InstructionFormat.RFormat:
                        this.DecodeAndExecuteRFormat(instructionData, executor);
                        break;

                    case InstructionFormat.JFormat:
                        this.DecodeAndExecuteJFormat(instructionData, executor);
                        break;
                }
            }
            else
            {
                Console.WriteLine(string.Format("{0}: The current instruction (op code = {1}) wasn't executed.", this.ProgramCounter - 4, opCode));
            }
        }
        #endregion

        #region Program Methods
        /// <summary>
        /// Runs the given program
        /// </summary>
        /// <param name="program">The program to run</param>
        public void Run(Program program)
        {
            this.LoadProgram(program);
            this.Run();
        }

        /// <summary>
        /// Loads the given program into memory
        /// </summary>
        /// <param name="program">The program to run</param>
        public void LoadProgram(Program program)
        {
            //this.ClearMemory();
            this.textArea = program.TextAreaStart;
            this.dataArea = program.DataAreaStart;
            this.entryPoint = program.EntryPoint;

            //Copy program data
            program.Data.CopyTo(this.memory, this.textArea);

            //Calc the end address of the program
            this.programEnd = (uint)(this.textArea + 4 * program.NumInstructions);
        }

        /// <summary>
        /// Runs the loaded program
        /// </summary>
        public void Run()
        {
            //Set the PC
            this.ProgramCounter = (uint)this.entryPoint;

            while (this.ProgramCounter < this.programEnd)
            {
                this.Fetch();
                this.Execute();
            }
        }

        /// <summary>
        /// Stops the current program
        /// </summary>
        public void Stop()
        {
            this.ProgramCounter = this.programEnd;
        }

        /// <summary>
        /// Restarts the current loaded program
        /// </summary>
        public void RestartProgram()
        {
            this.ProgramCounter = (uint)this.textArea;
        }
        #endregion

		#region JIT
		/// <summary>
		/// Returns the internal representation of the VM
		/// </summary>
		internal InternalVMData GetInternalData()
		{
			return new InternalVMData()
			{
				Registers = this.registers,
				Memory = this.memory,
				VirtualMachine = this
			};
		}
		#endregion

		#endregion

	}
}
