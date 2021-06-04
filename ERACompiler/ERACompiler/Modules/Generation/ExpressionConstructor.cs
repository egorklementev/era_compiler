using ERACompiler.Structures;

namespace ERACompiler.Modules.Generation
{
    class ExpressionConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            Generator g = Program.currentCompiler.generator;
            CodeNode exprNode = new CodeNode(aastNode, parent);

            // 1) Store result of the left operand in FR0
            CodeNode firstOpNode = base.Construct((AASTNode)aastNode.Children[0], exprNode);
            byte fr0 = firstOpNode.ByteToReturn;
            exprNode.Children.AddLast(firstOpNode);
            exprNode.ByteToReturn = fr0;

            if (aastNode.Children.Count > 1)
            {
                exprNode.OperandByte = fr0; // Update operand register

                // 2) Store result of the right operand in FR0/FR1
                CodeNode secondOpNode = base.Construct((AASTNode)aastNode.Children[2], exprNode);
                byte fr1 = secondOpNode.ByteToReturn;                        
                exprNode.Children.AddLast(secondOpNode);

                // 3) Generate a code of the operation itself using these two register
                //    and put an extra byte indicating the register with the result 
                //    (this byte is being removed upper in the call stack)
                string op = aastNode.Children[1].Token.Value;
                CodeNode operationNode = new CodeNode("Operation \'" + op + "\'", exprNode);
                switch (op)
                {
                    // ATTENTION: What about command "format"? I use 32 everywhere.
                    case "+":
                        {
                            // FR0 += FR1; FR0  # In this case order does not matter                        
                            operationNode.Add(GenerateADD(fr1, fr0));
                            exprNode.ByteToReturn = fr0;
                            break;
                        }
                    case "-":
                        {
                            // FR0 -= FR1; FR0
                            operationNode.Add(GenerateSUB(fr1, fr0));
                            exprNode.ByteToReturn = fr0;
                            break;
                        }
                    case ">=":
                        {
                            // FR2 := 1;
                            // FR3 := <lsr>;
                            // <lsr>
                            // FR0 >= FR0;
                            // FR1 -= FR2;
                            // if FR1 goto FR3;
                            // fr0
                            CodeNode fr2Node = GetFreeRegisterNode(aastNode, operationNode);
                            operationNode.Children.AddLast(fr2Node);
                            byte fr2 = fr2Node.ByteToReturn;
                            CodeNode fr3Node = GetFreeRegisterNode(aastNode, operationNode);
                            operationNode.Children.AddLast(fr3Node);
                            byte fr3 = fr3Node.ByteToReturn;
                            CodeNode lsrNode = new CodeNode("LSR", operationNode)
                                .Add(GenerateLDC(1, fr2));
                            lsrNode
                                .Add(GenerateLDL(fr3, GetCurrentBinarySize(lsrNode)))
                                .Add(GenerateLSR(fr0, fr0))
                                .Add(GenerateSUB(fr2, fr1))
                                .Add(GenerateCBR(fr1, fr3));
                            operationNode.Children.AddLast(lsrNode);

                            exprNode.ByteToReturn = fr0;
                            g.FreeReg(fr2);
                            g.FreeReg(fr3);
                            break;
                        }
                    case "<=":
                        {
                            // FR2 := 1;
                            // FR3 := <lsr>;
                            // <lsr>
                            // FR0 >= FR0;
                            // FR1 -= FR2;
                            // if FR1 goto FR3;
                            // fr0
                            CodeNode fr2Node = GetFreeRegisterNode(aastNode, operationNode);
                            operationNode.Children.AddLast(fr2Node);
                            byte fr2 = fr2Node.ByteToReturn;
                            CodeNode fr3Node = GetFreeRegisterNode(aastNode, operationNode);
                            operationNode.Children.AddLast(fr3Node);
                            byte fr3 = fr3Node.ByteToReturn;
                            CodeNode lslNode = new CodeNode("LSL", operationNode)
                                .Add(GenerateLDC(1, fr2));
                            lslNode
                                .Add(GenerateLDL(fr3, GetCurrentBinarySize(lslNode)))
                                .Add(GenerateLSL(fr0, fr0))
                                .Add(GenerateSUB(fr2, fr1))
                                .Add(GenerateCBR(fr1, fr3));
                            operationNode.Children.AddLast(lslNode);

                            exprNode.ByteToReturn = fr0;
                            g.FreeReg(fr2);
                            g.FreeReg(fr3);
                            break;
                        }
                    case "&":
                        {
                            // FR0 &= FR1; FR0
                            operationNode.Add(GenerateAND(fr1, fr0));
                            exprNode.ByteToReturn = fr0;
                            break;
                        }
                    case "|":
                        {
                            // FR0 |= FR1; FR0
                            operationNode.Add(GenerateOR(fr1, fr0));
                            exprNode.ByteToReturn = fr0;
                            break;
                        }
                    case "^":
                        {
                            // FR0 ^= FR1; FR0
                            operationNode.Add(GenerateXOR(fr1, fr0));
                            exprNode.ByteToReturn = fr0;
                            break;
                        }
                    case "?":
                        {
                            // FR0 ?= FR1; FR0
                            operationNode.Add(GenerateCND(fr1, fr0));
                            exprNode.ByteToReturn = fr0;
                            break;
                        }
                    case "=":
                    case "/=":
                    case ">":
                    case "<":
                        {
                            int mask = op.Equals("=") ? 4 : op.Equals("/=") ? 3 : op.Equals(">") ? 1 : op.Equals("<") ? 2 : 7;
                            CodeNode fr2Node = GetFreeRegisterNode(aastNode, operationNode);
                            operationNode.Children.AddLast(fr2Node);
                            byte fr2 = fr2Node.ByteToReturn;
                            // FR2 := mask;
                            // FR1 ?= FR0;
                            // FR2 &= FR1;
                            // FR0 = 1;
                            // FR1 = <true>;
                            // if FR2 goto FR0;
                            // FR0 := 0;
                            // <true>
                            // fr0
                            operationNode.Children.AddLast(new CodeNode("PreLabel", operationNode)
                                .Add(GenerateLDC(mask, fr2))
                                .Add(GenerateCND(fr0, fr1))
                                .Add(GenerateAND(fr1, fr2))
                                .Add(GenerateLDC(1, fr0)));
                            CodeNode labelNode = new CodeNode("fr1 label", operationNode);
                            operationNode.Children.AddLast(labelNode);
                            operationNode.Children.AddLast(new CodeNode("PostLabel", operationNode)
                                .Add(GenerateCBR(fr2, fr1))
                                .Add(GenerateLDC(0, fr0)));
                            labelNode.Add(GenerateLDL(fr1, GetCurrentBinarySize(labelNode)));
                            exprNode.ByteToReturn = fr0;
                            g.FreeReg(fr2);
                            break;
                        }
                    case "*":
                        {
                            // WHAT A MONSTROSITY!
                            // -------------------
                            // FR2 := 0;
                            // FR2 := FR2 + 32;
                            // FR3 := mult (27);
                            // FR4 := add (27);
                            // FR5 := not_add (21);
                            // FR6 := 1; # Mask
                            // FR7 := 0; # For result                            
                            // FR8 := 1; # For iteration
                            // <mult>
                            // FR9 := 0; # For loop exit
                            // FR6 &= FR1;                            
                            // if FR6 goto FR4;
                            // if FR8 goto FR5;
                            // <add>
                            // FR7 += FR0;
                            // <not_add>
                            // FR6 := 1;                            
                            // FR8 := 1;                            
                            // FR0 <= FR0;
                            // FR1 >= FR1;
                            // FR2 -= FR8;
                            // FR9 ?= FR2;
                            // FR9 &= FR8;
                            // if FR9 goto FR3;
                            // FR0 := FR7;
                            // fr0

                            CodeNode fr2Node = GetFreeRegisterNode(aastNode, operationNode);
                            operationNode.Children.AddLast(fr2Node);
                            byte fr2 = fr2Node.ByteToReturn;

                            CodeNode fr3Node = GetFreeRegisterNode(aastNode, operationNode);
                            operationNode.Children.AddLast(fr3Node);
                            byte fr3 = fr3Node.ByteToReturn;

                            CodeNode fr4Node = GetFreeRegisterNode(aastNode, operationNode);
                            operationNode.Children.AddLast(fr4Node);
                            byte fr4 = fr4Node.ByteToReturn;

                            CodeNode fr5Node = GetFreeRegisterNode(aastNode, operationNode);
                            operationNode.Children.AddLast(fr5Node);
                            byte fr5 = fr5Node.ByteToReturn;

                            CodeNode fr6Node = GetFreeRegisterNode(aastNode, operationNode);
                            operationNode.Children.AddLast(fr6Node);
                            byte fr6 = fr6Node.ByteToReturn;

                            CodeNode fr7Node = GetFreeRegisterNode(aastNode, operationNode);
                            operationNode.Children.AddLast(fr7Node);
                            byte fr7 = fr7Node.ByteToReturn;

                            CodeNode fr8Node = GetFreeRegisterNode(aastNode, operationNode);
                            operationNode.Children.AddLast(fr8Node);
                            byte fr8 = fr8Node.ByteToReturn;

                            CodeNode fr9Node = GetFreeRegisterNode(aastNode, operationNode);
                            operationNode.Children.AddLast(fr9Node);
                            byte fr9 = fr9Node.ByteToReturn;

                            operationNode.Children.AddLast(new CodeNode("mult 1", operationNode)
                                .Add(GenerateLDC(0, fr2))
                                .Add(GenerateLDA(fr2, fr2, 32)));

                            CodeNode fr3LabelNode = new CodeNode("fr3 label", operationNode);
                            CodeNode fr4LabelNode = new CodeNode("fr4 label", operationNode);
                            CodeNode fr5LabelNode = new CodeNode("fr5 label", operationNode);
                            operationNode.Children.AddLast(fr3LabelNode.Add(new byte[8]));
                            operationNode.Children.AddLast(fr4LabelNode.Add(new byte[8]));
                            operationNode.Children.AddLast(fr5LabelNode.Add(new byte[8]));

                            operationNode.Children.AddLast(new CodeNode("mult 2", operationNode)
                                .Add(GenerateLDC(1, fr6))
                                .Add(GenerateLDC(0, fr7))
                                .Add(GenerateLDC(1, fr8)));

                            fr3LabelNode.Bytes.Clear();
                            fr3LabelNode.Add(GenerateLDL(fr3, GetCurrentBinarySize(fr3LabelNode)));

                            operationNode.Children.AddLast(new CodeNode("mult 3", operationNode)
                                .Add(GenerateLDC(0, fr9))
                                .Add(GenerateAND(fr1, fr6))
                                .Add(GenerateCBR(fr6, fr4))
                                .Add(GenerateCBR(fr8, fr5)));

                            fr4LabelNode.Bytes.Clear();
                            fr4LabelNode.Add(GenerateLDL(fr4, GetCurrentBinarySize(fr4LabelNode)));

                            operationNode.Children.AddLast(new CodeNode("mult 4", operationNode)
                                .Add(GenerateADD(fr0, fr7)));

                            fr5LabelNode.Bytes.Clear();
                            fr5LabelNode.Add(GenerateLDL(fr5, GetCurrentBinarySize(fr5LabelNode)));

                            operationNode.Children.AddLast(new CodeNode("mult 5", operationNode)
                                .Add(GenerateLDC(1, fr6))
                                .Add(GenerateLDC(1, fr8))
                                .Add(GenerateLSL(fr0, fr0))
                                .Add(GenerateLSR(fr1, fr1))
                                .Add(GenerateSUB(fr8, fr2))
                                .Add(GenerateCND(fr2, fr9))
                                .Add(GenerateAND(fr8, fr9))
                                .Add(GenerateCBR(fr9, fr3))
                                .Add(GenerateMOV(fr7, fr0)));

                            exprNode.ByteToReturn = fr0;
                            g.FreeReg(fr2);
                            g.FreeReg(fr3);
                            g.FreeReg(fr4);
                            g.FreeReg(fr5);
                            g.FreeReg(fr6);
                            g.FreeReg(fr7);
                            g.FreeReg(fr8);
                            g.FreeReg(fr9);
                            break;
                        }
                    default:
                        break;
                }
                exprNode.Children.AddLast(operationNode);

                // 4) Free the other register (the result register is freed upper in the call stack)
                g.FreeReg(fr1);
            }

            return exprNode;
        }
    }
}
