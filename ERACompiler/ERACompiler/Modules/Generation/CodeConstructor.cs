using ERACompiler.Structures;
using ERACompiler.Structures.Types;
using ERACompiler.Utilities.Errors;
using System;
using System.Collections.Generic;

namespace ERACompiler.Modules.Generation
{
    /// <summary>
    /// Main class used to construct the binary code (in a form of Code Nodes) from AAST nodes.
    /// </summary>
    public class CodeConstructor
    {
        protected int codeAddrBase = 18; // Code is located right after static data
        protected readonly int staticDataAddrBase = 18; // Always the same
        protected readonly int FP = 28, SP = 29, SB = 30, PC = 31; // Special registers

        /// <summary>
        /// The main method that is usually overwritten in child classes (Code Constructors).
        /// Given an AAST node and (possible) CodeNode parent constructs (recursively) Code Node 
        /// that contains either other child Code Nodes or generated binary code inside.
        /// </summary>
        /// <param name="aastNode">An AAST node based on which CodeNode is constructed.</param>
        /// <param name="parent">Possible CodeNode parent (is needed to build CodeNode tree).</param>
        /// <returns>New constructed CodeNode.</returns>
        public virtual CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            if (Program.currentCompiler.generator.codeConstructors.ContainsKey(aastNode.ASTType))
            {
                return Program.currentCompiler.generator.codeConstructors[aastNode.ASTType].Construct(aastNode, parent);
            }
            else
            {
                return Program.currentCompiler.generator.codeConstructors["AllChildren"].Construct(aastNode, parent);
            }
        }
            
        /// <summary>
        /// Convenient way of getting Linked List of bytes from a regular set of bytes or array of bytes.
        /// </summary>
        /// <param name="bytes">Set of bytes, or array of bytes.</param>
        /// <returns>Linked List containing the same given bytes.</returns>
        protected LinkedList<byte> GetLList(params byte[] bytes)
        {
            return new LinkedList<byte>(bytes);
        }

        /// <summary>
        /// Generates bytes (2 bytes) stored into Linked List given a format, operation code, and two register numbers.
        /// ERA assembly command format is the same for all commands:
        /// 2 bits are format, 4 bits are operation code, 5 bits and 5 bits are two register numbers.
        /// All in all, 16 bits or 2 bytes.
        /// </summary>
        /// <param name="format">Format of the command (32, 16, or 8)</param>
        /// <param name="opCode">Operation code of the command (0-15)</param>
        /// <param name="regI">i register (0-31)</param>
        /// <param name="regJ">j register (0-31)</param>
        /// <returns>A Linked List with two bytes generated using input data representing a single ERA assembly command.</returns>
        protected LinkedList<byte> GenerateCommand(int format, int opCode, int regI, int regJ)
        {
            // In LDL or PRINT case. Special case since they are pseudo-commands 
            // and such format (format of 2) will not appear in the resulting binary file.
            if (format != 2)
            {
                format = format == 32 ? 3 : format == 16 ? 1 : 0;                
            }
            return GetLList(
                (byte)((opCode << 2) | (format << 6) | (regI >> 3)),
                (byte)(regJ | (regI << 5))
                );
        }

        /// <summary>
        /// Converts given constants of some type to the Linked List of bytes (4 bytes)
        /// which represens given constant.
        /// </summary>
        /// <typeparam name="T">int, uint, or ulong</typeparam>
        /// <param name="constant">Constant to covert to bytes</param>
        /// <returns>A Linked List of bytes representing given constant.</returns>
        protected LinkedList<byte> GetConstBytes<T>(T constant)
        {
            if (constant is int _int)
            {
                byte[] r = BitConverter.GetBytes(_int); // Reversed
                return GetLList(r[3], r[2], r[1], r[0]);
            }
            if (constant is uint _uint)
            {
                byte[] r = BitConverter.GetBytes(_uint); // Reversed
                return GetLList(r[3], r[2], r[1], r[0]);
            }
            if (constant is ulong _ulong)
            {
                byte[] r = BitConverter.GetBytes(_ulong); // Reversed
                return GetLList(r[3], r[2], r[1], r[0]);
            }
            return new LinkedList<byte>();
        }

        /// <summary>
        /// Generates a STOP command.
        /// </summary>
        /// <returns>A Linked List of 2 bytes representing a command.</returns>
        protected LinkedList<byte> GenerateSTOP()
        {
            return GenerateCommand(8, 0, 0, 0);
        }

        /// <summary>
        /// Generates a SKIP command.
        /// </summary>
        /// <returns>A Linked List of 2 bytes representing a command.</returns>
        protected LinkedList<byte> GenerateSKIP()
        {
            return GenerateCommand(16, 0, 0, 0);
        }

        /// <summary>
        /// Generates a CBR command.
        /// </summary>
        /// <param name="regI">i register</param>
        /// <param name="regJ">j register</param>
        /// <returns>A Linked List of 2 bytes representing a command.</returns>
        protected LinkedList<byte> GenerateCBR(int regI, int regJ)
        {
            return GenerateCommand(32, 15, regI, regJ);
        }

        /// <summary>
        /// Generates a CND command.
        /// </summary>
        /// <param name="regI">i register</param>
        /// <param name="regJ">j register</param>
        /// <param name="format">Command format</param>
        /// <returns>A Linked List of 2 bytes representing a command.</returns>
        protected LinkedList<byte> GenerateCND(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 14, regI, regJ);
        }

        /// <summary>
        /// Generates a LSR command.
        /// </summary>
        /// <param name="regI">i register</param>
        /// <param name="regJ">j register</param>
        /// <param name="format">Command format</param>
        /// <returns>A Linked List of 2 bytes representing a command.</returns>
        protected LinkedList<byte> GenerateLSR(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 13, regI, regJ);
        }

        /// <summary>
        /// Generates a LSL command.
        /// </summary>
        /// <param name="regI">i register</param>
        /// <param name="regJ">j register</param>
        /// <param name="format">Command format</param>
        /// <returns>A Linked List of 2 bytes representing a command.</returns>
        protected LinkedList<byte> GenerateLSL(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 12, regI, regJ);
        }

        /// <summary>
        /// Generates a XOR command.
        /// </summary>
        /// <param name="regI">i register</param>
        /// <param name="regJ">j register</param>
        /// <param name="format">Command format</param>
        /// <returns>A Linked List of 2 bytes representing a command.</returns>
        protected LinkedList<byte> GenerateXOR(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 11, regI, regJ);
        }

        /// <summary>
        /// Generates a AND command.
        /// </summary>
        /// <param name="regI">i register</param>
        /// <param name="regJ">j register</param>
        /// <param name="format">Command format</param>
        /// <returns>A Linked List of 2 bytes representing a command.</returns>
        protected LinkedList<byte> GenerateAND(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 10, regI, regJ);
        }

        /// <summary>
        /// Generates a OR command.
        /// </summary>
        /// <param name="regI">i register</param>
        /// <param name="regJ">j register</param>
        /// <param name="format">Command format</param>
        /// <returns>A Linked List of 2 bytes representing a command.</returns>
        protected LinkedList<byte> GenerateOR(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 9, regI, regJ);
        }

        /// <summary>
        /// Generates a ASL command.
        /// </summary>
        /// <param name="regI">i register</param>
        /// <param name="regJ">j register</param>
        /// <param name="format">Command format</param>
        /// <returns>A Linked List of 2 bytes representing a command.</returns>
        protected LinkedList<byte> GenerateASL(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 8, regI, regJ);
        }

        /// <summary>
        /// Generates a ASR command.
        /// </summary>
        /// <param name="regI">i register</param>
        /// <param name="regJ">j register</param>
        /// <param name="format">Command format</param>
        /// <returns>A Linked List of 2 bytes representing a command.</returns>
        protected LinkedList<byte> GenerateASR(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 7, regI, regJ);
        }

        /// <summary>
        /// Generates a SUB command.
        /// </summary>
        /// <param name="regI">i register</param>
        /// <param name="regJ">j register</param>
        /// <param name="format">Command format</param>
        /// <returns>A Linked List of 2 bytes representing a command.</returns>
        protected LinkedList<byte> GenerateSUB(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 6, regI, regJ);
        }

        /// <summary>
        /// Generates a ADD command.
        /// </summary>
        /// <param name="regI">i register</param>
        /// <param name="regJ">j register</param>
        /// <param name="format">Command format</param>
        /// <returns>A Linked List of 2 bytes representing a command.</returns>
        protected LinkedList<byte> GenerateADD(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 5, regI, regJ);
        }

        /// <summary>
        /// Generates a MOV command.
        /// </summary>
        /// <param name="regI">i register</param>
        /// <param name="regJ">j register</param>
        /// <param name="format">Command format</param>
        /// <returns>A Linked List of 2 bytes representing a command.</returns>
        protected LinkedList<byte> GenerateMOV(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 4, regI, regJ);
        }

        /// <summary>
        /// Generates a ST command.
        /// </summary>
        /// <param name="regI">i register</param>
        /// <param name="regJ">j register</param>
        /// <returns>A Linked List of 2 bytes representing a command.</returns>
        protected LinkedList<byte> GenerateST(int regI, int regJ)
        {
            return GenerateCommand(32, 3, regI, regJ);
        }
        
        /// <summary>
        /// Generates a LDL pseudo-command.
        /// </summary>
        /// <param name="reg">register to store to a label address</param>
        /// <param name="address">label address</param>
        /// <returns>A Linked List of 2 bytes representing a command.</returns>
        protected LinkedList<byte> GenerateLDL(int reg, int address)
        {
            LinkedList<byte> toReturn = new LinkedList<byte>(GenerateLDC(0, reg));
            foreach (byte b in GenerateLDA(reg, reg, address)) 
            {
                toReturn.AddLast(b);
            }
            return toReturn;
        }

        /// <summary>
        /// Generates a LDA pseudo-command.
        /// </summary>
        /// <param name="regI">i register</param>
        /// <param name="regJ">j register</param>
        /// <param name="constant">constant to load</param>
        /// <returns>A Linked List of 2 bytes representing a command.</returns>
        protected LinkedList<byte> GenerateLDA(int regI, int regJ, int constant)
        {
            LinkedList<byte> toReturn = new LinkedList<byte>(GenerateCommand(8, 2, regI, regJ));
            foreach (byte b in GetConstBytes(constant))
            {
                toReturn.AddLast(b);
            }
            return toReturn;
        }

        /// <summary>
        /// Generates a LDC command.
        /// </summary>
        /// <param name="constant">constant to load</param>
        /// <param name="regJ">register to store to a constant</param>
        /// <returns>A Linked List of 2 bytes representing a command.</returns>
        protected LinkedList<byte> GenerateLDC(int constant, int regJ) // Constant is in range [0..31]
        {
            return GenerateCommand(32, 2, constant, regJ);
        }

        /// <summary>
        /// Generates a LD command.
        /// </summary>
        /// <param name="regI">i register</param>
        /// <param name="regJ">j register</param>
        /// <returns>A Linked List of 2 bytes representing a command.</returns>
        protected LinkedList<byte> GenerateLD(int regI, int regJ)
        {
            return GenerateCommand(32, 1, regI, regJ);
        }

        /// <summary>
        /// Generates a PRINT command.
        /// </summary>
        /// <param name="reg">register to be printed out</param>
        /// <returns>A Linked List of 2 bytes representing a command.</returns>
        protected LinkedList<byte> GeneratePRINT(int reg)
        {
            return GenerateCommand(2, 0, reg, 0);
        }

        /// <summary>
        /// Constructs a Code Node that has generated binary code of loading a given variable to the given register.
        /// </summary>
        /// <param name="parent">Parent Code Node.</param>
        /// <param name="varName">Variable name to be loaded into register.</param>
        /// <param name="reg">Register to load to given variable.</param>
        /// <param name="ctx">Current context.</param>
        /// <returns>Constructed Code Node that represents binary code that load from the stack given variable to the given register.</returns>
        protected CodeNode GetLoadVariableNode(CodeNode? parent, string varName, byte reg, Context? ctx)
        {
            CodeNode loadVarNode = new CodeNode("Load variable \'" + varName + "\' into R" + reg.ToString(), parent);

            int bytesToLoad = ctx.GetVarType(varName).GetSize();

            if (ctx.IsVarGlobal(varName))
            {
                loadVarNode.Children.AddLast(
                    new CodeNode("load var cmds 1", loadVarNode)
                    .Add(GenerateLDA(SB, 27, ctx.GetStaticOffset(varName) - 4 + bytesToLoad))
                    .Add(GenerateLD(27, reg)));
                if (bytesToLoad < 4)
                {
                    int mask = (int) Math.Pow(256, bytesToLoad) - 1;
                    CodeNode frNode = GetFreeRegisterNode(ctx, loadVarNode);
                    byte fr = frNode.ByteToReturn;
                    loadVarNode.Children.AddLast(frNode);
                    loadVarNode.Children.AddLast(
                        new CodeNode("load var cmds 2", loadVarNode)
                        .Add(GenerateLDC(0, fr))
                        .Add(GenerateLDA(fr, fr, mask))
                        .Add(GenerateAND(fr, reg)));
                    Program.currentCompiler.generator.FreeReg(fr);
                }
            }
            else
            {
                int blockOffset = ctx.GetVarDeclarationBlockOffset(varName);
                loadVarNode.Children.AddLast(new CodeNode("load var cmds 1", loadVarNode).Add(GenerateMOV(FP, 27)));          
                for (int i = 0; i < blockOffset; i++)
                {
                    loadVarNode.Children.AddLast(new CodeNode("load var cmds block offset", loadVarNode)
                        .Add(GenerateLDA(27, 27, -4))
                        .Add(GenerateLD(27, 27)));
                }
                loadVarNode.Children.AddLast(new CodeNode("load var cmds 2", loadVarNode)
                    .Add(GenerateLDA(27, 27, ctx.GetFrameOffset(varName) - 4 + bytesToLoad))
                    .Add(GenerateLD(27, reg)));
                if (bytesToLoad < 4)
                {
                    int mask = (int) Math.Pow(256, bytesToLoad) - 1;
                    CodeNode frNode = GetFreeRegisterNode(ctx, loadVarNode);
                    byte fr = frNode.ByteToReturn;
                    loadVarNode.Children.AddLast(frNode);
                    loadVarNode.Children.AddLast(
                        new CodeNode("load var cmds 3", loadVarNode)
                        .Add(GenerateLDC(0, fr))
                        .Add(GenerateLDA(fr, fr, mask))
                        .Add(GenerateAND(fr, reg)));
                    Program.currentCompiler.generator.FreeReg(fr);
                }
            }
            
            return loadVarNode;
        }

        /// <summary>
        /// Constructs a Code Node that has generated binary code of loading a given variable address (on the stack) to the given register.
        /// </summary>
        /// <param name="parent">Parent Code Node.</param>
        /// <param name="varName">Variable name which address is to be loaded into register.</param>
        /// <param name="reg">Register to load to given variable address.</param>
        /// <param name="ctx">Current context.</param>
        /// <returns>Constructed Code Node that represents binary code that loads given variable address to the given register.</returns>
        protected CodeNode GetLoadVariableAddressNode(CodeNode? parent, string varName, byte reg, Context? ctx)
        {
            CodeNode loadVarNode = new CodeNode("Load variable address \'" + varName + "\' into R" + reg.ToString(), parent);

            if (ctx.IsVarGlobal(varName))
            {
                loadVarNode
                    .Add(GenerateLDA(SB, 27, ctx.GetStaticOffset(varName)))
                    .Add(GenerateMOV(27, reg));
            }
            else
            {
                int blockOffset = ctx.GetVarDeclarationBlockOffset(varName);
                loadVarNode.Add(GenerateMOV(FP, 27));          
                for (int i = 0; i < blockOffset; i++)
                {
                    loadVarNode
                        .Add(GenerateLDA(27, 27, -4))
                        .Add(GenerateLD(27, 27));
                }
                loadVarNode
                    .Add(GenerateLDA(27, 27, ctx.GetFrameOffset(varName)))
                    .Add(GenerateMOV(27, reg));
            }
            
            return loadVarNode;
        }

        /// <summary>
        /// Constructs a Code Node that has generated binary code of storing value from given register to the stack where given variable is located.
        /// </summary>
        /// <param name="parent">Parent Code Node.</param>
        /// <param name="varName">Variable that is on the stack and where we want to store given register to.</param>
        /// <param name="reg">A register with some value that we want to put on the stack where given variable is located.</param>
        /// <param name="ctx">Current context.</param>
        /// <returns>Constructed Code Node with generated strorage variable binary code commands.</returns>
        protected CodeNode GetStoreVariableNode(CodeNode? parent, string varName, byte reg, Context? ctx)
        {
            CodeNode storeVarNode = new CodeNode("Store variable \'" + varName + "\' from R" + reg.ToString(), parent);

            int bytesToLoad = ctx.GetVarType(varName).GetSize();

            if (ctx.IsVarGlobal(varName))
            {
                // R27 := SB + staticOffset;
                // ->R27 := reg;
                storeVarNode.Children.AddLast(new CodeNode("store var cmds 1", storeVarNode)
                    .Add(GenerateLDA(SB, 27, ctx.GetStaticOffset(varName) - 4 + bytesToLoad)));
                if (bytesToLoad < 4)
                {
                    int mask = ((int) Math.Pow(256, 4) - 1) << (bytesToLoad * 8); // ff ff ff 00 or ff ff 00 00
                    int mask2 = (int) Math.Pow(256, bytesToLoad) - 1; // 00 00 00 ff or 00 00 ff ff
                    CodeNode fr0Node = GetFreeRegisterNode(ctx, storeVarNode);
                    byte fr0 = fr0Node.ByteToReturn;
                    storeVarNode.Children.AddLast(fr0Node);
                    CodeNode fr1Node = GetFreeRegisterNode(ctx, storeVarNode);
                    byte fr1 = fr1Node.ByteToReturn;
                    storeVarNode.Children.AddLast(fr1Node);
                    storeVarNode.Children.AddLast(new CodeNode("store var cmds 2", storeVarNode)
                        .Add(GenerateLD(27, fr0))
                        .Add(GenerateLDC(0, fr1))
                        .Add(GenerateLDA(fr1, fr1, mask))
                        .Add(GenerateAND(fr1, fr0))
                        .Add(GenerateLDC(0, fr1))
                        .Add(GenerateLDA(fr1, fr1, mask2))
                        .Add(GenerateAND(fr1, reg))
                        .Add(GenerateOR(fr0, reg)));
                    Program.currentCompiler.generator.FreeReg(fr0);
                    Program.currentCompiler.generator.FreeReg(fr1);
                }
                storeVarNode.Children.AddLast(new CodeNode("store var cmds 3", storeVarNode)
                    .Add(GenerateST(reg, 27)));
            }
            else
            {
                int blockOffset = ctx.GetVarDeclarationBlockOffset(varName);
                storeVarNode.Children.AddLast(new CodeNode("store var cmds 1", storeVarNode)
                    .Add(GenerateMOV(FP, 27)));
                for (int i = 0; i < blockOffset; i++)
                {                        
                    storeVarNode.Children.AddLast(new CodeNode("store var block offset", storeVarNode)
                        .Add(GenerateLDA(27, 27, -4))
                        .Add(GenerateLD(27, 27)));
                }
                storeVarNode.Children.AddLast(new CodeNode("store var cmds 2", storeVarNode)
                    .Add(GenerateLDA(27, 27, ctx.GetFrameOffset(varName) - 4 + bytesToLoad)));
                if (bytesToLoad < 4)
                {
                    int mask = ((int) Math.Pow(256, 4) - 1) << (bytesToLoad * 8); // ff ff ff 00 or ff ff 00 00
                    int mask2 = (int) Math.Pow(256, bytesToLoad) - 1; // 00 00 00 ff or 00 00 ff ff
                    CodeNode fr0Node = GetFreeRegisterNode(ctx, storeVarNode);
                    byte fr0 = fr0Node.ByteToReturn;
                    storeVarNode.Children.AddLast(fr0Node);
                    CodeNode fr1Node = GetFreeRegisterNode(ctx, storeVarNode);
                    byte fr1 = fr1Node.ByteToReturn;
                    storeVarNode.Children.AddLast(fr1Node);
                    storeVarNode.Children.AddLast(new CodeNode("store var cmds 2", storeVarNode)
                        .Add(GenerateLD(27, fr0))
                        .Add(GenerateLDC(0, fr1))
                        .Add(GenerateLDA(fr1, fr1, mask))
                        .Add(GenerateAND(fr1, fr0))
                        .Add(GenerateLDC(0, fr1))
                        .Add(GenerateLDA(fr1, fr1, mask2))
                        .Add(GenerateAND(fr1, reg))
                        .Add(GenerateOR(fr0, reg)));
                    Program.currentCompiler.generator.FreeReg(fr0);
                    Program.currentCompiler.generator.FreeReg(fr1);
                }
                storeVarNode.Children.AddLast(new CodeNode("store var cmds 3", storeVarNode)
                    .Add(GenerateST(reg, 27)));
            }
            return storeVarNode;
        }

        /// <summary>
        /// Constructs a Code Node that has generated asm commands that allocate (statement-aware) variables to register (if possible)
        /// and vise versa.
        /// </summary>
        /// <param name="aastNode">Statement that we want to construct and before which we want to allocate registers.</param>
        /// <param name="parent">Parent Code Node</param>
        /// <returns>Constructed Code Node with register allocation asm commands.</returns>
        protected CodeNode GetRegisterAllocationNode(AASTNode aastNode, CodeNode? parent)
        {
            Generator g = Program.currentCompiler.generator;
            Context? ctx = SemanticAnalyzer.FindParentContext(aastNode);
            if (ctx == null)
            {
                throw new CompilationErrorException("TODO: ");
            }
            CodeNode regAllocNode = new CodeNode("Register allocation", parent);
            HashSet<string> vars = GetAllUsedVars(aastNode);

            foreach (string var in vars)
            {
                if (ctx.IsVarDeclared(var) && !ctx.IsVarRoutine(var) && !ctx.IsVarConstant(var) && !ctx.IsVarLabel(var) && !ctx.IsVarData(var))
                {
                    int liStart = ctx.GetLIStart(var);

                    if (!g.regAllocVTR.ContainsKey(var) && liStart <= aastNode.BlockPosition) // Allocation (if possible)
                    {
                        for (byte ri = 0; ri < 27; ri++)
                        {
                            if (!g.regOccup[ri])
                            {
                                bool arrayCheck = !ctx.IsVarDynamicArray(var) && !ctx.IsVarArray(var);
                                if (arrayCheck)
                                {
                                    g.regAllocVTR.Add(var, ri);
                                    g.regAllocRTV.Add(ri, var);
                                    g.OccupateReg(ri);
                                    regAllocNode.Children.AddLast(GetLoadVariableNode(regAllocNode, var, ri, ctx));
                                }
                                break;
                            }
                        }
                    }
                }
            }
            return regAllocNode;
        }

        /// <summary>
        /// Constructs a Code Node that has generated asm commands that deallocates registers (if possible) after given statement has been already constructed.
        /// </summary>
        /// <param name="aastNode">Already constructed statement.</param>
        /// <param name="parent">Parent Code Node</param>
        /// <param name="dependOnStatementNum">Whether we deallocate only those variables that are never appear after given statement 
        /// or just to deallocate everything what has been allocated (in current context branch).</param>
        /// <returns>Constructed Code Node with asm commands representing register deallocation.</returns>
        protected CodeNode GetRegisterDeallocationNode(AASTNode aastNode, CodeNode? parent, bool dependOnStatementNum = true)
        {
            Generator g = Program.currentCompiler.generator;
            Context? ctx = SemanticAnalyzer.FindParentContext(aastNode);
            if (ctx == null)
            {
                throw new CompilationErrorException("TODO: ");
            }
            CodeNode regDeallocNode = new CodeNode("Register deallocation", parent);

            foreach (string var in g.regAllocVTR.Keys)
            {
                if ((ctx.IsVarDeclared(var) && !dependOnStatementNum || ctx.IsVarDeclaredInThisContext(var) && dependOnStatementNum) 
                    && !ctx.IsVarRoutine(var) && !ctx.IsVarConstant(var) && !ctx.IsVarLabel(var))
                {
                    int liEnd = ctx.GetLIEnd(var);

                    if (liEnd <= aastNode.BlockPosition || !dependOnStatementNum)
                    {
                        byte reg = g.regAllocVTR[var];
                        regDeallocNode.Children.AddLast(GetStoreVariableNode(regDeallocNode, var, reg, ctx));
                        g.regAllocVTR.Remove(var);
                        g.regAllocRTV.Remove(reg);
                        g.FreeReg(reg);
                    }
                }
            }
            return regDeallocNode;
        }

        /// <summary>
        /// Constructs a Code Node that has generated asm commands representing deallocation of all dynamically allocated memory from the heap.
        /// It is not connected to any statement number or live interval. Usually is called when current context has been constructed fully.
        /// </summary>
        /// <param name="aastNode">Some statement in current context.</param>
        /// <param name="parent">Parent Code Node</param>
        /// <returns>Constructed Code Node with asm commands representing a dynamic memory deallocation.</returns>
        protected CodeNode GetDynamicMemoryDeallocationNode(AASTNode aastNode, CodeNode? parent)
        {
            Context? ctx = SemanticAnalyzer.FindParentContext(aastNode);
            Generator g = Program.currentCompiler.generator;
            CodeNode arrDeallocNode = new CodeNode("Dynamic array deallocation", parent);
            CodeNode frNode = GetFreeRegisterNode(aastNode, arrDeallocNode);
            arrDeallocNode.Children.AddLast(frNode);
            byte fr0 = frNode.ByteToReturn;
            foreach (AASTNode var in ctx.GetDeclaredVars())
            {
                if (ctx.IsVarDynamicArray(var.Token.Value) || ctx.IsVarStruct(var.Token))
                {
                    arrDeallocNode.Children.AddLast(GetLoadFromHeapNode(arrDeallocNode, fr0, 0)); // Size of dynamic array
                    arrDeallocNode.Children.AddLast(GetHeapTopChangeNode(arrDeallocNode, fr0, true, false));
                }
            }
            g.FreeReg(fr0);
            return arrDeallocNode;
        }

        /// <summary>
        /// Constructs a Code Node that has asm commands representing a free register allocation (if all are allocated already).
        /// Some variables could be deallocated randomly when calling this function.
        /// </summary>
        /// <param name="aastNode">Some statement in current context.</param>
        /// <param name="parent">Parent Code Node</param>
        /// <returns>Constructed Code Node with asm commands representing a free register allocation.</returns>
        protected CodeNode GetFreeRegisterNode(AASTNode aastNode, CodeNode? parent)
        {
            return GetFreeRegisterNode(SemanticAnalyzer.FindParentContext(aastNode), parent);
        }

        /// <summary>
        /// Constructs a Code Node that has asm commands representing a free register allocation (if all are allocated already).
        /// Some variables could be deallocated randomly when calling this function.
        /// </summary>
        /// <param name="ctx">Current context.</param>
        /// <param name="parent">Parent Code Node</param>
        /// <returns>Constructed Code Node with asm commands representing a free register allocation.</returns>
        protected CodeNode GetFreeRegisterNode(Context? ctx, CodeNode? parent)
        {
            Generator g = Program.currentCompiler.generator;
            CodeNode frNode = new CodeNode("Get free register", parent);

            for (byte ri = 0; ri < g.regOccup.Length; ri++)
            {
                if (!g.regOccup[ri])
                {
                    g.OccupateReg(ri);
                    return frNode.SetByteToReturn(ri);
                }
            }

            // If all are occupated, load out one of them (the first suitable one).
            // ATTENTION: Is it ok, or am I stupid?            
            byte regToFree = 0;
            while (regToFree < 27) // God bless this while to not loop forever! NOTE: It won't
            {
                if (g.regAllocRTV.ContainsKey(regToFree)) break;
                regToFree++;
            }

            if (g.regAllocRTV.ContainsKey(regToFree))
            {
                string varName = g.regAllocRTV[regToFree];
                g.regAllocRTV.Remove(regToFree);
                g.regAllocVTR.Remove(varName);
                frNode.Children.AddLast(GetStoreVariableNode(frNode, varName, regToFree, ctx));
                return frNode.SetByteToReturn(regToFree);
            }
            else
            {
                throw new CompilationErrorException("Out of registers!!!");
            }
        }

        /// <summary>
        /// Constructs a Code Node with asm commands representing loading of 4 bytes from heap at given address to the given register.
        /// Used in loops and 'if' statement.
        /// </summary>
        /// <param name="parent">Parent Code Node</param>
        /// <param name="reg">Register where we want to load 4 bytes from heap</param>
        /// <param name="address">Address from where we want to load 4 bytes</param>
        /// <returns>Constructed Node with asm commands representing loading of 4 bytes from the heap.</returns>
        protected CodeNode GetLoadFromHeapNode(CodeNode? parent, byte reg, int address)
        {
            return new CodeNode("Load from heap at " + address.ToString(), parent)
                .Add(GenerateLDC(0, reg))
                .Add(GenerateLD(reg, reg))
                .Add(GenerateLDA(reg, reg, address))
                .Add(GenerateLD(reg, reg));
        }

        /// <summary>
        /// Constructs a Code Node with asm commands representing storing of 4 bytes to heap at given address from the given register.
        /// Used in loops and 'if' statement.
        /// </summary>
        /// <param name="parent">Parent Code Node</param>
        /// <param name="reg">Register from where we want to store 4 bytes to the heap</param>
        /// <param name="address">Address to where we want to store 4 bytes</param>
        /// <returns>Constructed Node with asm commands representing storing of 4 bytes to the heap.</returns>
        protected CodeNode GetStoreToHeapNode(CodeNode? parent, byte reg, int address)
        {
            return new CodeNode("Store to heap at " + address.ToString(), parent)
                .Add(GenerateLDC(0, 27))
                .Add(GenerateLD(27, 27))
                .Add(GenerateLDA(27, 27, address))
                .Add(GenerateST(reg, 27));
        }

        /// <summary>
        /// Constructs a Code Node with asm commands representing a heap top modification on a given offset.
        /// </summary>
        /// <param name="parent">Parent Code Node</param>
        /// <param name="offset">How much we can change heap top</param>
        /// <param name="useAsReg">It this true, 'offset' would be considered as register number and 
        /// instead of constant heap top change, execution-time change commands would be generated</param>
        /// <param name="decrease">If 'useAsReg' is true, defines whether we allocating heap memory or deallocating ('decrease = true' means allocation)</param>
        /// <returns></returns>
        protected CodeNode GetHeapTopChangeNode(CodeNode? parent, int offset, bool useAsReg = false, bool decrease = true)
        {
            CodeNode heapTopNode = new CodeNode("Heap top change by " + (useAsReg ? (decrease ? ("-R" + offset) : ("+R" + offset)) : offset.ToString()) + " bytes", parent)
                .Add(GenerateLDC(0, 27))
                .Add(GenerateLD(27, 27));
            if (useAsReg)
            {
                if (decrease)
                {
                    heapTopNode.Add(GenerateSUB(offset, 27)); // When we load the heap
                }
                else
                {
                    heapTopNode.Add(GenerateADD(offset, 27)); // When we free the heap
                }
            }
            else
            {
                heapTopNode.Add(GenerateLDA(27, 27, offset));
            }
            heapTopNode
                .Add(GenerateLDC(0, SB))
                .Add(GenerateST(27, SB))
                .Add(GenerateLDC(4, SB)); // Tricky trick 
            return heapTopNode;
        }

        /// <summary>
        /// Recursively calculates the size of an address represented by a given expression.
        /// For example, if expression has some int pointers, then expression clearly points to some integer (or 4 bytes).
        /// If expression has only byte pointers, the expression clearly points to some byte address (1 byte).
        /// </summary>
        /// <param name="aastNode">Expression AASTNode</param>
        /// <returns>The size of a variable that given expression points to.</returns>
        protected int GetSizeOfExpressionAddressedVariable(AASTNode aastNode)
        {
            Context? ctx = SemanticAnalyzer.FindParentContext(aastNode);
            int size = 0;
            if (aastNode.ASTType.Equals("Dereference"))
            {
                // Careful here
                if (aastNode.Children[2].Children.Count == 1 && 
                    aastNode.Children[2].Children[0].ASTType.Equals("Primary") &&
                    aastNode.Children[2].Children[0].Children[^1].ASTType.Equals("IDENTIFIER"))
                {
                    switch (ctx.GetVarType(aastNode.Children[2].Children[0].Children[^1].Token).Type)
                    {
                        case VarType.ERAType.BYTE_ADDR:
                        case VarType.ERAType.CONST_BYTE_ADDR:
                            {
                                return 1;
                            }
                        case VarType.ERAType.SHORT_ADDR:
                        case VarType.ERAType.CONST_SHORT_ADDR:
                            {
                                return 2;
                            }
                        default:
                            {
                                return 4;
                            }
                    }
                }
            }
            if (aastNode.ASTType.Equals("IDENTIFIER"))
            {
                    switch (ctx.GetVarType(aastNode.Token).Type)
                    {
                        case VarType.ERAType.BYTE_ADDR:
                        case VarType.ERAType.CONST_BYTE_ADDR:
                            {
                                return 1;
                            }
                        case VarType.ERAType.SHORT_ADDR:
                        case VarType.ERAType.CONST_SHORT_ADDR:
                            {
                                return 2;
                            }
                        default:
                            {
                                return 4;
                            }
                    }
            }
            foreach (AASTNode child in aastNode.Children)
            {
                size = Math.Max(size, GetSizeOfExpressionAddressedVariable(child));
            }
            return size;
        }

        /// <summary>
        /// Traverses up the Code Node tree, collecting the number of bytes from all ancestors and left siblings.
        /// Basically, gives exact address (in bytes) of given Code Node.
        /// Works only when Code Node tree is fully constructed.
        /// </summary>
        /// <param name="node">Code Node from where to start traversal.</param>
        /// <returns>Exact address (in bytes) of given Code Node</returns>
        public static int GetCurrentBinarySize(CodeNode node)
        {
            int count = 0;
            if (node != null)
            {
                count += node.Bytes.Count;
                if (node.Parent != null)
                {
                    foreach (CodeNode sibling in node.Parent.Children)
                    {
                        if (sibling == node)
                        {
                            break;
                        }
                        count += sibling.Count();
                    }
                }
            }
            else
            {
                return count;
            }
            return count + GetCurrentBinarySize(node.Parent);
        }

        /// <summary>
        /// Calculates a set of all unique variables (identifiers) used in a given AAST node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private HashSet<string> GetAllUsedVars(AASTNode node)
        {
            HashSet<string> set = new HashSet<string>();
            foreach (AASTNode child in node.Children)
            {
                if (child.ASTType.Equals("Variable definition") || child.ASTType.Equals("Constant definition") || child.ASTType.Equals("Array definition") || child.ASTType.Equals("IDENTIFIER"))
                {
                    set.Add(child.Token.Value);
                }               
                set.UnionWith(GetAllUsedVars(child));
            }
            return set;
        }

    }
}
