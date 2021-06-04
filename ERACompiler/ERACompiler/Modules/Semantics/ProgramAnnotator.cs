using ERACompiler.Structures;

namespace ERACompiler.Modules.Semantics
{
    class ProgramAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            AASTNode program = new AASTNode(astNode, parent, SemanticAnalyzer.no_type);
            program.Context = new Context("Program", null, program);

            foreach (ASTNode child in astNode.Children)
            {
                program.Children.Add(base.Annotate(child, program));
            }

            return program;
        }
    }
}
