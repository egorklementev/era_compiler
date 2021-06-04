using ERACompiler.Structures;

namespace ERACompiler.Modules.Semantics
{
    class FirstChildAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            return base.Annotate(astNode.Children[0], parent);
        }
    }
}
