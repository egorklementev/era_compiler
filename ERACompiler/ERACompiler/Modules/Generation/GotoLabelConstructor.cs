using ERACompiler.Structures;

namespace ERACompiler.Modules.Generation
{
    class GotoLabelConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            CodeNode labelNode = new CodeNode(aastNode, parent);
            return labelNode;
        }
    }
}
