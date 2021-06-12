using ERACompiler.Structures;
using System.Linq;

namespace ERACompiler.Modules.Generation
{
    class CodeBlockConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            Context? ctx = aastNode.Context;
            CodeNode codeBlockNode = new CodeNode(aastNode, parent);

            codeBlockNode.Children.AddLast(
                new CodeNode("Copy SP to FP", codeBlockNode).Add(GenerateMOV(SP, FP)));

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

            foreach (AASTNode statement in aastNode.Children)
            {
                codeBlockNode.Children.AddLast(GetRegisterAllocationNode(statement, codeBlockNode));
                codeBlockNode.Children.AddLast(base.Construct(statement, codeBlockNode)); // Recursive statement bincode generation
                codeBlockNode.Children.AddLast(GetRegisterDeallocationNode(statement, codeBlockNode));
            }

            codeBlockNode.Children.AddLast(GetDynamicMemoryDeallocationNode(aastNode, codeBlockNode));

            return codeBlockNode;
        }
    }
}
