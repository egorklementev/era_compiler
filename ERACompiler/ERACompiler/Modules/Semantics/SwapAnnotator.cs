using ERACompiler.Structures;

namespace ERACompiler.Modules.Semantics
{
    class SwapAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            AASTNode swap = new AASTNode(astNode, parent, SemanticAnalyzer.no_type);
            // TODO: perform type check
            foreach (ASTNode child in astNode.Children)
            {
            swap.Children.Add(base.Annotate(child, swap));
            }
            return swap;
        }
    }
}
