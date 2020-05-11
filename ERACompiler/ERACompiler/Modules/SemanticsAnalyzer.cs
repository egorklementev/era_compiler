using ERACompiler.Structures;
using ERACompiler.Structures.Types;
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
                            Context prntCtx = FindContext(annotatedNode);
                            prntCtx.AddVar(annotatedNode, id);
                            
                            // ### Expression ? ###
                        
                        }
                        else
                        {                            
                            string id = node.Children[0].Children[0].CrspToken.Value; // Goes to the ArrayDeclaration node
                            int size = ComputeExpression(node.Children[0].Children[1]);
                            annotatedNode = new AASTNode(node, new ArrayType(type, size)) { Parent = parent };
                            Context prntCtx = FindContext(annotatedNode);
                            prntCtx.AddVar(annotatedNode, id);
                        }
                        break;
                    }
                case ASTNode.ASTNodeType.CONST_DEFINITION:
                    {
                        string id = node.Children[0].CrspToken.Value;
                        annotatedNode = new AASTNode(node, new VarType(VarType.VarTypeType.CONSTANT)) { Parent = parent };
                        Context prntCtx = FindContext(annotatedNode);
                        prntCtx.AddVar(annotatedNode, id);
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
    
        private int ComputeExpression(ASTNode expression)
        {
            return 0;
        }
    }
}
