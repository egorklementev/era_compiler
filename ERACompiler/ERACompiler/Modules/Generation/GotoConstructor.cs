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
            // Final(e) CodeNode is added later at the end of the Program constructor
            return gotoNode;
        }
    }
}
