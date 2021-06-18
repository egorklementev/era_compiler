using ERACompiler.Structures;

namespace ERACompiler.Modules.Semantics
{
    class AssemblyBlockAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            AASTNode asmBlock = new AASTNode(astNode, parent, SemanticAnalyzer.no_type);
            asmBlock.Children.Add(base.Annotate(astNode.Children[1], asmBlock)); // First child
            foreach (ASTNode child in astNode.Children[3].Children)
            {
                // Something wrong here... UPD: nothing wrong
                if (child.ASTType.Equals("Assembly statement"))
                {
                    asmBlock.Children.Add(base.Annotate(child, asmBlock));
                }
            }
            return asmBlock;
        }
    }
}
