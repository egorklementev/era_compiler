using ERACompiler.Structures;
using ERACompiler.Structures.Types;
using ERACompiler.Utilities.Errors;

namespace ERACompiler.Modules.Semantics
{
    class AssemblyStatementAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            astNode = astNode.Children[0];
            AASTNode asmStmnt = new AASTNode(astNode, parent, SemanticAnalyzer.no_type);
            if (astNode.ASTType.Equals("format ( 8 | 16 | 32 )"))                
            {
                int frmt = int.Parse(astNode.Children[1].Token.Value);
                if (!(frmt == 8 || frmt == 16 || frmt == 32))
                {
                    throw new SemanticErrorException(
                        "Incorrect format at assembly block!!!\r\n" +
                        "  At (Line: " + astNode.Children[1].Token.Position.Line.ToString() +
                        ", Char: " + astNode.Children[1].Token.Position.Char.ToString() + ")."
                        );
                }                
            }
            if (astNode.ASTType.Equals("Register := Expression"))
            {
                // Check if expression is constant
                Context ctx = SemanticAnalyzer.FindParentContext(parent);
                if (!SemanticAnalyzer.IsExprConstant(astNode.Children[2], ctx))
                {
                    throw new SemanticErrorException(
                        "This expression should be constant (refer to the documentation)!!!\r\n" +
                        "  At (Line: " + astNode.Children[2].Token.Position.Line.ToString() +
                        ", Char: " + astNode.Children[2].Token.Position.Char.ToString() + ")."
                        );
                }
            }
            else if (astNode.ASTType.Equals("Register := Register + Expression"))
            {
                // Check if expression is constant
                Context ctx = SemanticAnalyzer.FindParentContext(parent);
                if (!SemanticAnalyzer.IsExprConstant(astNode.Children[4], ctx))
                {
                    throw new SemanticErrorException(
                        "This expression should be constant (refer to the documentation)!!!\r\n" +
                        "  At (Line: " + astNode.Children[4].Token.Position.Line.ToString() +
                        ", Char: " + astNode.Children[4].Token.Position.Char.ToString() + ")."
                        );
                }
            }
            else if (astNode.ASTType.Equals("Register := Identifier"))
            {
                // Check if identifier is label
                Context ctx = SemanticAnalyzer.FindParentContext(parent);
                if (ctx.IsVarDeclared(astNode.Children[2].Token) && !ctx.IsVarLabel(astNode.Children[2].Token))
                {
                    if (!ctx.IsVarConstant(astNode.Children[2].Token))
                    {
                        throw new SemanticErrorException(
                            "Label expected!!!\r\n" +
                            "  At (Line: " + astNode.Children[2].Token.Position.Line.ToString() +
                            ", Char: " + astNode.Children[2].Token.Position.Char.ToString() + ")."
                            );
                    }
                }
            }
            else if (astNode.ASTType.Equals("< Identifier >"))
            {
                Context? ctx = SemanticAnalyzer.FindParentContext(parent);
                ctx?.AddVar(new AASTNode(astNode.Children[1], parent, new VarType(VarType.ERAType.LABEL)), astNode.Children[1].Token.Value);
            }

            foreach (ASTNode child in astNode.Children)
            {
                asmStmnt.Children.Add(base.Annotate(child, asmStmnt)); // Just pass everything down
            }
            return asmStmnt;
        }
    }
}
