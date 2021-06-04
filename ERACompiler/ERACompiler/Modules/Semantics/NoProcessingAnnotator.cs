using ERACompiler.Structures;

namespace ERACompiler.Modules.Semantics
{
    class NoProcessingAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            return new AASTNode(astNode, parent, SemanticAnalyzer.no_type);
        }
    }
}
