using ERACompiler.Structures;
using ERACompiler.Structures.Types;
using ERACompiler.Utilities.Errors;

namespace ERACompiler.Modules.Semantics
{
    class StructureAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            string structName = astNode.Children[1].Token.Value;
            AASTNode structure = new AASTNode(astNode, parent, new StructType(structName));
            Context? ctx = SemanticAnalyzer.FindParentContext(parent)
                ?? throw new SemanticErrorException("No parent context found!!!\r\n  At line " + astNode.Token.Position.Line);
            ctx?.AddVar(structure, structName);
            structure.Context = new Context(structName, ctx, structure);
            foreach (ASTNode child in astNode.Children[2].Children)
            {
                structure.Children.Add(base.Annotate(child, structure));
            }
            return structure;
        }
    }
}
