using ERACompiler.Structures;
using ERACompiler.Structures.Types;
using ERACompiler.Utilities.Errors;
using System.Collections.Generic;

namespace ERACompiler.Modules.Semantics
{
    class PrimaryAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            AASTNode somePrim = new AASTNode(astNode, parent, SemanticAnalyzer.no_type);
            Context? ctx = SemanticAnalyzer.FindParentContext(parent) 
                ?? throw new SemanticErrorException("No parent context found!!!\r\n  At line " + astNode.Token.Position.Line);

            // Identifier
            somePrim.Children.Add(base.Annotate(astNode.Children[0], somePrim));
            ASTNode idLink = astNode.Children[0];

            // If constant, convert to number
            if (ctx.IsVarDeclared(idLink.Token) && ctx.IsVarConstant(idLink.Token))
            {
                int constValue = ctx.GetConstValue(idLink.Token);
                ASTNode number = new ASTNode(parent, new List<ASTNode>(), idLink.Token, "NUMBER");
                ASTNode opMinus = new ASTNode(number, new List<ASTNode>(), idLink.Token, "[ - ]");
                if (constValue < 0)
                {
                    opMinus.Children.Add(new ASTNode(opMinus, new List<ASTNode>(), idLink.Token, "OPERATOR"));
                    constValue *= -1;
                }
                number.Children.Add(opMinus);
                ASTNode literal = new ASTNode(number, new List<ASTNode>(), new Token(TokenType.NUMBER, constValue.ToString(), idLink.Token.Position), "SOME_LITERAL");
                number.Children.Add(literal);
                return base.Annotate(number, parent);
            }
            else
            {
                // { '.' Identifier }
                if (astNode.Children[1].Children.Count > 0)
                {
                    foreach (ASTNode child in astNode.Children[1].Children)
                    {
                        if (!ctx.IsVarStruct(idLink.Token))
                        {
                            throw new SemanticErrorException(
                                "Trying to access non-struct variable via \'.\' notation!!!\r\n" +
                                "\tAt (Line: " + idLink.Token.Position.Line.ToString() +
                                ", Char: " + idLink.Token.Position.Char.ToString() + ")."
                                );
                        }
                        if (child.ASTType.Equals("IDENTIFIER"))
                            idLink = child;
                        somePrim.Children.Add(base.Annotate(child, somePrim));
                    }
                }

                // [ ArrayAccess | CallArgs ]
                if (astNode.Children[2].Children.Count > 0)
                {
                    if (astNode.Children[2].Children[0].Children[0].ASTType.Equals("Call arguments"))
                    {
                        somePrim.Children.Add(base.Annotate(astNode.Children[2].Children[0].Children[0], somePrim));
                    }
                    else
                    {
                        if (!ctx.IsVarArray(idLink.Token) && !ctx.IsVarData(idLink.Token.Value))
                        {
                            throw new SemanticErrorException(
                                "Trying to access non-array variable via \'[]\' notation!!!\r\n" +
                                "\tAt (Line: " + idLink.Token.Position.Line.ToString() +
                                ", Char: " + idLink.Token.Position.Char.ToString() + ")."
                                );
                        }
                        // If expression is constant we can check for array boundaries
                        if (SemanticAnalyzer.IsExprConstant(astNode.Children[2].Children[0].Children[0].Children[1], ctx))
                        {
                            int index = SemanticAnalyzer.CalculateConstExpr(astNode.Children[2].Children[0].Children[0].Children[1], ctx);
                            int arrSize = ctx.GetArrSize(idLink.Token);
                            if (index < 0)
                                throw new SemanticErrorException(
                                "Negative array index!!!\r\n" +
                                "\tAt (Line: " + idLink.Token.Position.Line.ToString() +
                                    ", Char: " + idLink.Token.Position.Char.ToString() + ")."
                                );
                            // If we know the size of the array already (arrSize != 0 indicates this)
                            if (arrSize != 0 && index >= arrSize)
                                throw new SemanticErrorException(
                                "Accessing element with index higher than array the size!!!\r\n" +
                                "\tAt (Line: " + idLink.Token.Position.Line.ToString() +
                                    ", Char: " + idLink.Token.Position.Char.ToString() + ")."
                                );
                        }
                        somePrim.Children.Add(base.Annotate(astNode.Children[2].Children[0].Children[0].Children[1], somePrim));
                    }
                }
            }

            return somePrim;
        }
    }
}
