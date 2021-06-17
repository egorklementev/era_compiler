using ERACompiler.Structures;
using System;

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
                int bytesToLoad = GetExpressionSizeInBytes((AASTNode)aastNode.Children[2]);
                int mask = bytesToLoad == 4 ? -1 : (int)Math.Pow(256, bytesToLoad) - 1; // 00 00 00 ff or 00 00 ff ff or ff ff ff ff
                CodeNode frNode = GetFreeRegisterNode(ctx, derefNode);
                byte fr1 = frNode.ByteToReturn;
                derefNode.Children.AddLast(frNode);
                derefNode.Children.AddLast(new CodeNode("load deref cmds 1", derefNode)
                    .Add(GenerateLDC(4 - bytesToLoad, fr1))
                    .Add(GenerateSUB(fr1, fr0))
                    .Add(GenerateLD(fr0, fr0))
                    .Add(GenerateLDC(0, fr1))
                    .Add(GenerateLDA(fr1, fr1, mask))
                    .Add(GenerateAND(fr1, fr0)));
                g.FreeReg(fr1);
            }
            derefNode.ByteToReturn = fr0;

            return derefNode;
        }
    }
}
