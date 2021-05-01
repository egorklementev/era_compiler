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

        private readonly bool[] regOccup = new bool[27]; // For reigster allocation algorithm (R27 is always used, no variable stored in R27)
        private readonly Dictionary<string, byte> regAllocVTR = new Dictionary<string, byte>(); // Variable-to-Register dictionary
        private readonly Dictionary<byte, string> regAllocRTV = new Dictionary<byte, string>(); // Register-to-Variable dictionary
        //private readonly Dictionary<byte, int> heapAlloc = new Dictionary<byte, int>(); // Register-to-Label
        //private readonly Dictionary<int, byte> lblAllocLTR = new Dictionary<int, byte>(); // Label-to-Register

        private const int FP = 28, SP = 29, SB = 30, PC = 31;

        private readonly uint memorySize = 2 * 1024 * 1024; // ATTENTION: 2 megabytes for now. TODO: make this value configurable

        /// <summary>
        /// Default constructor
        /// </summary>
        public Generator()
        {
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
                        // Offset in bytes
                        int offset = BitConverter.ToInt32(
                            new byte[]
                            {
                                node.Next.Next.Next.Next.Value,
                                node.Next.Next.Next.Value,
                                node.Next.Next.Value,
                                node.Next.Value,
                            }
                            );
                        int curPos = i - 18; // ATTENTION: Check needed
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
            if (Program.config.ConvertToAsmCode)
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
            LinkedList<byte> bytes;
            switch (node.ASTType)
            {
                case "Program":
                    bytes = ConstructProgram(node);
                    break;
                case "Assembly block":
                    bytes = ConstructAssemblyBlock(node);
                    break;
                case "Expression":
                    bytes = ConstructExpression(node);
                    break;
                case "Code":
                    bytes = ConstructCode(node);
                    break;
                case "Variable definition":
                    bytes = ConstructVarDef(node);
                    break;
                case "Array definition":
                    bytes = ConstructArrDef(node);
                    break;
                case "Assignment":
                    bytes = ConstructAssignment(node);
                    break;
                case "If":
                    bytes = ConstructIf(node);
                    break;
                case "For":
                    bytes = ConstructFor(node);
                    break;
                case "Block body":
                    bytes = ConstructBlockBody(node);
                    break;
                case "While":
                    bytes = ConstructWhile(node);
                    break;
                case "Loop While":
                    bytes = ConstructLoopWhile(node);
                    break;
                case "Routine body":
                    bytes = ConstructRoutineBody(node);
                    break;
                case "Return":
                    bytes = ConstructReturn(node);
                    break;
                case "Call":
                    bytes = ConstructCall(node);
                    break;
                case "Print":
                    bytes = ConstructPrint(node);
                    break;
                default: // If skip, just go for children nodes
                    {
                        bytes = new LinkedList<byte>();
                        foreach (AASTNode child in node.Children)
                        {
                            bytes = MergeLists(bytes, Construct(child));
                        }
                        break;
                    }                
            }

            return bytes;
        }

        private LinkedList<byte> ConstructPrint(AASTNode node)
        {
            LinkedList<byte> printBytes = ConstructExpression((AASTNode)node.Children[1]);
            byte fr0 = printBytes.Last.Value;
            printBytes.RemoveLast();
            printBytes = MergeLists(printBytes, GeneratePRINT(fr0));
            FreeReg(fr0);
            return printBytes;
        }

        private LinkedList<byte> ConstructReturn(AASTNode node)
        {
            LinkedList<byte> returnBytes = new LinkedList<byte>();
            Context ctx = SemanticAnalyzer.FindParentContext(node);

            if (node.Children.Count > 0)
            {
                // Call or Expr
                returnBytes = MergeLists(returnBytes, ConstructExpression((AASTNode)node.Children[0]));
                byte fr1 = returnBytes.Last.Value;
                returnBytes.RemoveLast();
                returnBytes = MergeLists(returnBytes, GenerateMOV(fr1, 26));
                OccupateReg(26);
                FreeReg(fr1);
            }

            returnBytes = MergeLists(returnBytes, DeallocateRegisters(node, ctx, 0, true));

            // Deallocate dynamic arrays
            returnBytes = MergeLists(returnBytes, GetFreeReg(node));
            byte fr0 = returnBytes.Last.Value;
            returnBytes.RemoveLast();
            foreach (AASTNode var in ctx.GetDeclaredVars())
            {
                if (ctx.IsVarDynamicArray(var.Token.Value))
                {
                    returnBytes = MergeLists(returnBytes, LoadFromHeap(0, fr0));
                    returnBytes = MergeLists(returnBytes, ChangeHeapTop(fr0, true, false));
                }
            }
            FreeReg(fr0);

            // Deallocate all scope related memory from the stack
            int ctxNum = 0;
            ASTNode anchor = node.Parent;
            while (!anchor.ASTType.Equals("Routine body"))
            {
                if (((AASTNode)anchor).Context != null)
                    ctxNum++;
                anchor = anchor.Parent;
            }
            for (int i = 0; i < ctxNum; i++)
            {
                returnBytes = MergeLists(returnBytes, GenerateLDA(FP, FP, -4));
                returnBytes = MergeLists(returnBytes, GenerateMOV(FP, SP)); // Return stack pointer
                returnBytes = MergeLists(returnBytes, GenerateLD(FP, FP)); // Return frame pointer
            }

            returnBytes = MergeLists(returnBytes, GenerateLDA(FP, FP, -4));
            returnBytes = MergeLists(returnBytes, GenerateLD(FP, 27));
            returnBytes = MergeLists(returnBytes, GenerateLDA(FP, FP, -4));
            returnBytes = MergeLists(returnBytes, GenerateMOV(FP, SP)); // Return stack pointer
            returnBytes = MergeLists(returnBytes, GenerateLD(FP, FP)); // Return frame pointer
            returnBytes = MergeLists(returnBytes, GenerateCBR(27, 27));

            return returnBytes;
        }

        private LinkedList<byte> ConstructRoutineBody(AASTNode node)
        {
            LinkedList<byte> routineBytes = new LinkedList<byte>();
            Context ctx = SemanticAnalyzer.FindParentContext(node);

            int frameSize = 0;
            if (ctx.GetDeclaredVars().Count > 0)
            {
                frameSize =
                    ctx.GetFrameOffset(ctx.GetDeclaredVars().Last().Token.Value) +
                    ctx.GetDeclaredVars().Last().AASTType.GetSize();
            }

            routineBytes = MergeLists(routineBytes, GenerateST(FP, SP)); 
            routineBytes = MergeLists(routineBytes, GenerateMOV(SP, FP)); 
            routineBytes = MergeLists(routineBytes, GenerateLDA(FP, FP, 4));
            routineBytes = MergeLists(routineBytes, GenerateST(27, FP)); 
            routineBytes = MergeLists(routineBytes, GenerateLDA(FP, FP, 4));
            routineBytes = MergeLists(routineBytes, GenerateMOV(FP, SP));
            routineBytes = MergeLists(routineBytes, GenerateLDA(SP, SP, frameSize));

            int statementNum = 1;
            foreach (AASTNode statement in node.Children)
            {
                routineBytes = MergeLists(routineBytes, AllocateRegisters(statement, ctx, statementNum));
                routineBytes = MergeLists(routineBytes, Construct(statement));
                statementNum++;
                routineBytes = MergeLists(routineBytes, DeallocateRegisters(statement, ctx, statementNum));
            }

            // Deallocate dynamic arrays
            routineBytes = MergeLists(routineBytes, GetFreeReg(node));
            byte fr0 = routineBytes.Last.Value;
            routineBytes.RemoveLast();
            foreach (AASTNode var in ctx.GetDeclaredVars())
            {
                if (ctx.IsVarDynamicArray(var.Token.Value))
                {
                    routineBytes = MergeLists(routineBytes, LoadFromHeap(0, fr0)); // Size of dynamic array
                    routineBytes = MergeLists(routineBytes, ChangeHeapTop(fr0, true, false));
                }
            }
            FreeReg(fr0);

            routineBytes = MergeLists(routineBytes, GenerateLDA(FP, FP, -4));
            routineBytes = MergeLists(routineBytes, GenerateLD(FP, 27));
            routineBytes = MergeLists(routineBytes, GenerateLDA(FP, FP, -4));
            routineBytes = MergeLists(routineBytes, GenerateMOV(FP, SP)); // Return stack pointer
            routineBytes = MergeLists(routineBytes, GenerateLD(FP, FP)); // Return frame pointer
            routineBytes = MergeLists(routineBytes, GenerateCBR(27, 27));

            return routineBytes;
        }

        private LinkedList<byte> ConstructArrDef(AASTNode node)
        {
            LinkedList<byte> arrDefBytes = new LinkedList<byte>();
            Context ctx = SemanticAnalyzer.FindParentContext(node);

            if (((ArrayType)node.AASTType).Size == 0) // We have to allocate it on the heap
            {
                int offsetSize = ctx.GetArrayOffsetSize(node.Token.Value) / 2;
                arrDefBytes = MergeLists(arrDefBytes, ConstructExpression((AASTNode)node.Children[0]));
                byte fr0 = arrDefBytes.Last.Value; // Execution-time array size
                arrDefBytes.RemoveLast();
                while (offsetSize > 0)
                {
                    arrDefBytes = MergeLists(arrDefBytes, GenerateASL(fr0, fr0));
                    offsetSize /= 2;
                }
                arrDefBytes = MergeLists(arrDefBytes, GetFreeReg(node));
                byte fr1 = arrDefBytes.Last.Value;
                arrDefBytes.RemoveLast();
                arrDefBytes = MergeLists(arrDefBytes, GetFreeReg(node));
                byte fr2 = arrDefBytes.Last.Value;
                arrDefBytes.RemoveLast();
                /* heap: [array_size 4 bytes] [0th element] [1st element] ... [last element];  Allocated register contains address of 0th element. */
                arrDefBytes = MergeLists(arrDefBytes, GenerateLDA(fr0, fr0, 4)); // Allocate 4 additional bytes for array size
                arrDefBytes = MergeLists(arrDefBytes, ChangeHeapTop(fr0, true)); // heapTop 
                arrDefBytes = MergeLists(arrDefBytes, StoreToHeap(0, fr0)); // heap[0] := fr0;
                arrDefBytes = MergeLists(arrDefBytes, GenerateLDC(0, fr1)); // fr1 := 0;
                arrDefBytes = MergeLists(arrDefBytes, GenerateLD(fr1, fr2)); // fr2 := ->fr1;
                arrDefBytes = MergeLists(arrDefBytes, GenerateLDA(fr2, fr2, 4)); // fr2 := fr2 + 4; # Skip array size bytes
                if (regAllocVTR.ContainsKey(node.Token.Value)) 
                {
                    arrDefBytes = MergeLists(
                            arrDefBytes,
                            GenerateMOV(fr2, regAllocVTR[node.Token.Value])
                        );
                }
                else
                {
                    arrDefBytes = MergeLists(arrDefBytes, LoadOutVariable(node.Token.Value, fr2, ctx));
                }

                FreeReg(fr0);
                FreeReg(fr1);
                FreeReg(fr2);
            }

            return arrDefBytes;
        }

        private LinkedList<byte> ConstructLoopWhile(AASTNode node)
        {
            LinkedList<byte> loopWhileBytes = new LinkedList<byte>();

            loopWhileBytes = MergeLists(loopWhileBytes, GetFreeReg(node));
            byte fr1 = loopWhileBytes.Last.Value;
            loopWhileBytes.RemoveLast();

            loopWhileBytes = MergeLists(loopWhileBytes, GenerateLDL(fr1));
            int loopStartPos = loopWhileBytes.Count;
            ResolveLabel(loopWhileBytes, loopStartPos);

            loopWhileBytes = MergeLists(loopWhileBytes, ChangeHeapTop(-4));

            loopWhileBytes = MergeLists(loopWhileBytes, StoreToHeap(0, fr1));
            FreeReg(fr1);

            loopWhileBytes = MergeLists(loopWhileBytes, Construct((AASTNode)node.Children[0]));
            loopWhileBytes = MergeLists(loopWhileBytes, ConstructExpression((AASTNode)node.Children[1]));
            byte fr0 = loopWhileBytes.Last.Value;
            loopWhileBytes.RemoveLast();
            
            loopWhileBytes = MergeLists(loopWhileBytes, GetFreeReg(node));
            byte fr2 = loopWhileBytes.Last.Value;
            loopWhileBytes.RemoveLast();
            loopWhileBytes = MergeLists(loopWhileBytes, GenerateMOV(fr0, fr2));

            loopWhileBytes = MergeLists(loopWhileBytes, LoadFromHeap(0, fr1));

            loopWhileBytes = MergeLists(loopWhileBytes, ChangeHeapTop(4));

            loopWhileBytes = MergeLists(loopWhileBytes, GenerateCBR(fr2, fr1));

            FreeReg(fr0);
            FreeReg(fr1);
            FreeReg(fr2);

            return loopWhileBytes;
        }

        private LinkedList<byte> ConstructWhile(AASTNode node)
        {
            LinkedList<byte> whileBytes = new LinkedList<byte>();

            whileBytes = MergeLists(whileBytes, GetFreeReg(node));
            byte fr1 = whileBytes.Last.Value;
            whileBytes.RemoveLast();

            whileBytes = MergeLists(whileBytes, GetFreeReg(node));
            byte fr2 = whileBytes.Last.Value;
            whileBytes.RemoveLast();

            whileBytes = MergeLists(whileBytes, GetFreeReg(node));
            byte fr3 = whileBytes.Last.Value;
            whileBytes.RemoveLast();

            // Allocate heap space
            byte[] freeRegs = new byte[] { fr1, fr2, fr3 };
            int[] freeRegsAddr = new int[] { 0, 4, 8 };

            whileBytes = MergeLists(whileBytes, GenerateLDL(fr1));
            int loopStartPos = whileBytes.Count;
            whileBytes = MergeLists(whileBytes, GenerateLDL(fr2));
            int loopBodyPos = whileBytes.Count;
            whileBytes = MergeLists(whileBytes, GenerateLDL(fr3));
            int loopEndPos = whileBytes.Count;

            ResolveLabel(whileBytes, loopStartPos);

            whileBytes = MergeLists(whileBytes, ChangeHeapTop(-12));

            for (int j = 0; j < freeRegs.Length; j++)
            {
                whileBytes = MergeLists(whileBytes, StoreToHeap(freeRegsAddr[j], freeRegs[j]));
                FreeReg(freeRegs[j]);
            }

            whileBytes = MergeLists(whileBytes, ConstructExpression((AASTNode)node.Children[0]));
            byte fr0 = whileBytes.Last.Value;
            whileBytes.RemoveLast();

            for (int j = 0; j < freeRegs.Length; j++)
            {
                whileBytes = MergeLists(whileBytes, GetFreeReg(node));
                freeRegs[j] = whileBytes.Last.Value;
                whileBytes.RemoveLast();
                whileBytes = MergeLists(whileBytes, LoadFromHeap(freeRegsAddr[j], freeRegs[j]));
            }

            whileBytes = MergeLists(whileBytes, ChangeHeapTop(12));

            whileBytes = MergeLists(whileBytes, GenerateCBR(fr0, freeRegs[1]));
            whileBytes = MergeLists(whileBytes, GetFreeReg(node));
            fr0 = whileBytes.Last.Value;
            whileBytes.RemoveLast();
            whileBytes = MergeLists(whileBytes, GenerateLDC(1, fr0));
            whileBytes = MergeLists(whileBytes, GenerateCBR(fr0, freeRegs[2]));
            ResolveLabel(whileBytes, loopBodyPos);
            FreeReg(fr0);

            whileBytes = MergeLists(whileBytes, ChangeHeapTop(-12));

            for (int j = 0; j < freeRegs.Length; j++)
            {
                whileBytes = MergeLists(whileBytes, StoreToHeap(freeRegsAddr[j], freeRegs[j]));
                FreeReg(freeRegs[j]);
            }

            whileBytes = MergeLists(whileBytes, Construct((AASTNode)node.Children[1]));

            freeRegs = new byte[] { fr1, fr2, fr3 };

            for (int j = 0; j < freeRegs.Length; j++)
            {
                whileBytes = MergeLists(whileBytes, LoadFromHeap(freeRegsAddr[j], freeRegs[j]));
                OccupateReg(freeRegs[j]);
            }

            whileBytes = MergeLists(whileBytes, ChangeHeapTop(12));

            whileBytes = MergeLists(whileBytes, GenerateLDC(1, fr0));
            whileBytes = MergeLists(whileBytes, GenerateCBR(fr0, freeRegs[0]));
            ResolveLabel(whileBytes, loopEndPos);

            for (int i = 0; i < freeRegs.Length; i++)
            {
                FreeReg(freeRegs[i]);
            }

            return whileBytes;
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
                    ctx.GetDeclaredVars().Last().AASTType.GetSize();
            }

            bbBytes = MergeLists(bbBytes, GenerateST(FP, SP)); // Store where to return
            bbBytes = MergeLists(bbBytes, GenerateMOV(SP, FP)); 
            bbBytes = MergeLists(bbBytes, GenerateLDA(FP, FP, 4));
            bbBytes = MergeLists(bbBytes, GenerateMOV(FP, SP));
            bbBytes = MergeLists(bbBytes, GenerateLDA(SP, SP, frameSize));

            int statementNum = 1;
            foreach (AASTNode statement in node.Children)
            {
                bbBytes = MergeLists(bbBytes, AllocateRegisters(statement, ctx, statementNum));
                bbBytes = MergeLists(bbBytes, Construct(statement));
                statementNum++;
                bbBytes = MergeLists(bbBytes, DeallocateRegisters(statement, ctx, statementNum));
            }

            // Deallocate dynamic arrays
            bbBytes = MergeLists(bbBytes, GetFreeReg(node));
            byte fr0 = bbBytes.Last.Value;
            bbBytes.RemoveLast();
            foreach (AASTNode var in ctx.GetDeclaredVars())
            {
                if (ctx.IsVarDynamicArray(var.Token.Value))
                {
                    bbBytes = MergeLists(bbBytes, LoadFromHeap(0, fr0)); // Size of dynamic array
                    bbBytes = MergeLists(bbBytes, ChangeHeapTop(fr0, true, false));
                }
            }
            FreeReg(fr0);

            bbBytes = MergeLists(bbBytes, GenerateLDA(FP, FP, -4));
            bbBytes = MergeLists(bbBytes, GenerateMOV(FP, SP)); // Return stack pointer
            bbBytes = MergeLists(bbBytes, GenerateLD(FP, FP)); // Return frame pointer

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

            /* 
             * --- 
             * Generate FROM expression bytes
             * ---
             */
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
            byte[] freeRegs = new byte[] { fr0, fr3, fr4, fr5 };
            int[] freeRegsAddr = new int[] { 0, 4, 8, 12 };

            // Loop start label
            forBytes = MergeLists(forBytes, GenerateLDL(fr3));
            int loopStartPos = forBytes.Count;

            // Loop end label
            forBytes = MergeLists(forBytes, GenerateLDL(fr4));
            int loopEndPos = forBytes.Count;

            forBytes = MergeLists(forBytes, GenerateLDC(6, fr5));

            ResolveLabel(forBytes, loopStartPos);

            forBytes = MergeLists(forBytes, ChangeHeapTop(-16));

            /* 
             * --- 
             * Generate TO expression bytes
             * ---
             */
            for (int j = 0; j < freeRegs.Length; j++)
            {
                forBytes = MergeLists(forBytes, StoreToHeap(freeRegsAddr[j], freeRegs[j]));
                FreeReg(freeRegs[j]);
            }

            byte fr1;
            if (hasTo)
            {
                forBytes = MergeLists(forBytes, ConstructExpression((AASTNode)node.Children[iTo]));
                fr1 = forBytes.Last.Value;
                forBytes.RemoveLast();
            }
            else
            {
                forBytes = MergeLists(forBytes, GetFreeReg(node));
                fr1 = forBytes.Last.Value;
                forBytes.RemoveLast();
                forBytes = MergeLists(forBytes, GenerateLDC(defaultTo, fr1));
            }

            // Check for deallocated registers
            for (int j = 0; j < freeRegs.Length; j++)
            {
                forBytes = MergeLists(forBytes, GetFreeReg(node));
                freeRegs[j] = forBytes.Last.Value;
                forBytes.RemoveLast();
                forBytes = MergeLists(forBytes, LoadFromHeap(freeRegsAddr[j], freeRegs[j]));
            }
            // ---

            forBytes = MergeLists(forBytes, ChangeHeapTop(16));

            forBytes = MergeLists(forBytes, GetFreeReg(node));
            byte fr2 = forBytes.Last.Value;
            forBytes.RemoveLast();

            forBytes = MergeLists(forBytes, GenerateMOV(freeRegs[0], fr2));
            forBytes = MergeLists(forBytes, GenerateCND(fr1, fr2));
            forBytes = MergeLists(forBytes, GenerateAND(freeRegs[3], fr2));
            forBytes = MergeLists(forBytes, GenerateCBR(fr2, freeRegs[2]));
            FreeReg(fr1);
            FreeReg(fr2);

            forBytes = MergeLists(forBytes, ChangeHeapTop(-16));

            /* 
             * --- 
             * Generate FOR_BLOCK expression bytes
             * ---
             */
            for (int j = 0; j < freeRegs.Length; j++)
            {
                forBytes = MergeLists(forBytes, StoreToHeap(freeRegsAddr[j], freeRegs[j]));
                FreeReg(freeRegs[j]);
            }

            forBytes = MergeLists(forBytes, Construct((AASTNode)node.Children[^1]));

            for (int j = 0; j < freeRegs.Length; j++)
            {
                forBytes = MergeLists(forBytes, GetFreeReg(node));
                freeRegs[j] = forBytes.Last.Value;
                forBytes.RemoveLast();
                forBytes = MergeLists(forBytes, LoadFromHeap(freeRegsAddr[j], freeRegs[j]));
            }
            // ---

            /* 
             * --- 
             * Generate STEP expression bytes
             * ---
             */
            for (int j = 0; j < freeRegs.Length; j++)
            {
                forBytes = MergeLists(forBytes, StoreToHeap(freeRegsAddr[j], freeRegs[j]));
                FreeReg(freeRegs[j]);
            }

            if (hasStep)
            {
                forBytes = MergeLists(forBytes, ConstructExpression((AASTNode)node.Children[iStep]));
                fr2 = forBytes.Last.Value;
                forBytes.RemoveLast();
            }
            else
            {
                forBytes = MergeLists(forBytes, GetFreeReg(node));
                fr2 = forBytes.Last.Value;
                forBytes.RemoveLast();
                forBytes = MergeLists(forBytes, GenerateLDC(defaultStep, fr2));
            }

            for (int j = 0; j < freeRegs.Length; j++)
            {
                forBytes = MergeLists(forBytes, GetFreeReg(node));
                freeRegs[j] = forBytes.Last.Value;
                forBytes.RemoveLast();
                forBytes = MergeLists(forBytes, LoadFromHeap(freeRegsAddr[j], freeRegs[j]));
            }
            // ---

            forBytes = MergeLists(forBytes, ChangeHeapTop(16));

            forBytes = MergeLists(forBytes, GenerateADD(fr2, freeRegs[0]));

            forBytes = MergeLists(forBytes, ChangeHeapTop(-16));

            for (int j = 0; j < freeRegs.Length; j++)
            {
                forBytes = MergeLists(forBytes, StoreToHeap(freeRegsAddr[j], freeRegs[j]));
                FreeReg(freeRegs[j]);
            }

            freeRegs = new byte[] { fr0, fr3, fr4, fr5 };

            for (int j = 0; j < freeRegs.Length; j++)
            {
                forBytes = MergeLists(forBytes, LoadFromHeap(freeRegsAddr[j], freeRegs[j]));
                OccupateReg(freeRegs[j]);
            }

            forBytes = MergeLists(forBytes, ChangeHeapTop(16));
            
            // Update iterator variable
            forBytes = MergeLists(forBytes, GetFreeReg(node));
            fr6 = forBytes.Last.Value;
            forBytes.RemoveLast();
            forBytes = MergeLists(forBytes, GenerateLDA(SP, fr6, 4));
            forBytes = MergeLists(forBytes, GenerateST(freeRegs[0], fr6));
            forBytes = MergeLists(forBytes, GenerateLDC(1, fr6)); // Since we do not want to override FR3
            forBytes = MergeLists(forBytes, GenerateCBR(fr6, freeRegs[1]));
            ResolveLabel(forBytes, loopEndPos);
            FreeReg(fr6);

            // Deallocate heap & free registers
            for (int j = 0; j < freeRegs.Length; j++)
            {
                FreeReg(freeRegs[j]);
            }

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

            if (node.Children.Count < 3) // No "else" block
            {
                ifBytes = MergeLists(ifBytes, GenerateLDL(fr1));
                int l1Pos = ifBytes.Count;
                ifBytes = MergeLists(ifBytes, GenerateCBR(fr0, fr1));
                ifBytes = MergeLists(ifBytes, GenerateLDL(fr1));
                int l2Pos = ifBytes.Count;
                ifBytes = MergeLists(ifBytes, GenerateCBR(fr1, fr1));
                FreeReg(fr0);
                FreeReg(fr1);
                ResolveLabel(ifBytes, l1Pos);                
                ifBytes = MergeLists(ifBytes, Construct((AASTNode)node.Children[1]));
                ResolveLabel(ifBytes, l2Pos);
            }
            else // With "else" block
            {   
                ifBytes = MergeLists(ifBytes, GenerateLDL(fr1));
                int l1Pos = ifBytes.Count;
                ifBytes = MergeLists(ifBytes, GenerateCBR(fr0, fr1));
                FreeReg(fr0);
                FreeReg(fr1);
                ifBytes = MergeLists(ifBytes, Construct((AASTNode)node.Children[2]));
                ifBytes = MergeLists(ifBytes, GenerateLDL(fr1));
                int l2Pos = ifBytes.Count;
                ifBytes = MergeLists(ifBytes, GenerateCBR(fr1, fr1));
                ResolveLabel(ifBytes, l1Pos);
                ifBytes = MergeLists(ifBytes, Construct((AASTNode)node.Children[1]));
                ResolveLabel(ifBytes, l2Pos);
            }

            return ifBytes;
        }

        private LinkedList<byte> ConstructAssignment(AASTNode node)
        {
            LinkedList<byte> asgnBytes = new LinkedList<byte>();
            Context ctx = SemanticAnalyzer.FindParentContext(node);

            asgnBytes = MergeLists(asgnBytes, ConstructExpression((AASTNode)node.Children[1]));
            byte fr0 = asgnBytes.Last.Value;
            asgnBytes.RemoveLast();

            asgnBytes = MergeLists(asgnBytes, ConstructReceiver((AASTNode)node.Children[0]));
            byte fr1 = asgnBytes.Last.Value;
            asgnBytes.RemoveLast();

            // TODO: Dot-notation here
            if (regAllocVTR.ContainsKey(node.Children[0].Token.Value) && !ctx.IsVarArray(node.Children[0].Token))
            {
                asgnBytes = MergeLists(asgnBytes, GenerateMOV(fr0, fr1));
            }
            else
            {
                if (node.Children[0].ASTType.Equals("REGISTER"))
                {
                    asgnBytes = MergeLists(asgnBytes, GenerateMOV(fr0, fr1));
                }
                else
                {
                    asgnBytes = MergeLists(asgnBytes, GenerateST(fr0, fr1));
                }
            }

            FreeReg(fr0);
            FreeReg(fr1);

            return asgnBytes;
        }

        private LinkedList<byte> ConstructVarDef(AASTNode node)
        {
            LinkedList<byte> varDefBytes = new LinkedList<byte>();
            Context ctx = SemanticAnalyzer.FindParentContext(node);

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
                        // If we have initial assignment - store it to register/memory
                        if (node.Children.Count > 0)
                        {
                            varDefBytes = MergeLists(varDefBytes, ConstructExpression((AASTNode)node.Children[0]));
                            byte fr0 = varDefBytes.Last.Value;
                            varDefBytes.RemoveLast();

                            if (regAllocVTR.ContainsKey(node.Token.Value))
                            {                                
                                byte reg = regAllocVTR[node.Token.Value];                                
                                varDefBytes = MergeLists(varDefBytes, GenerateMOV(fr0, reg));
                            }
                            else
                            {
                                varDefBytes = MergeLists(varDefBytes, LoadOutVariable(node.Token.Value, fr0, ctx));
                            }

                            FreeReg(fr0);
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
            
            codeBytes = MergeLists(codeBytes, GenerateMOV(SP, FP)); // FP := SP;
            
            if (ctx.GetDeclaredVars().Count > 0)
            {
                int frameSize =
                    ctx.GetFrameOffset(ctx.GetDeclaredVars().Last().Token.Value) +
                    ctx.GetDeclaredVars().Last().AASTType.GetSize();
                codeBytes = MergeLists(codeBytes, GenerateLDA(SP, SP, frameSize));
            }

            int statementNum = 1;
            foreach (AASTNode statement in node.Children)
            {
                codeBytes = MergeLists(codeBytes, AllocateRegisters(statement, ctx, statementNum));
                // Recursive statement bincode generation
                codeBytes = MergeLists(codeBytes, Construct(statement));
                statementNum++;
                codeBytes = MergeLists(codeBytes, DeallocateRegisters(statement, ctx, statementNum));
            }

            // Deallocate dynamic arrays
            codeBytes = MergeLists(codeBytes, GetFreeReg(node));
            byte fr0 = codeBytes.Last.Value;
            codeBytes.RemoveLast();
            foreach (AASTNode var in ctx.GetDeclaredVars())
            {
                if (ctx.IsVarDynamicArray(var.Token.Value))
                {
                    codeBytes = MergeLists(codeBytes, LoadFromHeap(0, fr0)); // Size of dynamic array
                    codeBytes = MergeLists(codeBytes, ChangeHeapTop(fr0, true, false));
                }
            }
            FreeReg(fr0);

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
                exprBytes = MergeLists(exprBytes, ConstructOperand((AASTNode)node.Children[2], fr0));
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
                    case ">=":
                        {
                            // FR0 >= FR1; FR0
                            exprBytes = MergeLists(exprBytes, MergeLists(GenerateLSR(fr1, fr0), GetLList(fr0)));
                            break;
                        }
                    case "<=":
                        {
                            // FR0 <= FR1; FR0
                            exprBytes = MergeLists(exprBytes, MergeLists(GenerateLSL(fr1, fr0), GetLList(fr0)));
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
                            // FR1 ?= FR0;
                            // FR2 &= FR1;
                            // FR0 = 1;
                            // FR1 = <true>;
                            // if FR2 goto FR0;
                            // FR0 := 0;
                            // <true>
                            // fr0
                            exprBytes = MergeLists(exprBytes, GenerateLDC(mask, fr2));
                            exprBytes = MergeLists(exprBytes, GenerateCND(fr0, fr1));
                            exprBytes = MergeLists(exprBytes, GenerateAND(fr1, fr2));
                            exprBytes = MergeLists(exprBytes, GenerateLDC(1, fr0));
                            exprBytes = MergeLists(exprBytes, GenerateLDL(fr1));
                            int trueLabel = exprBytes.Count;
                            exprBytes = MergeLists(exprBytes, GenerateCBR(fr2, fr1));
                            exprBytes = MergeLists(exprBytes, GenerateLDC(0, fr0));
                            ResolveLabel(exprBytes, trueLabel);
                            exprBytes = MergeLists(exprBytes, GetLList(fr0));
                            FreeReg(fr2);
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
                            exprBytes = MergeLists(exprBytes, GenerateLDL(fr3));
                            int multPos = exprBytes.Count;
                            exprBytes = MergeLists(exprBytes, GenerateLDL(fr4));
                            int addPos = exprBytes.Count;
                            exprBytes = MergeLists(exprBytes, GenerateLDL(fr5));
                            int naddPos = exprBytes.Count;
                            exprBytes = MergeLists(exprBytes, GenerateLDC(1, fr6));
                            exprBytes = MergeLists(exprBytes, GenerateLDC(0, fr7));
                            exprBytes = MergeLists(exprBytes, GenerateLDC(1, fr8));
                            ResolveLabel(exprBytes, multPos); // <mult>
                            exprBytes = MergeLists(exprBytes, GenerateLDC(0, fr9));
                            exprBytes = MergeLists(exprBytes, GenerateAND(fr1, fr6));
                            exprBytes = MergeLists(exprBytes, GenerateCBR(fr6, fr4));
                            exprBytes = MergeLists(exprBytes, GenerateCBR(fr8, fr5));
                            ResolveLabel(exprBytes, addPos); // <add>
                            exprBytes = MergeLists(exprBytes, GenerateADD(fr0, fr7));
                            ResolveLabel(exprBytes, naddPos); // <not_add>
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

        private LinkedList<byte> ConstructOperand(AASTNode node, byte fr_op = 100)
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
                        opBytes = MergeLists(opBytes, ConstructPrimary(node, true, fr_op));
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
            // It shloud be in SemanticAnalyzer, but it is here. I am sorry.
            if (((AASTNode)node.Children[1]).AASTValue < 0)
            {
                throw new SemanticErrorException("Incorrect explicit address!!!\n" +
                    "  At (Line: " + node.Children[1].Token.Position.Line + ", Char: " + node.Children[1].Token.Position.Char + " )."
                    );
            }

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
            refBytes = MergeLists(refBytes, ConstructPrimary((AASTNode)node.Children[1], false, 100, false));
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

        private LinkedList<byte> ConstructPrimary(AASTNode node, bool rightValue = true, byte fr_op = 100, bool isReceiver = true)
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

                    int offset = ctx.GetArrayOffsetSize(varName) / 2;
                    while (offset > 0)
                    {
                        primBytes = MergeLists(primBytes, GenerateASL(fr0, fr0));
                        offset /= 2;
                    }

                    primBytes = MergeLists(primBytes, GetFreeReg(node));
                    byte fr1 = primBytes.Last.Value;
                    primBytes.RemoveLast();

                    if (regAllocVTR.ContainsKey(varName))
                    {
                        primBytes = MergeLists(primBytes, GenerateMOV(regAllocVTR[varName], fr1));
                    }
                    else
                    {
                        primBytes = MergeLists(primBytes, LoadInVariable(varName, fr1, ctx, ctx.IsVarDynamicArray(varName)));
                        regAllocVTR.Add(varName, fr1); 
                        regAllocRTV.Add(fr1, varName);
                    }

                    primBytes = MergeLists(primBytes, GenerateADD(fr0, fr1));
                    if (rightValue)
                        primBytes = MergeLists(primBytes, GenerateLD(fr1, fr1));
                    primBytes = MergeLists(primBytes, GetLList(fr1));

                    FreeReg(fr0);
                }
                else
                {
                    // Load out to heap the first operand (due to recursion)
                    if (fr_op != 100)
                    {
                        primBytes = MergeLists(primBytes, ChangeHeapTop(-4));
                        primBytes = MergeLists(primBytes, StoreToHeap(0, fr_op));
                        primBytes = MergeLists(primBytes, ConstructCall(node));
                        byte fr = primBytes.Last.Value;
                        primBytes.RemoveLast();
                        primBytes = MergeLists(primBytes, LoadFromHeap(0, fr_op));
                        primBytes = MergeLists(primBytes, ChangeHeapTop(4));
                        primBytes = MergeLists(primBytes, GetLList(fr));
                    } 
                    else
                    {
                        primBytes = MergeLists(primBytes, ConstructCall(node));
                    }
                }
            }
            else
            {
                string varName = node.Children[0].Token.Value;

                // If this variable is already assigned to the register - we are fine.
                // Otherwise we should find a register for it and load it to this register.
                if (regAllocVTR.ContainsKey(varName))
                {
                    // FR0 := Rxy;
                    // fr0
                    if (rightValue)
                    {
                        byte reg = regAllocVTR[varName];
                        primBytes = MergeLists(primBytes, GetFreeReg(node, new List<int>() { reg }));
                        byte fr0 = primBytes.Last.Value;
                        primBytes.RemoveLast();
                        primBytes = MergeLists(primBytes, GenerateMOV(reg, fr0));
                        primBytes = MergeLists(primBytes, GetLList(fr0));
                    }
                    else
                    {
                        if (isReceiver)
                        {
                            primBytes = MergeLists(primBytes, GetLList(regAllocVTR[varName]));
                        }
                        else // Reference
                        {
                            byte reg = regAllocVTR[varName];
                            primBytes = MergeLists(primBytes, GetFreeReg(node, new List<int>() { reg }));
                            byte fr0 = primBytes.Last.Value;
                            primBytes.RemoveLast();
                            primBytes = MergeLists(primBytes, LoadInVariable(varName, fr0, ctx, rightValue));
                            primBytes = MergeLists(primBytes, GetLList(fr0));
                        }
                    }
                }
                else
                {
                    primBytes = MergeLists(primBytes, GetFreeReg(node));
                    byte fr0 = primBytes.Last.Value;
                    primBytes.RemoveLast();
                    primBytes = MergeLists(primBytes, LoadInVariable(varName, fr0, ctx, rightValue));
                    regAllocVTR.Add(varName, fr0);
                    regAllocRTV.Add(fr0, varName);
                    primBytes = MergeLists(primBytes, GetLList(fr0));
                }
            }

            return primBytes;
        }

        private LinkedList<byte> ConstructCall(AASTNode node)
        {
            // prim [ iden, call args ]
            // 
            // Generate call bytes TODO: Dot notation (routines in modules)
            //
            // Construct parameters and put them in the stack
            // Deallocate everything
            // R27 = SB + offset(func);
            // if R27 goto R27;
            // Allocate back
            // Manage return value (if any) 

            LinkedList<byte> callBytes = new LinkedList<byte>();
            Context ctx = SemanticAnalyzer.FindParentContext(node);

            int param_i = 2;
            foreach (AASTNode expr in node.Children[1].Children)
            {
                callBytes = MergeLists(callBytes, ConstructExpression(expr));
                byte fr0 = callBytes.Last.Value;
                callBytes.RemoveLast();

                callBytes = MergeLists(callBytes, GenerateMOV(SP, 27));
                callBytes = MergeLists(callBytes, GenerateLDA(27, 27, param_i * 4));
                callBytes = MergeLists(callBytes, GenerateST(fr0, 27));

                param_i++;
                FreeReg(fr0);
            }

            callBytes = MergeLists(callBytes, DeallocateRegisters(node, ctx, 0, true));
            callBytes = MergeLists(callBytes, GenerateLDA(SB, 27, ctx.GetStaticOffset(node.Children[0].Token.Value)));
            callBytes = MergeLists(callBytes, GenerateLD(27, 27));
            callBytes = MergeLists(callBytes, GenerateCBR(27, 27));
            callBytes = MergeLists(callBytes, AllocateRegisters(node, ctx, GetStatementNumber(node)));

            if (ctx.GetRoutineReturnType(node.Children[0].Token).Type != VarType.ERAType.NO_TYPE) // Return value is in R26
            {
                callBytes = MergeLists(callBytes, GetFreeReg(node));
                byte fr0 = callBytes.Last.Value;
                callBytes.RemoveLast();
                callBytes = MergeLists(callBytes, GenerateMOV(26, fr0));
                callBytes = MergeLists(callBytes, GetLList(fr0));
                FreeReg(26);
            }

            return callBytes;
        }

        private LinkedList<byte> ConstructNumber(AASTNode node)
        {
            LinkedList<byte> numBytes = new LinkedList<byte>();

            numBytes = MergeLists(numBytes, GetFreeReg(node));
            byte fr0 = numBytes.Last.Value;
            numBytes.RemoveLast();

            if (node.AASTValue > 31 || node.AASTValue < 0)
            {
                // FR0 := 0;
                // FR0 := FR0 + node.AASTValue;
                // FR0
                numBytes = MergeLists(numBytes, GenerateLDC(0, fr0));
                numBytes = MergeLists(numBytes, GenerateLDA(fr0, fr0, node.AASTValue));
                // Put an additional byte indicating a register with the result                            
                numBytes = MergeLists(numBytes, GetLList(fr0));
                //OccupateReg(fr0);
            }
            else
            {
                // FR0 := node.AASTValue;
                // FR0
                numBytes = MergeLists(numBytes, GenerateLDC(node.AASTValue, fr0));
                // Put an additional byte indicating a register with the result (R26 or R27)
                numBytes = MergeLists(numBytes, GetLList(fr0));
                //OccupateReg(fr0);
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
            LinkedList<byte> programBytes = new LinkedList<byte>();
            programBytes.AddLast(0x01); // Version
            programBytes.AddLast(0x00); // Padding
            
            // First descent - identify all static data
            LinkedList<byte> staticBytes = GetConstBytes(memorySize); // [HEAP TOP] We need this value when addressing heap 
            staticBytes = MergeLists(staticBytes, GetLList(new byte[node.AASTValue])); // We use precalculated length from Semantic Analyzer            
            int staticLength = (staticBytes.Count + staticBytes.Count % 2) / 2; // We count in words (2 bytes)

            // First unit offset - to store correct addresses inside static frame
            int techOffset = 16; // LDA(SB), LDA(SB + codeOffset), 27 = ->27, if 27 goto 27
            LinkedList<byte> techBytes = GenerateLDA(SB, SB, 4); // Skip heap top bytes

            // Identify all modules and routines
            int modulesAndRoutines = 0;
            foreach (AASTNode child in node.Children)
            { 
                if (child.ASTType.Equals("Routine") || child.ASTType.Equals("Module") || child.ASTType.Equals("Code"))
                {
                    modulesAndRoutines++;
                }
            }

            techOffset += modulesAndRoutines * 16;

            // Identify all code data
            LinkedList<byte> codeBytes = new LinkedList<byte>();
            foreach (AASTNode child in node.Children)
            {
                if (child.ASTType.Equals("Routine") || child.ASTType.Equals("Module") || child.ASTType.Equals("Code"))
                {
                    techBytes = MergeLists(techBytes, GenerateLDA(SB, 27, node.Context.GetStaticOffset(child.Context.Name)));
                    techBytes = MergeLists(techBytes, GenerateLDC(0, 26));
                    techBytes = MergeLists(techBytes, GenerateLDA(26, 26, staticBytes.Count + techOffset + codeBytes.Count));
                    techBytes = MergeLists(techBytes, GenerateST(26, 27));
                }

                codeBytes = MergeLists(codeBytes, Construct(child));
            }

            // Go to code module uncoditionally
            techBytes = MergeLists(techBytes, GenerateLDA(SB, 27, node.Context.GetStaticOffset("code")));
            techBytes = MergeLists(techBytes, GenerateLD(27, 27));
            techBytes = MergeLists(techBytes, GenerateCBR(27, 27));

            // Move code data by the static data length
            codeAddrBase += staticBytes.Count;
            int codeLength = (techBytes.Count + codeBytes.Count + codeBytes.Count % 2) / 2 + 2;

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
            programBytes = MergeLists(programBytes, techBytes);
            programBytes = MergeLists(programBytes, codeBytes);

            // Skip & Stop
            programBytes = MergeLists(programBytes, GenerateSKIP());
            programBytes = MergeLists(programBytes, GenerateSTOP());
            
            return programBytes;
        }

        /*
            Helper functions
        */

        private void ResolveLabel(LinkedList<byte> list, int pos)
        {
            var node = list.First;
            for (int i = 0; i < pos - 4; i++)
            {
                node = node.Next;
            }
            byte[] r = BitConverter.GetBytes(list.Count - pos + 5);
            node.Value = r[3];
            node.Next.Value = r[2];
            node.Next.Next.Value = r[1];
            node.Next.Next.Next.Value = r[0];
        }

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

        private LinkedList<byte> GenerateLDL(int reg)
        {
            return MergeLists(
                GenerateLDC(0, reg),
                GenerateLDA(reg, reg, 0, true)
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

        private LinkedList<byte> GeneratePRINT(int reg)
        {
            return GenerateCommand(2, 0, reg, 0);
        }

        private LinkedList<byte> GenerateCommand(int format, int opCode, int regI, int regJ)
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

        private LinkedList<byte> GetConstBytes(int constant)
        {
            byte[] r = BitConverter.GetBytes(constant); // Reversed
            return GetLList(r[3], r[2], r[1], r[0]);
        }

        private LinkedList<byte> GetConstBytes(uint constant)
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
                    if (!exclude.Contains(regToFree) && (regAllocRTV.ContainsKey(regToFree))) break;
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
            else
            {
                throw new CompilationErrorException("Out of registers!!!");
            }
        }

        private LinkedList<byte> LoadInVariable(string varName, byte reg, Context ctx, bool rightValue = true)
        {
            if (ctx.IsVarGlobal(varName))
            {
                LinkedList<byte> lst = GenerateLDA(SB, 27, ctx.GetStaticOffset(varName));
                if (rightValue)
                    lst = MergeLists(lst, GenerateLD(27, reg));
                else
                    lst = MergeLists(lst, GenerateMOV(27, reg));
                return lst;
            }
            else
            {
                int blockOffset = ctx.GetVarDeclarationBlockOffset(varName);
                LinkedList<byte> offsetCommands = GenerateMOV(FP, 27);          
                for (int i = 0; i < blockOffset; i++)
                {
                    offsetCommands = MergeLists(offsetCommands, GenerateLDA(27, 27, -4));
                    offsetCommands = MergeLists(offsetCommands, GenerateLD(27, 27));
                }
                offsetCommands = MergeLists(offsetCommands, GenerateLDA(27, 27, ctx.GetFrameOffset(varName)));
                if (rightValue)
                    offsetCommands = MergeLists(offsetCommands, GenerateLD(27, reg));
                else
                    offsetCommands = MergeLists(offsetCommands, GenerateMOV(27, reg));
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
                    GenerateST(reg, 27)
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
                offsetCommands = MergeLists(offsetCommands, GenerateST(reg, 27));
                return offsetCommands;
            }
        }

        private LinkedList<byte> LoadFromHeap(int labelAddress, byte reg)
        {
            LinkedList<byte> loadHeapBytes = GenerateLDC(0, reg);
            loadHeapBytes = MergeLists(loadHeapBytes, GenerateLD(reg, reg));
            loadHeapBytes = MergeLists(loadHeapBytes, GenerateLDA(reg, reg, labelAddress));
            loadHeapBytes = MergeLists(loadHeapBytes, GenerateLD(reg, reg));
            return loadHeapBytes;
        }

        private LinkedList<byte> StoreToHeap(int address, byte reg)
        {
            LinkedList<byte> bytes = GenerateLDC(0, 27);
            bytes = MergeLists(bytes, GenerateLD(27, 27));
            bytes = MergeLists(bytes, GenerateLDA(27, 27, address));
            bytes = MergeLists(bytes, GenerateST(reg, 27));
            return bytes;
        }

        private LinkedList<byte> ChangeHeapTop(int offset, bool useAsReg = false, bool decrease = true)
        {
            LinkedList<byte> lst = new LinkedList<byte>();
            lst = MergeLists(lst, GenerateLDC(0, 27)); 
            lst = MergeLists(lst, GenerateLD(27, 27));
            if (useAsReg)
            {
                if (decrease)
                {
                    lst = MergeLists(lst, GenerateSUB(offset, 27)); // When we load the heap
                }
                else
                {
                    lst = MergeLists(lst, GenerateADD(offset, 27)); // When we free the heap
                }
            }
            else
            {
                lst = MergeLists(lst, GenerateLDA(27, 27, offset));
            }
            lst = MergeLists(lst, GenerateLDC(0, SB)); 
            lst = MergeLists(lst, GenerateST(27, SB));
            lst = MergeLists(lst, GenerateLDC(4, SB)); // Tricky trick 
            return lst;
        }

        private LinkedList<byte> GetHeapTop(AASTNode node, byte reg)
        {
            LinkedList<byte> lst = GetFreeReg(node);
            byte fr0 = lst.Last.Value;
            lst.RemoveLast();
            lst = MergeLists(lst, GenerateLDC(0, fr0));
            lst = MergeLists(lst, GenerateLD(fr0, reg));
            FreeReg(fr0);
            return lst;
        }

        private LinkedList<byte> AllocateRegisters(AASTNode node, Context ctx, int statementNum)
        {
            LinkedList<byte> regAllocBytes = new LinkedList<byte>();

            HashSet<string> vars = GetAllUsedVars(node);
            foreach (string var in vars)
            {
                if (ctx.IsVarDeclaredInThisContext(var))
                {
                    int liStart = ctx.GetLIStart(var);
                    int liEnd = ctx.GetLIEnd(var);

                    if (!regAllocVTR.ContainsKey(var) && liStart <= statementNum) // Allocation (if possible)
                    {
                        for (byte ri = 0; ri < 27; ri++)
                        {
                            if (!regOccup[ri])
                            {
                                bool arrayCheck = !ctx.IsVarArray(var) || ctx.IsVarDynamicArray(var);
                                regAllocBytes = MergeLists(regAllocBytes, LoadInVariable(var, ri, ctx, arrayCheck));
                                regAllocVTR.Add(var, ri);
                                regAllocRTV.Add(ri, var);
                                OccupateReg(ri);
                                break;
                            }
                        }
                    }
                }
            }
            return regAllocBytes;
        }

        private LinkedList<byte> DeallocateRegisters(AASTNode node, Context ctx, int statementNum, bool unconditionally = false)
        {
            LinkedList<byte> regDeallocBytes = new LinkedList<byte>();
            HashSet<string> vars = GetAllUsedVars(node);
            foreach (string var in vars)
            {
                if (ctx.IsVarDeclaredInThisContext(var))
                {
                    int liStart = ctx.GetLIStart(var);
                    int liEnd = ctx.GetLIEnd(var);

                    if (regAllocVTR.ContainsKey(var) && (liEnd < statementNum || unconditionally)) // Deallocation
                    {
                        byte reg = regAllocVTR[var];
                        if (!ctx.IsVarArray(var)) // No need to load out array stuff
                            regDeallocBytes = MergeLists(regDeallocBytes, LoadOutVariable(var, reg, ctx));
                        regAllocVTR.Remove(var);
                        regAllocRTV.Remove(reg);
                        FreeReg(reg);
                    }
                }
            }
            return regDeallocBytes;
        }

        private int GetStatementNumber(AASTNode node)
        {
            if (node.BlockPosition != 0)
                return node.BlockPosition;
            else
                return GetStatementNumber((AASTNode)node.Parent);
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
                if (child.ASTType.Equals("Variable definition") || child.ASTType.Equals("Constant definition") || child.ASTType.Equals("Array definition") || child.ASTType.Equals("IDENTIFIER"))
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
                                    asmCode.Append(sFormat).Append(regj).Append(" := ").Append(bregi).Append(";\r\n");
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
    }
}
