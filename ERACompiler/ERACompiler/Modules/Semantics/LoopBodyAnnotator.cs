using ERACompiler.Structures;

namespace ERACompiler.Modules.Semantics
{
    class LoopBodyAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            AASTNode loop = new AASTNode(astNode, parent, SemanticAnalyzer.no_type);
            loop.Children.Add(base.Annotate(astNode.Children[1], loop));
            return loop;
        }
    }
}
