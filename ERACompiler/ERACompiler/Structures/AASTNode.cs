using ERACompiler.Structures.Types;

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

    }
}
