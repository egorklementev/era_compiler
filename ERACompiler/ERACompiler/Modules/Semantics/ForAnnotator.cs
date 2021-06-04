using ERACompiler.Structures;
using ERACompiler.Structures.Types;

namespace ERACompiler.Modules.Semantics
{
    class ForAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            AASTNode forNode = new AASTNode(astNode, parent, SemanticAnalyzer.no_type);
            forNode.Children.Add(base.Annotate(astNode.Children[1], forNode));
            ((AASTNode)forNode.Children[0]).AASTType = new VarType(VarType.ERAType.INT);
            ((AASTNode)forNode.Children[0]).LIStart = 1;
            Program.currentCompiler.semantics.varToAddToCtx = (AASTNode) forNode.Children[0];
            // If 'from' expression exists
            if (astNode.Children[2].Children.Count > 0)
            {
                forNode.Children.Add(base.Annotate(astNode.Children[2].Children[1], forNode));
                ((AASTNode)forNode.Children[^1]).AASTValue = 1; // For generator
            }
            // If 'to' expression exists
            if (astNode.Children[3].Children.Count > 0)
            {
                forNode.Children.Add(base.Annotate(astNode.Children[3].Children[1], forNode));
                ((AASTNode)forNode.Children[^1]).AASTValue = 2; // For generator
            }
            // If 'step' expression exists
            if (astNode.Children[4].Children.Count > 0)
            {
                forNode.Children.Add(base.Annotate(astNode.Children[4].Children[1], forNode));
                ((AASTNode)forNode.Children[^1]).AASTValue = 3; // For generator
            }
            forNode.Children.Add(base.Annotate(astNode.Children[5], forNode)); // Loop body
            return forNode;
        }
    }
}
