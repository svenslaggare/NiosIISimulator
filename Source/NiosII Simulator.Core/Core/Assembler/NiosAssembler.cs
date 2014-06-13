using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NiosII_Simulator.Core.Assembler
{
    /// <summary>
    /// Represents a Nios II assembler
    /// </summary>
    public class NiosAssembler
	{

		#region Fields
		private readonly IDictionary<string, Func<CurrentInstruction, Instruction>> instructionAssemblers;                                 //The instruction assemblers
		private readonly IDictionary<string, Func<CurrentInstruction, List<string[]>>> macros;                                             //The macros
		private static ISet<string> branchInstructions = new HashSet<string>()
        { 
            "br",
            "beq",
            "bne",
            "bgt",
            "blt",
            "bge",
            "ble",
        };
		#endregion

		#region Constructors
		private NiosAssembler()
        {
            this.instructionAssemblers = new Dictionary<string, Func<CurrentInstruction, Instruction>>();
            this.macros = new Dictionary<string, Func<CurrentInstruction, List<string[]>>>();

			#region Instruction Assemblers

			#region Instruction Assembler Generators
			Func<string, OperationCodes, int, Func<CurrentInstruction, Instruction>> rFormatGenerator = (instName, opCode, opxCode) =>
            {
                return inst =>
                {
                    if (inst.Operands.Length == 3)
                    {
                        Registers? regA = DecodeRegister(inst.Operands[2]);
                        Registers? regB = DecodeRegister(inst.Operands[1]);
                        Registers? regC = DecodeRegister(inst.Operands[0]);

                        if (regA.HasValue && regB.HasValue && regC.HasValue)
                        {
                            return new RFormatInstruction(
                                opCode.Code(),
                                opxCode,
                                regA.Value.Number(),
                                regB.Value.Number(),
                                regC.Value.Number()).AsInstruction();
                        }
                        else
                        {
                            throw new AssemblerException(inst.LineNumber, instName, "Invalid operands");
                        }
                    }
                    else
                    {
                        throw new AssemblerException(inst.LineNumber, instName, "There can be only 3 operands.");
                    }
                };
            };

            Func<string, OperationCodes, Func<CurrentInstruction, Instruction>> iFormatGenerator = (instName, opCode) =>
            {
                return inst =>
                {
                    if (inst.Operands.Length == 3)
                    {
                        Registers? regA = DecodeRegister(inst.Operands[1]);
                        Registers? regB = DecodeRegister(inst.Operands[0]);
                        short imm = 0;

                        if (regA.HasValue && regB.HasValue && short.TryParse(inst.Operands[2], out imm))
                        {
                            return new IFormatInstruction(
                                opCode.Code(),
                                regA.Value.Number(),
                                regB.Value.Number(),
                                imm).AsInstruction();
                        }
                        else
                        {
                            throw new AssemblerException(inst.LineNumber, instName, "Invalid operands");
                        }
                    }
                    else
                    {
                        throw new AssemblerException(inst.LineNumber, instName, "There can be only 3 operands.");
                    }
                };
            };
            #endregion

            #region Arithmetic
            instructionAssemblers.Add("add", rFormatGenerator("add", OperationCodes.Add, OperationXCodes.Add));
            instructionAssemblers.Add("addi", iFormatGenerator("addi", OperationCodes.Addi));

            instructionAssemblers.Add("sub", rFormatGenerator("sub", OperationCodes.Sub, OperationXCodes.Sub));
            macros.Add("subi", inst =>
            {
                if (inst.Operands.Length == 3)
                {
                    return new List<string[]>() { new string[] { "addi", inst.Operands[0], inst.Operands[1], "-" + inst.Operands[2] } };
                }
                else
                {
                    throw new AssemblerException(inst.LineNumber, "subi", "There can be only 3 operands.");
                }
            });
            #endregion

            #region Logic
            instructionAssemblers.Add("and", rFormatGenerator("and", OperationCodes.And, OperationXCodes.And));
            instructionAssemblers.Add("andi", iFormatGenerator("andi", OperationCodes.Andi));
            instructionAssemblers.Add("andhi", iFormatGenerator("andhi", OperationCodes.Andhi));

            instructionAssemblers.Add("or", rFormatGenerator("or", OperationCodes.Or, OperationXCodes.Or));
            instructionAssemblers.Add("ori", iFormatGenerator("ori", OperationCodes.Ori));
            instructionAssemblers.Add("orhi", iFormatGenerator("orhi", OperationCodes.Orhi));

            instructionAssemblers.Add("xor", rFormatGenerator("xor", OperationCodes.Xor, OperationXCodes.Xor));
            instructionAssemblers.Add("xori", iFormatGenerator("xori", OperationCodes.Xori));
            instructionAssemblers.Add("xorhi", iFormatGenerator("xorhi", OperationCodes.Xorhi));

            instructionAssemblers.Add("nor", rFormatGenerator("nor", OperationCodes.Nor, OperationXCodes.Nor));
            #endregion

            #region Shift

            #region Shift Imm Generator
            Func<string, OperationCodes, int, Func<CurrentInstruction, Instruction>> shiftImmGenerator = (instName, opCode, opxCode) =>
            {
                return inst =>
                {
                    if (inst.Operands.Length == 3)
                    {
                        Registers? regA = DecodeRegister(inst.Operands[1]);
                        Registers? regC = DecodeRegister(inst.Operands[0]);
                        int imm = 0;
                        bool correctImm = int.TryParse(inst.Operands[2], out imm);

                        if (imm < 0 || imm > Math.Pow(2, 5))
                        {
                            correctImm = false;
                        }

                        if (regA.HasValue && regC.HasValue && correctImm)
                        {
                            return new RFormatInstruction(
                                opCode.Code(),
                                opxCode | imm,
                                regA.Value.Number(),
                                0,
                                regC.Value.Number()).AsInstruction();
                        }
                        else
                        {
                            throw new AssemblerException(inst.LineNumber, instName, "Invalid operands");
                        }
                    }
                    else
                    {
                        throw new AssemblerException(inst.LineNumber, instName, "There can be only 3 operands.");
                    }
                };
            };
            #endregion

            instructionAssemblers.Add("sll", rFormatGenerator("sll", OperationCodes.Sll, OperationXCodes.Sll));
            instructionAssemblers.Add("srl", rFormatGenerator("srl", OperationCodes.Srl, OperationXCodes.Srl));
            instructionAssemblers.Add("slli", shiftImmGenerator("slli", OperationCodes.Slli, OperationXCodes.Slli));
            instructionAssemblers.Add("srli", shiftImmGenerator("srli", OperationCodes.Srli, OperationXCodes.Srli));
            #endregion

			#region Compare
			instructionAssemblers.Add("cmpeq", rFormatGenerator("cmpeq", OperationCodes.Cmpeq, OperationXCodes.Cmpeq));
			instructionAssemblers.Add("cmpne", rFormatGenerator("cmpne", OperationCodes.Cmpne, OperationXCodes.Cmpne));
			instructionAssemblers.Add("cmpge", rFormatGenerator("cmpge", OperationCodes.Cmpge, OperationXCodes.Cmpge));
			instructionAssemblers.Add("cmplt", rFormatGenerator("cmplt", OperationCodes.Cmplt, OperationXCodes.Cmplt));

			macros.Add("cmple", inst =>
			{
				if (inst.Operands.Length == 3)
				{
					return new List<string[]>() { new string[] { "cmpge", inst.Operands[1], inst.Operands[0], inst.Operands[2] } };
				}
				else
				{
					throw new AssemblerException(inst.LineNumber, "cmple", "There can be only 3 operands.");
				}
			});

			macros.Add("cmpgt", inst =>
			{
				if (inst.Operands.Length == 3)
				{
					return new List<string[]>() { new string[] { "cmplt", inst.Operands[1], inst.Operands[0], inst.Operands[2] } };
				}
				else
				{
					throw new AssemblerException(inst.LineNumber, "cmpgt", "There can be only 3 operands.");
				}
			});

			instructionAssemblers.Add("cmpeqi", iFormatGenerator("cmpeqi", OperationCodes.Cmpeqi));
			instructionAssemblers.Add("cmpnei", iFormatGenerator("cmpnei", OperationCodes.Cmpnei));
			instructionAssemblers.Add("cmpgei", iFormatGenerator("cmpgei", OperationCodes.Cmpgei));
			instructionAssemblers.Add("cmplti", iFormatGenerator("cmplti", OperationCodes.Cmplti));

			//macros.Add("cmplei", inst =>
			//{
			//	if (inst.Operands.Length == 3)
			//	{
			//		return new List<string[]>() { new string[] { "cmplti", inst.Operands[0], inst.Operands[1], inst.Operands[2] + "+1" } };
			//	}
			//	else
			//	{
			//		throw new AssemblerException(inst.LineNumber, "cmplei", "There can be only 3 operands.");
			//	}
			//});

			//macros.Add("cmpgti", inst =>
			//{
			//	if (inst.Operands.Length == 3)
			//	{
			//		return new List<string[]>() { new string[] { "cmpgei", inst.Operands[0], inst.Operands[1], inst.Operands[2] + "+1"  } };
			//	}
			//	else
			//	{
			//		throw new AssemblerException(inst.LineNumber, "cmpgti", "There can be only 3 operands.");
			//	}
			//});
			#endregion

			#region Branch

			#region Generator
			Func<string, OperationCodes, Func<CurrentInstruction, Instruction>> branchAssemblerGenerator = (instrName, opCode) =>
            {
                Func<CurrentInstruction, Instruction> instructionAssembler = inst =>
                {
                    if (inst.Operands.Length == 3)
                    {
                        Registers? regA = DecodeRegister(inst.Operands[0]);
                        Registers? regB = DecodeRegister(inst.Operands[1]);

						short offset = 0;
						bool couldParse = true;

						string targetStr = inst.Operands[2];
						string newOperand = "";

						if (this.ParseDirective(inst.LineNumber, "br", inst.SymbolTable, targetStr, out newOperand, true))
						{
							targetStr = newOperand;
						}

						//Check if an symbolic reference
						if (inst.SymbolTable.ContainsKey(targetStr))
						{
							uint symbolicValue = inst.SymbolTable[targetStr];
							offset = (short)(symbolicValue - ((inst.LineNumber + 1) * 4));
						}
						else
						{
							couldParse = short.TryParse(targetStr, out offset);
						}

                        if (regA.HasValue && regB.HasValue && couldParse)
                        {
                            return new IFormatInstruction(
                                opCode.Code(),
                                regA.Value.Number(),
                                regB.Value.Number(),
                                offset).AsInstruction();
                        }
                        else
                        {
                            throw new AssemblerException(inst.LineNumber, instrName, "Invalid operands");
                        }
                    }
                    else
                    {
                        throw new AssemblerException(inst.LineNumber, instrName, "There can be only 3 operands.");
                    }
                };

                return instructionAssembler;
            };
            #endregion

            instructionAssemblers.Add("br", inst =>
            {
                if (inst.Operands.Length == 1)
                {
                    short offset = 0;
                    bool couldParse = true;

					string targetStr = inst.Operands[0];
					string newOperand = "";

					if (this.ParseDirective(inst.LineNumber, "br", inst.SymbolTable, targetStr, out newOperand, true))
					{
						targetStr = newOperand;
					}

                    //Check if an symbolic reference
					if (inst.SymbolTable.ContainsKey(targetStr))
                    {
						uint symbolicValue = inst.SymbolTable[targetStr];
                        offset = (short)(symbolicValue - ((inst.LineNumber + 1) * 4));
                    }
                    else
                    {
						couldParse = short.TryParse(targetStr, out offset);
                    }

                    if (couldParse)
                    {
                        return new IFormatInstruction(
                            OperationCodes.Br.Code(),
                            0,
                            0,
                            offset).AsInstruction();
                    }
                    else
                    {
                        throw new AssemblerException(inst.LineNumber, "br", "Invalid operands");
                    }
                }
                else
                {
                    throw new AssemblerException(inst.LineNumber, "br", "There can be only 1 operands.");
                }
            });

            instructionAssemblers.Add("bne", branchAssemblerGenerator("bne", OperationCodes.Bne));
            instructionAssemblers.Add("beq", branchAssemblerGenerator("beq", OperationCodes.Beq));
            instructionAssemblers.Add("bge", branchAssemblerGenerator("bge", OperationCodes.Bge));
            instructionAssemblers.Add("blt", branchAssemblerGenerator("blt", OperationCodes.Blt));

            macros.Add("bgt", inst =>
            {
                if (inst.Operands.Length == 3)
                {
                    return new List<string[]>() { new string[] { "blt", inst.Operands[1], inst.Operands[0], inst.Operands[2] } };
                }
                else
                {
                    throw new AssemblerException(inst.LineNumber, "bgt", "There can be only 2 operands.");
                }
            });

            macros.Add("ble", inst =>
            {
                if (inst.Operands.Length == 3)
                {
                    return new List<string[]>() { new string[] { "bge", inst.Operands[1], inst.Operands[0], inst.Operands[2] } };
                }
                else
                {
                    throw new AssemblerException(inst.LineNumber, "ble", "There can be only 2 operands.");
                }
            });
            #endregion

            #region Move Instructions
            macros.Add("movi", inst =>
            {
                if (inst.Operands.Length == 2)
                {
                    return new List<string[]>() { new string[] { "addi", inst.Operands[0], "r0", inst.Operands[1] } };
                }
                else
                {
                    throw new AssemblerException(inst.LineNumber, "movi", "There can be only 2 operands.");
                }
            });

            macros.Add("mov", inst =>
            {
                if (inst.Operands.Length == 2)
                {
                    return new List<string[]>() { new string[] { "add", inst.Operands[0], inst.Operands[1], "r0" } };
                }
                else
                {
                    throw new AssemblerException(inst.LineNumber, "movi", "There can be only 2 operands.");
                }
            });

            macros.Add("movia", inst =>
            {
                if (inst.Operands.Length == 2)
                {
                    return new List<string[]>()
                    {
                        new string[] { "orhi", inst.Operands[0], "r0", string.Format("%hiadj({0})", inst.Operands[1]) },
                        new string[] { "addi", inst.Operands[0], inst.Operands[0], string.Format("%lo({0})", inst.Operands[1]) }
                    };
                }
                else
                {
                    throw new AssemblerException(inst.LineNumber, "movia", "There can be only 2 operands.");
                }
            });
            #endregion

            #region Memory

            #region Generator
            Func<string, OperationCodes, Func<CurrentInstruction, Instruction>> memoryGenerator = (instName, opCode) =>
            {
                return inst =>
                {
                    if (inst.Operands.Length == 2)
                    {
                        Registers? regB = DecodeRegister(inst.Operands[0]);

                        //Extract regA and the offset from the second operand
                        Match secondOp = Regex.Match(inst.Operands[1], @"(.+)\((.+)\)");
                        bool couldParse = false;
                        Registers? regA = null;
                        short offset = 0;

                        if (secondOp.Success)
                        {
                            regA = DecodeRegister(secondOp.Groups[2].Value);
                            couldParse = short.TryParse(secondOp.Groups[1].Value, out offset);
                        }

                        if (regA.HasValue && regB.HasValue && couldParse)
                        {
                            return new IFormatInstruction(
                                opCode.Code(),
                                regA.Value.Number(),
                                regB.Value.Number(),
                                offset).AsInstruction();
                        }
                        else
                        {
                            throw new AssemblerException(inst.LineNumber, instName, "Invalid operands");
                        }
                    }
                    else
                    {
                        throw new AssemblerException(inst.LineNumber, instName, "There can be only 2 operands.");
                    }
                };
            };
            #endregion

            instructionAssemblers.Add("stw", memoryGenerator("stw", OperationCodes.Stw));
            instructionAssemblers.Add("ldw", memoryGenerator("ldw", OperationCodes.Ldw));
			instructionAssemblers.Add("ldb", memoryGenerator("ldb", OperationCodes.Ldb));
			instructionAssemblers.Add("ldbu", memoryGenerator("ldbu", OperationCodes.Ldbu));
			instructionAssemblers.Add("stb", memoryGenerator("stb", OperationCodes.Stb));
            #endregion

            #region Call/Return
            instructionAssemblers.Add("call", inst =>
            {
                if (inst.Operands.Length == 1)
                {
                    int offset = 0;
                    bool couldParse = true;

                    //Check if an symbolic reference
                    if (inst.SymbolTable.ContainsKey(inst.Operands[0]))
                    {
                        offset = (int)inst.SymbolTable[inst.Operands[0]];
                    }
                    else
                    {
                        couldParse = int.TryParse(inst.Operands[0], out offset);
                    }

                    if (couldParse)
                    {
                        //Fix the offset
                        offset = offset / 4;

                        return new JFormatInstruction(
                            OperationCodes.Call.Code(),
                            offset).AsInstruction();
                    }
                    else
                    {
                        throw new AssemblerException(inst.LineNumber, "call", "Invalid operands");
                    }
                }
                else
                {
                    throw new AssemblerException(inst.LineNumber, "call", "There can be only 1 operands.");
                }
            });

            instructionAssemblers.Add("ret", inst =>
            {
                if (inst.Operands.Length == 0)
                {
                    return new RFormatInstruction(
                        OperationCodes.Ret.Code(),
                        OperationXCodes.Ret,
                        Registers.RA.Number(),
                        0,
                        0).AsInstruction();
                        
                }
                else
                {
                    throw new AssemblerException(inst.LineNumber, "ret", "There can be only 0 operands.");
                }
            });
            #endregion

            #region Other
            macros.Add("nop", inst =>
            {
                if (inst.Operands.Length == 0)
                {
                    return new List<string[]>() { new string[] { "add", "r0", "r0", "r0" } };
                }
                else
                {
                    throw new AssemblerException(inst.LineNumber, "nop", "There can be only 0 operands.");
                }
            });
            #endregion

			#endregion
		}

		/// <summary>
		/// Creates a new Nios II assembler
		/// </summary>
		public static NiosAssembler New()
		{
			return new NiosAssembler();
		}
		#endregion

		#region Properties
		
		#endregion

		#region Help Classes
		/// <summary>
		/// The current instruction to assemble
		/// </summary>
		private class CurrentInstruction
		{
			/// <summary>
			/// The line number
			/// </summary>
			public int LineNumber { get; set; }

			/// <summary>
			/// The operands
			/// </summary>
			public string[] Operands { get; set; }

			/// <summary>
			/// The symbol table
			/// </summary>
			public Dictionary<string, uint> SymbolTable { get; set; }
		}
		#endregion

		#region Methods

		#region Helpers
		/// <summary>
		/// Decodes the register from the given string
		/// </summary>
		/// <param name="registerString">The string</param>
		/// <returns>The register or null</returns>
		private Registers? DecodeRegister(string registerString)
		{
			Registers register = Registers.R0;

			if (Enum.TryParse<Registers>(registerString, true, out register))
			{
				return register;
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Returns the key for the given value in the given dictionary
		/// </summary>
		/// <typeparam name="TKey">The type of the key</typeparam>
		/// <typeparam name="TValue">The type of the value</typeparam>
		/// <param name="dictionary">The dictionary</param>
		/// <param name="value">The value</param>
		/// <returns>The key or the defult value</returns>
		private TKey KeyFor<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TValue value)
		{
			foreach (var currentKeyValue in dictionary)
			{
				if (currentKeyValue.Value.Equals(value))
				{
					return currentKeyValue.Key;
				}
			}

			return default(TKey);
		}

		/// <summary>
		/// Removes the comments from the given line
		/// </summary>
		/// <param name="line">The line</param>
		public static string RemoveComments(string line)
		{
			Match match = Regex.Match(line, @"([a-zA-Z0-9\s,():\-\.%]*)(\s)*(#.*)?");

			if (match.Success)
			{
				return match.Groups[1].Value;
			}

			return null;
		}

		/// <summary>
		/// Extracts the instruction and operand from the given line
		/// </summary>
		/// <param name="line">The line</param>
		/// <returns>The first element is the instruction, the rest the operands</returns>
		public static string[] ExtractInstructionAndOperands(string line)
		{
			var splitedLine = Regex.Split(line, @"((\s)*,(\s)*)|((\s)+)");

			return splitedLine.Where(currentLine =>
			{
				return !Regex.IsMatch(currentLine, @",|((\s)+)") && currentLine != "";
			}).ToArray();
		}
		#endregion

		#region Assemble/Disassemble Methods
		/// <summary>
		/// Parses the given directive
		/// </summary>
		/// <param name="lineNum">The line number</param>
		/// <param name="instruction">The instruction</param>
		/// <param name="operand">The operand</param>
		/// <param name="symbolTable">The symbol table</param>
		/// <param name="newOperand">The new operand if parsed</param>
		/// <param name="isBranchInst">If the current instruction is a branch instruction</param>
		/// <returns>True if parsed else false</returns>
		private bool ParseDirective(int lineNum, string instruction, Dictionary<string, uint> symbolTable, string operand, out string newOperand, bool isBranchInst = false)
		{
			//Check if directive
			if (operand.StartsWith("%"))
			{
				Match directiveMatch = Regex.Match(operand, "%([a-zA-Z0-9]+)\\(([a-zA-Z0-9]+)\\)");

				if (directiveMatch.Groups.Count == 3)
				{
					string directive = directiveMatch.Groups[1].ToString();
					string valueStr = directiveMatch.Groups[2].ToString();
					int value = 0;

					if (int.TryParse(valueStr, out value))
					{
						switch (directive)
						{
							case "lo":
								value = value & 0xFFFF;
								break;
							case "hi":
								value = (value >> 16) & 0xFFFF;
								break;
							case "hiadj":
								value = ((value >> 16) & 0xFFFF) + ((value >> 15) & 0x1);
								break;
							default:
								throw new AssemblerException(lineNum, instruction, "Assembler macro '" + directive + "' not found.");
						}

						if (isBranchInst)
						{
							value -= (lineNum + 1) * 4;
						}

						newOperand = value.ToString();
						return true;
					}
					else
					{
						if (!symbolTable.ContainsKey(valueStr))
						{
							//Error handling
							throw new AssemblerException(lineNum, instruction, "Invalid integer value for assembler macro.");
						}
						else
						{
							return this.ParseDirective(
								lineNum,
								instruction,
								symbolTable,
								string.Format("%{0}({1})", directive, symbolTable[valueStr]),
								out newOperand,
								isBranchInst);
						}
					}
				}
			}

			newOperand = "";
			return false;
		}

		/// <summary>
		/// The first pass for the assembler
		/// </summary>
		private void FirstPass(string[] lines, List<string[]> instructionAndOperandList, Dictionary<string, uint> symbolTable, HashSet<string> functions)
		{
			int currentLineNum = 0;
			for (int i = 0; i < lines.Length; i++)
			{
				string currentLine = lines[i];

				//Get the instruction and operands
				string[] instructionAndOperands = ExtractInstructionAndOperands(RemoveComments(currentLine));

				//Ignore empty lines
				if (instructionAndOperands.Length > 0)
				{
					bool handled = false;

					//Must be atleast 1 token
					if (instructionAndOperands.Length >= 1)
					{
						//Check if starts with an label
						if (instructionAndOperands[0].EndsWith(":"))
						{
							string label = instructionAndOperands[0].Substring(0, instructionAndOperands[0].Length - 1);
							symbolTable.Add(label, (uint)currentLineNum * 4);

							//Must be atleast 2 tokens
							if (instructionAndOperands.Length >= 2)
							{
								instructionAndOperands = instructionAndOperands.Skip(1).ToArray();
								handled = true;
							}

							//Only a label, insert a NOP
							if (instructionAndOperands.Length == 1)
							{
								instructionAndOperands = new string[] { "nop" };
								handled = true;
							}
						}

						//Check if a marco
						if (macros.ContainsKey(instructionAndOperands[0]))
						{
							//Get the marco
							var marco = macros[instructionAndOperands[0]];

							CurrentInstruction currentInstruction = new CurrentInstruction()
							{
								LineNumber = i,
								Operands = instructionAndOperands.Skip(1).ToArray(),
							};

							instructionAndOperandList.AddRange(marco(currentInstruction));
							handled = true;
						}
						else
						{
							instructionAndOperandList.Add(instructionAndOperands);
							handled = true;
						}
					}

					if (!handled)
					{
						throw new AssemblerException(i, currentLine, string.Format("Unhandled instruction \"{0}\"", currentLine));
					}

					if (handled)
					{
						//Check if call instruction
						if (instructionAndOperands[0] == "call" && instructionAndOperands.Length >= 2)
						{
							//Add the called function to the list of functions
							functions.Add(instructionAndOperands[1]);
						}
					}

					currentLineNum++;
				}
			}
		}

		/// <summary>
		/// The second pass for the assembler
		/// </summary>
		private void SecondPass(List<Instruction> instructions, List<string[]> instructionAndOperandList,
			Dictionary<string, uint> symbolTable, HashSet<string> functions, Dictionary<uint, int> functionTable)
		{
			//The function table (entry point and sizes for functions, needed for the jitter).
			bool isFunction = false;
			uint currentFunc = 0;
			int funcSize = 0;

			//Begin the second pass
			for (int lineNum = 0; lineNum < instructionAndOperandList.Count; lineNum++)
			{
				string[] currentLine = instructionAndOperandList[lineNum];
				string instruction = currentLine[0].ToLower();

				//Find the decoder
				if (instructionAssemblers.ContainsKey(instruction))
				{
					//Proccess the operands
					string[] operands = currentLine.Skip(1).Select(operand =>
					{
						if (!branchInstructions.Contains(instruction))
						{
							//Check if directive
							string newOperand = "";
							bool isDirective = this.ParseDirective(lineNum, instruction, symbolTable, operand, out newOperand);

							if (isDirective)
							{
								operand = newOperand;
							}
						}

						if (symbolTable.ContainsKey(operand) && !branchInstructions.Contains(instruction))
						{
							//Replace label with its value
							return symbolTable[operand].ToString();
						}
						else
						{
							return operand;
						}
					}).ToArray();

					var instructionDecoder = instructionAssemblers[instruction];
					CurrentInstruction currentInstruction = new CurrentInstruction()
					{
						LineNumber = lineNum,
						Operands = operands,
						SymbolTable = symbolTable
					};

					instructions.Add(instructionDecoder(currentInstruction));

					//Check if the symbol table has an entry for the current instruction
					uint address = (uint)lineNum * 4;
					if (symbolTable.ContainsValue(address))
					{
						if (!isFunction)
						{
							string funcName = this.KeyFor(symbolTable, address);

							//Check if func
							if (functions.Contains(funcName))
							{
								isFunction = true;
								funcSize = 0;
								currentFunc = address / 4;
							}
						}
					}

					if (isFunction)
					{
						funcSize += 1;

						if (currentLine[0] == "ret")
						{
							isFunction = false;
							functionTable.Add(currentFunc, funcSize);
						}
					}
				}
				else
				{
					throw new AssemblerException(lineNum, instruction, string.Format("Could not instruction assembler for instruction \"{0}\"", instruction));
				}
			}
		}

		/// <summary>
		/// Assembles a program from the given lines
		/// </summary>
		/// <param name="content">The lines</param>
		/// <param name="callTable">The call table</param>
		/// <returns>The assembled program</returns>
		public Program Assemble(string[] lines, IDictionary<string, uint> callTable = null)
		{
			//The instructions
			List<Instruction> instructions = new List<Instruction>();

			Dictionary<string, uint> symbolTable = new Dictionary<string, uint>();
			List<string[]> instructionAndOperandList = new List<string[]>();
			HashSet<string> functions = new HashSet<string>();

			//The function table (entry point and sizes for functions, needed for the jitter).
			Dictionary<uint, int> functionTable = new Dictionary<uint, int>();

			//Insert the call table
			if (callTable != null)
			{
				foreach (var current in callTable)
				{
					symbolTable.Add(current.Key, current.Value);
				}
			}

			this.FirstPass(lines, instructionAndOperandList, symbolTable, functions);
			this.SecondPass(instructions, instructionAndOperandList, symbolTable, functions, functionTable);

			return Program.NewProgram(instructions.ToArray(), symbolTable, functionTable);
		}

		/// <summary>
		/// Assembles a program from the given content
		/// </summary>
		/// <param name="content">The content</param>
		/// <param name="callTable">The call table</param>
		/// <returns>The assembled program</returns>
		public Program Assemble(string content, IDictionary<string, uint> callTable = null)
		{
			//Split the content into lines
			string[] lines = Regex.Split(content, "(\n|\r\n)");
			return this.Assemble(lines, callTable);
		}

		/// <summary>
		/// Assembles a program from the given lines
		/// </summary>
		/// <param name="content">The lines</param>
		/// <returns>The assembled program</returns>
		public Program AssembleFromLines(params string[] lines)
		{
			return this.Assemble(lines);
		}

		/// <summary>
		/// Disassembles the given program into text lines
		/// </summary>
		/// <param name="program">The program</param>
		/// <returns>The lines</returns>
		public string[] Disassemble(Program program)
		{
			List<string> lines = new List<string>();

			for (int i = 0; i < program.NumInstructions; i++)
			{
				uint instructionStart = (uint)i * 4 + program.TextAreaStart;

				byte byte1 = program.Data[instructionStart];
				byte byte2 = program.Data[instructionStart + 1];
				byte byte3 = program.Data[instructionStart + 2];
				byte byte4 = program.Data[instructionStart + 3];

				int currentInstruction = byte1 | (byte2 << 8) | (byte3 << 16) | (byte4 << 24);
			}

			return lines.ToArray();
		}
		#endregion

		#endregion

	}
}
