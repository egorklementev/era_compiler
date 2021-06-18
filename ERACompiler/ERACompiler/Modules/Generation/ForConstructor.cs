using ERACompiler.Structures;
using ERACompiler.Utilities.Errors;

namespace ERACompiler.Modules.Generation
{
    class ForConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            Generator g = Program.currentCompiler.generator;
            CodeNode forNode = new CodeNode(aastNode, parent);

            // Special deallocation since we do not know what have been deallocated inside the loop block body
            // (basically, when leaving a block body, we deallocate everyting since we want to get rid of loop iterators),
            // so we have to have no allocated registers at all before entering the loop block body, so that
            // the loop block body has to allocate everyting.
            // REASON: to resolve 'goto' artifacts and some other "features".
            forNode.Children.AddLast(GetRegisterDeallocationNode(aastNode, forNode, false)); 

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
            while (aastNode.Children[i].ASTType.Equals("Expression"))
            {
                switch (((AASTNode)aastNode.Children[i]).AASTValue)
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
             * Generate FROM expression bytes
             */
            byte fr0;
            if (hasFrom)
            {
                CodeNode exprNode = base.Construct((AASTNode)aastNode.Children[iFrom], forNode);
                fr0 = exprNode.ByteToReturn;
                forNode.Children.AddLast(exprNode);
            }
            else
            {
                CodeNode fr0Node = GetFreeRegisterNode(aastNode, forNode);
                fr0 = fr0Node.ByteToReturn;
                forNode.Children.AddLast(fr0Node);
                forNode.Children.AddLast(new CodeNode("fr0 ldc", forNode).Add(GenerateLDC(defaultFrom, fr0)));
            }

            // Store FROM value to the iterator variable
            CodeNode fr6Node = GetFreeRegisterNode(aastNode, forNode);
            byte fr6 = fr6Node.ByteToReturn;
            forNode.Children.AddLast(fr6Node);
            forNode.Children.AddLast(new CodeNode("For iterator store", forNode)
                .Add(GenerateLDA(SP, fr6, 4))
                .Add(GenerateST(fr0, fr6)));
            g.FreeReg(fr6);

            // Allocate registers
            CodeNode fr3Node = GetFreeRegisterNode(aastNode, forNode);
            byte fr3 = fr3Node.ByteToReturn;
            forNode.Children.AddLast(fr3Node);

            CodeNode fr4Node = GetFreeRegisterNode(aastNode, forNode);
            byte fr4 = fr4Node.ByteToReturn;
            forNode.Children.AddLast(fr4Node);

            CodeNode fr5Node = GetFreeRegisterNode(aastNode, forNode);
            byte fr5 = fr5Node.ByteToReturn;
            forNode.Children.AddLast(fr5Node);

            // Allocate heap for registers that should be saved for loop operation
            byte[] freeRegs = new byte[] { fr0, fr3, fr4, fr5 };
            int[] freeRegsAddr = new int[] { 0, 4, 8, 12 };

            // Loop start label
            CodeNode loopStartLabelDeclNode = new CodeNode("Label declaration", forNode).Add(new byte[8]);
            loopStartLabelDeclNode.ByteToReturn = fr3;
            CodeNode loopStartLabelNode = new CodeNode("Label", forNode);
            loopStartLabelNode.LabelDecl = loopStartLabelDeclNode;
            forNode.Children.AddLast(loopStartLabelDeclNode); // to fr3

            // Loop end label
            CodeNode loopEndLabelDeclNode = new CodeNode("Label declaration", forNode).Add(new byte[8]);
            loopEndLabelDeclNode.ByteToReturn = fr4;
            CodeNode loopEndLabelNode = new CodeNode("Label", forNode);
            loopEndLabelNode.LabelDecl = loopEndLabelDeclNode;
            forNode.Children.AddLast(loopEndLabelDeclNode); // to fr4

            forNode.Children.AddLast(new CodeNode("fr5 := 6", forNode).Add(GenerateLDC(6, fr5)));

            forNode.Children.AddLast(loopStartLabelNode);

            /* 
             * Generate TO expression bytes
             */
            forNode.Children.AddLast(GetHeapTopChangeNode(forNode, -16));

            for (int j = 0; j < freeRegs.Length; j++)
            {
                forNode.Children.AddLast(GetStoreToHeapNode(forNode, freeRegs[j], freeRegsAddr[j]));
                g.FreeReg(freeRegs[j]);
            }

            byte fr1;
            if (hasTo)
            {
                CodeNode exprNode = base.Construct((AASTNode)aastNode.Children[iTo], forNode);
                fr1 = exprNode.ByteToReturn;
                forNode.Children.AddLast(exprNode);
            }
            else
            {
                CodeNode fr1Node = GetFreeRegisterNode(aastNode, forNode);
                fr1 = fr1Node.ByteToReturn;
                forNode.Children.AddLast(fr1Node);
                forNode.Children.AddLast(new CodeNode("fr1 ldc", forNode).Add(GenerateLDC(defaultTo, fr1)));
            }

            // Check for deallocated registers
            for (int j = 0; j < freeRegs.Length; j++)
            {
                CodeNode frNode = GetFreeRegisterNode(aastNode, forNode);
                freeRegs[j] = frNode.ByteToReturn;
                forNode.Children.AddLast(frNode);
                forNode.Children.AddLast(GetLoadFromHeapNode(forNode, freeRegs[j], freeRegsAddr[j]));
            }

            forNode.Children.AddLast(GetHeapTopChangeNode(forNode, 16));
            // ---

            CodeNode fr2Node = GetFreeRegisterNode(aastNode, forNode);
            byte fr2 = fr2Node.ByteToReturn;
            forNode.Children.AddLast(fr2Node);

            forNode.Children.AddLast(new CodeNode("loop end check", forNode)
                .Add(GenerateMOV(freeRegs[0], fr2))
                .Add(GenerateCND(fr1, fr2))
                .Add(GenerateAND(freeRegs[3], fr2))
                .Add(GenerateCBR(fr2, freeRegs[2])));
            g.FreeReg(fr1);
            g.FreeReg(fr2);

            /* 
             * Generate FOR_BLOCK expression bytes
             */
            forNode.Children.AddLast(GetHeapTopChangeNode(forNode, -16));

            for (int j = 0; j < freeRegs.Length; j++)
            {
                forNode.Children.AddLast(GetStoreToHeapNode(forNode, freeRegs[j], freeRegsAddr[j]));
                g.FreeReg(freeRegs[j]);
            }

            
            forNode.Children.AddLast(base.Construct((AASTNode)aastNode.Children[^1], forNode));

            /* 
             * Generate STEP expression bytes
             */

            if (hasStep)
            {
                CodeNode exprNode = base.Construct((AASTNode)aastNode.Children[iStep], forNode);
                fr2 = exprNode.ByteToReturn;
                forNode.Children.AddLast(exprNode);
            }
            else
            {
                CodeNode frNode = GetFreeRegisterNode(aastNode, forNode);
                fr2 = frNode.ByteToReturn;
                forNode.Children.AddLast(frNode);
                forNode.Children.AddLast(new CodeNode("fr2 ldc", forNode).Add(GenerateLDC(defaultStep, fr2)));
            }

            for (int j = 0; j < freeRegs.Length; j++)
            {
                CodeNode frNode = GetFreeRegisterNode(aastNode, forNode);
                freeRegs[j] = frNode.ByteToReturn;
                forNode.Children.AddLast(frNode);
                forNode.Children.AddLast(GetLoadFromHeapNode(forNode, freeRegs[j], freeRegsAddr[j]));
            }
            forNode.Children.AddLast(GetHeapTopChangeNode(forNode, 16));

            // ---

            forNode.Children.AddLast(new CodeNode("fr2 fr0 add", forNode).Add(GenerateADD(fr2, freeRegs[0])));
            g.FreeReg(fr2);
            
            forNode.Children.AddLast(GetHeapTopChangeNode(forNode, -16));
            for (int j = 0; j < freeRegs.Length; j++)
            {
                forNode.Children.AddLast(GetStoreToHeapNode(forNode, freeRegs[j], freeRegsAddr[j]));
                g.FreeReg(freeRegs[j]);
            }

            freeRegs = new byte[] { fr0, fr3, fr4, fr5 };

            for (int j = 0; j < freeRegs.Length; j++)
            {
                forNode.Children.AddLast(GetLoadFromHeapNode(forNode, freeRegs[j], freeRegsAddr[j]));
                g.OccupateReg(freeRegs[j]);
            }
            forNode.Children.AddLast(GetHeapTopChangeNode(forNode, 16));

            // Update iterator variable
            // Iterator is always 'int'
            CodeNode frIterNode = GetFreeRegisterNode(aastNode, forNode);
            fr6 = frIterNode.ByteToReturn;
            forNode.Children.AddLast(frIterNode);
            forNode.Children.AddLast(new CodeNode("for loop iterator update", forNode)
                .Add(GenerateLDA(SP, fr6, 4))
                .Add(GenerateST(freeRegs[0], fr6))
                .Add(GenerateLDC(1, fr6)) // Since we do not want to override FR3
                .Add(GenerateCBR(fr6, freeRegs[1])));
            forNode.Children.AddLast(loopEndLabelNode);
            g.FreeReg(fr6);

            // Deallocate heap & free registers
            for (int j = 0; j < freeRegs.Length; j++)
            {
                g.FreeReg(freeRegs[j]);
            }

            return forNode;
        }
    }
}
