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
                int frameSize =
                    ctx.GetFrameOffset(ctx.GetDeclaredVars().Last().Token.Value) +
                    ctx.GetDeclaredVars().Last().AASTType.GetSize();
                codeBlockNode.Children.AddLast(
                    new CodeNode("Stack allocation", codeBlockNode).Add(GenerateLDA(SP, SP, frameSize)));
            }

            foreach (AASTNode statement in aastNode.Children)
            {
                codeBlockNode.Children.AddLast(GetRegisterAllocationNode(statement, codeBlockNode));
                codeBlockNode.Children.AddLast(base.Construct(statement, codeBlockNode)); // Recursive statement bincode generation
                codeBlockNode.Children.AddLast(GetRegisterDeallocationNode(statement, codeBlockNode));
            }

            codeBlockNode.Children.AddLast(GetDynamicArrayDeallocationNode(aastNode, codeBlockNode));

            return codeBlockNode;
        }
    }
}
