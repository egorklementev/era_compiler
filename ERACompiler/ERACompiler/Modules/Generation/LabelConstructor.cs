using ERACompiler.Structures;

namespace ERACompiler.Modules.Generation
{
    class LabelConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            CodeNode labelNode = new CodeNode(aastNode, parent);
            labelNode.LabelAddress = GetCurrentBinarySize(labelNode);
            return labelNode;
        }
    }
}
