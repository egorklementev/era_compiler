using ERACompiler.Structures;

namespace ERACompiler.Modules.Semantics
{
    class AllChildrenAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            AASTNode nodeToPass = new AASTNode(astNode, parent, SemanticAnalyzer.no_type);
            foreach (ASTNode child in astNode.Children)
            {
                nodeToPass.Children.Add(base.Annotate(child, nodeToPass));
            }
            return nodeToPass;
        }
    }
}
