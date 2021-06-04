using ERACompiler.Structures;
using ERACompiler.Structures.Types;

namespace ERACompiler.Modules.Generation
{
    class VariableDefinitionConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            Generator g = Program.currentCompiler.generator;
            CodeNode varDefNode = new CodeNode(aastNode, parent);
            Context? ctx = SemanticAnalyzer.FindParentContext(aastNode);

            byte format = 0xc0;
            switch (aastNode.AASTType.Type)
            {
                case VarType.ERAType.INT:
                    format = 0xc0;
                    break;
                case VarType.ERAType.SHORT:
                    format = 0x40;
                    break;
                case VarType.ERAType.BYTE:
                    format = 0x00;
                    break;
            }

            switch (aastNode.AASTType.Type)
            {
                case VarType.ERAType.INT:
                case VarType.ERAType.INT_ADDR:
                case VarType.ERAType.SHORT:
                case VarType.ERAType.BYTE:
                    {
                        // If we have initial assignment - store it to register/memory
                        if (aastNode.Children.Count > 0)
                        {
                            CodeNode exprNode = base.Construct((AASTNode)aastNode.Children[0], varDefNode);
                            varDefNode.Children.AddLast(exprNode);
                            byte fr0 = exprNode.ByteToReturn;

                            if (g.regAllocVTR.ContainsKey(aastNode.Token.Value))
                            {                                
                                byte reg = g.regAllocVTR[aastNode.Token.Value];                                
                                varDefNode.Children.AddLast(new CodeNode("VarDef MOV", varDefNode).Add(GenerateMOV(fr0, reg)));
                            }
                            varDefNode.Children.AddLast(GetStoreVariableNode(varDefNode, aastNode.Token.Value, fr0, ctx));

                            g.FreeReg(fr0);
                        }
                        break;
                    }
                default:
                    break;
            }

            return varDefNode;
        }
    }
}
