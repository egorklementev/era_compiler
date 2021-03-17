using ERACompiler.Structures;
using ERACompiler.Structures.Types;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ERACompiler.Utilities.Errors;

namespace ERACompiler.Modules
{
    /// <summary>
    /// Generates the actual assembly code
    /// </summary>
    public class Generator
    {
        // The length of these blocks can be obtained by difference
        private readonly int staticDataAddrBase = 18;
        private int codeAddrBase = 18;

        private readonly Random random;

        private readonly bool[] regOccup = new bool[27]; // For reigster allocation algorithm (R27 is always used, no variable stored in R27)
        private readonly Dictionary<string, byte> regAllocVTR = new Dictionary<string, byte>(); // Variable-to-Register dictionary
        private readonly Dictionary<byte, string> regAllocRTV = new Dictionary<byte, string>(); // Register-to-Variable dictionary
        private readonly Dictionary<byte, int> lblAllocRTL = new Dictionary<byte, int>(); // Register-to-Label
        private readonly Dictionary<int, byte> lblAllocLTR = new Dictionary<int, byte>(); // Label-to-Register

        private const int FP = 28, SP = 29, SB = 30, PC = 31;

        private int heapTop = 0;
        private int stackTop = 0;
        private readonly uint memorySize = 2 * 1024 * 1024; // ATTENTION: 2 megabytes for now. TODO: make this value configurable

        /// <summary>
        /// Default constructor
        /// </summary>
        public Generator()
        {
            random = new Random();
        }

        /// <summary>
        /// Constructs a binary code (or assembly code) from a given AAST root.
        /// </summary>
        /// <param name="root">AAST root node</param>
        /// <returns>Corresponding binary code in byte array</returns>
        public byte[] GetBinaryCode(AASTNode root)
        {
            LinkedList<byte> code = Construct(root);

            // Resolve labels
            byte rem = 0x00;
            var node = code.First;
            for (int i = 0; i < 6; i++) node = node.Next;
            int staticOffset = 2 * BitConverter.ToInt32(
                            new byte[]
                            {
                                node.Next.Next.Next.Value,
                                node.Next.Next.Value,
                                node.Next.Value,
                                node.Value,
                            }
                            );
            for (int i = 0; i < 12 + staticOffset; i++) node = node.Next;
            for (int i = 18 + staticOffset; i < code.Count; i++)
            {
                if (i % 2 == 1)
                {
                    if ((rem & 0xfc) == 0x08) // Skip LDA constant
                    {
                        i += 4;
                        node = node.Next.Next.Next.Next;
                    }
                    else if ((rem & 0xfc) == 0x88)
                    {
                        // Offset in words (2 bytes)
                        int offset = BitConverter.ToInt32(
                            new byte[]
                            {
                                node.Next.Next.Next.Next.Value,
                                node.Next.Next.Next.Value,
                                node.Next.Next.Value,
                                node.Next.Value,
                            }
                            );
                        int curPos = i - 18;                    
                        LinkedList<byte> jumpTo = GetConstBytes(offset + curPos);
                        node.Next.Value = jumpTo.First.Value;
                        node.Next.Next.Value = jumpTo.First.Next.Value;
                        node.Next.Next.Next.Value = jumpTo.Last.Previous.Value;
                        node.Next.Next.Next.Next.Value = jumpTo.Last.Value;
                        node.Previous.Value &= 0x7f;
                        i += 4;
                        node = node.Next.Next.Next.Next;
                    }
                }
                rem = node.Value;
                node = node.Next;
            }

            // If we want assembly code, not binary
            if (Program.convertToAssemblyCode)
            {
                return ConvertToAssemblyCode(code);
            }

            byte[] bytes = new byte[code.Count];
            node = code.First;
            for (int i = 0; i < code.Count; i++)
            {
                bytes[i] = node.Value;
                node = node.Next;
            }
            return bytes;
        }

        private LinkedList<byte> Construct(AASTNode node)
        {
            switch (node.ASTType)
            {
                case "Program":
                    return ConstructProgram(node);
                case "Assembly block":
                    return ConstructAssemblyBlock(node);
                case "Expression":
                    return ConstructExpression(node);
                case "Code":
                    return ConstructCode(node);
                case "Variable definition":
                    return ConstructVarDef(node);
                case "Assignment":
                    return ConstructAssignment(node);
                case "If":
                    return ConstructIf(node);
                case "For":
                    return ConstructFor(node);
                case "Block body":
                    return ConstructBlockBody(node);
                default: // If skip, just go for children nodes
                    {
                        LinkedList<byte> bytes = new LinkedList<byte>();
                        foreach (AASTNode child in node.Children)
                        {
                            bytes = MergeLists(bytes, Construct(child));
                        }
                        return bytes;
                    }                
            }
        }

        private LinkedList<byte> ConstructBlockBody(AASTNode node)
        {
            LinkedList<byte> bbBytes = new LinkedList<byte>();
            Context ctx = node.Context;

            int frameSize = 0;
            if (ctx.GetDeclaredVars().Count > 0)
            {
                frameSize =
                    ctx.GetFrameOffset(ctx.GetDeclaredVars().Last().Token.Value) +
                    ctx.GetDeclaredVars().Last().AASTType.GetSize(); // ATTENTION: ??? Does it work? Check needed.
                stackTop += frameSize;
            }

            bbBytes = MergeLists(bbBytes, GenerateST(FP, SP)); // Store where to return
            bbBytes = MergeLists(bbBytes, GenerateMOV(SP, FP)); 
            bbBytes = MergeLists(bbBytes, GenerateLDA(FP, FP, 4));
            bbBytes = MergeLists(bbBytes, GenerateMOV(FP, SP));
            bbBytes = MergeLists(bbBytes, GenerateLDA(SP, SP, frameSize));

            int statementNum = 1;
            foreach (AASTNode statement in node.Children)
            {
                // -- Register allocation 
                HashSet<string> vars = GetAllUsedVars(statement);
                foreach (string var in vars)
                {
                    if (ctx.IsVarDeclared(var))
                    {
                        int liStart = ctx.GetLIStart(var);
                        int liEnd = ctx.GetLIEnd(var);

                        if (regAllocVTR.ContainsKey(var) && liEnd < statementNum) // Deallocation
                        {
                            bbBytes = MergeLists(bbBytes, LoadOutVariable(var, regAllocVTR[var], ctx));
                            FreeReg(regAllocVTR[var]);
                        }

                        if (!regAllocVTR.ContainsKey(var) && liStart <= statementNum) // Allocation (if possible)
                        {
                            for (byte ri = 0; ri < 27; ri++)
                            {
                                if (!regOccup[ri])
                                {
                                    bbBytes = MergeLists(bbBytes, LoadInVariable(var, ri, ctx));
                                    regAllocVTR.Add(var, ri);
                                    regAllocRTV.Add(ri, var);
                                    OccupateReg(ri);
                                    break;
                                }
                            }
                        }
                    }
                }

                bbBytes = MergeLists(bbBytes, Construct(statement));
                statementNum++;
            }

            bbBytes = MergeLists(bbBytes, GenerateLDA(FP, FP, -4));
            bbBytes = MergeLists(bbBytes, GenerateMOV(FP, SP)); // Return stack pointer
            bbBytes = MergeLists(bbBytes, GenerateLD(FP, FP)); // Return frame pointer

            stackTop -= frameSize;

            return bbBytes;
        }

        private LinkedList<byte> ConstructFor(AASTNode node)
        {
            LinkedList<byte> forBytes = new LinkedList<byte>();

            int defaultFrom = 0;
            int defaultTo = 10;
            int defaultStep = 1;

            bool hasFrom = false;
            bool hasTo = false;
            bool hasStep = false;

            int iFrom = 0;
            int iTo = 0;
            int iStep = 0;

            // Identify blocks (if any)
            int i = 1;
            while (node.Children[i].ASTType.Equals("Expression"))
            {
                switch (((AASTNode)node.Children[i]).AASTValue)
                {
                    case 1:
                        {
                            iFrom = i;
                            hasFrom = true;
                            break;
                        }
                    case 2:
                        {
                            iTo = i;
                            hasTo = true;
                            break;
                        }
                    case 3:
                        {
                            iStep = i;
                            hasStep = true;
                            break;
                        }
                    default:
                        {
                            throw new CompilationErrorException("Something is wrong with FOR loop bincode generation!!!");
                        }
                }
                i++;
            }

            // Generate FROM expression bytes
            byte fr0;
            if (hasFrom)
            {
                forBytes = MergeLists(forBytes, ConstructExpression((AASTNode)node.Children[iFrom]));
                fr0 = forBytes.Last.Value;
                forBytes.RemoveLast();
            }
            else
            {
                forBytes = MergeLists(forBytes, GetFreeReg(node));
                fr0 = forBytes.Last.Value;
                forBytes.RemoveLast();
                forBytes = MergeLists(forBytes, GenerateLDC(defaultFrom, fr0));
            }

            // Store FROM value to the iterator variable
            forBytes = MergeLists(forBytes, GetFreeReg(node));
            byte fr6 = forBytes.Last.Value;
            forBytes.RemoveLast();
            forBytes = MergeLists(forBytes, GenerateLDA(SP, fr6, 4));
            forBytes = MergeLists(forBytes, GenerateST(fr0, fr6));
            FreeReg(fr6);

            // Allocate registers
            forBytes = MergeLists(forBytes, GetFreeReg(node));
            byte fr3 = forBytes.Last.Value;
            forBytes.RemoveLast();

            forBytes = MergeLists(forBytes, GetFreeReg(node));
            byte fr4 = forBytes.Last.Value;
            forBytes.RemoveLast();

            forBytes = MergeLists(forBytes, GetFreeReg(node));
            byte fr5 = forBytes.Last.Value;
            forBytes.RemoveLast();

            // Allocate heap for registers that should be saved for loop operation
            heapTop += 16;
            CheckForStackOverflow();

            lblAllocLTR.Add(heapTop - 0, fr0);
            lblAllocRTL.Add(fr0, heapTop - 0);

            lblAllocLTR.Add(heapTop - 4, fr3);
            lblAllocRTL.Add(fr3, heapTop - 4);

            lblAllocLTR.Add(heapTop - 8, fr4);
            lblAllocRTL.Add(fr4, heapTop - 8);

            lblAllocLTR.Add(heapTop - 12, fr5);
            lblAllocRTL.Add(fr5, heapTop - 12);

            // Loop start label
            forBytes = MergeLists(forBytes, GenerateLDL(fr3, 15));

            // Generate TO expression bytes
            byte fr1;
            LinkedList<byte> toExpr = new LinkedList<byte>();
            if (hasTo)
            {
                toExpr = MergeLists(toExpr, ConstructExpression((AASTNode)node.Children[iTo]));
                fr1 = toExpr.Last.Value;
                toExpr.RemoveLast();
            }
            else
            {
                toExpr = MergeLists(toExpr, GetFreeReg(node));
                fr1 = toExpr.Last.Value;
                toExpr.RemoveLast();
                toExpr = MergeLists(toExpr, GenerateLDC(defaultTo, fr1));
            }

            // Generate FOR_BLOCK command bytes
            LinkedList<byte> forBlockCommands = Construct((AASTNode)node.Children[^1]);

            // Generate STEP expression commands
            byte fr2;
            LinkedList<byte> stepExpr = new LinkedList<byte>();
            if (hasStep)
            {
                stepExpr = MergeLists(stepExpr, ConstructExpression((AASTNode)node.Children[iStep]));
                fr2 = stepExpr.Last.Value;
                stepExpr.RemoveLast();
            }
            else
            {
                stepExpr = MergeLists(stepExpr, GetFreeReg(node));
                fr2 = stepExpr.Last.Value;
                stepExpr.RemoveLast();
                stepExpr = MergeLists(stepExpr, GenerateLDC(defaultStep, fr2));
            }

            // We have to generate this addition separately since old FR2 value will be lost soon
            LinkedList<byte> stepCommand = GenerateADD(fr2, fr0);         

            // Here it lost
            forBytes = MergeLists(forBytes, GetFreeReg(node));
            fr2 = forBytes.Last.Value;
            forBytes.RemoveLast();

            // Loop end label
            forBytes = MergeLists(forBytes, GenerateLDL(fr4, 29 + toExpr.Count + forBlockCommands.Count + stepExpr.Count));
            forBytes = MergeLists(forBytes, GenerateLDC(6, fr5));
            forBytes = MergeLists(forBytes, toExpr);
            // It can be that FR0 has been deallocated to the heap, 
            // so we should check it and load it back if needed.
            // Same with FR5, FR4, and FR3 registers.
            if (lblAllocRTL.ContainsKey(fr0))
            {
                forBytes = MergeLists(forBytes, GenerateMOV(fr0, fr2));
            }
            else
            {
                forBytes = MergeLists(forBytes, GetFreeReg(node));
                fr0 = forBytes.Last.Value;
                forBytes.RemoveLast();                
                forBytes = MergeLists(forBytes, LoadInLabel(heapTop - 0, fr0));
                forBytes = MergeLists(forBytes, GenerateMOV(fr0, fr2));
            }            
            forBytes = MergeLists(forBytes, GenerateCND(fr1, fr2));
            if (lblAllocRTL.ContainsKey(fr5)) 
            {            
                forBytes = MergeLists(forBytes, GenerateAND(fr5, fr2));
            }
            else
            {
                forBytes = MergeLists(forBytes, GetFreeReg(node));
                fr5 = forBytes.Last.Value;
                forBytes.RemoveLast();
                forBytes = MergeLists(forBytes, LoadInLabel(heapTop - 12, fr5));
                forBytes = MergeLists(forBytes, GenerateAND(fr5, fr2));
            }
            if (lblAllocRTL.ContainsKey(fr4))
            {
                forBytes = MergeLists(forBytes, GenerateCBR(fr2, fr4));
            }
            else
            {
                forBytes = MergeLists(forBytes, GetFreeReg(node));
                fr4 = forBytes.Last.Value;
                forBytes.RemoveLast();
                forBytes = MergeLists(forBytes, LoadInLabel(heapTop - 8, fr4));
                forBytes = MergeLists(forBytes, GenerateCBR(fr2, fr4));
            }
            // Add all expressions and commands
            forBytes = MergeLists(forBytes, forBlockCommands);
            forBytes = MergeLists(forBytes, stepExpr);
            forBytes = MergeLists(forBytes, stepCommand);

            // Update iterator variable
            forBytes = MergeLists(forBytes, GetFreeReg(node));
            fr6 = forBytes.Last.Value;
            forBytes.RemoveLast();
            forBytes = MergeLists(forBytes, GenerateLDA(SP, fr6, 4));
            forBytes = MergeLists(forBytes, GenerateST(fr0, fr6));            
            forBytes = MergeLists(forBytes, GenerateLDC(1, fr6)); // Since we do not want to override FR3

            if (lblAllocRTL.ContainsKey(fr3))
            {
                forBytes = MergeLists(forBytes, GenerateCBR(fr6, fr3));
            }
            else
            {
                forBytes = MergeLists(forBytes, GetFreeReg(node));
                fr3 = forBytes.Last.Value;
                forBytes.RemoveLast();
                forBytes = MergeLists(forBytes, LoadInLabel(heapTop - 4, fr3));
                forBytes = MergeLists(forBytes, GenerateCBR(fr6, fr3));
            }

            // Deallocate heap & free registers
            lblAllocLTR.Remove(heapTop - 12);
            lblAllocRTL.Remove(fr5);

            lblAllocLTR.Remove(heapTop - 8);
            lblAllocRTL.Remove(fr4);

            lblAllocLTR.Remove(heapTop - 4);
            lblAllocRTL.Remove(fr3);

            lblAllocLTR.Remove(heapTop - 0);
            lblAllocRTL.Remove(fr0);

            heapTop -= 16;
            FreeReg(fr0);
            FreeReg(fr1);
            FreeReg(fr2);
            FreeReg(fr3);
            FreeReg(fr4);
            FreeReg(fr5);
            FreeReg(fr6);

            return forBytes;
        }

        private LinkedList<byte> ConstructIf(AASTNode node)
        {
            LinkedList<byte> ifBytes = new LinkedList<byte>();

            ifBytes = MergeLists(ifBytes, ConstructExpression((AASTNode)node.Children[0]));
            byte fr0 = ifBytes.Last.Value;
            ifBytes.RemoveLast();

            ifBytes = MergeLists(ifBytes, GetFreeReg(node));
            byte fr1 = ifBytes.Last.Value;
            ifBytes.RemoveLast();        

            LinkedList<byte> trueBlockCommands = Construct((AASTNode)node.Children[1]);

            if (node.Children.Count < 3) // No "else" block
            {
                ifBytes = MergeLists(ifBytes, GenerateLDL(fr1, 17));
                ifBytes = MergeLists(ifBytes, GenerateCBR(fr0, fr1));
                ifBytes = MergeLists(ifBytes, GenerateLDL(fr1, 7 + trueBlockCommands.Count));         
                ifBytes = MergeLists(ifBytes, GenerateCBR(fr1, fr1));
                ifBytes = MergeLists(ifBytes, trueBlockCommands);
            }
            else // With "else" block
            {   
                LinkedList<byte> falseBlockCommands = Construct((AASTNode)node.Children[2]);

                ifBytes = MergeLists(ifBytes, GenerateLDL(fr1, 17 + falseBlockCommands.Count));
                ifBytes = MergeLists(ifBytes, GenerateCBR(fr0, fr1));
                ifBytes = MergeLists(ifBytes, falseBlockCommands);
                ifBytes = MergeLists(ifBytes, GenerateLDL(fr1, 7 + falseBlockCommands.Count + trueBlockCommands.Count));
                ifBytes = MergeLists(ifBytes, GenerateCBR(fr1, fr1));
                ifBytes = MergeLists(ifBytes, trueBlockCommands);
            }

            FreeReg(fr0);
            FreeReg(fr1);

            return ifBytes;
        }

        private LinkedList<byte> ConstructAssignment(AASTNode node)
        {
            LinkedList<byte> asgnBytes = new LinkedList<byte>();

            asgnBytes = MergeLists(asgnBytes, ConstructExpression((AASTNode)node.Children[1]));
            byte fr0 = asgnBytes.Last.Value;
            asgnBytes.RemoveLast();

            asgnBytes = MergeLists(asgnBytes, ConstructReceiver((AASTNode)node.Children[0]));
            byte fr1 = asgnBytes.Last.Value;
            asgnBytes.RemoveLast();

            if (regAllocRTV.ContainsKey(fr1)) // If a register is already allocated for variable
            {
                asgnBytes = MergeLists(asgnBytes, GenerateMOV(fr0, fr1));
            }
            else
            {
                asgnBytes = MergeLists(asgnBytes, GenerateST(fr0, fr1));
            }

            FreeReg(fr0);
            FreeReg(fr1);

            return asgnBytes;
        }

        private LinkedList<byte> ConstructVarDef(AASTNode node)
        {
            LinkedList<byte> varDefBytes = new LinkedList<byte>();

            byte format = 0xc0;
            switch (node.AASTType.Type)
            {
                case VarType.ERAType.INT:
                    format = 0xc0;
                    break;
                case VarType.ERAType.SHORT:
                    format = 0x40;
                    break;
                case VarType.ERAType.BYTE:
                    format = 0x00;
                    break;
            }

            switch (node.AASTType.Type)
            {
                case VarType.ERAType.INT:
                case VarType.ERAType.SHORT:
                case VarType.ERAType.BYTE:
                    {
                        // If we have initial asignment - store it to register/memory
                        if (node.Children.Count > 0)
                        {
                            varDefBytes = MergeLists(varDefBytes, ConstructExpression((AASTNode)node.Children[0]));
                            if (regAllocVTR.ContainsKey(node.Token.Value))
                            {
                                // Store to register
                                byte reg = (byte)regAllocVTR[node.Token.Value];
                                byte fr0 = varDefBytes.Last.Value;
                                varDefBytes.RemoveLast();

                                // Rxy := FR0;
                                varDefBytes = MergeLists(varDefBytes, GetLList((byte)(format | 0x10 | (fr0 >> 3)), (byte)((fr0 << 5) | reg)));

                                FreeReg(fr0);
                            }
                            else
                            {
                                // TODO: Store to memory
                            }
                            
                        }
                        break;
                    }
                default:
                    break;
            }

            return varDefBytes;
        }

        private LinkedList<byte> ConstructCode(AASTNode node)
        {
            LinkedList<byte> codeBytes = new LinkedList<byte>();
            Context ctx = node.Context;

            // FP := SP;
            codeBytes = MergeLists(codeBytes, GenerateMOV(SP, FP));

            int frameSize = 0;
            if (ctx.GetDeclaredVars().Count > 0)
            {
                frameSize =
                    ctx.GetFrameOffset(ctx.GetDeclaredVars().Last().Token.Value) +
                    ctx.GetDeclaredVars().Last().AASTType.GetSize(); // ATTENTION: ??? Does it work? Check needed.
                codeBytes = MergeLists(codeBytes, GenerateLDA(SP, SP, frameSize));
                stackTop += frameSize;
            }

            int statementNum = 1;
            foreach (AASTNode statement in node.Children)
            {
                // -- Register allocation --
                HashSet<string> vars = GetAllUsedVars(statement);
                foreach (string var in vars)
                {
                    if (ctx.IsVarDeclared(var))
                    {
                        int liStart = ctx.GetLIStart(var);
                        int liEnd = ctx.GetLIEnd(var);

                        if (regAllocVTR.ContainsKey(var) && liEnd < statementNum) // Deallocation
                        {
                            codeBytes = MergeLists(codeBytes, LoadOutVariable(var, regAllocVTR[var], ctx));
                            FreeReg(regAllocVTR[var]);
                        }

                        if (!regAllocVTR.ContainsKey(var) && liStart <= statementNum) // Allocation (if possible)
                        {
                            for (byte ri = 0; ri < 27; ri++)
                            {
                                if (!regOccup[ri])
                                {
                                    codeBytes = MergeLists(codeBytes, LoadInVariable(var, ri, ctx));
                                    regAllocVTR.Add(var, ri);
                                    regAllocRTV.Add(ri, var);
                                    OccupateReg(ri);
                                    break;
                                }
                            }
                        }
                    }
                }

                // Recursive statement bincode generation
                codeBytes = MergeLists(codeBytes, Construct(statement));
                statementNum++;
            }

            stackTop -= frameSize;

            return codeBytes;
        }

        private LinkedList<byte> ConstructExpression(AASTNode node)
        {
            LinkedList<byte> exprBytes = new LinkedList<byte>();

            // 1) Store result of the left operand in FR0
            exprBytes = MergeLists(exprBytes, ConstructOperand((AASTNode)node.Children[0]));
            byte fr0 = exprBytes.Last.Value;                        

            if (node.Children.Count > 1)
            {
                exprBytes.RemoveLast();
                // 2) Store result of the right operand in FR0/FR1
                exprBytes = MergeLists(exprBytes, ConstructOperand((AASTNode)node.Children[2]));
                byte fr1 = exprBytes.Last.Value;
                exprBytes.RemoveLast();

                // 3) Generate a code of the operation itself using these two register
                //    and put an extra byte indicating the register with the result 
                //    (this byte is being removed upper in the call stack)
                string op = node.Children[1].Token.Value;
                switch (op)
                {
                    // ATTENTION: What about command "format"? I use 32 everywhere.
                    case "+":
                        {
                            // FR0 += FR1; FR0  # In this case order does not matter                        
                            exprBytes = MergeLists(exprBytes, MergeLists(GenerateADD(fr1, fr0), GetLList(fr0)));
                            break;
                        }
                    case "-":
                        {
                            // FR0 -= FR1; FR0
                            exprBytes = MergeLists(exprBytes, MergeLists(GenerateSUB(fr1, fr0), GetLList(fr0)));
                            break;
                        }
                    case "&":
                        {
                            // FR0 &= FR1; FR0
                            exprBytes = MergeLists(exprBytes, MergeLists(GenerateAND(fr1, fr0), GetLList(fr0)));
                            break;
                        }
                    case "|":
                        {
                            // FR0 |= FR1; FR0
                            exprBytes = MergeLists(exprBytes, MergeLists(GenerateOR(fr1, fr0), GetLList(fr0)));
                            break;
                        }
                    case "^":
                        {
                            // FR0 ^= FR1; FR0
                            exprBytes = MergeLists(exprBytes, MergeLists(GenerateXOR(fr1, fr0), GetLList(fr0)));
                            break;
                        }
                    case "?":
                        {
                            // FR0 ?= FR1; FR0
                            exprBytes = MergeLists(exprBytes, MergeLists(GenerateCND(fr1, fr0), GetLList(fr0)));
                            break;
                        }
                    case "=":
                    case "/=":
                    case ">":
                    case "<":
                        {
                            int mask = op.Equals("=") ? 4 : op.Equals("/=") ? 3 : op.Equals(">") ? 1 : op.Equals("<") ? 2 : 7;
                            exprBytes = MergeLists(exprBytes, GetFreeReg(node));
                            byte fr2 = exprBytes.Last.Value;
                            exprBytes.RemoveLast();
                            // FR2 := mask;
                            // FR0 ?= FR1;
                            // FR2 &= FR0;
                            // FR0 := FR2;
                            // fr0
                            exprBytes = MergeLists(exprBytes, GenerateLDC(mask, fr2));
                            exprBytes = MergeLists(exprBytes, GenerateCND(fr1, fr0));
                            exprBytes = MergeLists(exprBytes, GenerateAND(fr0, fr2));
                            exprBytes = MergeLists(exprBytes, GenerateMOV(fr2, fr0));
                            exprBytes = MergeLists(exprBytes, GetLList(fr0));
                            break;
                        }
                    case "*":
                        {
                            // WHAT A MONSTROSITY!
                            // -------------------
                            // FR2 := 0;
                            // FR2 := FR2 + 32;
                            // FR3 := mult (27);
                            // FR4 := add (27);
                            // FR5 := not_add (21);
                            // FR6 := 1; # Mask
                            // FR7 := 0; # For result                            
                            // FR8 := 1; # For iteration
                            // <mult>
                            // FR9 := 0; # For loop exit
                            // FR6 &= FR1;                            
                            // if FR6 goto FR4;
                            // if FR8 goto FR5;
                            // <add>
                            // FR7 += FR0;
                            // <not_add>
                            // FR6 := 1;                            
                            // FR8 := 1;                            
                            // FR0 <= FR0;
                            // FR1 >= FR1;
                            // FR2 -= FR8;
                            // FR9 ?= FR2;
                            // FR9 &= FR8;
                            // if FR9 goto FR3;
                            // FR0 := FR7;
                            // fr0

                            exprBytes = MergeLists(exprBytes, GetFreeReg(node));
                            byte fr2 = exprBytes.Last.Value;
                            exprBytes.RemoveLast();

                            exprBytes = MergeLists(exprBytes, GetFreeReg(node));
                            byte fr3 = exprBytes.Last.Value;
                            exprBytes.RemoveLast();

                            exprBytes = MergeLists(exprBytes, GetFreeReg(node));
                            byte fr4 = exprBytes.Last.Value;
                            exprBytes.RemoveLast();

                            exprBytes = MergeLists(exprBytes, GetFreeReg(node));
                            byte fr5 = exprBytes.Last.Value;
                            exprBytes.RemoveLast();

                            exprBytes = MergeLists(exprBytes, GetFreeReg(node));
                            byte fr6 = exprBytes.Last.Value;
                            exprBytes.RemoveLast();

                            exprBytes = MergeLists(exprBytes, GetFreeReg(node));
                            byte fr7 = exprBytes.Last.Value;
                            exprBytes.RemoveLast();

                            exprBytes = MergeLists(exprBytes, GetFreeReg(node));
                            byte fr8 = exprBytes.Last.Value;
                            exprBytes.RemoveLast();

                            exprBytes = MergeLists(exprBytes, GetFreeReg(node));
                            byte fr9 = exprBytes.Last.Value;
                            exprBytes.RemoveLast();

                            exprBytes = MergeLists(exprBytes, GenerateLDC(0, fr2));
                            exprBytes = MergeLists(exprBytes, GenerateLDA(fr2, fr2, 32));
                            exprBytes = MergeLists(exprBytes, GenerateLDL(fr3, 27));
                            exprBytes = MergeLists(exprBytes, GenerateLDL(fr4, 27));
                            exprBytes = MergeLists(exprBytes, GenerateLDL(fr5, 21));
                            exprBytes = MergeLists(exprBytes, GenerateLDC(1, fr6));
                            exprBytes = MergeLists(exprBytes, GenerateLDC(0, fr7));
                            exprBytes = MergeLists(exprBytes, GenerateLDC(1, fr8));
                            exprBytes = MergeLists(exprBytes, GenerateLDC(0, fr9));
                            exprBytes = MergeLists(exprBytes, GenerateAND(fr1, fr6));
                            exprBytes = MergeLists(exprBytes, GenerateCBR(fr6, fr4));
                            exprBytes = MergeLists(exprBytes, GenerateCBR(fr8, fr5));
                            exprBytes = MergeLists(exprBytes, GenerateADD(fr0, fr7));
                            exprBytes = MergeLists(exprBytes, GenerateLDC(1, fr6));
                            exprBytes = MergeLists(exprBytes, GenerateLDC(1, fr8));
                            exprBytes = MergeLists(exprBytes, GenerateLSL(fr0, fr0));
                            exprBytes = MergeLists(exprBytes, GenerateLSR(fr1, fr1));
                            exprBytes = MergeLists(exprBytes, GenerateSUB(fr8, fr2));
                            exprBytes = MergeLists(exprBytes, GenerateCND(fr2, fr9));
                            exprBytes = MergeLists(exprBytes, GenerateAND(fr8, fr9));
                            exprBytes = MergeLists(exprBytes, GenerateCBR(fr9, fr3));
                            exprBytes = MergeLists(exprBytes, GenerateMOV(fr7, fr0));
                            exprBytes = MergeLists(exprBytes, GetLList(fr0));

                            FreeReg(fr2);
                            FreeReg(fr3);
                            FreeReg(fr4);
                            FreeReg(fr5);
                            FreeReg(fr6);
                            FreeReg(fr7);
                            FreeReg(fr8);
                            FreeReg(fr9);

                            break;
                        }
                    default:
                        break;
                }

                // 4) Free the other register (the result register is freed upper in the call stack)
                FreeReg(fr1);
            }

            return exprBytes;
        }

        private LinkedList<byte> ConstructReceiver(AASTNode node)
        {
            LinkedList<byte> recBytes = new LinkedList<byte>();

            switch (node.ASTType)
            {
                case "Primary":
                    {
                        recBytes = MergeLists(recBytes, ConstructPrimary(node, false));
                        break;
                    }
                case "REGISTER":
                    {
                        recBytes = MergeLists(recBytes, GetLList(IdentifyRegister(node.Token.Value)));
                        break;
                    }
                case "Dereference":
                    {
                        recBytes = MergeLists(recBytes, ConstructDereference(node, false));
                        break;
                    }
                case "Explicit address":
                    {
                        recBytes = MergeLists(recBytes, ConstructExplicitAddress(node, false));
                        break;
                    }
                default:
                    break;
            }

            return recBytes;
        }

        private LinkedList<byte> ConstructOperand(AASTNode node)
        {
            LinkedList<byte> opBytes = new LinkedList<byte>();

            switch (node.ASTType)
            {
                case "NUMBER":
                    {
                        opBytes = MergeLists(opBytes, ConstructNumber(node));
                        break;
                    }                
                case "Primary":
                    {
                        opBytes = MergeLists(opBytes, ConstructPrimary(node));
                        break;
                    }
                case "REGISTER":
                    {
                        opBytes = MergeLists(opBytes, GetLList(IdentifyRegister(node.Token.Value)));
                        break;
                    }
                case "Dereference":
                    {
                        opBytes = MergeLists(opBytes, ConstructDereference(node));
                        break;
                    }
                case "Reference":
                    {
                        opBytes = MergeLists(opBytes, ConstructReference(node));
                        break;
                    }
                case "Explicit address":
                    {
                        opBytes = MergeLists(opBytes, ConstructExplicitAddress(node));
                        break;
                    }
                case "Expression":
                    {
                        opBytes = MergeLists(opBytes, ConstructExpression(node));
                        break;
                    }
                default:
                    break;
            }

            return opBytes;
        }

        private LinkedList<byte> ConstructExplicitAddress(AASTNode node, bool rightValue = true)
        {
            LinkedList<byte> expAddrBytes = new LinkedList<byte>();
            expAddrBytes = MergeLists(expAddrBytes, GetFreeReg(node));
            byte fr0 = expAddrBytes.Last.Value;
            expAddrBytes.RemoveLast();
            expAddrBytes = MergeLists(expAddrBytes, GenerateLDC(0, fr0));
            expAddrBytes = MergeLists(expAddrBytes, GenerateLDA(fr0, fr0, ((AASTNode)node.Children[1]).AASTValue));
            if (rightValue)
                expAddrBytes = MergeLists(expAddrBytes, GenerateLD(fr0, fr0));
            expAddrBytes = MergeLists(expAddrBytes, GetLList(fr0));
            OccupateReg(fr0);
            return expAddrBytes;
        }

        private LinkedList<byte> ConstructReference(AASTNode node)
        {
            LinkedList<byte> refBytes = new LinkedList<byte>();
            refBytes = MergeLists(refBytes, ConstructPrimary((AASTNode)node.Children[1]));
            // TODO: finish this
            return refBytes;
        }

        private LinkedList<byte> ConstructDereference(AASTNode node, bool rightValue = true)
        {
            LinkedList<byte> derefBytes = new LinkedList<byte>();
            if (node.Children[1].ASTType.Equals("Primary"))
            {
                derefBytes = MergeLists(derefBytes, ConstructPrimary((AASTNode)node.Children[1]));
                byte fr0 = derefBytes.Last.Value;
                derefBytes.RemoveLast();
                if (rightValue)
                    derefBytes = MergeLists(derefBytes, GenerateLD(fr0, fr0));
                derefBytes = MergeLists(derefBytes, GetLList(fr0));
                OccupateReg(fr0);
            }
            else
            {
                derefBytes = MergeLists(derefBytes, GetFreeReg(node));
                byte fr0 = derefBytes.Last.Value;
                derefBytes.RemoveLast();
                if (rightValue)
                {
                    derefBytes = MergeLists(derefBytes, GenerateLD(IdentifyRegister(node.Children[1].Token.Value), fr0));
                    derefBytes = MergeLists(derefBytes, GetLList(fr0));
                }
                else
                {
                    derefBytes = MergeLists(derefBytes, GetLList(IdentifyRegister(node.Children[1].Token.Value)));
                }
                OccupateReg(fr0);
            }
            return derefBytes;
        }

        private LinkedList<byte> ConstructPrimary(AASTNode node, bool rightValue = true)
        {
            LinkedList<byte> primBytes = new LinkedList<byte>();

            Context ctx = SemanticAnalyzer.FindParentContext(node);

            // TODO: Dot-notation
            /*
            int i = 0;
            while (true)
            {
                if (node.Children.Count > (i + 1) && node.Children[i + 1].ASTType.Equals("OPERATOR"))
                {                                
                    // If module or structure - change context and retrieve appropriate address
                    i += 2;
                }
                else
                {
                    break;
                }
            }
            */

            if (!node.Children[^1].ASTType.Equals("IDENTIFIER")) // CallArgs or Expression
            {
                if (node.Children[^1].ASTType.Equals("Expression"))
                {
                    string varName = node.Children[0].Token.Value;

                    primBytes = MergeLists(primBytes, ConstructExpression((AASTNode)node.Children[^1]));
                    byte fr0 = primBytes.Last.Value;
                    primBytes.RemoveLast();

                    int offset = ctx.GetArrayOffset(varName);
                    offset /= 2;
                    while (offset > 0)
                    {
                        primBytes = MergeLists(primBytes, GenerateASL(fr0, fr0));
                        offset /= 2;
                    }

                    primBytes = MergeLists(primBytes, GetFreeReg(node));
                    byte fr1 = primBytes.Last.Value;
                    primBytes.RemoveLast();

                    if (ctx.IsVarGlobal(varName))
                    {
                        primBytes = MergeLists(primBytes, GenerateLDA(SB, fr1, ctx.GetStaticOffset(varName)));
                    }
                    else
                    {
                        primBytes = MergeLists(primBytes, GenerateLDA(FP, fr1, ctx.GetFrameOffset(varName)));
                    }

                    primBytes = MergeLists(primBytes, GenerateADD(fr0, fr1));
                    if (rightValue)
                        primBytes = MergeLists(primBytes, GenerateLD(fr1, fr1));
                    primBytes = MergeLists(primBytes, GetLList(fr1));

                    OccupateReg(fr1);
                    FreeReg(fr0);
                }
                else
                {
                    // Generate call bytes
                }
            }
            else
            {
                string varName = node.Children[0].Token.Value;

                // If this variable is already assigned to the register - we are fine.
                // Otherwise we should find a register for it and load it to this register.
                if (regAllocVTR.ContainsKey(varName))
                {
                    if (rightValue)
                    {
                        // FR0 := Rxy;
                        // fr0
                        byte reg = regAllocVTR[varName];
                        primBytes = MergeLists(primBytes, GetFreeReg(node, new List<int>() { reg }));
                        byte fr0 = primBytes.Last.Value;
                        primBytes.RemoveLast();
                        primBytes = MergeLists(primBytes, GenerateMOV(reg, fr0));
                        primBytes = MergeLists(primBytes, GetLList(fr0));
                        OccupateReg(fr0);
                    }
                    else
                    {
                        primBytes = MergeLists(primBytes, GetLList(regAllocVTR[varName]));
                    }
                }
                else
                {                    
                    if (ctx.IsVarGlobal(varName))
                    {
                        // FR0 := SB + offset;
                        // FR0 := ->FR0;
                        // fr0
                        primBytes = MergeLists(primBytes, GetFreeReg(node));
                        byte fr0 = primBytes.Last.Value;
                        primBytes.RemoveLast();
                        primBytes = MergeLists(primBytes, GenerateLDA(SB, fr0, ctx.GetStaticOffset(varName)));
                        if (rightValue)
                            primBytes = MergeLists(primBytes, GenerateLD(fr0, fr0));
                        primBytes = MergeLists(primBytes, GetLList(fr0));
                        OccupateReg(fr0);
                    }
                    else
                    {
                        // FR0 := FP + offset;
                        // FR0 := ->FR0;
                        // fr0
                        primBytes = MergeLists(primBytes, GetFreeReg(node));
                        byte fr0 = primBytes.Last.Value;
                        primBytes.RemoveLast();
                        primBytes = MergeLists(primBytes, GenerateLDA(FP, fr0, ctx.GetFrameOffset(varName)));
                        if (rightValue)
                            primBytes = MergeLists(primBytes, GenerateLD(fr0, fr0));
                        primBytes = MergeLists(primBytes, GetLList(fr0));
                        OccupateReg(fr0);
                    }
                }
            }

            return primBytes;
        }

        private LinkedList<byte> ConstructNumber(AASTNode node)
        {
            LinkedList<byte> numBytes = new LinkedList<byte>();

            numBytes = MergeLists(numBytes, GetFreeReg(node));
            byte fr0 = numBytes.Last.Value;
            numBytes.RemoveLast();

            if (node.AASTValue > 31 || node.AASTValue < 0)
            {
                // FR0 := FR0 + node.AASTValue;
                // FR0
                numBytes = MergeLists(numBytes, GenerateLDA(fr0, fr0, node.AASTValue));
                // Put an additional byte indicating a register with the result                            
                numBytes = MergeLists(numBytes, GetLList(fr0));
                OccupateReg(fr0);
            }
            else
            {
                // FR0 := node.AASTValue;
                // FR0
                numBytes = MergeLists(numBytes, GenerateLDC(node.AASTValue, fr0));
                // Put an additional byte indicating a register with the result (R26 or R27)
                numBytes = MergeLists(numBytes, GetLList(fr0));
                OccupateReg(fr0);
            }

            return numBytes;
        }

        private LinkedList<byte> ConstructAssemblyBlock(AASTNode node)
        {
            LinkedList<byte> asmBytes = new LinkedList<byte>();

            return asmBytes;
        }

        private LinkedList<byte> ConstructProgram(AASTNode node)
        {
            LinkedList<byte> programBytes = new LinkedList<byte>(); // 2 technical + 8 static + 8 code
            programBytes.AddLast(0x01); // Version
            programBytes.AddLast(0x00); // Padding
            
            // First descent - identify all static data
            LinkedList<byte> staticBytes = GetConstBytes((int)(memorySize)); // We need this value when addressing heap ATTENTION: Do we???
            staticBytes = MergeLists(staticBytes, GetLList(new byte[node.AASTValue])); // We use precalculated length from Semantic Analyzer            
            int staticLength = (staticBytes.Count + staticBytes.Count % 2) / 2; // We count in words (2 bytes)
            
            // Move code data by the static data length
            codeAddrBase += staticBytes.Count;

            // Second descent - identify all code data
            LinkedList<byte> codeBytes = new LinkedList<byte>();
            foreach (AASTNode child in node.Children)
            {
                codeBytes = MergeLists(codeBytes, Construct(child));
            }
            int codeLength = (codeBytes.Count + codeBytes.Count % 2) / 2 + 2;

            // Convert static data and code lengths to chunks of four bytes
            LinkedList<byte> sda = GetConstBytes(staticDataAddrBase);
            LinkedList<byte> sdl = GetConstBytes(staticLength);
            LinkedList<byte> cda = GetConstBytes(codeAddrBase);
            LinkedList<byte> cdl = GetConstBytes(codeLength);
            
            var sda_i = sda.First;
            for (int i = 0; i < 4; i++) // Static data address
            {
                programBytes.AddLast(sda_i.Value);
                sda_i = sda_i.Next;
            }

            var sdl_i = sdl.First;
            for (int i = 0; i < 4; i++) // Static data length
            {
                programBytes.AddLast(sdl_i.Value);
                sdl_i = sdl_i.Next;
            }

            var cda_i = cda.First;
            for (int i = 0; i < 4; i++) // Code address
            {
                programBytes.AddLast(cda_i.Value);
                cda_i = cda_i.Next;
            }

            var cdl_i = cdl.First;
            for (int i = 0; i < 4; i++) // Code length
            {
                programBytes.AddLast(cdl_i.Value);
                cdl_i = cdl_i.Next;
            }

            // Merge previosly constructed bytes
            programBytes = MergeLists(programBytes, staticBytes);
            programBytes = MergeLists(programBytes, codeBytes);

            // Skip & Stop
            programBytes = MergeLists(programBytes, GetLList(0x40, 0x00, 0x00, 0x00));
            
            return programBytes;
        }

        /*
         Helper functions
        */

        private LinkedList<byte> GetLList(params byte[] bytes)
        {
            return new LinkedList<byte>(bytes);
        }

        private LinkedList<byte> GenerateSTOP()
        {
            return GenerateCommand(8, 0, 0, 0);
        }

        private LinkedList<byte> GenerateSKIP()
        {
            return GenerateCommand(16, 0, 0, 0);
        }

        private LinkedList<byte> GenerateCBR(int regI, int regJ)
        {
            return GenerateCommand(32, 15, regI, regJ);
        }

        private LinkedList<byte> GenerateCND(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 14, regI, regJ);
        }

        private LinkedList<byte> GenerateLSR(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 13, regI, regJ);
        }

        private LinkedList<byte> GenerateLSL(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 12, regI, regJ);
        }

        private LinkedList<byte> GenerateXOR(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 11, regI, regJ);
        }

        private LinkedList<byte> GenerateAND(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 10, regI, regJ);
        }

        private LinkedList<byte> GenerateOR(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 9, regI, regJ);
        }

        private LinkedList<byte> GenerateASL(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 8, regI, regJ);
        }

        private LinkedList<byte> GenerateASR(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 7, regI, regJ);
        }

        private LinkedList<byte> GenerateSUB(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 6, regI, regJ);
        }

        private LinkedList<byte> GenerateADD(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 5, regI, regJ);
        }

        private LinkedList<byte> GenerateMOV(int regI, int regJ, int format = 32)
        {
            return GenerateCommand(format, 4, regI, regJ);
        }

        private LinkedList<byte> GenerateST(int regI, int regJ)
        {
            return GenerateCommand(32, 3, regI, regJ);
        }

        private LinkedList<byte> GenerateLDL(int reg, int offset)
        {
            return MergeLists(
                GenerateLDC(0, reg),
                GenerateLDA(reg, reg, offset, true)
                );
        }

        private LinkedList<byte> GenerateLDA(int regI, int regJ, int constant, bool forLDL = false)
        {
            return MergeLists(
                GenerateCommand(forLDL ? 2 : 8, 2, regI, regJ),
                GetConstBytes(constant)
                );
        }

        private LinkedList<byte> GenerateLDC(int constant, int regJ) // Constant is in range [0..31]
        {
            return GenerateCommand(32, 2, constant, regJ);
        }

        private LinkedList<byte> GenerateLD(int regI, int regJ)
        {
            return GenerateCommand(32, 1, regI, regJ);
        }

        private LinkedList<byte> GenerateCommand(int format, int opCode, int regI, int regJ)
        {
            // In LDL case
            if (format != 2)
            {
                format = format == 32 ? 11 : format == 16 ? 1 : 0;                
            }
            return GetLList(
                (byte)((opCode << 2) | (format << 6) | (regI >> 3)),
                (byte)(regJ | (regI << 5))
                );
        }

        private LinkedList<byte> GetConstBytes(int constant)
        {
            byte[] r = BitConverter.GetBytes(constant); // Reversed
            return GetLList(r[3], r[2], r[1], r[0]);
        }

        private LinkedList<byte> GetFreeReg(AASTNode node)
        {
            return GetFreeReg(node, new List<int>());
        }

        private LinkedList<byte> GetFreeReg(AASTNode node, List<int> exclude)
        {
            for (byte ri = 0; ri < regOccup.Length; ri++)
            {
                if (!regOccup[ri])
                {
                    OccupateReg(ri);
                    return GetLList(ri);
                }
            }

            // If all are occupated, load out one of them (the first suitable one).
            // ATTENTION: Is it ok, or am I stupid?            
            Context ctx = SemanticAnalyzer.FindParentContext(node);
            byte regToFree = 0;
            if (exclude.Count < 27) // If it is false, we are doomed...
            {
                while (regToFree < 27) // God bless this while to not loop forever! NOTE: It won't
                {
                    if (!exclude.Contains(regToFree) && (regAllocRTV.ContainsKey(regToFree) || lblAllocRTL.ContainsKey(regToFree))) break;
                    regToFree++;
                }
            }            
            else
            {
                throw new CompilationErrorException("Out of registers!!!");
            }

            if (regAllocRTV.ContainsKey(regToFree))
            {
                string varName = regAllocRTV[regToFree];
                regAllocRTV.Remove(regToFree);
                regAllocVTR.Remove(varName);

                return MergeLists(
                    LoadOutVariable(varName, regToFree, ctx),
                    GetLList(regToFree)
                    );
            }
            else if (lblAllocRTL.ContainsKey(regToFree)) // Load out to heap
            {
                int labelAddress = lblAllocRTL[regToFree];
                lblAllocLTR.Remove(labelAddress);
                lblAllocRTL.Remove(regToFree);
           
                return MergeLists(
                    LoadOutLabel(labelAddress, regToFree),
                    GetLList(regToFree)
                    );
            }
            else
            {
                throw new CompilationErrorException("Out of registers!!!");
            }
        }

        private LinkedList<byte> LoadInVariable(string varName, byte reg, Context ctx)
        {
            if (ctx.IsVarGlobal(varName))
            {
                // R27 := SB + staticOffset;
                // reg := ->R27;
                return MergeLists(
                    GenerateLDA(SB, 27, ctx.GetStaticOffset(varName)),
                    GenerateLD(reg, 27)
                    );
            }
            else
            {
                int blockOffset = ctx.GetVarDeclarationBlockOffset(varName);
                LinkedList<byte> offsetCommands = GenerateMOV(FP, 27); // R27 := FP;                
                for (int i = 0; i < blockOffset; i++)
                {
                    // R27 := R27 - 4; # ATTENTION: May be optimized
                    // R27 := ->R27;
                    offsetCommands = MergeLists(offsetCommands, GenerateLDA(27, 27, -4));
                    offsetCommands = MergeLists(offsetCommands, GenerateLD(27, 27));
                }
                // R27 := R27 + frameOffset;
                // reg := ->27;
                offsetCommands = MergeLists(offsetCommands, GenerateLDA(27, 27, ctx.GetFrameOffset(varName)));
                offsetCommands = MergeLists(offsetCommands, GenerateLD(reg, 27));
                return offsetCommands;
            }
        }

        private LinkedList<byte> LoadOutVariable(string varName, byte reg, Context ctx)
        {
            if (ctx.IsVarGlobal(varName))
            {
                // R27 := SB + staticOffset;
                // ->R27 := reg;
                return MergeLists(
                    GenerateLDA(SB, 27, ctx.GetStaticOffset(varName)),
                    GenerateST(27, reg)
                    );
            }
            else
            {
                int blockOffset = ctx.GetVarDeclarationBlockOffset(varName);
                LinkedList<byte> offsetCommands = GenerateMOV(FP, 27); // R27 := FP;                
                for (int i = 0; i < blockOffset; i++)
                {                        
                    // R27 := R27 - 4; # ATTENTION: May be optimized
                    // R27 := ->R27;
                    offsetCommands = MergeLists(offsetCommands, GenerateLDA(27, 27, -4));
                    offsetCommands = MergeLists(offsetCommands, GenerateLD(27, 27));
                }
                // R27 := R27 + frameOffset;
                // ->R27 := reg;
                offsetCommands = MergeLists(offsetCommands, GenerateLDA(27, 27, ctx.GetFrameOffset(varName)));
                offsetCommands = MergeLists(offsetCommands, GenerateST(27, reg));
                return offsetCommands;
            }
        }

        private LinkedList<byte> LoadInLabel(int labelAddress, byte reg)
        {
            LinkedList<byte> lblLoadBytes = GenerateLDC(0, reg);
            lblLoadBytes = MergeLists(lblLoadBytes, GenerateLDA(reg, reg, (int)(memorySize - labelAddress)));
            lblLoadBytes = MergeLists(lblLoadBytes, GenerateLD(reg, reg));
            return lblLoadBytes;
        }

        private LinkedList<byte> LoadOutLabel(int labelAddress, byte reg)
        {
            LinkedList<byte> lblStoreBytes = GenerateLDC(0, 27);
            lblStoreBytes = MergeLists(lblStoreBytes, GenerateLDA(27, 27, (int)(memorySize - labelAddress)));
            lblStoreBytes = MergeLists(lblStoreBytes, GenerateST(27, reg));
            return lblStoreBytes;
        }

        private void OccupateReg(byte regNum)
        {
            regOccup[regNum] = true;
        }
        
        private void FreeReg(byte regNum)
        {
            // If register is allocated to a variable - do not free it
            // It will be swapped if needed when calling GetFreeReg()
            if (!regAllocRTV.ContainsKey(regNum))
                regOccup[regNum] = false;
        }

        private byte IdentifyRegister(string reg)
        {
            if (reg[0] == 'r')
            {
                if (reg.Length > 2)
                {
                    return (byte)(reg[1] - '0' + (reg[0] - '0') * 10);
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

        private LinkedList<byte> MergeLists(LinkedList<byte> lst1, LinkedList<byte> lst2)
        {
            // NOTE: There is no way it can be faster using native Linked List.
            // May be custom Linked List should be used in future.
            LinkedList<byte> merged = new LinkedList<byte>(lst1);
            foreach (byte b in lst2)
            {
                merged.AddLast(b);
            }
            return merged;
        }

        private HashSet<string> GetAllUsedVars(AASTNode node)
        {
            HashSet<string> set = new HashSet<string>();            
            foreach (AASTNode child in node.Children)
            {
                if (child.ASTType.Equals("Variable definition") || child.ASTType.Equals("IDENTIFIER"))
                {
                    set.Add(child.Token.Value);
                }               
                set.UnionWith(GetAllUsedVars(child));
            }
            return set;
        }

        private byte[] ConvertToAssemblyCode(LinkedList<byte> bincode)
        {
            int i = -1;
            byte a = 0x00; // Since I want to print out bytes in groups of two
            StringBuilder asmCode = new StringBuilder();
            int LDAiter = 0;
            int staticSkip = 0;

            foreach (byte b in bincode)
            {
                i++;
                
                if (LDAiter > 0)
                {
                    LDAiter--;
                    if (LDAiter == 3) asmCode.Append(' ', 7);
                    asmCode.Append(BitConverter.ToString(new byte[] { b }));
                    if (i % 2 == 1) asmCode.Append(" ");
                    if (LDAiter == 0) asmCode.Append("\r\n");
                    continue;
                }
                
                if (i > 5 && i < 10)
                {                    
                    staticSkip |= b << (4 * (9 - i));
                    //continue;
                }

                if (i > 17 && i <= 17 + staticSkip * 2) 
                {
                    asmCode.Append(BitConverter.ToString(new byte[] { b })).Append(" ");
                    continue;
                }

                if (i % 2 == 1)
                {
                    if (i == 17 + staticSkip * 2 + 2) asmCode.Append("\r\n");
                    if (i < 18)
                    {
                        asmCode.Append(BitConverter.ToString(new byte[] { a, b })).Append("\r\n");
                    }
                    else
                    {
                        int format = a >> 6 == 3 ? 32 : a >> 6 == 0 ? 8 : 16;
                        string sFormat = "[f" +
                            (format.ToString().Length == 1 ? "0" + format.ToString() : format.ToString())
                            + "]  ";
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
                                    asmCode.Append(sFormat).Append("stop;\r\n");
                                }
                                else
                                {
                                    asmCode.Append(sFormat).Append("skip;\r\n");
                                }
                                break;
                            case 1: // LD
                                asmCode.Append(sFormat).Append(regj).Append(" := ->").Append(regi).Append(";\r\n");
                                break;
                            case 2: // LDC / LDA
                                if (format == 8)
                                {
                                    asmCode.Append(sFormat).Append(regj).Append(" := ").Append(regi).Append(" + const;\r\n");
                                    LDAiter = 4;
                                }
                                else
                                {
                                    asmCode.Append(sFormat).Append(regj).Append(" := ").Append(regi[1..]).Append(";\r\n");
                                }
                                break;
                            case 3: // ST
                                asmCode.Append(sFormat).Append("->").Append(regj).Append(" := ").Append(regi).Append(";\r\n");
                                break;
                            case 4: // MOV
                                asmCode.Append(sFormat).Append(regj).Append(" := ").Append(regi).Append(";\r\n");
                                break;
                            case 5: // ADD
                                asmCode.Append(sFormat).Append(regj).Append(" += ").Append(regi).Append(";\r\n");
                                break;
                            case 6: // SUB
                                asmCode.Append(sFormat).Append(regj).Append(" -= ").Append(regi).Append(";\r\n");
                                break;
                            case 7: // ASR
                                asmCode.Append(sFormat).Append(regj).Append(" >>= ").Append(regi).Append(";\r\n");
                                break;
                            case 8: // ASL
                                asmCode.Append(sFormat).Append(regj).Append(" <<= ").Append(regi).Append(";\r\n");
                                break;
                            case 9: // OR
                                asmCode.Append(sFormat).Append(regj).Append(" |= ").Append(regi).Append(";\r\n");
                                break;
                            case 10: // AND
                                asmCode.Append(sFormat).Append(regj).Append(" &= ").Append(regi).Append(";\r\n");
                                break;
                            case 11: // XOR
                                asmCode.Append(sFormat).Append(regj).Append(" ^= ").Append(regi).Append(";\r\n");
                                break;
                            case 12: // LSL
                                asmCode.Append(sFormat).Append(regj).Append(" <= ").Append(regi).Append(";\r\n");
                                break;
                            case 13: // LSR
                                asmCode.Append(sFormat).Append(regj).Append(" >= ").Append(regi).Append(";\r\n");
                                break;
                            case 14: // CND
                                asmCode.Append(sFormat).Append(regj).Append(" ?= ").Append(regi).Append(";\r\n");
                                break;
                            case 15: // CBR
                                asmCode.Append(sFormat).Append("if ").Append(regi).Append(" goto ").Append(regj).Append(";\r\n");
                                break;
                            default:
                                asmCode.Append(' ', 7).Append("unknown\r\n");
                                break;
                        }
                    }
                }
                a = b;
            }

            return Encoding.ASCII.GetBytes(asmCode.ToString());
        }

        private void CheckForStackOverflow()
        {
            // ATTENTION: It is not correct since there are also code and static data included into memory size
            // but we have to work with that at least
            if (heapTop + stackTop >= memorySize) 
                throw new CompilationErrorException("Stack overflow!!!");
        }
    }
}
