using ERACompiler.Structures;
using ERACompiler.Utilities.Errors;

namespace ERACompiler.Modules.Generation
{
    class AssignmentConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            Generator g = Program.currentCompiler.generator;
            CodeNode asgmntNode = new CodeNode(aastNode, parent);

            Context? ctx = SemanticAnalyzer.FindParentContext(aastNode);

            CodeNode exprNode = base.Construct((AASTNode)aastNode.Children[1], asgmntNode);
            asgmntNode.Children.AddLast(exprNode);
            byte fr0 = exprNode.ByteToReturn;
            
            CodeNode receiverNode = base.Construct((AASTNode)aastNode.Children[0], asgmntNode);
            asgmntNode.Children.AddLast(receiverNode);
            byte fr1 = receiverNode.ByteToReturn;

            switch (aastNode.Children[0].ASTType)
            {
                case "Primary": // Array or variable (TODO: dot-notation)
                    {
                        if (aastNode.Children[0].Children[^1].ASTType.Equals("IDENTIFIER"))
                        {
                            if (g.regAllocVTR.ContainsKey(aastNode.Children[0].Children[^1].Token.Value))
                            {
                                asgmntNode.Children.AddLast(new CodeNode("Assignment mov", asgmntNode).Add(GenerateMOV(fr0, fr1)));
                            }
                            else
                            {
                                asgmntNode.Children.AddLast(new CodeNode("Assignment store", asgmntNode).Add(GenerateST(fr0, fr1)));
                            }
                        }
                        else if (aastNode.Children[0].Children[^1].ASTType.Equals("Expression"))
                        {
                            asgmntNode.Children.AddLast(new CodeNode("Assignment store", asgmntNode).Add(GenerateST(fr0, fr1)));
                        }
                        else
                        {
                            throw new CompilationErrorException("Attempt to assign a routine call!!!\r\n" +
                                "At (Line: " + aastNode.Children[0].Children[0].Token.Position.Line.ToString() + ", Char: " +
                                aastNode.Children[0].Children[0].Token.Position.Char.ToString() + ").");
                        }
                        break;
                    }
                case "Dereference":
                    {
                        asgmntNode.Children.AddLast(new CodeNode("Assignment store", asgmntNode).Add(GenerateST(fr0, fr1)));
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
    }
}
