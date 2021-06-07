using ERACompiler.Structures;
using ERACompiler.Utilities.Errors;
using System;
using System.Collections.Generic;

namespace ERACompiler.Modules.Generation
{
    public class CodeConstructor
    {

        // The length of these blocks can be obtained by difference
        protected int codeAddrBase = 18;
        protected readonly int staticDataAddrBase = 18;
        protected readonly int FP = 28, SP = 29, SB = 30, PC = 31; // Special registers

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
            
        protected LinkedList<byte> GetLList(params byte[] bytes)
        {
            return new LinkedList<byte>(bytes);
        }

        protected LinkedList<byte> GenerateCommand(int format, int opCode, int regI, int regJ)
        {
            // In LDL | PRINT case
            if (format != 2)
            {
                format = format == 32 ? 3 : format == 16 ? 1 : 0;                
            }
            return GetLList(
                (byte)((opCode << 2) | (format << 6) | (regI >> 3)),
                (byte)(regJ | (regI << 5))
                );
        }

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

        protected LinkedList<byte> GenerateSTOP()
        {
            return GenerateCommand(8, 0, 0, 0);
        }

        protected LinkedList<byte> GenerateSKIP()
        {
            return GenerateCommand(16, 0, 0, 0);
        }

        protected LinkedList<byte> GenerateCBR(int regI, int regJ)
        {
            return GenerateCommand(32, 15, regI, regJ);
        }

        protected LinkedList<byte> GenerateCND(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 14, regI, regJ);
        }

        protected LinkedList<byte> GenerateLSR(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 13, regI, regJ);
        }

        protected LinkedList<byte> GenerateLSL(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 12, regI, regJ);
        }

        protected LinkedList<byte> GenerateXOR(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 11, regI, regJ);
        }

        protected LinkedList<byte> GenerateAND(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 10, regI, regJ);
        }

        protected LinkedList<byte> GenerateOR(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 9, regI, regJ);
        }

        protected LinkedList<byte> GenerateASL(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 8, regI, regJ);
        }

        protected LinkedList<byte> GenerateASR(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 7, regI, regJ);
        }

        protected LinkedList<byte> GenerateSUB(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 6, regI, regJ);
        }

        protected LinkedList<byte> GenerateADD(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 5, regI, regJ);
        }

        protected LinkedList<byte> GenerateMOV(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 4, regI, regJ);
        }

        protected LinkedList<byte> GenerateST(int regI, int regJ)
        {
            return GenerateCommand(32, 3, regI, regJ);
        }

        protected LinkedList<byte> GenerateLDL(int reg, int address)
        {
            LinkedList<byte> toReturn = new LinkedList<byte>(GenerateLDC(0, reg));
            foreach (byte b in GenerateLDA(reg, reg, address)) 
            {
                toReturn.AddLast(b);
            }
            return toReturn;
        }

        protected LinkedList<byte> GenerateLDA(int regI, int regJ, int constant)
        {
            LinkedList<byte> toReturn = new LinkedList<byte>(GenerateCommand(8, 2, regI, regJ));
            foreach (byte b in GetConstBytes(constant))
            {
                toReturn.AddLast(b);
            }
            return toReturn;
        }

        protected LinkedList<byte> GenerateLDC(int constant, int regJ) // Constant is in range [0..31]
        {
            return GenerateCommand(32, 2, constant, regJ);
        }

        protected LinkedList<byte> GenerateLD(int regI, int regJ)
        {
            return GenerateCommand(32, 1, regI, regJ);
        }

        protected LinkedList<byte> GeneratePRINT(int reg)
        {
            return GenerateCommand(2, 0, reg, 0);
        }

        protected CodeNode GetLoadVariableNode(CodeNode? parent, string varName, byte reg, Context? ctx)
        {
            CodeNode loadVarNode = new CodeNode("Load variable \'" + varName + "\' into R" + reg.ToString(), parent);

            if (ctx.IsVarGlobal(varName))
            {
                loadVarNode
                    .Add(GenerateLDA(SB, 27, ctx.GetStaticOffset(varName)))
                    .Add(GenerateLD(27, reg));
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
                    .Add(GenerateLD(27, reg));
            }
            
            return loadVarNode;
        }

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

        protected CodeNode GetStoreVariableNode(CodeNode? parent, string varName, byte reg, Context? ctx)
        {
            CodeNode storeVarNode = new CodeNode("Store variable \'" + varName + "\' from R" + reg.ToString(), parent);

            if (ctx.IsVarGlobal(varName))
            {
                // R27 := SB + staticOffset;
                // ->R27 := reg;
                storeVarNode
                    .Add(GenerateLDA(SB, 27, ctx.GetStaticOffset(varName)))
                    .Add(GenerateST(reg, 27));
            }
            else
            {
                int blockOffset = ctx.GetVarDeclarationBlockOffset(varName);
                storeVarNode.Add(GenerateMOV(FP, 27)); // R27 := FP;                
                for (int i = 0; i < blockOffset; i++)
                {                        
                    // R27 := R27 - 4; # ATTENTION: May be optimized
                    // R27 := ->R27;
                    storeVarNode
                        .Add(GenerateLDA(27, 27, -4))
                        .Add(GenerateLD(27, 27));
                }
                // R27 := R27 + frameOffset;
                // ->R27 := reg;
                storeVarNode
                    .Add(GenerateLDA(27, 27, ctx.GetFrameOffset(varName)))
                    .Add(GenerateST(reg, 27));
            }
            return storeVarNode;
        }

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
                if (ctx.IsVarDeclared(var) && !ctx.IsVarRoutine(var) && !ctx.IsVarConstant(var) && !ctx.IsVarLabel(var))
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
                                    regAllocNode.Children.AddLast(GetLoadVariableNode(regAllocNode, var, ri, ctx));
                                    g.regAllocVTR.Add(var, ri);
                                    g.regAllocRTV.Add(ri, var);
                                    g.OccupateReg(ri);
                                }
                                break;
                            }
                        }
                    }
                }
            }
            return regAllocNode;
        }

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

        protected CodeNode GetFreeRegisterNode(AASTNode aastNode, CodeNode? parent)
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
            Context? ctx = SemanticAnalyzer.FindParentContext(aastNode);
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

        protected CodeNode GetLoadFromHeapNode(CodeNode? parent, byte reg, int address)
        {
            return new CodeNode("Load from heap at " + address.ToString(), parent)
                .Add(GenerateLDC(0, reg))
                .Add(GenerateLD(reg, reg))
                .Add(GenerateLDA(reg, reg, address))
                .Add(GenerateLD(reg, reg));
        }

        protected CodeNode GetStoreToHeapNode(CodeNode? parent, byte reg, int address)
        {
            return new CodeNode("Store to heap at " + address.ToString(), parent)
                .Add(GenerateLDC(0, 27))
                .Add(GenerateLD(27, 27))
                .Add(GenerateLDA(27, 27, address))
                .Add(GenerateST(reg, 27));
        }

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
