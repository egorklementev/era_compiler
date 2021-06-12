using ERACompiler.Structures;

namespace ERACompiler.Modules.Generation
{
    class NoChildrenConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            return new CodeNode(aastNode, parent);
        }
    }
}
