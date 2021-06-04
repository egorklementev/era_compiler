using ERACompiler.Structures;

namespace ERACompiler.Modules.Semantics
{
    class RoutineBodyAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            AASTNode body = new AASTNode(astNode, parent, SemanticAnalyzer.no_type);
            foreach(ASTNode child in astNode.Children[1].Children)
            {
                body.Children.Add(base.Annotate(child, body));
            }
            return body;
        }
    }
}
