using ERACompiler.Structures;
using ERACompiler.Structures.Types;
using ERACompiler.Utilities.Errors;
using System;
using System.Collections.Generic;

namespace ERACompiler.Modules.Generation
{
    class CallConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            // prim [ iden, call args ]
            // 
            // Generate call bytes. 
            // TODO: Dot notation (routines in modules)
            //
            // Construct parameters and put them in the stack
            // Deallocate everything
            // R27 = SB + offset(func);
            // if R27 goto R27;
            // Allocate back
            // Manage return value (if any) 

            Generator g = Program.currentCompiler.generator;
            Context? ctx = SemanticAnalyzer.FindParentContext(aastNode)
                ?? throw new CompilationErrorException("No parent context found!!!\r\n  At line " + aastNode.Token.Position.Line);
            CodeNode callNode = new CodeNode(aastNode, parent);

            int i = 0;
            List<VarType> paramTypes = ((RoutineType)ctx.GetVarType(aastNode.Children[0].Token)).ParamTypes;
            int param_i = 8;
            foreach (AASTNode expr in aastNode.Children[1].Children)
            {
                CodeNode exprNode = base.Construct(expr, callNode);
                byte fr0 = exprNode.ByteToReturn;
                callNode.Children.AddLast(exprNode);

                CodeNode fr1Node = GetFreeRegisterNode(ctx, callNode);
                byte fr1 = fr1Node.ByteToReturn;
                callNode.Children.AddLast(fr1Node);

                CodeNode fr2Node = GetFreeRegisterNode(ctx, callNode);
                byte fr2 = fr2Node.ByteToReturn;
                callNode.Children.AddLast(fr2Node);

                // Store parameters depending on their size on the stack before we enter a routine.
                int mask = paramTypes[i].GetSize() == 4 ? 0 : ((int)Math.Pow(256, 4) - 1) << (8 * paramTypes[i].GetSize()); // ff ff ff 00 or ff ff 00 00 or 00 00 00 00
                int mask2 = paramTypes[i].GetSize() == 4 ? -1 : (int)Math.Pow(256, paramTypes[i].GetSize()) - 1; // 00 00 00 ff or 00 00 ff ff or ff ff ff ff

                callNode.Children.AddLast(new CodeNode("Parameter store", callNode)
                    .Add(GenerateMOV(SP, 27))
                    .Add(GenerateLDA(27, 27, param_i - 4 + paramTypes[i].GetSize()))
                    .Add(GenerateLD(27, fr1))
                    .Add(GenerateLDC(0, fr2))
                    .Add(GenerateLDA(fr2, fr2, mask))
                    .Add(GenerateAND(fr2, fr1))
                    .Add(GenerateLDC(0, fr2))
                    .Add(GenerateLDA(fr2, fr2, mask2))
                    .Add(GenerateAND(fr2, fr0))
                    .Add(GenerateOR(fr1, fr0))
                    .Add(GenerateST(fr0, 27)));

                param_i += paramTypes[i].GetSize();
                i++;
                g.FreeReg(fr0);
                g.FreeReg(fr1);
                g.FreeReg(fr2);
            }

            // Recursively deallocate (statement-unaware) evertything, basically, up the AAST tree since we change our context completely.
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
                .Add(GenerateLDA(SB, 27, ctx.GetStaticOffset(aastNode.Children[0].Token.Value))) // TODO: module routine jumps. Here are only pure static routines.
                .Add(GenerateLD(27, 27))
                .Add(GenerateCBR(27, 27)));

            // If routine is not NO_TYPE, then it has returned something and this return value is always in R26.
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
