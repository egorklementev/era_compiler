using ERACompiler.Structures;

namespace ERACompiler.Modules.Semantics
{
    class StatementAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            AASTNode stmnt = new AASTNode(astNode, parent, SemanticAnalyzer.no_type);

            // Label if any
            if (astNode.Children[0].Children.Count > 0)
            {
                stmnt.Children.Add(base.Annotate(astNode.Children[0].Children[0], stmnt));
            }

            // The statement itself
            stmnt.Children.Add(base.Annotate(astNode.Children[1].Children[0], stmnt));

            return stmnt;
        }
    }
}
