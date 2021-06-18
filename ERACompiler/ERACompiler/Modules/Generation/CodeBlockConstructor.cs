using ERACompiler.Structures;
using ERACompiler.Utilities.Errors;
using System.Linq;

namespace ERACompiler.Modules.Generation
{
    class CodeBlockConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            Context? ctx = aastNode.Context
                ?? throw new CompilationErrorException("No parent context found!!!\r\n  At line " + aastNode.Token.Position.Line);
            CodeNode codeBlockNode = new CodeNode(aastNode, parent);

            // Simulator automatically moves SP to the first byte after SKIP STOP commands (they are 0x40 0x00 0x00 0x00 in the memory dump file)
            // So, we manually move FP to SP.
            codeBlockNode.Children.AddLast(
                new CodeNode("Copy SP to FP", codeBlockNode).Add(GenerateMOV(SP, FP)));

            // Allocate current frame size and move SP to the first byte after current frame.
            if (ctx?.GetDeclaredVars().Count > 0)
            {
                int frameSize = 0;
                foreach (AASTNode variable in ctx.GetDeclaredVars())
                {
                    frameSize += variable.AASTType.GetSize();
                }
                codeBlockNode.Children.AddLast(
                    new CodeNode("Stack allocation", codeBlockNode).Add(GenerateLDA(SP, SP, frameSize)));
            }

            // Construct child statements using statement-aware register allocation and deallocation.
            foreach (AASTNode statement in aastNode.Children)
            {
                codeBlockNode.Children.AddLast(GetRegisterAllocationNode(statement, codeBlockNode));
                codeBlockNode.Children.AddLast(base.Construct(statement, codeBlockNode)); // Recursive statement bincode generation
                codeBlockNode.Children.AddLast(GetRegisterDeallocationNode(statement, codeBlockNode));
            }

            // Deallocate all dynamic arrays and structure from the heap.
            codeBlockNode.Children.AddLast(GetDynamicMemoryDeallocationNode(aastNode, codeBlockNode));

            return codeBlockNode;
        }
    }
}
