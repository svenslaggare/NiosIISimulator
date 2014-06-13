using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiosII_Simulator.Core
{
    /// <summary>
    /// The base class for Nios II exceptions
    /// </summary>
    public abstract class NiosIIException : Exception
    {
        private readonly string instruction;                                                                            //The instruction that caused the exception
        private readonly uint programCounter;                                                                           //The value of the program counter

        /// <summary>
        /// Creates a new Nios II exception
        /// </summary>
        /// <param name="instruction">The instruction that caused the exception</param>
        /// <param name="programCounter">The value of the program counter</param>
        public NiosIIException(string instruction, uint programCounter)
        {
            this.instruction = instruction;
            this.programCounter = programCounter;
        }

        /// <summary>
        /// Returns the instruction that caused the exception
        /// </summary>
        public string Instruction
        {
            get { return this.instruction; }
        }

        /// <summary>
        /// Returns the value of the program counter
        /// </summary>
        public uint ProgramCounter
        {
            get { return this.programCounter; }
        }
    }
}
