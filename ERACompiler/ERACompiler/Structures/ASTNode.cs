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
        public ASTNode Parent { get; }
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
        public ASTNodeType Type { get; }

        private int level; // How deep in the AST the node is located.

        public ASTNode(ASTNode parent, List<ASTNode> children, Token token, ASTNodeType type)
        {
            Parent = parent;
            Children = children;
            CrspToken = token;
            Type = type;

            if (parent != null) 
                level = parent.level + 1;            
            else            
                level = 0;            
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(string.Concat(Enumerable.Repeat("\t", level)));
            sb.Append("Node type: ").Append(Type.ToString()).Append("\r\n");
            sb.Append(string.Concat(Enumerable.Repeat("\t", level)));
            sb.Append("Token: ").Append(CrspToken.Value);

            if (Children.Count > 0)
            {
                foreach(ASTNode child in Children)
                {
                    sb.Append("\r\n").Append(child.ToString());
                }
            }

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
            DEREFERENCE,
            ARRAY_ELEMENT,
            DATA_ELEMENT,
            EXPLICIT_ADDRESS,
            OPERAND,
            OPERATOR,
            RECEIVER,
            PRIMITIVE_OPERATOR,
            ASSIGNMENT,
            CALL,
            IF,
            WHILE,
            FOR,
            BREAK,
            SWAP,
            GOTO,
            LOOP_BODY
        }

    }
}
