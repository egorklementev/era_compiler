using ERACompiler.Structures;

namespace ERACompiler.Modules.Semantics
{
    class GotoAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            AASTNode gotoNode = new AASTNode(astNode, parent, SemanticAnalyzer.no_type);
            gotoNode.Children.Add(base.Annotate(astNode.Children[1], gotoNode));
            return gotoNode;
        }
    }
}
