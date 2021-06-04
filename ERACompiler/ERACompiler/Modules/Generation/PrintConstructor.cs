using ERACompiler.Structures;

namespace ERACompiler.Modules.Generation
{
    class PrintConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            Generator g = Program.currentCompiler.generator;
            CodeNode printNode = new CodeNode(aastNode, parent);
            CodeNode exprNode = base.Construct((AASTNode)aastNode.Children[1], printNode);
            byte fr0 = exprNode.ByteToReturn;
            printNode.Children.AddLast(exprNode);
            printNode.Children.AddLast(new CodeNode("Print", printNode).Add(GeneratePRINT(fr0)));
            g.FreeReg(fr0);
            return printNode;
        }
    }
}
