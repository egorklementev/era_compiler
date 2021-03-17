using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace ERACompiler.Structures
{
    /// <summary>
    /// The class representing Abstract Syntax Tree node
    /// </summary>
    public class ASTNode
    {
        /// <summary>
        /// Parent node. Only the root node has parent equals to null.
        /// </summary>
        public ASTNode? Parent { get; set; }
        /// <summary>
        /// Child nodes of the node.
        /// </summary>
        public List<ASTNode> Children { get; }
        /// <summary>
        /// The token of the source code that corresponds to this node.
        /// </summary>
        public Token Token { get; }
        /// <summary>
        /// Represents the type of an AST entry.
        /// </summary>
        public string ASTType { get; }

        /// <summary>
        /// How deep in the AST the node is located. Used for proper tabulation in the ToString() method.
        /// </summary>
        protected int level = 0;

        /// <summary>
        /// Creates an AST node.
        /// </summary>
        /// <param name="parent">Ref to the parent AST node.</param>
        /// <param name="children">Ref to the list of AST children nodes.</param>
        /// <param name="token">Corresponding token from the lexical analyzer.</param>
        /// <param name="type">The type of the AST node (basically syntax rule).</param>
        public ASTNode(ASTNode? parent, List<ASTNode> children, Token token, string type)
        {
            Parent = parent;
            Children = children;
            Token = token;
            ASTType = type;

            if (parent != null) 
                level = parent.level + 2;                      
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>A string in JSON format of the node and its children (works recursively).</returns>
        public override string ToString()
        {
            // Json format
            StringBuilder sb = new StringBuilder();

            string tabs_lvl = string.Concat(Enumerable.Repeat("\t", level));
            string tabs_lvl1 = tabs_lvl + "\t";

            sb.Append(tabs_lvl)
                .Append("{\r\n");
            sb.Append(tabs_lvl1)
                .Append("\"node_type\": ").Append("\"" + ASTType + "\"").Append(",\r\n");            
            sb.Append(tabs_lvl1)
                .Append("\"token\": ").Append("\"" + Token.Value + "\"").Append(",\r\n");            
            sb.Append(tabs_lvl1)
                .Append("\"children\": [");            

            if (Children.Count > 0)
            {
                foreach(ASTNode child in Children)
                {
                    sb.Append("\r\n").Append(child.ToString()).Append(',');
                }
                sb.Remove(sb.Length - 1, 1); // Remove last ','
                sb.Append("\r\n");
                sb.Append(tabs_lvl1)
                    .Append("]\r\n");
            }
            else
            {
                sb.Append("]\r\n");
            }

            sb.Append(tabs_lvl)
                .Append('}');

            return sb.ToString();
        }

    }
}
