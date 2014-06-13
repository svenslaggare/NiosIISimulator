using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiosII_Simulator.Core
{
    /// <summary>
    /// The available registers
    /// </summary>
    public enum Registers
    {
        R0,
        R1,
        R2,
        R3,
        R4,
        R5,
        R6,
        R7,
        R8,
        R9,
        R10,
        R11,
        R12,
        R13,
        R14,
        R15,
        R16,
        R17,
        R18,
        R19,
        R20,
        R21,
        R22,
        R23,
        R24,
        R25,
        R26,
        R27,
        R28,
        R29,
        R30,
        R31,
		/// <summary>
		/// The assembler temporary register (R1)
		/// </summary>
        AT = R1,
		/// <summary>
		/// The exception temporary register (R24)
		/// </summary>
        ET = R24,
        BT = R25,
		/// <summary>
		/// The global pointer register (R26)
		/// </summary>
        GP = R26,
		/// <summary>
		/// The stack pointer  register (R27)
		/// </summary>
        SP = R27,
		/// <summary>
		/// The frame pointer register (R28(
		/// </summary>
        FP = R28,
		/// <summary>
		/// The exception address register (R29)
		/// </summary>
        EA = R29,
        BA = R30,
		/// <summary>
		/// The return address register (R31)
		/// </summary>
        RA = R31
    }

    /// <summary>
    /// Contains extension methods for the Registers enum
    /// </summary>
    public static class RegistersExtensions
    {
        /// <summary>
        /// Returns the register number of the current register
        /// </summary>
        public static int Number(this Registers reg)
        {
            return (int)reg;
        }
    }
}
