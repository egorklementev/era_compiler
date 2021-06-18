using ERACompiler.Structures;

namespace ERACompiler.Modules.Semantics
{
    /// <summary>
    /// Main class that annotates given AST node reducing overall tree making AAST smaller, but populating it with more 
    /// valuable and important information such as contexts, live intervals, etc.
    /// </summary>
    public class NodeAnnotator
    {
        /// <summary>
        /// Main method that is usually being overwritten by child Node Annotators.
        /// </summary>
        /// <param name="astNode">AST node to be annotated.</param>
        /// <param name="parent">Possible AAST parent node.</param>
        /// <returns>Annotated AST node.</returns>
        public virtual AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            if (Program.currentCompiler.semantics.nodeAnnotators.ContainsKey(astNode.ASTType)) 
            { 
                return Program.currentCompiler.semantics.nodeAnnotators[astNode.ASTType].Annotate(astNode, parent);
            } 
            else
            {
                return Program.currentCompiler.semantics.nodeAnnotators["Default"].Annotate(astNode, parent);
            }
        }
    }
}
