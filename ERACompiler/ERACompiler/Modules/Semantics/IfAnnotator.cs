using ERACompiler.Structures;

namespace ERACompiler.Modules.Semantics
{
    class IfAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            AASTNode ifNode = new AASTNode(astNode, parent, SemanticAnalyzer.no_type);
            ifNode.Children.Add(base.Annotate(astNode.Children[1], ifNode));
            ifNode.Children.Add(base.Annotate(astNode.Children[3], ifNode)); // If true
            // Annotate else block if any
            if (astNode.Children[4].Children[0].Children.Count > 0)
            {
                ifNode.Children.Add(base.Annotate(astNode.Children[4].Children[0].Children[1], ifNode));
            }
            return ifNode;
        }
    }
}
