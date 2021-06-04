using ERACompiler.Structures;
using ERACompiler.Structures.Types;

namespace ERACompiler.Modules.Semantics
{
    class CodeBlockAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            AASTNode code = new AASTNode(astNode, parent, new VarType(VarType.ERAType.MODULE))
            {
                Context = new Context("code", SemanticAnalyzer.FindParentContext(parent))
            };

            foreach (ASTNode child in astNode.Children[1].Children)
            {
                code.Children.Add(base.Annotate(child, code));
            }

            SemanticAnalyzer.FindParentContext(parent).AddVar(code, "code");

            return code;
        }
    }
}
