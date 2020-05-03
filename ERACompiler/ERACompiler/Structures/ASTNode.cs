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
        public ASTNode? Parent { get; set;  }
        /// <summary>
        /// Child nodes of the node.
        /// </summary>
        public List<ASTNode> Children { get; }
        /// <summary>
        /// The token of the source code that corresponds to this node.
        /// </summary>
        public Token CrspToken { get; }
        /// <summary>
        /// Represents the type of an AST entry.
        /// </summary>
        public ASTNodeType NodeType { get; }

        /// <summary>
        /// How deep in the AST the node is located. Used for proper tabulation in the ToString() method.
        /// </summary>
        protected int level;

        /// <summary>
        /// Creates an AST node.
        /// </summary>
        /// <param name="parent">Ref to the parent AST node.</param>
        /// <param name="children">Ref to the list of AST children nodes.</param>
        /// <param name="token">Corresponding token from the lexical analyzer.</param>
        /// <param name="type">The type of the AST node (basically syntax rule).</param>
        public ASTNode(ASTNode? parent, List<ASTNode>? children, Token token, ASTNodeType type)
        {
            Parent = parent;
            Children = children;
            CrspToken = token;
            NodeType = type;

            if (parent != null) 
                level = parent.level + 2;            
            else            
                level = 0;            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>A string in JSON format of the node and its children (works recursively).</returns>
        public override string ToString()
        {
            // Json format

            StringBuilder sb = new StringBuilder();

            sb.Append(string.Concat(Enumerable.Repeat("\t", level)))
                .Append("{\r\n");
            sb.Append(string.Concat(Enumerable.Repeat("\t", level + 1)))
                .Append("\"type\": ").Append("\"" + NodeType.ToString() + "\"").Append(",\r\n");            
            sb.Append(string.Concat(Enumerable.Repeat("\t", level + 1)))
                .Append("\"token\": ").Append("\"" + CrspToken.Value + "\"").Append(",\r\n");            
            sb.Append(string.Concat(Enumerable.Repeat("\t", level + 1)))
                .Append("\"children\": [");            

            if (Children.Count > 0)
            {
                foreach(ASTNode child in Children)
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

        /// <summary>
        /// The type of the AST node.
        /// </summary>
        public enum ASTNodeType
        {
            PROGRAM,
            UNIT,
            ANNOTATION,            
            CODE,
            DATA,
            MODULE,
            ROUTINE,
            PRAGMA_DECLARATION,
            VARIABLE,
            VARIABLE_DECLARATION,
            CONSTANT,
            STATEMENT,
            IDENTIFIER,
            LITERAL,
            DECLARATION,
            TYPE,
            VAR_DEFINITION,
            EXPRESSION,
            CONST_DEFINITION,
            LABEL,
            ASSEMBLER_BLOCK,
            ASSEMBLER_STATEMENT,
            EXTENSION_STATEMENT,
            DIRECTIVE,
            ATTRIBUTE,
            PARAMETERS,
            RESULTS,
            ROUTINE_BODY,
            PARAMETER,
            REGISTER,
            PRIMARY,
            VARIABLE_REFERENCE,
            REFERENCE,
            DEREFERENCE,
            ARRAY_ELEMENT,
            EXPLICIT_ADDRESS,
            OPERAND,
            OPERATOR,
            COMPARISON_OPERATOR,
            RECEIVER,
            ASSIGNMENT,
            CALL,
            CALL_ARGUMENTS,
            IF,
            WHILE,
            FOR,
            BREAK,
            SWAP,
            GOTO,
            LOOP,
            LOOP_BODY,
            ARRAY_DECLARATION,
            TEXT,
            STRUCTURE,
            STRUCTURE_ACCESS,
            BLOCK_BODY
        }

    }
}
