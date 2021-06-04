using ERACompiler.Structures;

namespace ERACompiler.Modules.Semantics
{
    class LiteralAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            AASTNode literal = new AASTNode(astNode, parent, SemanticAnalyzer.no_type)
            {
                AASTValue = int.Parse(astNode.Children[1].Token.Value) * (astNode.Children[0].Children.Count > 0 ? -1 : 1)
            };
            return literal;
        }
    }
}
