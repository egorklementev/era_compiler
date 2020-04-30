using System.Collections.Generic;
using ERACompiler.Structures.Types;

namespace ERACompiler.Structures
{
    public class AASTNode : ASTNode
    {        
        public VarType Type { get; set; }        

        private AASTNode(ASTNode parent, List<ASTNode> children, Token token, ASTNodeType type) : base(parent, children, token, type) {}

        public AASTNode(ASTNode node, VarType type) : this(node.Parent, node.Children, node.CrspToken, node.NodeType)
        {
            Type = type;
        }

    }
}
