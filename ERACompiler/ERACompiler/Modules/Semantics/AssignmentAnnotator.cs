using ERACompiler.Structures;
using ERACompiler.Utilities.Errors;

namespace ERACompiler.Modules.Semantics
{
    class AssignmentAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            AASTNode asgnmt = new AASTNode(astNode, parent, SemanticAnalyzer.no_type);
            // TODO: check for type accordance (debatable)
            // TODO: dot-notation here
            if (astNode.Children[0].Children[0].ASTType.Equals("Primary") && 
                SemanticAnalyzer.FindParentContext(asgnmt).IsVarConstant(astNode.Children[0].Children[0].Token))
            {
                Token id = astNode.Children[0].Children[0].Token;
                throw new SemanticErrorException("Attempt to modify a constant!!!\n" +
                    "  At(Line: " + id.Position.Line + ", Char: " + id.Position.Char + ").");
            }
            asgnmt.Children.Add(base.Annotate(astNode.Children[0], asgnmt)); // Receiver
            asgnmt.Children.Add(base.Annotate(astNode.Children[2], asgnmt)); // Expression
            return asgnmt;
        }
    }
}
