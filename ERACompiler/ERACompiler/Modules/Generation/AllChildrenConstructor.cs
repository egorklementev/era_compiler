using ERACompiler.Structures;

namespace ERACompiler.Modules.Generation
{
    class AllChildrenConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            CodeNode toReturn = new CodeNode(aastNode, parent);
            foreach (AASTNode child in aastNode.Children)
            {
                toReturn.Children.AddLast(base.Construct(child, toReturn));
            }
            return toReturn;
        }
    }
}
