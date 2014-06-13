using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiosII_Simulator.Core
{
    /// <summary>
    /// Contain methods to manipulate bit patterns
    /// </summary>
    public static class BitHelpers
    {
        /// <summary>
        /// Creates an integer from the given bit pattern
        /// </summary>
        /// <param name="bitPattern">The bit pattern</param>
        /// <returns>An integer</returns>
        /// <remarks>Any other character than 1 or 0 is treated as 0</remarks>
        public static int FromBitPattern(string bitPattern)
        {
            //Remove blank spaces
            bitPattern = bitPattern.Replace(" ", "");

            int value = 0;
            int currentPow = 1;

            for(int i = Math.Min(bitPattern.Length - 1, 32); i >= 0; i--)
            {
                char currentChar = bitPattern[i];

                if (currentChar == '1')
                {
                    value += currentPow;
                }

                currentPow *= 2;
            }

            return value;
        }

        /// <summary>
        /// Converts the given number into a binary string
        /// </summary>
        /// <param name="x">The number</param>
        public static string ToBinary(int x)
        {
            StringBuilder binString = new StringBuilder(32);
            
            while (x > 0)
            {
                bool isEven = x % 2 == 1;
                x = x / 2;

                if (isEven)
                {
                    binString.Insert(0, "1");
                }
                else
                {
                    binString.Insert(0, "0");
                }
            }

            if (binString.Length == 0)
            {
                binString.Append("0");
            }

            return binString.ToString();
        }

        /// <summary>
		/// Indicates if the given number is aligned to the given power of two
        /// </summary>
        /// <param name="x">The number</param>
        /// <param name="pow2">The power of two</param>
        public static bool IsAligned(int x, int pow2)
        {
            return x % (2 << pow2 - 1) == 0;
        }

        /// <summary>
		/// Indicates if the given number is aligned to the given power of two
        /// </summary>
        /// <param name="x">The number</param>
        /// <param name="pow2">The power of two</param>
        public static bool IsAligned(uint x, int pow2)
        {
            return x % (2 << pow2 - 1) == 0;
        }
    }
}
