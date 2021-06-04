using ERACompiler.Structures;

namespace ERACompiler.Modules.Semantics
{
    class BlockBodyAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            // TODO: name of the block body
            string bbName = "name";
            AASTNode bb = new AASTNode(astNode, parent, SemanticAnalyzer.no_type);
            bb.Context = new Context("BlockBody_" + bbName, SemanticAnalyzer.FindParentContext(parent), bb);
            bool setLIEnd = false;
            string varName = "";
            if (Program.currentCompiler.semantics.varToAddToCtx != null)
            {
                bb.Context.AddVar(Program.currentCompiler.semantics.varToAddToCtx, Program.currentCompiler.semantics.varToAddToCtx.Token.Value);
                varName = Program.currentCompiler.semantics.varToAddToCtx.Token.Value;
                setLIEnd = true;
                Program.currentCompiler.semantics.varToAddToCtx = null;
            }
            foreach (ASTNode child in astNode.Children)
            {
                bb.Children.Add(base.Annotate(child, bb));
            }
            if (setLIEnd)
            {                
                bb.Context.SetLIEnd(varName, SemanticAnalyzer.GetMaxDepth(bb));
            }
            return bb;
        }
    }
}
