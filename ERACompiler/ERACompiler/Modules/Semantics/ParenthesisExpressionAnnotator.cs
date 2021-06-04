using ERACompiler.Structures;

namespace ERACompiler.Modules.Semantics
{
    class ParenthesisExpressionAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            return base.Annotate(astNode.Children[1], parent);
        }
    }
}
