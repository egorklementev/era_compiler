using ERACompiler.Structures;

namespace ERACompiler.Modules.Generation
{
    class LoopWhileConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            Generator g = Program.currentCompiler.generator;
            CodeNode loopWhileNode = new CodeNode(aastNode, parent);

            // *see ForConstructor
            loopWhileNode.Children.AddLast(GetRegisterDeallocationNode(aastNode, loopWhileNode, false)); 

            CodeNode fr1Node = GetFreeRegisterNode(aastNode, loopWhileNode);
            byte fr1 = fr1Node.ByteToReturn;
            loopWhileNode.Children.AddLast(fr1Node);

            CodeNode loopStartLabelNode = new CodeNode("Loop start label", loopWhileNode); // fr1
            loopWhileNode.Children.AddLast(loopStartLabelNode);
            loopStartLabelNode.Add(GenerateLDL(fr1, GetCurrentBinarySize(loopStartLabelNode)));

            loopWhileNode.Children.AddLast(GetHeapTopChangeNode(loopWhileNode, -4));

            loopWhileNode.Children.AddLast(GetStoreToHeapNode(loopWhileNode, fr1, 0));
            g.FreeReg(fr1);

            loopWhileNode.Children.AddLast(base.Construct((AASTNode)aastNode.Children[0], loopWhileNode));

            CodeNode exprNode = base.Construct((AASTNode)aastNode.Children[1], loopWhileNode);
            byte fr0 = exprNode.ByteToReturn;
            loopWhileNode.Children.AddLast(exprNode);
            
            CodeNode fr2Node = GetFreeRegisterNode(aastNode, loopWhileNode);
            byte fr2 = fr2Node.ByteToReturn;
            loopWhileNode.Children.AddLast(fr2Node);

            loopWhileNode.Children.AddLast(new CodeNode("fr0 fr2 mov", loopWhileNode).Add(GenerateMOV(fr0, fr2)));
            loopWhileNode.Children.AddLast(GetLoadFromHeapNode(loopWhileNode, fr1, 0));
            loopWhileNode.Children.AddLast(GetHeapTopChangeNode(loopWhileNode, 4));
            loopWhileNode.Children.AddLast(new CodeNode("if fr2 cbr", loopWhileNode).Add(GenerateCBR(fr2, fr1)));

            g.FreeReg(fr0);
            g.FreeReg(fr1);
            g.FreeReg(fr2);

            return loopWhileNode;
        }
    }
}
