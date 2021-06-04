using ERACompiler.Structures;

namespace ERACompiler.Modules.Generation
{
    class WhileConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            Generator g = Program.currentCompiler.generator;
            CodeNode whileNode = new CodeNode(aastNode, parent);

            // *see ForConstructor
            whileNode.Children.AddLast(GetRegisterDeallocationNode(aastNode, whileNode, false)); 

            CodeNode fr1Node = GetFreeRegisterNode(aastNode, whileNode);
            byte fr1 = fr1Node.ByteToReturn;
            whileNode.Children.AddLast(fr1Node);

            CodeNode fr2Node = GetFreeRegisterNode(aastNode, whileNode);
            byte fr2 = fr2Node.ByteToReturn;
            whileNode.Children.AddLast(fr2Node);

            CodeNode fr3Node = GetFreeRegisterNode(aastNode, whileNode);
            byte fr3 = fr3Node.ByteToReturn;
            whileNode.Children.AddLast(fr3Node);

            // Allocate heap space
            byte[] freeRegs = new byte[] { fr1, fr2, fr3 };
            int[] freeRegsAddr = new int[] { 0, 4, 8 };

            CodeNode loopStartLabelNode = new CodeNode("Loop start label", whileNode).Add(new byte[8]); // fr1
            CodeNode loopBodyLabelNode = new CodeNode("Loop body label", whileNode).Add(new byte[8]); // fr2
            CodeNode loopEndLabelNode = new CodeNode("Loop end label", whileNode).Add(new byte[8]); // fr3
            whileNode.Children.AddLast(loopStartLabelNode);
            whileNode.Children.AddLast(loopBodyLabelNode);
            whileNode.Children.AddLast(loopEndLabelNode);

            loopStartLabelNode.Bytes.Clear();
            loopStartLabelNode.Add(GenerateLDL(fr1, GetCurrentBinarySize(loopStartLabelNode)));

            whileNode.Children.AddLast(GetHeapTopChangeNode(whileNode, -12));

            for (int j = 0; j < freeRegs.Length; j++)
            {
                whileNode.Children.AddLast(GetStoreToHeapNode(whileNode, freeRegs[j], freeRegsAddr[j]));
                g.FreeReg(freeRegs[j]);
            }

            CodeNode exprNode = base.Construct((AASTNode)aastNode.Children[0], whileNode);
            byte fr0 = exprNode.ByteToReturn;
            whileNode.Children.AddLast(exprNode);

            for (int j = 0; j < freeRegs.Length; j++)
            {
                CodeNode frNode = GetFreeRegisterNode(aastNode, whileNode);
                freeRegs[j] = frNode.ByteToReturn;
                whileNode.Children.AddLast(frNode);
                whileNode.Children.AddLast(GetLoadFromHeapNode(whileNode, freeRegs[j], freeRegsAddr[j]));
            }

            whileNode.Children.AddLast(GetHeapTopChangeNode(whileNode, 12));

            whileNode.Children.AddLast(new CodeNode("if fr0 cbr", whileNode)
                .Add(GenerateCBR(fr0, freeRegs[1])));
            CodeNode fr0Node = GetFreeRegisterNode(aastNode, whileNode);
            fr0 = fr0Node.ByteToReturn;
            whileNode.Children.AddLast(fr0Node);
            whileNode.Children.AddLast(new CodeNode("fr1 ldc, if fr0 cbr", whileNode)
                .Add(GenerateLDC(1, fr0))
                .Add(GenerateCBR(fr0, freeRegs[2])));
            loopBodyLabelNode.Bytes.Clear();
            loopBodyLabelNode.Add(GenerateLDL(fr2, GetCurrentBinarySize(loopBodyLabelNode)));
            g.FreeReg(fr0);

            whileNode.Children.AddLast(GetHeapTopChangeNode(whileNode, -12));

            for (int j = 0; j < freeRegs.Length; j++)
            {
                whileNode.Children.AddLast(GetStoreToHeapNode(whileNode, freeRegs[j], freeRegsAddr[j]));
                g.FreeReg(freeRegs[j]);
            }

            whileNode.Children.AddLast(base.Construct((AASTNode)aastNode.Children[1], whileNode));

            freeRegs = new byte[] { fr1, fr2, fr3 };

            for (int j = 0; j < freeRegs.Length; j++)
            {
                whileNode.Children.AddLast(GetLoadFromHeapNode(whileNode, freeRegs[j], freeRegsAddr[j]));
                g.OccupateReg(freeRegs[j]);
            }

            whileNode.Children.AddLast(GetHeapTopChangeNode(whileNode, 12));

            whileNode.Children.AddLast(new CodeNode("fr0 ldc, if fr0 cbr", whileNode)
                .Add(GenerateLDC(1, fr0))
                .Add(GenerateCBR(fr0, freeRegs[0])));
            loopEndLabelNode.Bytes.Clear();
            loopEndLabelNode.Add(GenerateLDL(fr3, GetCurrentBinarySize(loopEndLabelNode)));

            for (int i = 0; i < freeRegs.Length; i++)
            {
                g.FreeReg(freeRegs[i]);
            }

            return whileNode;
        }
    }
}
