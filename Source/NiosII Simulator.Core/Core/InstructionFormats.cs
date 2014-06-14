using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiosII_Simulator.Core
{
    /// <summary>
    /// Represents an instruction format
    /// </summary>
    public interface IInstructionFormat
    {
        /// <summary>
        /// Returns the operation code of the current instruction
        /// </summary>
        int OpCode { get; }

        /// <summary>
        /// Encodes the current instruction as an integer
        /// </summary>
        int Encode();
    }

    /// <summary>
    /// Contains extension methods for IInstructionFormat interface
    /// </summary>
    public static class IInstructionFormatExtensions
    {
        /// <summary>
        /// Converts the current instruction in a given format as an instruction
        /// </summary>
        public static Instruction AsInstruction(this IInstructionFormat instructionFormat)
        {
            return new Instruction(instructionFormat.Encode());
        }
    }

    /// <summary>
    /// Represents an instruction encoded in the I-format
    /// </summary>
    public struct IFormatInstruction : IInstructionFormat
    {
        private readonly int opCode;                                                                                            //The op code
        private readonly int registerA;                                                                                         //The A register
        private readonly int registerB;                                                                                         //The B register
        private readonly int immediate;																							//The immediate data field

        /// <summary>
        /// Creates a new instruction in the I-format
        /// </summary>
        /// <param name="opCode">The op code</param>
        /// <param name="registerA">The A register</param>
        /// <param name="registerB">The B register</param>
        /// <param name="immediate">The immediate data field (signed)</param>
        public IFormatInstruction(int opCode, int registerA, int registerB, short immediate)
        {
            this.opCode = opCode & 0x3F;
            this.registerA = registerA & 0x1F;
            this.registerB = registerB & 0x1F;
			this.immediate = immediate;
        }

		/// <summary>
		/// Creates a new instruction in the I-format
		/// </summary>
		/// <param name="opCode">The op code</param>
		/// <param name="registerA">The A register</param>
		/// <param name="registerB">The B register</param>
		/// <param name="immediate">The immediate data field (unsigned)</param>
		public IFormatInstruction(int opCode, int registerA, int registerB, ushort immediate)
		{
			this.opCode = opCode & 0x3F;
			this.registerA = registerA & 0x1F;
			this.registerB = registerB & 0x1F;
			this.immediate = immediate;
		}

        /// <summary>
        /// Returns the op code
        /// </summary>
        public int OpCode
        {
            get { return this.opCode; }
        }

        /// <summary>
        /// Returns the A register
        /// </summary>
        public int RegisterA
        {
            get { return this.registerA; }
        }

        /// <summary>
        /// Returns the B register
        /// </summary>
        public int RegisterB
        {
            get { return this.registerB; }
        }

        /// <summary>
        /// Returns the immediate data field
        /// </summary>
		public int Immediate
        {
            get { return (ushort)this.immediate; }
        }

        /// <summary>
        /// Returns a signed version the immediate data field
        /// </summary>
        public int SignedImmediate
        {
            get
            {
				uint imm = (uint)(Int16)this.Immediate;
				return (int)imm;
            }
        }

        /// <summary>
        /// Encodes the current instruction as an integer
        /// </summary>
        public int Encode()
        {
            int inst = 0;

            //Encode the parameters in their correct position
            int opCode = this.OpCode & 0x3F;

            int imm1 = this.Immediate & 0xFF;
            int imm2 = (this.Immediate >> 8) & 0xFF;
            int imm = (imm1 << 6) | (imm2 << 6 + 8);

            int regB = (this.RegisterB & 0x1F) << 22;
            int regA = (this.RegisterA & 0x1F) << 27;

            inst = opCode | imm | regB | regA;

            return inst;
        }

        /// <summary>
        /// Decodes the given data into an I-Format instruction
        /// </summary>
        /// <param name="instructionData">The instruction data</param>
        public static IFormatInstruction Decode(int instructionData)
        {
            //Decode it
            int opCode = instructionData & 0x3F;

            int imm1 = (instructionData >> 6) & 0xFF;
            int imm2 = (instructionData >> 6 + 8) & 0xFF;

            int imm = imm1 | (imm2 << 8);

            int regB = (instructionData >> 22) & 0x1F;
            int regA = (instructionData >> 27) & 0x1F;

            return new IFormatInstruction(opCode, regA, regB, (ushort)imm);
        }
    }

    /// <summary>
	/// Represents an instruction encoded in the R-format
    /// </summary>
    public struct RFormatInstruction : IInstructionFormat
    {
        private readonly int opCode;                                                                                            //The op code
        private readonly int opxCode;                                                                                           //The opx code
        private readonly int registerA;                                                                                         //The A register
        private readonly int registerB;                                                                                         //The B register
        private readonly int registerC;                                                                                         //The C register

        /// <summary>
        /// Creates a new instruction in the I-format
        /// </summary>
        /// <param name="opCode">The op code</param>
        /// <param name="opxCode">The opx code</param>
        /// <param name="registerA">The A register</param>
        /// <param name="registerB">The B register</param>
        /// <param name="registerB">The C register</param>
        public RFormatInstruction(int opCode, int opxCode, int registerA, int registerB, int registerC)
        {
            this.opCode = opCode & 0x3F;
            this.opxCode = opxCode & 0x7FF;
            this.registerA = registerA & 0x1F;
            this.registerB = registerB & 0x1F;
            this.registerC = registerC & 0x1F;
        }

        /// <summary>
        /// Returns the op code
        /// </summary>
        public int OpCode
        {
            get { return this.opCode; }
        }

        /// <summary>
        /// Returns the opx code
        /// </summary>
        public int OpxCode
        {
            get { return this.opxCode; }
        }

        /// <summary>
        /// Returns the A register
        /// </summary>
        public int RegisterA
        {
            get { return this.registerA; }
        }

        /// <summary>
        /// Returns the B register
        /// </summary>
        public int RegisterB
        {
            get { return this.registerB; }
        }

        /// <summary>
        /// Returns the C register
        /// </summary>
        public int RegisterC
        {
            get { return this.registerC; }
        }

        /// <summary>
        /// Encode the current instruction as an integer
        /// </summary>
        public int Encode()
        {
            int inst = 0;

            //Encode the parameters in their correct position
            int opCode = this.OpCode & 0x3F;

            int opx1 = this.OpxCode & 0xFF;
            int opx2 = (this.OpxCode >> 8) & 0x7;
            int opxCode = (opx1 << 6) | (opx2 << 6 + 8);

            int regC = (this.RegisterC & 0x1F) << 17;
            int regB = (this.RegisterB & 0x1F) << 22;
            int regA = (this.RegisterA & 0x1F) << 27;

            inst = opCode | opxCode | regC | regB | regA;

            return inst;
        }

        /// <summary>
        /// Decodes the given data into a R-Format instruction
        /// </summary>
        /// <param name="instructionData">The instruction data</param>
        public static RFormatInstruction Decode(int instructionData)
        {
            int opCode = instructionData & 0x3F;

            //Decode it
            int opx1 = (instructionData >> 6) & 0xFF;
            int opx2 = (instructionData >> 6 + 8) & 0x7;

            int opxCode = opx1 | (opx2 << 8);

            int regC = (instructionData >> 17) & 0x1F;
            int regB = (instructionData >> 22) & 0x1F;
            int regA = (instructionData >> 27) & 0x1F;

            return new RFormatInstruction(opCode, opxCode, regA, regB, regC);
        }
    }

    /// <summary>
	/// Represents an instruction encoded in the I-format
    /// </summary>
    public struct JFormatInstruction : IInstructionFormat
    {
        private readonly int opCode;                                                                                            //The op code
        private readonly int immediate;                                                                                         //The immediate data field

        /// <summary>
        /// Creates a new instruction in the I-format
        /// </summary>
        /// <param name="opCode">The op code</param>
        /// <param name="immediate">The imm immediate data field</param>
        public JFormatInstruction(int opCode, int immediate)
        {
            this.opCode = opCode & 0x3F;
            this.immediate = (int)((long)immediate & 0x3FFFFFFFF);
        }

        /// <summary>
        /// Returns the op code
        /// </summary>
        public int OpCode
        {
            get { return this.opCode; }
        }

        /// <summary>
        /// Returns the immediate data field
        /// </summary>
        public int Immediate
        {
            get { return this.immediate; }
        }

        /// <summary>
        /// Encodes the current instruction as an integer
        /// </summary>
        public int Encode()
        {
            int inst = 0;

            //Encode the parameters in their correct position
            int opCode = this.OpCode & 0x3F;

            int imm1 = this.Immediate & 0xFF;
            int imm2 = (this.Immediate >> 8) & 0xFF;
            int imm3 = (this.Immediate >> 16) & 0xFF;
            int imm4 = (this.Immediate >> 24) & 0x3;

            int imm = (imm1 << 6) | (imm2 << 6 + 8) | (imm3 << 6 + 16) | (imm4 << 6 + 24);

            inst = opCode | imm;

            return inst;
        }

        /// <summary>
        /// Decodes the given instruction data into a J-Format instruction
        /// </summary>
        /// <param name="instructionData">The instruction data</param>
        public static JFormatInstruction Decode(int instructionData)
        {
            int opCode = instructionData & 0x3F;
            int imm1 = (instructionData >> 6) & 0xFF;
            int imm2 = (instructionData >> 6 + 8) & 0xFF;
            int imm3 = (instructionData >> 6 + 16) & 0xFF;
            int imm4 = (instructionData >> 6 + 24) & 0x3;

            int imm = imm1 | (imm2 << 8) | (imm3 << 16) | (imm4 << 24);

            return new JFormatInstruction(opCode, imm);
        }
    }
}
