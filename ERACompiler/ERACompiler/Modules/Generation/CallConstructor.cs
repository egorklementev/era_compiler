using ERACompiler.Structures;
using ERACompiler.Structures.Types;
using ERACompiler.Utilities.Errors;

namespace ERACompiler.Modules.Generation
{
    class CallConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            // prim [ iden, call args ]
            // 
            // Generate call bytes TODO: Dot notation (routines in modules)
            //
            // Construct parameters and put them in the stack
            // Deallocate everything
            // R27 = SB + offset(func);
            // if R27 goto R27;
            // Allocate back
            // Manage return value (if any) 

            Generator g = Program.currentCompiler.generator;
            Context? ctx = SemanticAnalyzer.FindParentContext(aastNode);
            CodeNode callNode = new CodeNode(aastNode, parent);

            int param_i = 2;
            foreach (AASTNode expr in aastNode.Children[1].Children)
            {
                CodeNode exprNode = base.Construct(expr, callNode);
                byte fr0 = exprNode.ByteToReturn;
                callNode.Children.AddLast(exprNode);

                callNode.Children.AddLast(new CodeNode("Parameter store", callNode)
                    .Add(GenerateMOV(SP, 27))
                    .Add(GenerateLDA(27, 27, param_i * 4))
                    .Add(GenerateST(fr0, 27)));

                param_i++;
                g.FreeReg(fr0);
            }

            AASTNode mainContextNode = aastNode;
            while (!mainContextNode.ASTType.Equals("Program") && !mainContextNode.ASTType.Equals("Module"))
            {
                if (mainContextNode.Parent == null)
                {
                    throw new CompilationErrorException("Routine call is bad!!! (how did you manage to do that!?)");
                }
                if (mainContextNode.Context != null)
                {
                    callNode.Children.AddLast(GetRegisterDeallocationNode(mainContextNode, callNode, false));
                }
                mainContextNode = (AASTNode)mainContextNode.Parent;
            }
            callNode.Children.AddLast(new CodeNode("Call jump", callNode)
                .Add(GenerateLDA(SB, 27, ctx.GetStaticOffset(aastNode.Children[0].Token.Value)))
                .Add(GenerateLD(27, 27))
                .Add(GenerateCBR(27, 27)));
            // ATTENTION: Register allocation may be needed here

            if (ctx.GetRoutineReturnType(aastNode.Children[0].Token).Type != VarType.ERAType.NO_TYPE) // Return value is in R26
            {
                CodeNode fr0Node = GetFreeRegisterNode(aastNode, callNode);
                byte fr0 = fr0Node.ByteToReturn;
                callNode.Children.AddLast(fr0Node);
                callNode.Children.AddLast(new CodeNode("return value", callNode).Add(GenerateMOV(26, fr0)));
                callNode.ByteToReturn = fr0;
                g.FreeReg(26);
            }

            return callNode;
        }
    }
}
