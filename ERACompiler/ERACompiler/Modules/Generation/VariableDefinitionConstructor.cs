using ERACompiler.Structures;
using ERACompiler.Structures.Types;
using ERACompiler.Utilities.Errors;

namespace ERACompiler.Modules.Generation
{
    class VariableDefinitionConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            Generator g = Program.currentCompiler.generator;
            CodeNode varDefNode = new CodeNode(aastNode, parent);
            Context? ctx = SemanticAnalyzer.FindParentContext(aastNode)
                ?? throw new CompilationErrorException("No parent context found!!!\r\n  At line " + aastNode.Token.Position.Line);

            switch (aastNode.AASTType.Type)
            {
                case VarType.ERAType.INT:
                case VarType.ERAType.SHORT:
                case VarType.ERAType.BYTE:
                case VarType.ERAType.INT_ADDR:
                case VarType.ERAType.SHORT_ADDR:
                case VarType.ERAType.BYTE_ADDR:
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
