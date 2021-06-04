using ERACompiler.Structures;

namespace ERACompiler.Modules.Semantics
{
    class CallAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            AASTNode call = new AASTNode(astNode, parent, SemanticAnalyzer.no_type);
            call.Children.Add(base.Annotate(astNode.Children[0], call));
            call.Children.Add(base.Annotate(astNode.Children[1], call));
            return call;
        }
    }
}
