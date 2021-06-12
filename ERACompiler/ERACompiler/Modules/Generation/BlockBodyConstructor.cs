using ERACompiler.Structures;
using System.Linq;

namespace ERACompiler.Modules.Generation
{
    class BlockBodyConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            Context? ctx = aastNode.Context;
            CodeNode bbNode = new CodeNode(aastNode, parent);

            int frameSize = 0;
            foreach (AASTNode variable in ctx.GetDeclaredVars())
            {
                frameSize += variable.AASTType.GetSize();
            }

            bbNode.Children.AddLast(new CodeNode("Block body cmds 1", bbNode)
                .Add(GenerateST(FP, SP)) // Store where to return
                .Add(GenerateMOV(SP, FP))
                .Add(GenerateLDA(FP, FP, 4))
                .Add(GenerateMOV(FP, SP))
                .Add(GenerateLDA(SP, SP, frameSize)));

            foreach (AASTNode statement in aastNode.Children)
            {
                bbNode.Children.AddLast(GetRegisterAllocationNode(statement, bbNode));
                bbNode.Children.AddLast(base.Construct(statement, bbNode));
                bbNode.Children.AddLast(GetRegisterDeallocationNode(statement, bbNode));
            }
            // Final unconditional deallocation ('for' iterators special)
            if (aastNode.Children.Count > 0)
            {
                bbNode.Children.AddLast(GetRegisterDeallocationNode((AASTNode)aastNode.Children[^1], bbNode, false));
            }

            bbNode.Children.AddLast(GetDynamicMemoryDeallocationNode(aastNode, bbNode));

            bbNode.Children.AddLast(new CodeNode("Block body cmds 2", bbNode)
                .Add(GenerateLDA(FP, FP, -4))
                .Add(GenerateMOV(FP, SP)) // Return stack pointer
                .Add(GenerateLD(FP, FP))); // Return frame pointer

            return bbNode;
        }
    }
}
