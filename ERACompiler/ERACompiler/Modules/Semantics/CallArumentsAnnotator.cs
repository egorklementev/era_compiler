using ERACompiler.Structures;

namespace ERACompiler.Modules.Semantics
{
    class CallArumentsAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            AASTNode callArgs = new AASTNode(astNode, parent, SemanticAnalyzer.no_type);
            if (astNode.Children[1].Children.Count > 0) // If some arguments exist
            {
                // First expression
                callArgs.Children.Add(base.Annotate(astNode.Children[1].Children[0], callArgs));
                // The rest of expressions if any
                if (astNode.Children[1].Children[1].Children.Count > 0)
                {
                    foreach (ASTNode child in astNode.Children[1].Children[1].Children)
                    {
                        if (child.ASTType.Equals("Expression")) // Skip comma
                        {
                            callArgs.Children.Add(base.Annotate(child, callArgs));
                        }
                    }
                }
            }
            return callArgs;
        }
    }
}
