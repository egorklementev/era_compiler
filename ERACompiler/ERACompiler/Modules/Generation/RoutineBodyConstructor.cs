using ERACompiler.Structures;
using ERACompiler.Utilities.Errors;
using System.Linq;

namespace ERACompiler.Modules.Generation
{
    class RoutineBodyConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            Generator g = Program.currentCompiler.generator;
            Context? ctx = SemanticAnalyzer.FindParentContext(aastNode)
                ?? throw new CompilationErrorException("No parent context found!!!\r\n  At line " + aastNode.Token.Position.Line);
            CodeNode rbNode = new CodeNode(aastNode, parent);

            int frameSize = 0;
            foreach (AASTNode variable in ctx.GetDeclaredVars())
            {
                frameSize += variable.AASTType.GetSize();
            }

            rbNode.Children.AddLast(new CodeNode("Pre routine body", rbNode)
                .Add(GenerateST(FP, SP))
                .Add(GenerateMOV(SP, FP))
                .Add(GenerateLDA(FP, FP, 4))
                .Add(GenerateST(27, FP))
                .Add(GenerateLDA(FP, FP, 4))
                .Add(GenerateMOV(FP, SP))
                .Add(GenerateLDA(SP, SP, frameSize)));

            foreach (AASTNode statement in aastNode.Children)
            {
                rbNode.Children.AddLast(GetRegisterAllocationNode(statement, rbNode));
                rbNode.Children.AddLast(base.Construct(statement, rbNode));
                rbNode.Children.AddLast(GetRegisterDeallocationNode(statement, rbNode));
            }

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

            // Deallocate dynamic arrays
            rbNode.Children.AddLast(GetDynamicMemoryDeallocationNode(aastNode, rbNode));

            rbNode.Children.AddLast(new CodeNode("Post routine body", rbNode)
                .Add(GenerateLDA(FP, FP, -4))
                .Add(GenerateLD(FP, 27))
                .Add(GenerateLDA(FP, FP, -4))
                .Add(GenerateMOV(FP, SP))
                .Add(GenerateLD(FP, FP))
                .Add(GenerateCBR(27, 27)));

            return rbNode;
        }
    }
}
