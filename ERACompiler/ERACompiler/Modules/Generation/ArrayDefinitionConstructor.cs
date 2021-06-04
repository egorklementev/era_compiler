using ERACompiler.Structures;
using ERACompiler.Structures.Types;

namespace ERACompiler.Modules.Generation
{
    class ArrayDefinitionConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            Generator g = Program.currentCompiler.generator;
            CodeNode arrDefNode = new CodeNode(aastNode, parent);
            Context? ctx = SemanticAnalyzer.FindParentContext(aastNode);

            if (((ArrayType)aastNode.AASTType).Size == 0) // We have to allocate it on the heap
            {
                int offsetSize = ctx.GetArrayOffsetSize(aastNode.Token.Value) / 2;
                CodeNode exprNode = base.Construct((AASTNode)aastNode.Children[0], arrDefNode);
                byte fr0 = exprNode.ByteToReturn; // Execution-time array size
                arrDefNode.Children.AddLast(exprNode);
                while (offsetSize > 0)
                {
                    arrDefNode.Children.AddLast(new CodeNode("Arr def ASL", arrDefNode).Add(GenerateASL(fr0, fr0)));
                    offsetSize /= 2;
                }
                CodeNode fr1Node = GetFreeRegisterNode(aastNode, arrDefNode);
                arrDefNode.Children.AddLast(fr1Node);
                byte fr1 = fr1Node.ByteToReturn;
                CodeNode fr2Node = GetFreeRegisterNode(aastNode, arrDefNode);
                arrDefNode.Children.AddLast(fr2Node);
                byte fr2 = fr2Node.ByteToReturn;
                /* heap: [array_size 4 bytes] [0th element] [1st element] ... [last element];  Allocated register contains address of 0th element. */
                arrDefNode.Children.AddLast(new CodeNode("Array heap allocation 1", arrDefNode)
                    .Add(GenerateLDA(fr0, fr0, 4))); // Allocate 4 additional bytes for array size
                arrDefNode.Children.AddLast(GetHeapTopChangeNode(arrDefNode, fr0, true));
                arrDefNode.Children.AddLast(GetStoreToHeapNode(arrDefNode, fr0, 0)); // heap[0] := fr0;
                arrDefNode.Children.AddLast(new CodeNode("Array heap allocation 2", arrDefNode)
                    .Add(GenerateLDC(0, fr1))
                    .Add(GenerateLD(fr1, fr2))
                    .Add(GenerateLDA(fr2, fr2, 4)));
                arrDefNode.Children.AddLast(GetStoreVariableNode(arrDefNode, aastNode.Token.Value, fr2, ctx));

                g.FreeReg(fr0);
                g.FreeReg(fr1);
                g.FreeReg(fr2);
            }

            return arrDefNode;
        }
    }
}
