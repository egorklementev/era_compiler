using ERACompiler.Structures;
using ERACompiler.Structures.Types;
using ERACompiler.Utilities.Errors;

namespace ERACompiler.Modules.Semantics
{
    class ReturnAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            AASTNode returnNode = new AASTNode(astNode, parent, SemanticAnalyzer.no_type);
            // Check if "return" is in right place - inside the routine body
            ASTNode? parentCopy = returnNode.Parent;
            while (parentCopy != null)
            {
                if (parentCopy.ASTType.Equals("Routine body"))
                {
                    // Perform the rest of checks
                    if (astNode.Children[1].Children[0].Children.Count > 0) // If it has something to return
                    {
                        if (astNode.Children[1].Children[0].Children[0].Children[0].ASTType.Equals("Call"))
                        {
                            returnNode.Children.Add(base.Annotate(astNode.Children[1].Children[0].Children[0], returnNode));
                        }
                        else
                        {
                            returnNode.Children.Add(base.Annotate(astNode.Children[1].Children[0].Children[0].Children[0], returnNode));
                        }
                    }
                    return returnNode;
                }
                parentCopy = parentCopy.Parent;
            }
            throw new SemanticErrorException(
                "Return is not in place!!!\r\n" +
                "  At (Line: " + astNode.Children[0].Token.Position.Line.ToString() + ", " +
                "Char: " + astNode.Children[0].Token.Position.Char.ToString() + ")."
                );
        }
    }
}
