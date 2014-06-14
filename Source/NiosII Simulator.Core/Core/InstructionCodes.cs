using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiosII_Simulator.Core
{
	/// <summary>
	/// The list of operation codes
	/// </summary>
	public enum OperationCodes
	{
		Addi = 0x04,
		Add = 0x3a,
		Sub = 0x31,
		Ldw = 0x17,
		Stw = 0x15,
		Ldb = 0x07,
		Ldbu = 0x03,
		Stb = 0x05,
		Br = 0x06,
		Beq = 0x26,
		Bne = 0x1e,
		Bge = 0x0e,
		Blt = 0x16,
		And = 0x3a,
		Andi = 0x0c,
		Andhi = 0x2c,
		Or = 0x31,
		Ori = 0x14,
		Orhi = 0x34,
		Xor = 0x3a,
		Xori = 0x1c,
		Xorhi = 0x3c,
		Nor = 0x31,
		Srl = 0x3a,
		Sll = 0x3a,
		Srli = 0x3a,
		Slli = 0x3a,
		Cmpeq = 0x3a,
		Cmpne = 0x3a,
		Cmpge = 0x3a,
		Cmplt = 0x3a,
		Cmpnei = 0x18,
		Cmpeqi = 0x20,
		Cmpgei = 0x08,
		Cmplti = 0x10,
		Call = 0x0,
		Callr = 0x3a,
		Ret = 0x3a
	}

	/// <summary>
	/// Extension methods for the OperationCodes enum
	/// </summary>
	public static class OperationCodesExtensions
	{
		/// <summary>
		/// Returns the op code
		/// </summary>
		public static int Code(this OperationCodes opCode)
		{
			return (int)opCode;
		}
	}

	/// <summary>
	/// The list of operation X codes
	/// </summary>
	/// <remarks>This is a class instead of an enum because enum members must be constant and not computed.</remarks>
	public static class OperationXCodes
	{
		public static readonly int Add = BitHelpers.FromBitPattern("110001 00000");
		public static readonly int Sub = BitHelpers.FromBitPattern("111001 00000");

		public static readonly int And = BitHelpers.FromBitPattern("001110 00000");
		public static readonly int Or = BitHelpers.FromBitPattern("010110 00000");
		public static readonly int Xor = BitHelpers.FromBitPattern("011110 00000");
		public static readonly int Nor = BitHelpers.FromBitPattern("000110 00000");

		public static readonly int Srl = BitHelpers.FromBitPattern("011011 00000");
		public static readonly int Sll = BitHelpers.FromBitPattern("010011 00000");
		public static readonly int Srli = BitHelpers.FromBitPattern("011010 00000");
		public static readonly int Slli = BitHelpers.FromBitPattern("010010 00000");

		public static readonly int Cmpeq = BitHelpers.FromBitPattern("100000 00000");
		public static readonly int Cmpne = BitHelpers.FromBitPattern("011000 00000");
		public static readonly int Cmpge = BitHelpers.FromBitPattern("001000 00000");
		public static readonly int Cmplt = BitHelpers.FromBitPattern("010000 00000");

		public static readonly int Ret = BitHelpers.FromBitPattern("000101 00000");

		public static readonly int Callr = BitHelpers.FromBitPattern("011101 00000");
	}
}
