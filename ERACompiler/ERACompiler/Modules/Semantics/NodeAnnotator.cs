using ERACompiler.Structures;

namespace ERACompiler.Modules.Semantics
{
    public class NodeAnnotator
    {
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
