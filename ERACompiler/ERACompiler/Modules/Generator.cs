using ERACompiler.Structures;
using ERACompiler.Structures.Types;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ERACompiler.Utilities.Errors;
using ERACompiler.Modules.Generation;

namespace ERACompiler.Modules
{
    /// <summary>
    /// Generates the actual assembly code
    /// </summary>
    public class Generator
    {
        // The length of these blocks can be obtained by difference
        public readonly int staticDataAddrBase = 18;
        public int codeAddrBase = 18;

        public readonly bool[] regOccup = new bool[27]; // For reigster allocation algorithm (R27 is always used, no variable stored in R27)
        public readonly Dictionary<string, byte> regAllocVTR = new Dictionary<string, byte>(); // Variable-to-Register dictionary
        public readonly Dictionary<byte, string> regAllocRTV = new Dictionary<byte, string>(); // Register-to-Variable dictionary

        public readonly Dictionary<string, CodeConstructor> codeConstructors;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Generator()
        {
            codeConstructors = new Dictionary<string, CodeConstructor>
            {
                { "Program", new ProgramConstructor() },
                { "Code", new CodeBlockConstructor() },
                { "Expression", new ExpressionConstructor() },
                { "Variable definition", new VariableDefinitionConstructor() },
                { "Array definition", new ArrayDefinitionConstructor() },
                { "Assignment", new AssignmentConstructor() },
                { "If", new IfConstructor() },
                { "For", new ForConstructor() },
                { "Block body", new BlockBodyConstructor() },
                { "While", new WhileConstructor() },
                { "Loop While", new LoopWhileConstructor() },
                { "Routine body", new RoutineBodyConstructor() },
                { "Return", new ReturnConstructor() },
                { "Call", new CallConstructor() },
                { "Print", new PrintConstructor() },
                { "Reference", new ReferenceConstructor() },
                { "Dereference", new DereferenceConstructor() },
                { "Primary", new PrimaryConstructor() },
                { "Goto label", new GotoLabelConstructor() },
                { "Goto", new GotoConstructor() },
                { "Assembly block", new AssemblyBlockConstructor() },
                { "REGISTER", new RegisterConstructor() },
                { "NUMBER", new LiteralConstructor() },
                { "AllChildren", new AllChildrenConstructor() }
            };
        }

        /// <summary>
        /// Constructs a binary code (or assembly code) from a given AAST root.
        /// </summary>
        /// <param name="root">AAST root node</param>
        /// <returns>Corresponding binary code in byte array</returns>
        public static CodeNode GetProgramCodeNodeRoot(AASTNode root)
        {
            return  Program.currentCompiler.generator.codeConstructors[root.ASTType].Construct(root, null);
        }

        /*
            Helper functions
        */

        public void OccupateReg(byte regNum)
        {
            regOccup[regNum] = true;
        }
        
        public void FreeReg(byte regNum)
        {
            // If register is allocated to a variable - do not free it
            // It will be swapped if needed when calling GetFreeReg()
            if (!regAllocRTV.ContainsKey(regNum))
                regOccup[regNum] = false;
        }

        public static byte IdentifyRegister(string reg)
        {
            if (reg[0] == 'r')
            {
                if (reg.Length > 2)
                {
                    return (byte)(reg[2] - '0' + (reg[1] - '0') * 10);
                }
                else
                {
                    return (byte)(reg[1] - '0');
                }
            }
            else
            {
                return reg switch
                {
                    "pc" => 0xFF,
                    "sb" => 0xFE,
                    "sp" => 0xFD,
                    "fp" => 0xFC,
                    _ => 0x00,
                };
            }
        }

        public static string ConvertToAssemblyCode(LinkedList<byte> bincode, int offset, int padding = 0)
        {
            int i = -1;
            byte a = 0x00; // Since I want to print out bytes in groups of two
            StringBuilder asmCode = new StringBuilder();
            int LDAiter = 0;
            string tabs = "";
            if (padding > 0)
            {
                tabs = string.Concat(Enumerable.Repeat("\t", padding));
            }

            foreach (byte b in bincode)
            {
                i++;
                string ofst = (offset + i - 18 - i % 2).ToString();
                for (int j = 0; j < 8 - ofst.Length; j++)
                {
                    ofst = "0" + ofst;
                }
                ofst = "{" + ofst + "}";
                
                if (LDAiter > 0)
                {
                    LDAiter--;
                    if (LDAiter == 3)
                        asmCode.Append(tabs).Append('\"').Append(ofst).Append(" [f32] ");
                    asmCode.Append(BitConverter.ToString(new byte[] { b }));
                    if (i % 2 == 1) asmCode.Append(' ');
                    if (LDAiter == 0)
                    {
                        asmCode.Remove(asmCode.Length - 1, 1);
                        asmCode.Append("\",\r\n");
                    }
                    continue;
                }
                
                if (i % 2 == 1)
                {
                    int format = a >> 6 == 3 ? 32 : a >> 6 == 0 ? 8 : a >> 6 == 1 ? 16 : 2;
                    string sFormat = tabs + "\"" + ofst + " [f" +
                        (format.ToString().Length == 1 ? "0" + format.ToString() : format.ToString())
                        + "] ";
                    int op = (a & 0x3c) >> 2;
                    int bregi = ((a & 0x03) << 3) | (b >> 5);
                    int bregj = b & 0x1f;
                    string[] specRegs = new string[] { "FP", "SP", "SB", "PC" };
                    string regi = "R" + bregi.ToString();
                    string regj = "R" + bregj.ToString();
                    if (bregi > 27) regi = specRegs[bregi - 28];
                    if (bregj > 27) regj = specRegs[bregj - 28];
                    switch (op)
                    {
                        case 0: // SKIP / STOP
                            if (format == 8)
                            {
                                asmCode.Append(sFormat).Append("stop\",\r\n");
                            }
                            else if (format == 2)
                            {
                                asmCode.Append(sFormat).Append("PRINT ").Append(regi).Append("\",\r\n");
                            }
                            else
                            {
                                asmCode.Append(sFormat).Append("skip\",\r\n");
                            }
                            break;
                        case 1: // LD
                            asmCode.Append(sFormat).Append(regj).Append(" := ->").Append(regi).Append("\",\r\n");
                            break;
                        case 2: // LDC / LDA
                            if (format == 8)
                            {
                                asmCode.Append(sFormat).Append(regj).Append(" := ").Append(regi).Append(" + const\",\r\n");
                                LDAiter = 4;
                            }
                            else
                            {
                                asmCode.Append(sFormat).Append(regj).Append(" := ").Append(bregi).Append("\",\r\n");
                            }
                            break;
                        case 3: // ST
                            asmCode.Append(sFormat).Append("->").Append(regj).Append(" := ").Append(regi).Append("\",\r\n");
                            break;
                        case 4: // MOV
                            asmCode.Append(sFormat).Append(regj).Append(" := ").Append(regi).Append("\",\r\n");
                            break;
                        case 5: // ADD
                            asmCode.Append(sFormat).Append(regj).Append(" += ").Append(regi).Append("\",\r\n");
                            break;
                        case 6: // SUB
                            asmCode.Append(sFormat).Append(regj).Append(" -= ").Append(regi).Append("\",\r\n");
                            break;
                        case 7: // ASR
                            asmCode.Append(sFormat).Append(regj).Append(" >>= ").Append(regi).Append("\",\r\n");
                            break;
                        case 8: // ASL
                            asmCode.Append(sFormat).Append(regj).Append(" <<= ").Append(regi).Append("\",\r\n");
                            break;
                        case 9: // OR
                            asmCode.Append(sFormat).Append(regj).Append(" |= ").Append(regi).Append("\",\r\n");
                            break;
                        case 10: // AND
                            asmCode.Append(sFormat).Append(regj).Append(" &= ").Append(regi).Append("\",\r\n");
                            break;
                        case 11: // XOR
                            asmCode.Append(sFormat).Append(regj).Append(" ^= ").Append(regi).Append("\",\r\n");
                            break;
                        case 12: // LSL
                            asmCode.Append(sFormat).Append(regj).Append(" <= ").Append(regi).Append("\",\r\n");
                            break;
                        case 13: // LSR
                            asmCode.Append(sFormat).Append(regj).Append(" >= ").Append(regi).Append("\",\r\n");
                            break;
                        case 14: // CND
                            asmCode.Append(sFormat).Append(regj).Append(" ?= ").Append(regi).Append("\",\r\n");
                            break;
                        case 15: // CBR
                            asmCode.Append(sFormat).Append("if ").Append(regi).Append(" goto ").Append(regj).Append("\",\r\n");
                            break;
                        default:
                            asmCode.Append(tabs).Append("unknown\",\r\n");
                            break;
                    }
                }
                a = b;
            }

            asmCode.Remove(asmCode.Length - 3, 1); // Remove comma
            return asmCode.ToString();
        }
    }
}
