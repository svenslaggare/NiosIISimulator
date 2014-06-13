using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiosII_Simulator.Core.Assembler
{
    /// <summary>
    /// Represents an assembler exception
    /// </summary>
    public class AssemblerException : Exception
    {
        /// <summary>
        /// Returns the lines where the exception happend
        /// </summary>
        public int Line { get; private set; }

        /// <summary>
        /// Returns the instruction that caused the exception
        /// </summary>
        public string Instruction { get; private set; }

        /// <summary>
        /// Creates an new assembler exception
        /// </summary>
        /// <param name="line">The line</param>
        /// <param name="instruction">The instruction</param>
        /// <param name="message">The error message</param>
        public AssemblerException(int line, string instruction, string message)
            : base(string.Format("{0}: {1} ({2})", (line + 1),  message, instruction))
        {

        }
    }
}
