using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiosII_Simulator.Core.Exceptions
{
    /// <summary>
    /// Represents an exception for an misaligned address
    /// </summary>
    public class MisalignedAddressException : NiosIIException
    {
        /// <summary>
        /// Returns the address
        /// </summary>
        public uint Address { get; private set; }

        /// <summary>
        /// Creates a new MisalignedAddress exception
        /// </summary>
        /// <param name="instruction">The instruction that caused the exception</param>
        /// <param name="programCounter">The value of the program counter</param>
        /// <param name="address">The address</param>
        public MisalignedAddressException(string instruction, uint programCounter, uint address)
            : base(instruction, programCounter)
        {
            this.Address = address;
        }
    }
}
