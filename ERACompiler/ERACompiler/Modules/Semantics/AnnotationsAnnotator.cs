using ERACompiler.Structures;

namespace ERACompiler.Modules.Semantics
{
    class AnnotationsAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            AASTNode anns = new AASTNode(astNode, parent, SemanticAnalyzer.no_type);
            // Annotate the first child
            anns.Children.Add(base.Annotate(astNode.Children[1], anns));
            // Repeat for the rest
            foreach (ASTNode child in astNode.Children[2].Children) 
            {
                anns.Children.Add(base.Annotate(child, anns));
            }
            return anns;
        }
    }
}
