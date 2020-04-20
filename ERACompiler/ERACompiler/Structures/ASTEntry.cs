namespace ERACompiler.Structures
{
    /// <summary>
    /// The class representing Abstract Syntax Tree Entry
    /// </summary>
    public class ASTEntry
    {
        /// <summary>
        /// Parent node. Only the root node has parent equals to null.
        /// </summary>
        public ASTEntry Parent { get; }
        
        public ASTEntry()
        {
            
        }

    }
}
