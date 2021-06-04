using ERACompiler.Structures;
using ERACompiler.Utilities.Errors;

namespace ERACompiler.Modules.Semantics
{
    class BreakAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            AASTNode breakNode = new AASTNode(astNode, parent, SemanticAnalyzer.no_type);
            // Check if "break" is in the right place - inside the loop body
            ASTNode? parentCopy = breakNode.Parent;
            while (parentCopy != null)
            {
                if (parentCopy.ASTType.Equals("Loop body")) return breakNode;
                parentCopy = parentCopy.Parent;
            }
            throw new SemanticErrorException(
                "Break is not in place!!!\r\n" +
                "  At (Line: " + astNode.Children[0].Token.Position.Line.ToString() + ", " +
                "Char: " + astNode.Children[0].Token.Position.Char.ToString() + ")."
                );
        }
    }
}
