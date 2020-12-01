using ERACompiler.Structures;
using ERACompiler.Structures.Types;
using ERACompiler.Utilities;
using ERACompiler.Utilities.Errors;
using System.Collections.Generic;

namespace ERACompiler.Modules
{
    class SemanticsAnalyzer
    {
        public AASTNode BuildAAST(ASTNode ASTRoot)
        {
            return AnnotateNode(ASTRoot, null);
        }

        private AASTNode AnnotateNode(ASTNode node, AASTNode? parent)
        {
            // Annotate node itself
            AASTNode annotatedNode;

            switch (node.NodeType)
            {
                case ASTNode.ASTNodeType.PROGRAM:
                    {
                        annotatedNode = new AASTNode(node, new VarType(VarType.VarTypeType.NO_TYPE)) { Parent = parent };
                        annotatedNode.Context = new Context("global", null);
                        break;
                    }
                case ASTNode.ASTNodeType.DATA:
                    {
                        annotatedNode = new AASTNode(node, new VarType(VarType.VarTypeType.DATA)) { Parent = parent };
                        string id = node.Children[0].CrspToken.Value;
                        Context prntCtx = FindContext(annotatedNode); // Parent context
                        annotatedNode.Context = new Context("data_" + id, prntCtx);
                        prntCtx.AddVar(annotatedNode, id);
                        break;
                    }
                case ASTNode.ASTNodeType.MODULE:
                    {
                        annotatedNode = new AASTNode(node, new VarType(VarType.VarTypeType.MODULE)) { Parent = parent };
                        string id = node.Children[0].CrspToken.Value;
                        Context prntCtx = FindContext(annotatedNode);
                        annotatedNode.Context = new Context("module_" + id, prntCtx);
                        prntCtx.AddVar(annotatedNode, id);
                        break;
                    }
                case ASTNode.ASTNodeType.STRUCTURE:
                    {
                        string id = node.Children[0].CrspToken.Value;
                        annotatedNode = new AASTNode(node, new VarType(VarType.VarTypeType.STRUCTURE)) { Parent = parent };                        
                        Context prntCtx = FindContext(annotatedNode);
                        annotatedNode.Context = new Context("structure_" + id, prntCtx);                        
                        prntCtx.AddVar(annotatedNode, id);
                        break;
                    }
                case ASTNode.ASTNodeType.ROUTINE:
                    {
                        annotatedNode = new AASTNode(node, new VarType(VarType.VarTypeType.ROUTINE)) { Parent = parent };
                        string id = node.Children[0].NodeType == ASTNode.ASTNodeType.ATTRIBUTE ? node.Children[1].CrspToken.Value : node.Children[0].CrspToken.Value;
                        Context prntCtx = FindContext(annotatedNode);
                        annotatedNode.Context = new Context("routine_" + id, prntCtx);
                        prntCtx.AddVar(annotatedNode, id);
                        break;
                    }
                case ASTNode.ASTNodeType.PARAMETER:
                    {
                        if (node.Children.Count > 1)
                        {
                            string id = node.Children[1].CrspToken.Value;
                            VarType type;
                            switch (node.Children[0].CrspToken.Value)
                            {
                                case "int":
                                    {
                                        type = new VarType(VarType.VarTypeType.INTEGER);
                                        break;
                                    }
                                case "byte":
                                    {
                                        type = new VarType(VarType.VarTypeType.BYTE);
                                        break;
                                    }
                                case "short":
                                    {
                                        type = new VarType(VarType.VarTypeType.SHORT);
                                        break;
                                    }
                                default:
                                    {
                                        type = new StructType(node.Children[0].CrspToken.Value); 
                                        break;
                                    }
                            }
                            annotatedNode = new AASTNode(node, type) { Parent = parent };
                            Context prntCtx = FindContext(annotatedNode);
                            prntCtx.AddVar(annotatedNode, id);
                        }
                        else // Register
                        {
                            annotatedNode = new AASTNode(node, new VarType(VarType.VarTypeType.NO_TYPE)) { Parent = parent };
                        }
                        break;
                    }
                case ASTNode.ASTNodeType.CODE:
                    {
                        annotatedNode = new AASTNode(node, new VarType(VarType.VarTypeType.NO_TYPE)) { Parent = parent };
                        Context prntCtx = FindContext(annotatedNode);
                        annotatedNode.Context = new Context("code", prntCtx);
                        //prntCtx.AddVar(annotatedNode, "code"); ### Is it necessary?..
                        break;
                    }
                case ASTNode.ASTNodeType.VARIABLE_DEFINITION:
                    {
                        VarType type;
                        switch (node.Parent.Children[0].CrspToken.Value) // Goes to the VarDeclaration node
                        {
                            case "int":
                                {
                                    type = new VarType(VarType.VarTypeType.INTEGER);
                                    break;
                                }
                            case "byte":
                                {
                                    type = new VarType(VarType.VarTypeType.BYTE);
                                    break;
                                }
                            case "short":
                                {
                                    type = new VarType(VarType.VarTypeType.SHORT);
                                    break;
                                }
                            default:
                                {
                                    type = new StructType(node.Children[0].CrspToken.Value);
                                    break;
                                }
                        }

                        if (node.Children[0].NodeType != ASTNode.ASTNodeType.ARRAY_DECLARATION)
                        {
                            string id = node.Children[0].CrspToken.Value;
                            annotatedNode = new AASTNode(node, type) { Parent = parent };

                            // Expression (if any)
                            if (node.Children.Count > 1)
                            {
                                annotatedNode.Value = ComputeExpression(node.Children[1], parent);
                            }

                            Context prntCtx = FindContext(annotatedNode);
                            prntCtx.AddVar(annotatedNode, id);                            
                        }
                        else
                        {                            
                            string id = node.Children[0].Children[0].CrspToken.Value; // Goes to the ArrayDeclaration node
                            int size = ComputeExpression(node.Children[0].Children[1], parent);
                            annotatedNode = new AASTNode(node, new ArrayType(type, size)) { Parent = parent };
                            Context prntCtx = FindContext(annotatedNode);
                            prntCtx.AddVar(annotatedNode, id);
                        }
                        break;
                    }
                case ASTNode.ASTNodeType.CONST_DEFINITION:
                    {
                        string id = node.Children[0].CrspToken.Value;
                        annotatedNode = new AASTNode(node, new VarType(VarType.VarTypeType.CONSTANT)) { Parent = parent, Value = ComputeExpression(node.Children[1], parent) };
                        Context prntCtx = FindContext(annotatedNode);
                        prntCtx.AddVar(annotatedNode, id);
                        break;
                    }
                case ASTNode.ASTNodeType.ASSEMBLER_STATEMENT:
                    {
                        if (node.Children.Count > 2 && node.Children[2].NodeType == ASTNode.ASTNodeType.EXPRESSION)
                        {
                            ComputeExpression(node.Children[2], parent);
                        }

                        annotatedNode = new AASTNode(node, new VarType(VarType.VarTypeType.NO_TYPE)) { Parent = parent };
                        break;
                    }
                default:
                    {
                        annotatedNode = new AASTNode(node, new VarType(VarType.VarTypeType.NO_TYPE)) { Parent = parent };
                        break;
                    }
            }

            // Annotate all children
            List<AASTNode> children = new List<AASTNode>();            
            foreach (ASTNode child in node.Children)
            {
                children.Add(AnnotateNode(child, annotatedNode)); // Recursive call
            }
            annotatedNode.Children = children;

            return annotatedNode;
        }

        /// <summary>
        /// Returns a context that the given node belongs to.
        /// </summary>
        /// <param name="node">Node for which we want to find its context.</param>
        /// <returns>Reference to the context.</returns>
        private Context FindContext(AASTNode node)
        {
            while (true)
            {
                AASTNode? parent = node.Parent;
                if (parent == null)
                {
                    return null; // No context found. Probably something is wrong.
                }
                else
                {
                    if (!parent.Context.Name.Equals("none"))
                    {
                        return parent.Context;
                    }
                    else
                    {
                        return FindContext(parent);
                    }
                }
            }
        }
    
        private int ComputeExpression(ASTNode expression, AASTNode parent)
        {
            int result;

            // Special case for one-element expressions
            if (expression.Children.Count == 1)
            {
                return GetOperand(expression.Children[0], parent);
            }

            // Multiplication and binary operations have the highest priority
            for (int i = 0; i < expression.Children.Count; i++)
            {
                if (expression.Children[i].NodeType == ASTNode.ASTNodeType.OPERATOR)
                {
                    if (expression.Children[i].CrspToken.Value.Equals("*") ||
                        expression.Children[i].CrspToken.Value.Equals("&") ||
                        expression.Children[i].CrspToken.Value.Equals("|") ||
                        expression.Children[i].CrspToken.Value.Equals("^") ||
                        expression.Children[i].CrspToken.Value.Equals("?"))
                    {
                        int op1 = GetOperand(expression.Children[i - 1], parent); // Get first operand value
                        int op2 = GetOperand(expression.Children[i + 1], parent); // Get second operand value
                        int interRes = 0; // Intermediate result

                        switch (expression.Children[i].CrspToken.Value)
                        {
                            case "*":
                                {
                                    interRes = op1 * op2;
                                    break;
                                }
                            case "&":
                                {
                                    interRes = op1 & op2;
                                    break;
                                }
                            case "|":
                                {
                                    interRes = op1 | op2;
                                    break;
                                }
                            case "^":
                                {
                                    interRes = op1 ^ op2;
                                    break;
                                }
                            case "?":
                                {
                                    interRes = op1 > op2 ? 1 : op1 < op2 ? 2 : 4;
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }

                        expression.Children.RemoveRange(i - 1, 3);
                        expression.Children.Insert(
                            i - 1,
                            new ASTNode(expression, new List<ASTNode>(), new Token(TokenType.NUMBER, interRes.ToString(), new TokenPosition(-1, -1)), ASTNode.ASTNodeType.LITERAL)
                            );
                        i = 0; // Start again
                    }
                }
            }

            for (int i = 0; i < expression.Children.Count; i++)
            {
                if (expression.Children[i].NodeType == ASTNode.ASTNodeType.OPERATOR)
                {
                    if (expression.Children[i].CrspToken.Value.Equals("+") ||
                        expression.Children[i].CrspToken.Value.Equals("-") ||
                        expression.Children[i].CrspToken.Value.Equals("=") ||
                        expression.Children[i].CrspToken.Value.Equals("/=") ||
                        expression.Children[i].CrspToken.Value.Equals("<") ||
                        expression.Children[i].CrspToken.Value.Equals(">"))
                    {
                        int op1 = GetOperand(expression.Children[i - 1], parent); // Get first operand value
                        int op2 = GetOperand(expression.Children[i + 1], parent); // Get second operand value
                        int interRes = 0; // Intermediate result

                        switch (expression.Children[i].CrspToken.Value)
                        {
                            case "+":
                                {
                                    interRes = op1 + op2;
                                    break;
                                }
                            case "-":
                                {
                                    interRes = op1 - op2;
                                    break;
                                }
                            case "=":
                                {
                                    interRes = op1 == op2 ? 1 : 0;
                                    break;
                                }
                            case "/=":
                                {
                                    interRes = op1 != op2 ? 1 : 0;
                                    break;
                                }
                            case "<":
                                {
                                    interRes = op1 < op2 ? 1 : 0;
                                    break;
                                }
                            case ">":
                                {
                                    interRes = op1 > op2 ? 1 : 0;
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }

                        expression.Children.RemoveRange(i - 1, 3);
                        expression.Children.Insert(
                            i - 1,
                            new ASTNode(expression, new List<ASTNode>(), new Token(TokenType.NUMBER, interRes.ToString(), new TokenPosition(-1, -1)), ASTNode.ASTNodeType.LITERAL)
                            );
                        i = 0; // Start again
                    }
                }
            }

            if (expression.Children.Count == 1)
            {
                result = int.Parse(expression.Children[0].CrspToken.Value);
            }
            else
            {
                Logger.LogError(new SemanticsError(
                    "Incorrect expression at (" + expression.Parent.CrspToken.Position.Line + ", " + expression.Parent.CrspToken.Position.Char + ")!!!"
                    ));
                return -1;
            }

            return result;
        }

        private int GetOperand(ASTNode op, AASTNode parent)
        {
            // Special case for intermediate calculations
            if (op.NodeType == ASTNode.ASTNodeType.LITERAL)
            {
                return int.Parse(op.CrspToken.Value);
            }

            if (op.Children[0].NodeType == ASTNode.ASTNodeType.RECEIVER)
            {
                if (op.Children[0].Children[0].NodeType == ASTNode.ASTNodeType.IDENTIFIER)
                {
                    return FindContext(parent).GetVarValue(op.Children[0].Children[0]);
                }
            }

            if (op.Children[0].NodeType == ASTNode.ASTNodeType.LITERAL)
            {
                return int.Parse(op.Children[0].CrspToken.Value);
            }

            // ### TODO : Reference

            return 0;
        }
    }
}
