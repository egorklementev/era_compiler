using ERACompiler.Structures;

namespace ERACompiler.Modules.Generation
{
    class AssemblyBlockConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            CodeNode asmBlockNode = new CodeNode(aastNode, parent);
            // TODO: assembly block & statements
            return asmBlockNode;
        }
    }
}
