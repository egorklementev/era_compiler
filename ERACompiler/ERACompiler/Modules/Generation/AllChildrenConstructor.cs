using ERACompiler.Structures;

namespace ERACompiler.Modules.Generation
{
    class AllChildrenConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            // Just create a new CodeNode and construct its children
            CodeNode toReturn = new CodeNode(aastNode, parent);
            foreach (AASTNode child in aastNode.Children)
            {
                toReturn.Children.AddLast(base.Construct(child, toReturn));
            }
            return toReturn;
        }
    }
}
