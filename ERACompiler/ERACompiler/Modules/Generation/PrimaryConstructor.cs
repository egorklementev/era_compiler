using ERACompiler.Structures;
using System;

namespace ERACompiler.Modules.Generation
{
    class PrimaryConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            Generator g = Program.currentCompiler.generator;
            CodeNode primNode = new CodeNode(aastNode, parent);
            Context? ctx = SemanticAnalyzer.FindParentContext(aastNode);

            if (aastNode.Children[^1].ASTType.Equals("IDENTIFIER")) // Regular identifier access
            {
                string varName = aastNode.Children[^1].Token.Value;
                if (g.regAllocVTR.ContainsKey(varName)) // If allocated to a register
                {
                    if (parent.Name.Equals("Assignment")) // Left value
                    {
                        primNode.ByteToReturn = g.regAllocVTR[varName];
                    }
                    else // Right value
                    {
                        CodeNode fr0Node = GetFreeRegisterNode(aastNode, primNode);
                        primNode.ByteToReturn = fr0Node.ByteToReturn;
                        primNode.Children.AddLast(fr0Node);
                        primNode.Children.AddLast(new CodeNode("Copy for right value", primNode).Add(GenerateMOV(g.regAllocVTR[varName], primNode.ByteToReturn)));
                    }
                }
                else
                {
                    CodeNode fr0Node = GetFreeRegisterNode(aastNode, primNode);
                    primNode.ByteToReturn = fr0Node.ByteToReturn;
                    primNode.Children.AddLast(fr0Node);
                    if (ctx.IsVarDynamicArray(varName))
                    {
                        primNode.Children.AddLast(GetLoadVariableNode(primNode, aastNode.Children[^1].Token.Value, primNode.ByteToReturn, ctx));
                    }
                    else if (ctx.IsVarArray(varName))
                    {
                        primNode.Children.AddLast(GetLoadVariableAddressNode(primNode, aastNode.Children[^1].Token.Value, primNode.ByteToReturn, ctx));
                    }
                    else
                    {
                        if (parent.Name.Equals("Assignment")) // Left value
                        {
                            primNode.Children.AddLast(GetLoadVariableAddressNode(primNode, aastNode.Children[^1].Token.Value, primNode.ByteToReturn, ctx));
                        }
                        else // Right value
                        {
                            primNode.Children.AddLast(GetLoadVariableNode(primNode, aastNode.Children[^1].Token.Value, primNode.ByteToReturn, ctx));
                        }
                    }
                }
            }
            else if (aastNode.Children[^1].ASTType.Equals("Expression")) // Accessing an array
            {
                string varName = aastNode.Children[^2].Token.Value;

                CodeNode exprNode = base.Construct((AASTNode)aastNode.Children[^1], primNode);
                byte fr0 = exprNode.ByteToReturn;
                primNode.Children.AddLast(exprNode);

                int offset = ctx.GetArrayOffsetSize(varName) / 2;
                while (offset > 0)
                {
                    primNode.Children.AddLast(new CodeNode("Array index asl", primNode).Add(GenerateASL(fr0, fr0)));
                    offset /= 2;
                }

                CodeNode fr1Node = GetFreeRegisterNode(aastNode, primNode);
                byte fr1 = fr1Node.ByteToReturn;
                primNode.Children.AddLast(fr1Node);
                primNode.ByteToReturn = fr1;

                if (ctx.IsVarDynamicArray(varName))
                {
                    primNode.Children.AddLast(GetLoadVariableNode(primNode, varName, fr1, ctx));
                }
                else
                {
                    primNode.Children.AddLast(GetLoadVariableAddressNode(primNode, varName, fr1, ctx));
                }
                primNode.Children.AddLast(new CodeNode("Array element address load", primNode)
                    .Add(GenerateADD(fr0, fr1)));

                if (!parent.Name.Equals("Assignment")) // Right value
                {
                    CodeNode fr2Node = GetFreeRegisterNode(aastNode, primNode);
                    byte fr2 = fr2Node.ByteToReturn;
                    primNode.Children.AddLast(fr2Node);
                    int mask = ctx.GetArrayOffsetSize(varName) == 4 ? -1 : (int)Math.Pow(256, ctx.GetArrayOffsetSize(varName)) - 1;  // 00 00 00 ff or 00 00 ff ff or ff ff ff ff
                    primNode.Children.AddLast(new CodeNode("Array element value load", primNode)
                        .Add(GenerateLDC(4 - ctx.GetArrayOffsetSize(varName), fr2))
                        .Add(GenerateSUB(fr2, fr1))
                        .Add(GenerateLD(fr1, fr1))
                        .Add(GenerateLDC(0, fr2))
                        .Add(GenerateLDA(fr2, fr2, mask))
                        .Add(GenerateAND(fr1, fr2)));
                    g.FreeReg(fr2);
                }

                g.FreeReg(fr0);
            }
            else // Calling a routine
            {
                // Load out to heap the first operand (due to recursion)
                if (parent.OperandByte != 255)
                {
                    primNode.Children.AddLast(GetHeapTopChangeNode(primNode, -4));
                    primNode.Children.AddLast(GetStoreToHeapNode(primNode, parent.OperandByte, 0));
                    primNode.Children.AddLast(g.codeConstructors["Call"].Construct(aastNode, primNode));
                    primNode.ByteToReturn = primNode.Children.Last.Value.ByteToReturn;
                    primNode.Children.AddLast(GetLoadFromHeapNode(primNode, parent.OperandByte, 0));
                    primNode.Children.AddLast(GetHeapTopChangeNode(primNode, 4));
                } 
                else
                {
                    primNode.Children.AddLast(g.codeConstructors["Call"].Construct(aastNode, primNode));
                    primNode.ByteToReturn = primNode.Children.Last.Value.ByteToReturn;
                }
            }

            return primNode;
        }
    }
}
