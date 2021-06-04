using ERACompiler.Structures;
using ERACompiler.Utilities.Errors;

namespace ERACompiler.Modules.Generation
{
    class ReturnConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            Generator g = Program.currentCompiler.generator;
            CodeNode returnNode = new CodeNode(aastNode, parent);

            if (aastNode.Children.Count > 0)
            {
                // Call or Expr
                CodeNode exprNode = base.Construct((AASTNode)aastNode.Children[0], returnNode);
                byte fr1 = exprNode.ByteToReturn;
                returnNode.Children.AddLast(exprNode);
                returnNode.Children.AddLast(new CodeNode("r26 return mov", returnNode).Add(GenerateMOV(fr1, 26)));
                g.OccupateReg(26);
                g.FreeReg(fr1);
            }
            
            // Deallocate registers & dynamic arrays
            for (byte i = 0; i < 26; i++)
            {
                g.FreeReg(i);
                if (g.regAllocRTV.ContainsKey(i))
                {
                    string varName = g.regAllocRTV[i];
                    g.regAllocRTV.Remove(i);
                    g.regAllocVTR.Remove(varName);
                }
            }

            AASTNode mainContextNode = aastNode;
            while (!mainContextNode.ASTType.Equals("Routine"))
            {
                if (mainContextNode.Parent == null)
                {
                    throw new CompilationErrorException("Return is bad!!!");
                }
                if (mainContextNode.Context != null)
                {
                    returnNode.Children.AddLast(GetDynamicMemoryDeallocationNode(mainContextNode, returnNode));
                    if (mainContextNode.ASTType.Equals("For"))
                    {
                        returnNode.Children.AddLast(GetHeapTopChangeNode(returnNode, 16));
                    }
                    if (mainContextNode.ASTType.Equals("While"))
                    {
                        returnNode.Children.AddLast(GetHeapTopChangeNode(returnNode, 12));
                    }
                    if (mainContextNode.ASTType.Equals("Loop While"))
                    {
                        returnNode.Children.AddLast(GetHeapTopChangeNode(returnNode, 4));
                    }
                }
                mainContextNode = (AASTNode)mainContextNode.Parent;
            }

            // Deallocate all scope related memory from the stack
            int ctxNum = 0;
            ASTNode anchor = aastNode.Parent;
            while (!anchor.ASTType.Equals("Routine body"))
            {
                if (((AASTNode)anchor).Context != null)
                    ctxNum++;
                anchor = anchor.Parent;
            }
            for (int i = 0; i < ctxNum; i++)
            {
                returnNode.Children.AddLast(new CodeNode("Return stack back", returnNode)
                    .Add(GenerateLDA(FP, FP, -4))
                    .Add(GenerateMOV(FP, SP))
                    .Add(GenerateLD(FP, FP)));
            }

            returnNode.Children.AddLast(new CodeNode("Return to the calling function", returnNode)
                .Add(GenerateLDA(FP, FP, -4))
                .Add(GenerateLD(FP, 27))
                .Add(GenerateLDA(FP, FP, -4))
                .Add(GenerateMOV(FP, SP))
                .Add(GenerateLD(FP, FP))
                .Add(GenerateCBR(27, 27))
                );

            return returnNode;
        }
    }
}
