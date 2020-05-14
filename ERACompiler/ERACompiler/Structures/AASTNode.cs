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
        private AASTNode? parent;
        private int value;

        /// <summary>
        /// Represents local value of the variable (if the node represents value reference, for example)
        /// </summary>
        public int Value { get => value; set => this.value = value; }

        /// <summary>
        /// Parent node of the node.
        /// </summary>
        public new AASTNode? Parent 
        { 
            get => parent;
            set
            {
                parent = value;
                if (value != null)
                    level = value.level + 2;
            } 
        }

        /// <summary>
        /// Children nodes of this AAST node.
        /// </summary>
        public new List<AASTNode> Children { get; set; }

        /// <summary>
        /// integer, array of 30 bytes, structure A, etc.
        /// </summary>
        public VarType Type { get; set; }

        /// <summary>
        /// The context that this node owns.
        /// </summary>
        public Context Context { get; set; } 

        /// <summary>
        /// Creates AAST without context in it.
        /// </summary>
        /// <param name="node">AST node to be annotated.</param>
        /// <param name="type">The type of AAST.</param>
        public AASTNode(ASTNode node, VarType type) : base(null, null, node.CrspToken, node.NodeType)
        {
            Type = type;
            Children = new List<AASTNode>();
            Context = new Context("none", null); // Dummy context            
        }

        public override string ToString()
        {
            // Json format

            StringBuilder sb = new StringBuilder();

            sb.Append(string.Concat(Enumerable.Repeat("\t", level)))
                .Append("{\r\n");
            sb.Append(string.Concat(Enumerable.Repeat("\t", level + 1)))
                .Append("\"node_type\": ").Append("\"" + NodeType.ToString() + "\"").Append(",\r\n");
            sb.Append(string.Concat(Enumerable.Repeat("\t", level + 1)))
                .Append("\"type\": ").Append("\"" + Type.ToString() + "\"").Append(",\r\n");

            Context.level = level + 1; // To make the output correct
            sb.Append(string.Concat(Enumerable.Repeat("\t", level + 1)))
                .Append("\"context\": ");
            if (Context.Name.Equals("none"))
            {
                sb.Append("\"none\",\r\n");
            }
            else
            {
                sb.Append("\r\n").Append(Context.ToString()).Append(",\r\n");             
            }
            
            sb.Append(string.Concat(Enumerable.Repeat("\t", level + 1)))
                .Append("\"token\": ").Append("\"" + CrspToken.Value + "\"").Append(",\r\n");
            sb.Append(string.Concat(Enumerable.Repeat("\t", level + 1)))
                .Append("\"children\": [");

            if (Children.Count > 0)
            {
                foreach (ASTNode child in Children)
                {
                    sb.Append("\r\n").Append(child.ToString()).Append(',');
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
