using ERACompiler.Structures;

namespace ERACompiler.Modules.Generation
{
    class IfConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            Generator g = Program.currentCompiler.generator;
            CodeNode ifNode = new CodeNode(aastNode, parent);

            CodeNode exprNode = base.Construct((AASTNode)aastNode.Children[0], ifNode);
            byte fr0 = exprNode.ByteToReturn;
            ifNode.Children.AddLast(exprNode);

            CodeNode fr1Node = GetFreeRegisterNode(aastNode, ifNode);
            byte fr1 = fr1Node.ByteToReturn;
            ifNode.Children.AddLast(fr1Node);

            if (aastNode.Children.Count < 3) // No "else" block
            {
                CodeNode l1Node = new CodeNode("L1 label", ifNode).Add(new byte[8]);
                CodeNode l2Node = new CodeNode("L2 label", ifNode).Add(new byte[8]);

                ifNode.Children.AddLast(l1Node);
                ifNode.Children.AddLast(new CodeNode("CBR if true", ifNode).Add(GenerateCBR(fr0, fr1)));
                ifNode.Children.AddLast(l2Node);
                ifNode.Children.AddLast(new CodeNode("CBR if false", ifNode).Add(GenerateCBR(fr1, fr1)));
                g.FreeReg(fr0);
                g.FreeReg(fr1);
                l1Node.Bytes.Clear();
                l1Node.Add(GenerateLDL(fr1, GetCurrentBinarySize(l1Node)));
                ifNode.Children.AddLast(base.Construct((AASTNode)aastNode.Children[1], ifNode));
                l2Node.Bytes.Clear();
                l2Node.Add(GenerateLDL(fr1, GetCurrentBinarySize(l2Node)));
            }
            else // With "else" block
            {
                CodeNode l1Node = new CodeNode("L1 label", ifNode).Add(new byte[8]);
                CodeNode l2Node = new CodeNode("L2 label", ifNode).Add(new byte[8]);

                ifNode.Children.AddLast(l1Node);
                ifNode.Children.AddLast(new CodeNode("CBR if true", ifNode).Add(GenerateCBR(fr0, fr1)));
                g.FreeReg(fr0);
                g.FreeReg(fr1);
                ifNode.Children.AddLast(base.Construct((AASTNode)aastNode.Children[2], ifNode));
                ifNode.Children.AddLast(l2Node);
                ifNode.Children.AddLast(new CodeNode("CBR if false", ifNode).Add(GenerateCBR(fr1, fr1)));
                l1Node.Bytes.Clear();
                l1Node.Add(GenerateLDL(fr1, GetCurrentBinarySize(l1Node)));
                ifNode.Children.AddLast(base.Construct((AASTNode)aastNode.Children[1], ifNode));
                l2Node.Bytes.Clear();
                l2Node.Add(GenerateLDL(fr1, GetCurrentBinarySize(l2Node)));
            }

            return ifNode;
        }
    }
}
