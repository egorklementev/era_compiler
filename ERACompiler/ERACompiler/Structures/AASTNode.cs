using ERACompiler.Structures.Types;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERACompiler.Structures
{
    /// <summary>
    /// Annotated AST node. It contains additional information about node's type and context.
    /// </summary>
    public class AASTNode : ASTNode
    {

        /// <summary>
        /// integer, array of 30 bytes, structure A, etc.
        /// </summary>
        public VarType AASTType { get; set; }

        /// <summary>
        /// Used to store values of constant variables. Used for compile-time 
        /// constant expression calculations and for storing initial values.
        /// </summary>
        public int AASTValue { get; set; } = 0;

        /// <summary>
        /// The context that this node owns.
        /// </summary>
        public Context? Context { get; set; } = null;

        public AASTNode(ASTNode node, AASTNode? parent, VarType type) : base(parent, new List<ASTNode>(), new Token(node.Token), node.ASTType)
        {
            AASTType = type;
        }

        public override string ToString()
        {
            // Json format

            StringBuilder sb = new StringBuilder();

            sb.Append(string.Concat(Enumerable.Repeat("\t", level)))
                .Append("{\r\n");
            sb.Append(string.Concat(Enumerable.Repeat("\t", level + 1)))
                .Append("\"node_type\": ").Append("\"" + ASTType.ToString() + "\"").Append(",\r\n");
            sb.Append(string.Concat(Enumerable.Repeat("\t", level + 1)))
                .Append("\"type\": ").Append("\"" + AASTType.ToString() + "\"").Append(",\r\n");
            sb.Append(string.Concat(Enumerable.Repeat("\t", level + 1)))
                .Append("\"value\": ").Append("\"" + AASTValue.ToString() + "\"").Append(",\r\n");

            if (Context != null)            
            {
                Context.Level = level + 1; // To make the output correct
                sb.Append(string.Concat(Enumerable.Repeat("\t", level + 1)))
                    .Append("\"context\": ");
                sb.Append("\r\n").Append(Context.ToString()).Append(",\r\n");             
            }
            
            sb.Append(string.Concat(Enumerable.Repeat("\t", level + 1)))
                .Append("\"token\": ").Append("\"" + Token.Value + "\"").Append(",\r\n");
            sb.Append(string.Concat(Enumerable.Repeat("\t", level + 1)))
                .Append("\"children\": [");

            if (Children.Count > 0)
            {
                foreach (ASTNode child in Children)
                {
                    sb.Append("\r\n").Append(child?.ToString()).Append(',');
                }
                sb.Remove(sb.Length - 1, 1); // Remove last ','
                sb.Append("\r\n");
                sb.Append(string.Concat(Enumerable.Repeat("\t", level + 1)))
                    .Append("]\r\n");
            }
            else
            {
                sb.Append("]\r\n");
            }

            sb.Append(string.Concat(Enumerable.Repeat("\t", level)))
                .Append('}');

            return sb.ToString();
        }
    }
}
