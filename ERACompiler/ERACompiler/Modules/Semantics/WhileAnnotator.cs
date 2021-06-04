using ERACompiler.Structures;

namespace ERACompiler.Modules.Semantics
{
    class WhileAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            AASTNode whileNode = new AASTNode(astNode, parent, SemanticAnalyzer.no_type);
            whileNode.Children.Add(base.Annotate(astNode.Children[1], whileNode));
            whileNode.Children.Add(base.Annotate(astNode.Children[2], whileNode));
            return whileNode;
        }
    }
}
