using ERACompiler.Structures;

namespace ERACompiler.Modules.Generation
{
    class LiteralConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            CodeNode literalNode = new CodeNode(aastNode, parent);

            CodeNode fr0Node = GetFreeRegisterNode(aastNode, literalNode);
            byte fr0 = fr0Node.ByteToReturn;
            literalNode.Children.AddLast(fr0Node);
            literalNode.ByteToReturn = fr0;

            if (aastNode.AASTValue > 31 || aastNode.AASTValue < 0)
            {
                // FR0 := 0;
                // FR0 := FR0 + node.AASTValue;
                // FR0
                literalNode.Children.AddLast(new CodeNode("Load LDA constant", literalNode)
                    .Add(GenerateLDC(0, fr0))
                    .Add(GenerateLDA(fr0, fr0, aastNode.AASTValue)));
            }
            else
            {
                // FR0 := node.AASTValue;
                // FR0
                literalNode.Children.AddLast(new CodeNode("Load LDC constant", literalNode)
                    .Add(GenerateLDC(aastNode.AASTValue, fr0)));
            }

            return literalNode;
        }
    }
}
