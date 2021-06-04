using ERACompiler.Structures;
using ERACompiler.Structures.Types;

namespace ERACompiler.Modules.Semantics
{
    class ModuleAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            Context? ctx = SemanticAnalyzer.FindParentContext(parent);
            AASTNode module = new AASTNode(astNode, parent, new VarType(VarType.ERAType.MODULE));
            module.Context = new Context(astNode.Children[1].Token.Value, ctx, module);
            foreach (ASTNode child in astNode.Children[2].Children)
            {
                module.Children.Add(base.Annotate(child, module));
            }
            ctx?.AddVar(module, astNode.Children[1].Token.Value);
            return module;
        }
    }
}
