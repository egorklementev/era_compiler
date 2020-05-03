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
        /// Parent node of the node.
        /// </summary>
        public new AASTNode? Parent { get; set; }

        /// <summary>
        /// Children nodes of this AAST node.
        /// </summary>
        public new List<AASTNode> Children { get; set; }

        /// <summary>
        /// integer, array of 30 bytes, structure A, etc.
        /// </summary>
        public VarType Type { get; set; }

        /// <summary>
        /// Whether the node creates a new context or not
        /// </summary>
        public bool HasContext { get; } = false;

        /// <summary>
        /// The context that this node owns.
        /// </summary>
        public Context? Context { get; } 

        /// <summary>
        /// Creates AAST without context in it.
        /// </summary>
        /// <param name="node">AST node to be annotated.</param>
        /// <param name="type">The type of AAST.</param>
        public AASTNode(ASTNode node, VarType type) : base(null, null, node.CrspToken, node.NodeType)
        {
            Type = type;
            Children = new List<AASTNode>();
        }

        /// <summary>
        /// Creates AAST with the context in it.
        /// </summary>
        /// <param name="node">AST node to be annotated.</param>
        /// <param name="type">The type of AAST.</param>
        /// <param name="context">The context that is owned by this node.</param>
        public AASTNode(ASTNode node, VarType type, Context context) : this(node, type)
        {
            HasContext = true;
            Context = context;
        }

        public override string ToString()
        {
            // Json format

            StringBuilder sb = new StringBuilder();

            sb.Append(string.Concat(Enumerable.Repeat("\t", level)))
                .Append("{\r\n");
            sb.Append(string.Concat(Enumerable.Repeat("\t", level + 1)))
                .Append("\"type\": ").Append("\"" + Type.ToString() + "\"").Append(",\r\n");

            if (HasContext)
            {
                sb.Append(string.Concat(Enumerable.Repeat("\t", level + 1)))
                .Append("\"context\": ").Append("\"" + Context.ToString() + "\"").Append(",\r\n");
            }
            else
            {
                sb.Append(string.Concat(Enumerable.Repeat("\t", level + 1)))
                .Append("\"context\": ").Append("\"none\"").Append(",\r\n");
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
