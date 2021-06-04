using ERACompiler.Structures;
using ERACompiler.Structures.Types;
using ERACompiler.Utilities.Errors;
using System.Collections.Generic;

namespace ERACompiler.Modules.Semantics
{
    class VariableDeclarationAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            VarType type = SemanticAnalyzer.IdentifyType(astNode.Children[0], astNode.Children[1].Children[0].ASTType.Equals("Constant"));
            if (astNode.Children[1].Children[0].ASTType.Equals("Array")) 
                type = new ArrayType(type);            
            AASTNode varDecl = new AASTNode(astNode, parent, SemanticAnalyzer.no_type);
            varDecl.Children.AddRange(IdentifyVarDecl(astNode.Children[1].Children[0], varDecl, type));
            return varDecl;
        }
        
        private List<AASTNode> IdentifyVarDecl(ASTNode node, AASTNode parent, VarType type)
        {
            List<AASTNode> lst = new List<AASTNode>();
            Context? ctx = SemanticAnalyzer.FindParentContext(parent);

            switch (node.ASTType)
            {
                case "Variable":
                    {
                        // VarDefinition { , VarDefinition } ;
                        AASTNode firstDef = new AASTNode(node.Children[0], parent, type);
                        lst.Add(firstDef);
                        ctx.AddVar(firstDef, node.Children[0].Children[0].Token.Value); // VarDef's identifier
                                                                                        // Check expr if exists
                        if (node.Children[0].Children[1].Children.Count > 0)
                        {
                            AASTNode firstExpr = base.Annotate(node.Children[0].Children[1].Children[0].Children[1], firstDef);
                            firstDef.Children.Add(firstExpr);
                        }
                        // Repeat for { , VarDefinition }
                        foreach (ASTNode varDef in node.Children[1].Children)
                        {
                            if (varDef.ASTType.Equals("Variable definition")) // Skip comma rule
                            {
                                AASTNode def = new AASTNode(varDef, parent, type);
                                lst.Add(def);
                                ctx.AddVar(def, varDef.Children[0].Token.Value); // VarDef's identifier
                                if (varDef.Children[1].Children.Count > 0)
                                {
                                    AASTNode expr = base.Annotate(varDef.Children[1].Children[0].Children[1], def);
                                    def.Children.Add(expr);
                                }
                            }
                        }
                        break;
                    }

                case "Constant":
                    {
                        // 'const' ConstDefinition { , ConstDefinition } ;                        
                        AASTNode firstDef = new AASTNode(node.Children[1], parent, type);
                        lst.Add(firstDef);
                        ctx.AddVar(firstDef, node.Children[1].Children[0].Token.Value); // ConstDef's identifier

                        if (!SemanticAnalyzer.IsExprConstant(node.Children[1].Children[2], ctx))
                        {
                            throw new SemanticErrorException(
                                "Expression for a constant definition is not constant!!!\r\n" +
                                "\t At (Line: " + node.Children[1].Children[2].Token.Position.Line.ToString() +
                                ", Char: " + node.Children[1].Children[2].Token.Position.Char.ToString() + ")."
                                );
                        }
                        firstDef.AASTValue = SemanticAnalyzer.CalculateConstExpr(node.Children[1].Children[2], ctx);
                        // Repeat for { , ConstDefinition }
                        foreach (ASTNode varDef in node.Children[2].Children)
                        {
                            if (varDef.ASTType.Equals("Constant definition")) // Skip comma rule
                            {
                                AASTNode def = new AASTNode(varDef, parent, type);
                                lst.Add(def);
                                ctx.AddVar(def, varDef.Children[0].Token.Value); // ConstDef's identifier

                                if (!SemanticAnalyzer.IsExprConstant(varDef.Children[2], ctx))
                                {
                                    throw new SemanticErrorException(
                                        "Expression for a constant definition is not constant!!!\r\n" +
                                        "\t At (Line: " + varDef.Children[2].Token.Position.Line.ToString() +
                                        ", Char: " + varDef.Children[2].Token.Position.Char.ToString() + ")."
                                        );
                                }
                                def.AASTValue = SemanticAnalyzer.CalculateConstExpr(varDef.Children[2], ctx);
                            }
                        }
                        break;
                    }

                case "Array": 
                    {
                        // '[' ']' ArrDefinition { , ArrDefinition } ;
                        AASTNode firstDef = new AASTNode(node.Children[2], parent, type);
                        lst.Add(firstDef);
                        ctx.AddVar(firstDef, node.Children[2].Children[0].Token.Value); // ArrDef's identifier

                        //CheckVariablesForExistance(node.Children[2].Children[2], ctx); // Expression of ArrDefinition
                        if (SemanticAnalyzer.IsExprConstant(node.Children[2].Children[2], ctx))
                        {
                            int arrSize = SemanticAnalyzer.CalculateConstExpr(node.Children[2].Children[2], ctx);
                            if (arrSize <= 0) 
                                throw new SemanticErrorException(
                                "Incorrect array size!!!\r\n" +
                                "\t At (Line: " + node.Children[2].Children[2].Token.Position.Line.ToString() +
                                    ", Char: " + node.Children[2].Children[2].Token.Position.Char.ToString() + ")."
                                );
                            ((ArrayType)type).Size = arrSize;
                        }
                        else
                        {
                            // If size is not constant, just pass the expression
                            firstDef.Children.Add(base.Annotate(node.Children[2].Children[2], firstDef));
                        }
                        // Repeat for { , ArrDefinition }
                        foreach (ASTNode arrDef in node.Children[3].Children)
                        {
                            ArrayType arrType = new ArrayType(((ArrayType)type).ElementType); // Each array can have it's own size
                            if (arrDef.ASTType.Equals("Array definition")) // Skip comma rule
                            {
                                AASTNode def = new AASTNode(arrDef, parent, arrType);
                                lst.Add(def);
                                ctx.AddVar(def, arrDef.Children[0].Token.Value); // ArrDef's identifier

                                //CheckVariablesForExistance(arrDef.Children[2], ctx); // Expression of ArrDefinition
                                if (SemanticAnalyzer.IsExprConstant(arrDef.Children[2], ctx))
                                {
                                    int _arrSize = SemanticAnalyzer.CalculateConstExpr(arrDef.Children[2], ctx);
                                    if (_arrSize <= 0) 
                                        throw new SemanticErrorException(
                                        "Incorrect array size!!!\r\n" +
                                        "\t At (Line: " + arrDef.Children[2].Token.Position.Line.ToString() +
                                            ", Char: " + arrDef.Children[2].Token.Position.Char.ToString() + ")."
                                        );
                                    arrType.Size = _arrSize;
                                }
                                else
                                {
                                    // If size is not constant, just pass the expression
                                    def.Children.Add(base.Annotate(arrDef.Children[2], def));
                                }
                            }
                        }
                        break;
                    }
                default:
                    break;
            }

            return lst;
        }
    }
}
