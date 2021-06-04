using ERACompiler.Structures;

namespace ERACompiler.Modules.Semantics
{
    class LoopWhileAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            AASTNode loopWhileNode = new AASTNode(astNode, parent, SemanticAnalyzer.no_type);
            loopWhileNode.Children.Add(base.Annotate(astNode.Children[0], loopWhileNode));
            loopWhileNode.Children.Add(base.Annotate(astNode.Children[2], loopWhileNode));
            return loopWhileNode;
        }
    }
}
