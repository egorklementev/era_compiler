using ERACompiler.Structures;
using ERACompiler.Structures.Types;
using System.Collections.Generic;

namespace ERACompiler.Modules
{
    class SemanticAnalyzer
    {
        public AASTNode? BuildAAST(ASTNode ASTRoot)
        {
            return AnnotateNode(ASTRoot, null);
        }

        private AASTNode? AnnotateNode(ASTNode node, AASTNode? parent)
        {
            // Annotate node itself
            AASTNode? annotatedNode = null;

            switch (node.NodeType)
            {
                case ASTNode.ASTNodeType.PROGRAM:
                    {
                        annotatedNode = new AASTNode(node, new VarType(VarType.VarTypeType.NO_TYPE), new Context("global", null)) { Parent = parent };
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            // Annotate all children
            List<AASTNode?> children = new List<AASTNode?>();            
            foreach (ASTNode child in node.Children)
            {
                children.Add(AnnotateNode(child, annotatedNode));
            }

            return annotatedNode;
        }

        /// <summary>
        /// Returns a context that the given node belongs to.
        /// </summary>
        /// <param name="node">Node for which we want to find its context.</param>
        /// <returns>Reference to the context.</returns>
        private Context? FindContext(AASTNode node)
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
