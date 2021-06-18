using ERACompiler.Structures;
using ERACompiler.Utilities.Errors;
using System.Collections.Generic;

namespace ERACompiler.Modules.Generation
{
    class AssemblyBlockConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            // Just identify registers and generate asm commands

            int format = 32; // Default format
            CodeNode asmBlockNode = new CodeNode(aastNode, parent);
            Dictionary<string, CodeNode> labels = new Dictionary<string, CodeNode>();
            Dictionary<string, CodeNode> labelDecls = new Dictionary<string, CodeNode>();
            foreach (AASTNode asmStmnt in aastNode.Children)
            {
                switch (asmStmnt.ASTType)
                {
                    case "< Identifier >":
                        {
                            CodeNode label = new CodeNode((AASTNode)asmStmnt.Children[1], asmBlockNode);
                            label.Name = "Label"; // These labels are resolved as any other labels in the code, in the Program Constructor
                            asmBlockNode.Children.AddLast(label);
                            labels.Add(asmStmnt.Children[1].Token.Value, label); 
                            break;
                        }
                    case "Register := Identifier":
                        {
                            CodeNode labelDecl = new CodeNode("Label declaration", asmBlockNode).Add(new byte[8]);
                            labelDecl.ByteToReturn = Generator.IdentifyRegister(asmStmnt.Children[0].Token.Value);
                            asmBlockNode.Children.AddLast(labelDecl);
                            labelDecls.Add(asmStmnt.Children[2].Token.Value, labelDecl);
                            break;
                        }
                    case "Register := Register":
                        {
                            byte fr0 = Generator.IdentifyRegister(asmStmnt.Children[0].Token.Value);
                            byte fr1 = Generator.IdentifyRegister(asmStmnt.Children[2].Token.Value);
                            asmBlockNode.Children.AddLast(new CodeNode(asmStmnt.ASTType, asmBlockNode)
                                .Add(GenerateMOV(fr1, fr0, format)));
                            break;
                        }
                    case "Register := Expression":
                        {
                            byte fr0 = Generator.IdentifyRegister(asmStmnt.Children[0].Token.Value);
                            int cnst = ((AASTNode)asmStmnt.Children[2].Children[0]).AASTValue;
                            if (cnst < 0 || cnst > 31)
                            {
                                throw new CompilationErrorException("Only constants in range 0-31 are allowed!!!\r\n" +
                                    "  At (Line: " + asmStmnt.Children[2].Children[0].Token.Position.Line + ", " +
                                    "Char: " + asmStmnt.Children[2].Children[0].Token.Position.Char + ").");
                            }
                            asmBlockNode.Children.AddLast(new CodeNode(asmStmnt.ASTType, asmBlockNode)
                                .Add(GenerateLDC(cnst, fr0)));
                            break;
                        }
                    case "format ( 8 | 16 | 32 )":
                        {
                            format = ((AASTNode)aastNode.Children[1]).AASTValue;
                            break;
                        }
                    case "Register := Register + Expression":
                        {
                            byte fr0 = Generator.IdentifyRegister(asmStmnt.Children[0].Token.Value);
                            byte fr1 = Generator.IdentifyRegister(asmStmnt.Children[2].Token.Value);
                            int cnst = ((AASTNode)asmStmnt.Children[4].Children[0]).AASTValue;
                            asmBlockNode.Children.AddLast(new CodeNode(asmStmnt.ASTType, asmBlockNode)
                                .Add(GenerateLDA(fr1, fr0, cnst)));
                            break;
                        }
                    case "Register := -> [ Register ]":
                        {
                            byte fr0 = Generator.IdentifyRegister(asmStmnt.Children[0].Token.Value);
                            byte fr1 = Generator.IdentifyRegister(asmStmnt.Children[4].Token.Value);
                            asmBlockNode.Children.AddLast(new CodeNode(asmStmnt.ASTType, asmBlockNode)
                                .Add(GenerateLD(fr1, fr0)));
                            break;
                        }
                    case "-> [ Register ] := Register":
                        {
                            byte fr0 = Generator.IdentifyRegister(asmStmnt.Children[2].Token.Value);
                            byte fr1 = Generator.IdentifyRegister(asmStmnt.Children[5].Token.Value);
                            asmBlockNode.Children.AddLast(new CodeNode(asmStmnt.ASTType, asmBlockNode)
                                .Add(GenerateST(fr1, fr0)));
                            break;
                        }
                    case "Register += Register":
                        {
                            byte fr0 = Generator.IdentifyRegister(asmStmnt.Children[0].Token.Value);
                            byte fr1 = Generator.IdentifyRegister(asmStmnt.Children[2].Token.Value);
                            asmBlockNode.Children.AddLast(new CodeNode(asmStmnt.ASTType, asmBlockNode)
                                .Add(GenerateADD(fr1, fr0, format)));
                            break;
                        }
                    case "Register -= Register":
                        {
                            byte fr0 = Generator.IdentifyRegister(asmStmnt.Children[0].Token.Value);
                            byte fr1 = Generator.IdentifyRegister(asmStmnt.Children[2].Token.Value);
                            asmBlockNode.Children.AddLast(new CodeNode(asmStmnt.ASTType, asmBlockNode)
                                .Add(GenerateSUB(fr1, fr0, format)));
                            break;
                        }
                    case "Register >>= Register":
                        {
                            byte fr0 = Generator.IdentifyRegister(asmStmnt.Children[0].Token.Value);
                            byte fr1 = Generator.IdentifyRegister(asmStmnt.Children[2].Token.Value);
                            asmBlockNode.Children.AddLast(new CodeNode(asmStmnt.ASTType, asmBlockNode)
                                .Add(GenerateASR(fr1, fr0, format)));
                            break;
                        }
                    case "Register <<= Register":
                        {
                            byte fr0 = Generator.IdentifyRegister(asmStmnt.Children[0].Token.Value);
                            byte fr1 = Generator.IdentifyRegister(asmStmnt.Children[2].Token.Value);
                            asmBlockNode.Children.AddLast(new CodeNode(asmStmnt.ASTType, asmBlockNode)
                                .Add(GenerateASL(fr1, fr0, format)));
                            break;
                        }
                    case "Register |= Register":
                        {
                            byte fr0 = Generator.IdentifyRegister(asmStmnt.Children[0].Token.Value);
                            byte fr1 = Generator.IdentifyRegister(asmStmnt.Children[2].Token.Value);
                            asmBlockNode.Children.AddLast(new CodeNode(asmStmnt.ASTType, asmBlockNode)
                                .Add(GenerateOR(fr1, fr0, format)));
                            break;
                        }
                    case "Register &= Register":
                        {
                            byte fr0 = Generator.IdentifyRegister(asmStmnt.Children[0].Token.Value);
                            byte fr1 = Generator.IdentifyRegister(asmStmnt.Children[2].Token.Value);
                            asmBlockNode.Children.AddLast(new CodeNode(asmStmnt.ASTType, asmBlockNode)
                                .Add(GenerateAND(fr1, fr0, format)));
                            break;
                        }
                    case "Register ^= Register":
                        {
                            byte fr0 = Generator.IdentifyRegister(asmStmnt.Children[0].Token.Value);
                            byte fr1 = Generator.IdentifyRegister(asmStmnt.Children[2].Token.Value);
                            asmBlockNode.Children.AddLast(new CodeNode(asmStmnt.ASTType, asmBlockNode)
                                .Add(GenerateXOR(fr1, fr0, format)));
                            break;
                        }
                    case "Register >= Register":
                        {
                            byte fr0 = Generator.IdentifyRegister(asmStmnt.Children[0].Token.Value);
                            byte fr1 = Generator.IdentifyRegister(asmStmnt.Children[2].Token.Value);
                            asmBlockNode.Children.AddLast(new CodeNode(asmStmnt.ASTType, asmBlockNode)
                                .Add(GenerateLSR(fr1, fr0, format)));
                            break;
                        }
                    case "Register <= Register":
                        {
                            byte fr0 = Generator.IdentifyRegister(asmStmnt.Children[0].Token.Value);
                            byte fr1 = Generator.IdentifyRegister(asmStmnt.Children[2].Token.Value);
                            asmBlockNode.Children.AddLast(new CodeNode(asmStmnt.ASTType, asmBlockNode)
                                .Add(GenerateLSL(fr1, fr0, format)));
                            break;
                        }
                    case "Register ?= Register":
                        {
                            byte fr0 = Generator.IdentifyRegister(asmStmnt.Children[0].Token.Value);
                            byte fr1 = Generator.IdentifyRegister(asmStmnt.Children[2].Token.Value);
                            asmBlockNode.Children.AddLast(new CodeNode(asmStmnt.ASTType, asmBlockNode)
                                .Add(GenerateCND(fr1, fr0, format)));
                            break;
                        }
                    case "if Register goto Register":
                        {
                            byte fr0 = Generator.IdentifyRegister(asmStmnt.Children[1].Token.Value);
                            byte fr1 = Generator.IdentifyRegister(asmStmnt.Children[3].Token.Value);
                            asmBlockNode.Children.AddLast(new CodeNode(asmStmnt.ASTType, asmBlockNode)
                                .Add(GenerateCBR(fr0, fr1)));
                            break;
                        }
                    default:
                        {
                            throw new CompilationErrorException("Unknown assembly command!!!");
                        }
                }
                format = 32;
            }
            foreach (CodeNode label in labels.Values)
            {
                label.LabelDecl = labelDecls[label.AASTLink.Token.Value]; // When 'asm' is constructed, we can resolve all labels and their declarations
            }
            return asmBlockNode;
        }
    }
}
