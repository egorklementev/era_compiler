using ERACompiler.Structures;

namespace ERACompiler.Modules.Semantics
{
    class DefaultAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            return new AASTNode(astNode, null, SemanticAnalyzer.no_type);
        }
    }
}
