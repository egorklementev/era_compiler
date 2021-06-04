using ERACompiler.Structures;
using ERACompiler.Structures.Types;

namespace ERACompiler.Modules.Semantics
{
    class LabelAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            AASTNode label = new AASTNode(astNode, parent, new VarType(VarType.ERAType.LABEL));

            // Add label to the context
            Context? ctx = SemanticAnalyzer.FindParentContext(parent);
            ctx?.AddVar(label, astNode.Children[1].Token.Value);

            // Put identifier
            label.Children.Add(base.Annotate(astNode.Children[1], label));

            return label;
        }
    }
}
