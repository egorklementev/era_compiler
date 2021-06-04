using ERACompiler.Structures;

namespace ERACompiler.Modules.Generation
{
    class ReferenceConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            Context? ctx = SemanticAnalyzer.FindParentContext(aastNode);
            CodeNode refNode = new CodeNode(aastNode, parent);
            string varName = aastNode.Children[1].Token.Value;
            CodeNode frNode = GetFreeRegisterNode(aastNode, refNode);
            byte fr0 = frNode.ByteToReturn;
            refNode.Children.AddLast(frNode);
            if (ctx.IsVarDynamicArray(varName))
            {
                refNode.Children.AddLast(GetLoadVariableNode(refNode, varName, fr0, ctx));
            }
            else
            {
                refNode.Children.AddLast(GetLoadVariableAddressNode(refNode, varName, fr0, ctx));
            }
            refNode.ByteToReturn = fr0;
            return refNode;
        }
    }
}
