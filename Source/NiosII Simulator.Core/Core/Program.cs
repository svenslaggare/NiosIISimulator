using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiosII_Simulator.Core
{
    /// <summary>
    /// Represents an instruction
    /// </summary>
    public struct Instruction
    {
        private readonly int data;

        /// <summary>
        /// Creates an new instruction
        /// </summary>
        /// <param name="data">The data of the instruction</param>
        public Instruction(int data)
        {
            this.data = data;
        }

        /// <summary>
        /// Returns the data of the instruction
        /// </summary>
        public int Data
        {
            get { return this.data; }
        }

		public override string ToString()
		{
			return this.Data.ToString();
		}
    }

    /// <summary>
    /// The type of the data
    /// </summary>
    public enum DataType
    {
		/// <summary>
		/// 32 bits integer
		/// </summary>
        Word,
		/// <summary>
		/// 8 bits integer
		/// </summary>
        Byte
    }

    /// <summary>
    /// Represents a data variable
    /// </summary>
    public class DataVariable
    {
        /// <summary>
        /// The type of the variable
        /// </summary>
        public DataType DataType { get; private set; }

        /// <summary>
        /// The address
        /// </summary>
        public uint Address { get; private set; }

        private readonly byte byteValue;                                                                                        //The byte value
        private readonly int wordValue;                                                                                         //The word value

        private DataVariable(DataType dataType, uint address, byte byteValue, int wordValue)
        {
            this.DataType = dataType;
            this.Address = address;
            this.byteValue = byteValue;
            this.wordValue = wordValue;
        }

        /// <summary>
        /// Returns the value if the variable is an word
        /// </summary>
        public int Word
        {
            get
            {
                if (this.DataType == DataType.Word)
                {
                    return this.wordValue;
                }
                else
                {
                    throw new InvalidOperationException("The current variable isn't a word");
                }
            }
        }

        /// <summary>
        /// Returns the value if the variable is an byte
        /// </summary>
        public byte Byte
        {
            get
            {
                if (this.DataType == DataType.Byte)
                {
                    return this.byteValue;
                }
                else
                {
                    throw new InvalidOperationException("The current variable isn't a byte");
                }
            }
        }

        /// <summary>
        /// Creates a new byte at the given address
        /// </summary>
        /// <param name="address">The address</param>
        /// <param name="value">The value</param>
        public static DataVariable NewByte(uint address, byte value)
        {
            return new DataVariable(DataType.Byte, address, value, 0);
        }

        /// <summary>
        /// Creates a new word at the given address
        /// </summary>
        /// <param name="address">The address</param>
        /// <param name="value">The value</param>
        public static DataVariable NewWord(uint address, int value)
        {
            return new DataVariable(DataType.Word, address, 0, value);
        }
    }

    /// <summary>
    /// Represents the data area of the program
    /// </summary>
    public class DataArea
    {

        #region Fields
        private readonly int start;                                                         //Where the data area starts
        private readonly int size;                                                          //The size of the area
        private readonly List<DataVariable> dataTable;                                      //The data table
        #endregion

        #region Constructors
        /// <summary>
        /// Creates an new data area
        /// </summary>
        /// <param name="start">The start of the area</param>
        /// <param name="size">The size of the area</param>
        /// <param name="dataTable">The data table</param>
        public DataArea(int start, int size, List<DataVariable> dataTable)
        {
            this.start = start;
            this.size = size;
            this.dataTable = new List<DataVariable>(dataTable);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Returns the start of the data area
        /// </summary>
        public int Start
        {
            get { return this.start; }
        }

        /// <summary>
        /// Returns the size of the data area
        /// </summary>
        public int Size
        {
            get { return this.size; }
        }

        /// <summary>
        /// Returns the data table
        /// </summary>
        public IReadOnlyList<DataVariable> DataTable
        {
            get { return this.dataTable.AsReadOnly(); }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns an empty area at the given position
        /// </summary>
        /// <param name="start">The start of the area</param>
        public static DataArea EmptyArea(int start)
        {
            return new DataArea(start, 0, new List<DataVariable>());
        }
        #endregion

    }

    /// <summary>
    /// Represents a program for the VM
    /// </summary>
    public class Program
    {

        #region Fields
        private readonly byte[] data;                                                                                    //The data that represents the program
        private readonly int numInstructions;                                                                            //The number of instructions

        private readonly uint textAreaStart;                                                                             //Where the text area starts
        private readonly uint dataAreaStart;                                                                             //Where the data area starts
        private readonly uint entryPoint;                                                                                //The entry point

		private readonly IDictionary<string, uint> symbolTable;															 //The symbol table
		private readonly IDictionary<uint, int> functionTable;															 //The function table
		#endregion

        #region Constructors
        /// <summary>
        /// Creates a new program
        /// </summary>
        /// <param name="data">The data</param>
        /// <param name="numInstructions">The number of instructions</param>
        /// <param name="textAreaStart">Where the text area starts</param>
        /// <param name="dataAreaStart">Where the data area starts</param>
        /// <param name="entryPoint">The entry point</param>
		/// <param name="symbolTable">The symbol table</param>
		/// <param name="functionTable">The function table</param>
        private Program(byte[] data, int numInstructions, uint textAreaStart, uint dataAreaStart, uint entryPoint,
			IDictionary<string, uint> symbolTable, IDictionary<uint, int> functionTable)
        {
            this.data = data;
            this.numInstructions = numInstructions;
            this.textAreaStart = textAreaStart;
            this.dataAreaStart = dataAreaStart;
            this.entryPoint = entryPoint;

			this.symbolTable = symbolTable;
			this.functionTable = functionTable;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Returns the byte patterns that represents the program
        /// </summary>
        public byte[] Data
        {
            get { return this.data; }
        }

        /// <summary>
        /// Returns the number instructions
        /// </summary>
        public int NumInstructions
        {
            get { return this.numInstructions; }
        }

        /// <summary>
        /// Where the text area starts
        /// </summary>
        public uint TextAreaStart
        {
            get { return this.textAreaStart; }
        }

        /// <summary>
        /// Where the data area starts
        /// </summary>
        public uint DataAreaStart
        {
            get { return this.dataAreaStart; }
        }

        /// <summary>
        /// Returns the address of the entry point
        /// </summary>
        public uint EntryPoint
        {
            get { return this.entryPoint; }
        }

		/// <summary>
		/// Returns the symbol table
		/// </summary>
		public IDictionary<string, uint> SymbolTable
		{
			get { return this.symbolTable; }
		}

		/// <summary>
		/// Returns the function table.
		/// The function table contains the entry point (for call instruction) and their sizes (in number of instructions).
		/// </summary>
		public IDictionary<uint, int> FunctionTable
		{
			get { return this.functionTable; }
		}
        #endregion

        #region Methods
        /// <summary>
        /// Creates a new program
        /// </summary>
        /// <param name="instructions">The instructions</param>
        /// <param name="symbolTable">The symbol table</param>
		/// <param name="functionTable">The function table</param>
        /// <param name="dataArea">The data area</param>
        /// <returns>A new program</returns>
        public static Program NewProgram(Instruction[] instructions, IDictionary<string, uint> symbolTable, IDictionary<uint, int> functionTable, DataArea dataArea = null)
        {
            int programSize = instructions.Length * 4;
            uint textAreaStart = 0;

            if (dataArea == null)
            {
                dataArea = DataArea.EmptyArea(programSize);
            }

            programSize += dataArea.Start - (programSize - (int)textAreaStart);
            programSize += dataArea.Size;

            byte[] data = new byte[programSize];

            //Set the instructions
            for (int i = 0; i < instructions.Length; i++)
            {
                Instruction currentInstruction = instructions[i];
                int baseAddress = i * 4;

                data[baseAddress + 3] = (byte)(currentInstruction.Data >> 24 & 0xFF);
                data[baseAddress + 2] = (byte)(currentInstruction.Data >> 16 & 0xFF);
                data[baseAddress + 1] = (byte)(currentInstruction.Data >> 8 & 0xFF);
                data[baseAddress] = (byte)(currentInstruction.Data & 0xFF);
            }

            //Set the data
            foreach (DataVariable currentVariable in dataArea.DataTable)
            {
                if (currentVariable.DataType == DataType.Byte)
                {
                    data[currentVariable.Address] = currentVariable.Byte;
                }
                else
                {
                    uint baseAddress = currentVariable.Address;

                    data[baseAddress + 3] = (byte)(currentVariable.Word >> 24 & 0xFF);
                    data[baseAddress + 2] = (byte)(currentVariable.Word >> 16 & 0xFF);
                    data[baseAddress + 1] = (byte)(currentVariable.Word >> 8 & 0xFF);
                    data[baseAddress] = (byte)(currentVariable.Word & 0xFF);
                }
            }

            //Set the entry point
            uint entryPoint = textAreaStart;

            if (symbolTable.ContainsKey("main"))
            {
                entryPoint = symbolTable["main"];
            }

			return new Program(
				data,
				instructions.Length,
				textAreaStart,
				(uint)dataArea.Start,
				entryPoint,
				symbolTable,
				functionTable); 
        }
        
		/// <summary>
		/// Returns the instruction stored in the program
		/// </summary>
		public Instruction[] GetInstructions()
		{
			uint programEnd = (uint)(this.TextAreaStart + 4 * this.NumInstructions);
			List<Instruction> instructions = new List<Instruction>();

			uint address = this.TextAreaStart;

			while (address < programEnd)
			{
				byte byte1 = this.Data[address];
				byte byte2 = this.Data[address + 1];
				byte byte3 = this.Data[address + 2];
				byte byte4 = this.Data[address + 3];

				int instructionData = byte1 | (byte2 << 8) | (byte3 << 16) | (byte4 << 24);
				instructions.Add(new Instruction(instructionData));

				address += 4;
			}

			return instructions.ToArray();
		}
		#endregion

    }
}
