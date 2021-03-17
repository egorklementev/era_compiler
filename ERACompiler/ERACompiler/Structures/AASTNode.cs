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
        /// Used in Linear Scan for Statement/VarDecl numeration.
        /// </summary>
        public int BlockPosition { get; set; } = 0;

        /// <summary>
        /// Live interval start (var declaration)
        /// </summary>
        public int LIStart { get; set; } = 0;
        /// <summary>
        /// Live interval end (some statement)
        /// </summary>
        public int LIEnd { get; set; } = 0;

        /// <summary>
        /// The local address of this variable relative to the FP (in bytes)
        /// </summary>
        public int FrameOffset { get; set; } = 0;

        /// <summary>
        /// The global address of this variable relative to the SB (in bytes)
        /// </summary>
        public int StaticOffset { get; set; } = 0;

        /// <summary>
        /// Used to know whether this variable is global or not
        /// </summary>
        public bool IsGlobal { get; set; } = false;

        /// <summary>
        /// The context that this node owns.
        /// </summary>
        public Context? Context { get; set; } = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="node">AST node from which it is derived basic information.</param>
        /// <param name="parent">AAST node parent of this node.</param>
        /// <param name="type">Static type of this variable (if it is a variable).</param>
        public AASTNode(ASTNode node, AASTNode? parent, VarType type) : base(parent, new List<ASTNode>(), new Token(node.Token), node.ASTType)
        {
            AASTType = type;
        }

        public override string ToString()
        {
            // Json format

            StringBuilder sb = new StringBuilder();

            string tabs_lvl = string.Concat(Enumerable.Repeat("\t", level));
            string tabs_lvl1 = tabs_lvl + "\t";

            sb.Append(tabs_lvl)
                .Append("{\r\n");
            sb.Append(tabs_lvl1)
                .Append("\"node_type\": ").Append("\"" + ASTType.ToString() + "\"").Append(",\r\n");
            
            if (AASTType.Type != VarType.ERAType.NO_TYPE || Program.extendedSemanticMessages)
                sb.Append(tabs_lvl1)
                    .Append("\"var_type\": ").Append("\"" + AASTType.ToString() + "\"").Append(",\r\n");
            
            if (AASTValue != 0 || Program.extendedSemanticMessages)
                sb.Append(tabs_lvl1)
                    .Append("\"var_value\": ").Append("\"" + AASTValue.ToString() + "\"").Append(",\r\n");
            
            if (BlockPosition != 0 || Program.extendedSemanticMessages)
                sb.Append(tabs_lvl1)
                    .Append("\"block_position\": ").Append("\"" + BlockPosition.ToString() + "\"").Append(",\r\n");

            if (Context != null)            
            {
                Context.Level = level + 1; // To make the output correct
                sb.Append(tabs_lvl1)
                    .Append("\"context\": ");
                sb.Append("\r\n").Append(Context.ToString()).Append(",\r\n");             
            }
            
            sb.Append(tabs_lvl1)
                .Append("\"token\": ").Append("\"" + Token.Value + "\"").Append(",\r\n");
            sb.Append(tabs_lvl1)
                .Append("\"children\": [");

            if (Children.Count > 0)
            {
                foreach (ASTNode child in Children)
                {
                    sb.Append("\r\n").Append(child?.ToString()).Append(',');
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
