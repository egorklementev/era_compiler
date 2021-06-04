using ERACompiler.Structures;

namespace ERACompiler.Modules.Generation
{
    class DereferenceConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            Generator g = Program.currentCompiler.generator;
            Context? ctx = SemanticAnalyzer.FindParentContext(aastNode);
            CodeNode derefNode = new CodeNode(aastNode, parent);

            CodeNode exprNode = base.Construct((AASTNode)aastNode.Children[2], derefNode);
            byte fr0 = exprNode.ByteToReturn;
            derefNode.Children.AddLast(exprNode);

            if (!parent.Name.Equals("Assignment")) // Right value
            {
                // Load out all visible variable since we do not know what is going to be dereferenced
                foreach (string varName in ctx.GetAllVisibleVars())
                {
                    if (g.regAllocVTR.ContainsKey(varName))
                    {
                        derefNode.Children.AddLast(GetStoreVariableNode(derefNode, varName, g.regAllocVTR[varName], ctx));
                    }
                }
                derefNode.Children.AddLast(new CodeNode("Dereference load", derefNode).Add(GenerateLD(fr0, fr0)));
            }
            derefNode.ByteToReturn = fr0;

            return derefNode;
        }
    }
}
