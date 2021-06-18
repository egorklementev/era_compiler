using ERACompiler.Structures;
using ERACompiler.Utilities.Errors;
using System.Linq;

namespace ERACompiler.Modules.Generation
{
    class BlockBodyConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            Context? ctx = aastNode.Context
                ?? throw new CompilationErrorException("No parent context found!!!\r\n  At line " + aastNode.Token.Position.Line);
            CodeNode bbNode = new CodeNode(aastNode, parent);

            int frameSize = 0; // Frame size (variables, arrays, etc.) in bytes
            foreach (AASTNode variable in ctx.GetDeclaredVars())
            {
                frameSize += variable.AASTType.GetSize();
            }

            // Store where to return FP and move SP further depending on the frame size.
            bbNode.Children.AddLast(new CodeNode("Block body cmds 1", bbNode)
                .Add(GenerateST(FP, SP)) // Store where to return
                .Add(GenerateMOV(SP, FP))
                .Add(GenerateLDA(FP, FP, 4))
                .Add(GenerateMOV(FP, SP))
                .Add(GenerateLDA(SP, SP, frameSize)));

            foreach (AASTNode statement in aastNode.Children)
            {
                bbNode.Children.AddLast(GetRegisterAllocationNode(statement, bbNode)); // Statement-aware allocation
                bbNode.Children.AddLast(base.Construct(statement, bbNode)); // Statement construction
                bbNode.Children.AddLast(GetRegisterDeallocationNode(statement, bbNode)); // Statement-aware deallocation
            }
            // Final unconditional (statement-unaware) deallocation. Specially for 'for' iterators since they cannot be deallocated in usual way.
            if (aastNode.Children.Count > 0)
            {
                bbNode.Children.AddLast(GetRegisterDeallocationNode((AASTNode)aastNode.Children[^1], bbNode, false));
            }

            // Deallocate all dynamic arrays and structures.
            bbNode.Children.AddLast(GetDynamicMemoryDeallocationNode(aastNode, bbNode));

            bbNode.Children.AddLast(new CodeNode("Block body cmds 2", bbNode)
                .Add(GenerateLDA(FP, FP, -4))
                .Add(GenerateMOV(FP, SP)) // Return stack pointer
                .Add(GenerateLD(FP, FP))); // Return frame pointer

            return bbNode;
        }
    }
}
