using ERACompiler.Structures;
using ERACompiler.Structures.Types;
using System.Collections.Generic;

namespace ERACompiler.Modules
{
    class SemanticAnalyzer
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
                        annotatedNode = new AASTNode(node, new VarType(VarType.VarTypeType.NO_TYPE), new Context("global", null)) { Parent = parent };
                        break;
                    }
                case ASTNode.ASTNodeType.DATA:
                    {
                        annotatedNode = new AASTNode(node, new VarType(VarType.VarTypeType.DATA)) { Parent = parent };
                        FindContext(annotatedNode).AddVar(annotatedNode);
                        break;
                    }
                case ASTNode.ASTNodeType.MODULE:
                    {
                        string id = node.Children[0].CrspToken.Value;
                        annotatedNode = new AASTNode(node, new VarType(VarType.VarTypeType.MODULE), new Context("module_" + id, null)) { Parent = parent };
                        FindContext(annotatedNode).AddVar(annotatedNode);
                        break;
                    }
                case ASTNode.ASTNodeType.STRUCTURE:
                    {
                        StructType type = new StructType();

                        string id = node.Children[0].CrspToken.Value;
                        annotatedNode = new AASTNode(node, type, new Context("structure_" + id, null)) { Parent = parent };
                        
                        // TODO: add children to the 'type'
                        
                        FindContext(annotatedNode).AddVar(annotatedNode);
                        break;
                    }
                case ASTNode.ASTNodeType.ROUTINE:
                    {
                        string id = node.Children[0].NodeType == ASTNode.ASTNodeType.ATTRIBUTE ? node.Children[1].CrspToken.Value : node.Children[0].CrspToken.Value;
                        annotatedNode = new AASTNode(node, new VarType(VarType.VarTypeType.ROUTINE), new Context("routine_" + id, null)) { Parent = parent };
                        FindContext(annotatedNode).AddVar(annotatedNode);
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
                    if (parent.HasContext)
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
    }
}
