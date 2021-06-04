using ERACompiler.Structures;
using ERACompiler.Structures.Types;

namespace ERACompiler.Modules.Semantics
{
    class StructureAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            string structName = astNode.Children[1].Token.Value;
            AASTNode structure = new AASTNode(astNode, parent, new StructType(structName));
            Context? ctx = SemanticAnalyzer.FindParentContext(parent);
            ctx?.AddVar(structure, structName);
            structure.Context = new Context(structName, ctx);
            foreach (ASTNode child in astNode.Children[2].Children)
            {
                structure.Children.Add(base.Annotate(child, structure));
            }
            return structure;
        }
    }
}
