using ERACompiler.Structures;
using ERACompiler.Structures.Types;
using ERACompiler.Utilities.Errors;
using System;

namespace ERACompiler.Modules.Generation
{
    class AssignmentConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            Generator g = Program.currentCompiler.generator;
            CodeNode asgmntNode = new CodeNode(aastNode, parent);

            Context? ctx = SemanticAnalyzer.FindParentContext(aastNode)
                ?? throw new CompilationErrorException("No parent context found!!!\r\n  At line " + aastNode.Token.Position.Line);

            // Expression (right-value)
            CodeNode exprNode = base.Construct((AASTNode)aastNode.Children[1], asgmntNode);
            asgmntNode.Children.AddLast(exprNode);
            byte fr0 = exprNode.ByteToReturn;
            
            // Left-value to be assigned
            CodeNode receiverNode = base.Construct((AASTNode)aastNode.Children[0], asgmntNode);
            asgmntNode.Children.AddLast(receiverNode);
            byte fr1 = receiverNode.ByteToReturn;

            switch (aastNode.Children[0].ASTType)
            {
                case "Primary": // Array or variable (TODO: dot-notation for structures and modules)
                    {
                        AASTNode prim = (AASTNode)aastNode.Children[0];
                        if (prim.Children[^1].ASTType.Equals("IDENTIFIER"))
                        {
                            if (g.regAllocVTR.ContainsKey(prim.Children[^1].Token.Value))
                            {
                                asgmntNode.Children.AddLast(new CodeNode("Assignment mov", asgmntNode).Add(GenerateMOV(fr0, fr1)));
                                // We have to store new variable value to the stack every time to overcome some strange 'goto' artifacts that may occur.
                                // Yes, it is not as efficient, however we do not want to have unexpected or unobvious behavior of our ERA code.
                                asgmntNode.Children.AddLast(GetStoreVariableNode(asgmntNode, prim.Children[^1].Token.Value, fr0, ctx));
                            }
                            else
                            {
                                asgmntNode = ResolveTypes(asgmntNode, ctx, fr0, fr1, ctx.GetVarType(prim.Children[^1].Token).GetSize());
                            }
                        }
                        else if (prim.Children[^1].ASTType.Equals("Expression"))
                        {
                            asgmntNode = ResolveTypes(asgmntNode, ctx, fr0, fr1, ((ArrayType)ctx.GetVarType(prim.Children[^2].Token)).ElementType.GetSize());
                        }
                        else
                        {
                            throw new CompilationErrorException("Attempt to assign a routine call!!!\r\n" +
                                "At (Line: " + prim.Children[0].Token.Position.Line.ToString() + ", Char: " +
                                prim.Children[0].Token.Position.Char.ToString() + ").");
                        }
                        break;
                    }
                case "Dereference":
                    {
                        asgmntNode = ResolveTypes(asgmntNode, ctx, fr0, fr1, GetSizeOfExpressionAddressedVariable((AASTNode)aastNode.Children[0].Children[2]));
                        // Update all visible variable since we do not know what has been updated using dereference
                        foreach (string varName in ctx.GetAllVisibleVars())
                        {
                            if (g.regAllocVTR.ContainsKey(varName))
                            {
                                asgmntNode.Children.AddLast(GetLoadVariableNode(asgmntNode, varName, g.regAllocVTR[varName], ctx));
                            }
                        }
                        break;
                    }
                case "REGISTER":
                    {
                        asgmntNode.Children.AddLast(new CodeNode("Assignment mov", asgmntNode).Add(GenerateMOV(fr0, fr1)));
                        break;
                    }
            }

            g.FreeReg(fr0);
            g.FreeReg(fr1);

            return asgmntNode;
        }

        /// <summary>
        /// Resolves types since ST asm commands always rewrites 4 bytes on the stack, 
        /// however, in case of 'short' or 'byte' we do not want that.
        /// </summary>
        private CodeNode ResolveTypes(CodeNode node, Context? ctx, byte fr0, byte fr1, int bytesToStore)
        {
            Generator g = Program.currentCompiler.generator;
            int mask = bytesToStore == 4 ? 0 : ((int)Math.Pow(256, 4) - 1) << (8 * bytesToStore); // ff ff ff 00 or ff ff 00 00 or 00 00 00 00
            int mask2 = bytesToStore == 4 ? -1 : (int)Math.Pow(256, bytesToStore) - 1; // 00 00 00 ff or 00 00 ff ff or ff ff ff ff
            CodeNode fr2Node = GetFreeRegisterNode(ctx, node);
            byte fr2 = fr2Node.ByteToReturn;
            node.Children.AddLast(fr2Node);
            CodeNode fr3Node = GetFreeRegisterNode(ctx, node);
            byte fr3 = fr3Node.ByteToReturn;
            node.Children.AddLast(fr3Node);
            node.Children.AddLast(new CodeNode("asgmnt store cmds 1", node)
                .Add(GenerateLDC(4 - bytesToStore, fr2))
                .Add(GenerateSUB(fr2, fr1))
                .Add(GenerateLD(fr1, fr2))
                .Add(GenerateLDC(0, fr3))
                .Add(GenerateLDA(fr3, fr3, mask))
                .Add(GenerateAND(fr3, fr2))
                .Add(GenerateLDC(0, fr3))
                .Add(GenerateLDA(fr3, fr3, mask2))
                .Add(GenerateAND(fr3, fr0))
                .Add(GenerateOR(fr2, fr0))
                .Add(GenerateST(fr0, fr1)));
            g.FreeReg(fr2);
            g.FreeReg(fr3);
            return node;
        }
    }
}
