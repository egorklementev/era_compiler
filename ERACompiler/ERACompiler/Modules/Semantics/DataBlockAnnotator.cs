using ERACompiler.Structures;
using ERACompiler.Structures.Types;

namespace ERACompiler.Modules.Semantics
{
    class DataBlockAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            Context ctx = SemanticAnalyzer.FindParentContext(parent);
            AASTNode data = new AASTNode(astNode, parent, new DataType());
            ((DataType)data.AASTType).Size = astNode.Children[3].Children.Count + 1;
            data.Children.Add(base.Annotate(astNode.Children[1], data)); // Identifier
            data.Children.Add(base.Annotate(astNode.Children[2], data)); // The first literal
            foreach (ASTNode child in astNode.Children[3].Children)
            {
                data.Children.Add(base.Annotate(child, data)); // The rest of literals
            }
            ctx.AddVar(data, astNode.Children[1].Token.Value);
            return data;
        }
    }
}
