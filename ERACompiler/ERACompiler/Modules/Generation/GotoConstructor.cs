using ERACompiler.Structures;

namespace ERACompiler.Modules.Generation
{
    class GotoConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            CodeNode gotoNode = new CodeNode(aastNode, parent);
            // We have to deallocate everything since we do not know where are we jumping
            gotoNode.Children.AddLast(GetRegisterDeallocationNode(aastNode, gotoNode, false)); 
            gotoNode.Children.AddLast(new CodeNode("goto jump", gotoNode).Add(new byte[10])); 
            // Final(e) CodeNode is added later at the end of the current context
            return gotoNode;
        }
    }
}
