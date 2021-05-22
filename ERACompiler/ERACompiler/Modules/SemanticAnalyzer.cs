using ERACompiler.Structures;
using ERACompiler.Structures.Types;
using ERACompiler.Utilities;
using ERACompiler.Utilities.Errors;
using System;
using System.Collections.Generic;

namespace ERACompiler.Modules
{
    class SemanticAnalyzer
    {
        private readonly VarType no_type = new VarType(VarType.ERAType.NO_TYPE); // Placeholder
        private int blockBodyCounter = 0;
        private AASTNode varToAddToCtx = null; // For for loops

        public AASTNode BuildAAST(ASTNode ASTRoot)
        {
            AASTNode program = AnnotateNode(ASTRoot, null);

            // Additional checks
            PostChecks(program, program.Context);

            // The "code" segment should be present in a program
            if (!program.Context.IsVarDeclared(new Token(TokenType.KEYWORD, "code", new TokenPosition(0, 0))))
            {
                throw new SemanticErrorException(
                    "No \"code\" block found in the program!!!"
                    );
            }
            return program;
        }

        private AASTNode AnnotateNode(ASTNode node, AASTNode parent)
        {
            switch (node.ASTType)
            {
                case "Program":
                    return AnnotateProgram(node, parent);
                case "Annotations":
                    return AnnotateAnnotations(node, parent);
                case "Data":
                    return AnnotateData(node, parent);
                case "Structure":
                    return AnnotateStruct(node, parent);
                case "Routine":
                    return AnnotateRoutine(node, parent);
                case "Routine body":
                    return AnnotateRoutineBody(node, parent);
                case "Assembly block":
                    return AnnotateAssemblyBlock(node, parent);
                case "Expression":
                    return AnnotateExpr(node, parent);
                case "Assignment":
                    return AnnotateAssignment(node, parent);
                case "Call arguments":
                    return AnnotateCallArgs(node, parent);
                case "NUMBER":
                    return AnnotateLiteral(node, parent);
                case "Primary":
                    return AnnotatePrimary(node, parent);
                case "( Expression )":
                    return AnnotateNode(node.Children[1], parent);
                case "Variable declaration":
                    return AnnotateVarDeclaration(node, parent);
                case "Code":
                    return AnnotateCodeBlock(node, parent);
                case "Module":
                    return AnnotateModule(node, parent);
                case "Call":
                    return AnnotateCall(node, parent);
                case "Swap":
                    return AnnotateSwap(node, parent);
                case "If":
                    return AnnotateIf(node, parent);
                case "Block body":
                    return AnnotateBlockBody(node, parent);
                case "For":
                    return AnnotateFor(node, parent);
                case "While":
                    return AnnotateWhile(node, parent);
                case "Loop While":
                    return AnnotateLoopWhile(node, parent);
                case "Loop body":
                    return AnnotateLoopBody(node, parent);
                case "Break":
                    return AnnotateBreak(node, parent);
                case "Return":
                    return AnnotateReturn(node, parent);
                case "Statement":
                    return AnnotateStatement(node, parent);
                case "Goto":
                    return AnnotateGoto(node, parent);
                case "KEYWORD":
                case "OPERATOR":
                case "REGISTER":
                case "DELIMITER":
                case "IDENTIFIER":
                    return new AASTNode(node, parent, no_type);
                case "Loop":
                case "Call ;":
                case "Operand":
                case "Operator":
                case "Receiver":
                case "Some unit":
                case "Primary | Register":
                case "Extension statement":
                case "Some module statement":
                case "Identifier | Register":
                case "VarDeclaration | Statement":
                    return AnnotateNode(node.Children[0], parent);
                case "Print":
                case "Reference":
                case "Dereference":
                case "Explicit address":
                    return AnnotatePass(node, parent);
                default:
                    return new AASTNode(node, null, no_type);
            }
         }

        private AASTNode AnnotateGoto(ASTNode node, AASTNode parent)
        {
            AASTNode gotoNode = new AASTNode(node, parent, no_type);
            gotoNode.Children.Add(AnnotateNode(node.Children[1], gotoNode));
            return gotoNode;
        }

        private AASTNode AnnotateLoopWhile(ASTNode node, AASTNode parent)
        {
            AASTNode loopWhileNode = new AASTNode(node, parent, no_type);
            loopWhileNode.Children.Add(AnnotateNode(node.Children[0], loopWhileNode));
            loopWhileNode.Children.Add(AnnotateExpr(node.Children[2], loopWhileNode));
            return loopWhileNode;
        }

        private AASTNode AnnotateStatement(ASTNode node, AASTNode parent)
        {
            AASTNode stmnt = new AASTNode(node, parent, no_type);

            // Label if any
            if (node.Children[0].Children.Count > 0)
            {
                stmnt.Children.Add(AnnotateLabel(node.Children[0].Children[0], stmnt));
            }

            // The statement itself
            stmnt.Children.Add(AnnotateNode(node.Children[1].Children[0], stmnt));

            return stmnt;
        }

        private ASTNode AnnotateLabel(ASTNode node, AASTNode parent)
        {
            AASTNode label = new AASTNode(node, parent, new VarType(VarType.ERAType.LABEL));

            // Add label to the context
            Context ctx = FindParentContext(parent);
            ctx.AddVar(label, node.Children[1].Token.Value);

            // Put identifier
            label.Children.Add(AnnotateNode(node.Children[1], label));

            return label;
        }

        private AASTNode AnnotateReturn(ASTNode node, AASTNode parent)
        {
            AASTNode returnNode = new AASTNode(node, parent, no_type);
            // Check if "return" is in right place - inside the routine body
            ASTNode? parentCopy = returnNode.Parent;
            while (parentCopy != null)
            {
                if (parentCopy.ASTType.Equals("Routine body"))
                {
                    // Perform the rest of checks
                    if (node.Children[1].Children[0].Children.Count > 0) // If it has something to return
                    {
                        if (node.Children[1].Children[0].Children[0].ASTType.Equals("Call"))
                        {
                            if (FindParentContext(returnNode)
                                .GetRoutineReturnType(
                                node.Children[1].Children[0].Children[0].Children[0].Token).Type == VarType.ERAType.NO_TYPE
                                )
                            {
                                throw new SemanticErrorException(
                                    "Attempt to return NO TYPE routine!!!\r\n" +
                                    "  At (Line: " + node.Children[0].Token.Position.Line.ToString() + ", " +
                                    "Char: " + node.Children[0].Token.Position.Char.ToString() + ")."
                                    );

                            } 
                            returnNode.Children.Add(AnnotateCall(node.Children[1].Children[0].Children[0], returnNode));
                        }
                        else
                        {
                            returnNode.Children.Add(AnnotateExpr(node.Children[1].Children[0].Children[0].Children[0], returnNode));
                        }
                    }
                    return returnNode;
                }
                parentCopy = parentCopy.Parent;
            }
            throw new SemanticErrorException(
                "Return is not in place!!!\r\n" +
                "  At (Line: " + node.Children[0].Token.Position.Line.ToString() + ", " +
                "Char: " + node.Children[0].Token.Position.Char.ToString() + ")."
                );
        }

        private AASTNode AnnotateBreak(ASTNode node, AASTNode parent)
        {
            AASTNode breakNode = new AASTNode(node, parent, no_type);
            // Check if "break" is in the right place - inside the loop body
            ASTNode? parentCopy = breakNode.Parent;
            while (parentCopy != null)
            {
                if (parentCopy.ASTType.Equals("Loop body")) return breakNode;
                parentCopy = parentCopy.Parent;
            }
            throw new SemanticErrorException(
                "Break is not in place!!!\r\n" +
                "  At (Line: " + node.Children[0].Token.Position.Line.ToString() + ", " +
                "Char: " + node.Children[0].Token.Position.Char.ToString() + ")."
                );
        }

        private AASTNode AnnotateLoopBody(ASTNode node, AASTNode parent)
        {
            AASTNode loop = new AASTNode(node, parent, no_type);
            loop.Children.Add(AnnotateNode(node.Children[1], loop));
            return loop;
        }

        private AASTNode AnnotateWhile(ASTNode node, AASTNode parent)
        {
            AASTNode whileNode = new AASTNode(node, parent, no_type);
            //CheckVariablesForExistance(node.Children[1], FindParentContext(parent));
            whileNode.Children.Add(AnnotateExpr(node.Children[1], whileNode));
            whileNode.Children.Add(AnnotateNode(node.Children[2], whileNode));
            return whileNode;
        }

        private AASTNode AnnotateFor(ASTNode node, AASTNode parent)
        {
            AASTNode forNode = new AASTNode(node, parent, no_type);
            forNode.Children.Add(AnnotateNode(node.Children[1], forNode));
            ((AASTNode)forNode.Children[0]).AASTType = new VarType(VarType.ERAType.INT);
            ((AASTNode)forNode.Children[0]).LIStart = 1;
            varToAddToCtx = (AASTNode) forNode.Children[0];
            // If 'from' expression exists
            if (node.Children[2].Children.Count > 0)
            {
                forNode.Children.Add(AnnotateExpr(node.Children[2].Children[1], forNode));
                ((AASTNode)forNode.Children[^1]).AASTValue = 1; // For generator
            }
            // If 'to' expression exists
            if (node.Children[3].Children.Count > 0)
            {
                forNode.Children.Add(AnnotateExpr(node.Children[3].Children[1], forNode));
                ((AASTNode)forNode.Children[^1]).AASTValue = 2; // For generator
            }
            // If 'step' expression exists
            if (node.Children[4].Children.Count > 0)
            {
                forNode.Children.Add(AnnotateExpr(node.Children[4].Children[1], forNode));
                ((AASTNode)forNode.Children[^1]).AASTValue = 3; // For generator
            }
            forNode.Children.Add(AnnotateNode(node.Children[5], forNode)); // Loop body
            return forNode;
        }

        private AASTNode AnnotateIf(ASTNode node, AASTNode parent)
        {
            AASTNode ifNode = new AASTNode(node, parent, no_type);
            //CheckVariablesForExistance(node.Children[1], FindParentContext(parent));
            ifNode.Children.Add(AnnotateExpr(node.Children[1], ifNode));
            ifNode.Children.Add(AnnotateBlockBody(node.Children[3], ifNode)); // If true
            // Annotate else block if any
            if (node.Children[4].Children[0].Children.Count > 0)
            {
                ifNode.Children.Add(AnnotateBlockBody(node.Children[4].Children[0].Children[1], ifNode));
            }
            return ifNode;
        }

        private AASTNode AnnotateBlockBody(ASTNode node, AASTNode parent)
        {
            AASTNode bb = new AASTNode(node, parent, no_type)
            {
                Context = new Context("BlockBody_" + (++blockBodyCounter).ToString(), FindParentContext(parent))
            };
            bool setLIEnd = false;
            string varName = "";
            if (varToAddToCtx != null)
            {
                bb.Context.AddVar(varToAddToCtx, varToAddToCtx.Token.Value);
                varName = varToAddToCtx.Token.Value;
                setLIEnd = true;
                varToAddToCtx = null;
            }
            foreach (ASTNode child in node.Children)
            {
                bb.Children.Add(AnnotateNode(child, bb));
            }
            if (setLIEnd)
            {                
                bb.Context.SetLIEnd(varName, GetMaxDepth(bb));
            }
            return bb;
        }

        private AASTNode AnnotateSwap(ASTNode node, AASTNode parent)
        {
            //CheckVariablesForExistance(node, FindParentContext(parent));
            AASTNode swap = new AASTNode(node, parent, no_type);
            // TODO: perform type check
            foreach (ASTNode child in node.Children)
            {
                swap.Children.Add(AnnotateNode(child, swap));
            }
            return swap;
        }

        private AASTNode AnnotateCall(ASTNode node, AASTNode parent)
        {
            AASTNode call = new AASTNode(node, parent, no_type);
            call.Children.Add(AnnotateNode(node.Children[0], call));
            call.Children.Add(AnnotateNode(node.Children[1], call));
            return call;
        }

        private AASTNode AnnotateCallArgs(ASTNode node, AASTNode parent)
        {
            AASTNode callArgs = new AASTNode(node, parent, no_type);
            if (node.Children[1].Children.Count > 0) // If some arguments exist
            {
                // First expression
                callArgs.Children.Add(AnnotateExpr(node.Children[1].Children[0], callArgs));
                // The rest of expressions if any
                if (node.Children[1].Children[1].Children.Count > 0)
                {
                    foreach (ASTNode child in node.Children[1].Children[1].Children)
                    {
                        if (child.ASTType.Equals("Expression")) // Skip comma
                        {
                            callArgs.Children.Add(AnnotateExpr(child, callArgs));
                        }
                    }
                }
            }
            return callArgs;
        }

        private AASTNode AnnotatePass(ASTNode node, AASTNode parent)
        {
            AASTNode nodeToPass = new AASTNode(node, parent, no_type);
            foreach (ASTNode child in node.Children)
            {
                nodeToPass.Children.Add(AnnotateNode(child, nodeToPass));
            }
            return nodeToPass;
        }

        private AASTNode AnnotateAssignment(ASTNode node, AASTNode parent)
        {
            AASTNode asgnmt = new AASTNode(node, parent, no_type);
            // TODO: check for type accordance (debatable)
            // TODO: dot-notation here
            if (node.Children[0].Children[0].ASTType.Equals("Primary") && FindParentContext(asgnmt).IsVarConstant(node.Children[0].Children[0].Token))
            {
                Token id = node.Children[0].Children[0].Token;
                throw new SemanticErrorException("Attempt to modify a constant!!!\n" +
                    "  At(Line: " + id.Position.Line + ", Char: " + id.Position.Char + ").");
            }
            asgnmt.Children.Add(AnnotateNode(node.Children[0], asgnmt)); // Receiver
            asgnmt.Children.Add(AnnotateExpr(node.Children[2], asgnmt)); // Expression
            return asgnmt;
        }

        private AASTNode AnnotateAssemblyBlock(ASTNode node, AASTNode parent)
        {
            AASTNode asmBlock = new AASTNode(node, parent, no_type);
            asmBlock.Children.Add(AnnotateAssemblyStatement(node.Children[1].Children[0], asmBlock)); // First child
            foreach (ASTNode child in node.Children[3].Children)
            {
                // Something wrong here...
                if (child.ASTType.Equals("Assembly statement"))
                {
                    asmBlock.Children.Add(AnnotateAssemblyStatement(child.Children[0], asmBlock));
                }
            }
            return asmBlock;
        }

        private AASTNode AnnotateAssemblyStatement(ASTNode node, AASTNode parent)
        {
            AASTNode asmStmnt = new AASTNode(node, parent, no_type);
            if (node.ASTType.Equals("format ( 8 | 16 | 32 )"))                
            {
                int frmt = int.Parse(node.Children[1].Token.Value);
                if (!(frmt == 8 || frmt == 16 || frmt == 32))
                {
                    throw new SemanticErrorException(
                        "Incorrect format at assembly block!!!\r\n" +
                        "  At (Line: " + node.Children[1].Token.Position.Line.ToString() +
                        ", Char: " + node.Children[1].Token.Position.Char.ToString() + ")."
                        );
                }                
            }
            if (node.ASTType.Equals("Register := Expression"))
            {
                // Check if expression is constant
                Context ctx = FindParentContext(parent);
                if (!IsExprConstant(node.Children[2], ctx))
                {
                    throw new SemanticErrorException(
                        "This expression should be constant (refer to the documentation)!!!\r\n" +
                        "  At (Line: " + node.Children[2].Token.Position.Line.ToString() +
                        ", Char: " + node.Children[2].Token.Position.Char.ToString() + ")."
                        );
                }
            }
            else if (node.ASTType.Equals("Register := Register + Expression"))
            {
                // Check if expression is constant
                Context ctx = FindParentContext(parent);
                if (!IsExprConstant(node.Children[4], ctx))
                {
                    throw new SemanticErrorException(
                        "This expression should be constant (refer to the documentation)!!!\r\n" +
                        "  At (Line: " + node.Children[4].Token.Position.Line.ToString() +
                        ", Char: " + node.Children[4].Token.Position.Char.ToString() + ")."
                        );
                }
            }
            else if (node.ASTType.Equals("Register := Identifier"))
            {
                // Check if identifier is label
                Context ctx = FindParentContext(parent);
                if (!ctx.IsVarDeclared(node.Children[2].Token))
                {
                    throw new SemanticErrorException(
                        "Label is not declared!!!\r\n" +
                        "  At (Line: " + node.Children[2].Token.Position.Line.ToString() +
                        ", Char: " + node.Children[2].Token.Position.Char.ToString() + ")."
                        );
                }
                if (!ctx.IsVarLabel(node.Children[2].Token))
                {
                    throw new SemanticErrorException(
                        "Label expected!!!\r\n" +
                        "  At (Line: " + node.Children[2].Token.Position.Line.ToString() +
                        ", Char: " + node.Children[2].Token.Position.Char.ToString() + ")."
                        );
                }
            }
            foreach (ASTNode child in node.Children)
            {
                asmStmnt.Children.Add(AnnotateNode(child, asmStmnt)); // Just pass everything down
            }
            return asmStmnt;
        }

        private AASTNode AnnotateRoutineBody(ASTNode node, AASTNode parent)
        {
            AASTNode body = new AASTNode(node, parent, no_type);
            foreach(ASTNode child in node.Children[1].Children)
            {
                body.Children.Add(AnnotateNode(child, body));
            }
            return body;
        }

        private AASTNode AnnotateRoutine(ASTNode node, AASTNode parent)
        {
            string routineName = node.Children[1].Token.Value;
            Context ctx = FindParentContext(parent);
            List<VarType> paramTypes = new List<VarType>();
            VarType returnType = no_type; // Default value
            // Determine parameter types and return type
            if (node.Children[3].Children.Count > 0)
            {
                paramTypes.AddRange(RetrieveParamTypes(node.Children[3].Children[0])); // Parameters
            }
            if (node.Children[5].Children.Count > 0)
            {
                returnType = IdentifyType(node.Children[5].Children[1]);
            }
            AASTNode routine = new AASTNode(node, parent, new RoutineType(paramTypes, returnType));
            ctx.AddVar(routine, routineName);
            routine.Context = new Context(routineName, ctx);
            // Add params to the context if any
            if (node.Children[3].Children.Count > 0)
            {
                AASTNode firstParam = new AASTNode(node.Children[3].Children[0].Children[0], routine, paramTypes[0]);
                firstParam.Token.Type = TokenType.IDENTIFIER;
                firstParam.Token.Value = node.Children[3].Children[0].Children[0].Children[1].Token.Value;
                firstParam.LIStart = 1;
                routine.Context.AddVar(firstParam, node.Children[3].Children[0].Children[0].Children[1].Token.Value);
                int i = 1;
                foreach (ASTNode child in node.Children[3].Children[0].Children[1].Children)
                {
                    if (child.ASTType.Equals("Parameter")) // Skip comma rule
                    {
                        AASTNode param = new AASTNode(child, routine, paramTypes[i]);
                        param.Token.Type = TokenType.IDENTIFIER;
                        param.Token.Value = child.Children[1].Token.Value;
                        param.LIStart = 1;
                        routine.Context.AddVar(param, child.Children[1].Token.Value);
                        i++;
                    }
                }
            }            
            // Annotate routine body
            routine.Children.Add(AnnotateNode(node.Children[6], routine));

            // Check if return statement exists
            if (ctx.GetRoutineReturnType(node.Children[1].Token).Type != VarType.ERAType.NO_TYPE)
            {
                /*if (!CheckForReturn(routine.Children[0]))
                {
                   TODO: !!! 
                }*/
            } 

            // Set LI end of parameters
            int maxDepth = GetMaxDepth(routine);
            Dictionary<string, AASTNode>.ValueCollection parameters = routine.Context.GetDeclaredVars();
            int j = 0;
            foreach (AASTNode param in parameters)
            {
                if (j >= paramTypes.Count) break;
                routine.Context.SetLIEnd(param.Token.Value, maxDepth);
                j++;
            }
            return routine;
        }

        private List<VarType> RetrieveParamTypes(ASTNode node)
        {
            List<VarType> lst = new List<VarType>
            {
                // First child
                IdentifyType(node.Children[0].Children[0])
            };
            // The rest of them
            foreach (ASTNode child in node.Children[1].Children)
            {
                if (child.ASTType.Equals("Parameter")) // Skip comma rule
                {
                    lst.Add(IdentifyType(child.Children[0]));
                }
            }
            return lst;
        }

        private AASTNode AnnotateStruct(ASTNode node, AASTNode parent)
        {
            string structName = node.Children[1].Token.Value;
            AASTNode structure = new AASTNode(node, parent, new StructType(structName));
            Context ctx = FindParentContext(parent);
            ctx.AddVar(structure, structName);
            structure.Context = new Context(structName, ctx);
            foreach (ASTNode child in node.Children[2].Children)
            {
                structure.Children.Add(AnnotateNode(child, structure));
            }
            return structure;
        }

        private AASTNode AnnotateData(ASTNode node, AASTNode parent)
        {
            Context ctx = FindParentContext(parent);
            AASTNode data = new AASTNode(node, parent, new VarType(VarType.ERAType.DATA));
            data.Children.Add(AnnotateNode(node.Children[1], data)); // Identifier
            data.Children.Add(AnnotateLiteral(node.Children[2], data)); // The first literal
            foreach (ASTNode child in node.Children[3].Children)
            {
                data.Children.Add(AnnotateLiteral(child, data)); // The rest of literals
            }
            ctx.AddVar(data, node.Children[1].Token.Value);
            return data;
        }

        private AASTNode AnnotateAnnotations(ASTNode node, AASTNode parent)
        {
            AASTNode anns = new AASTNode(node, parent, no_type);
            // Annotate the first child
            anns.Children.Add(AnnotatePragmaDecl(node.Children[1], anns));
            // Repeat for the rest
            foreach (ASTNode child in node.Children[2].Children) 
            {
                anns.Children.Add(AnnotatePragmaDecl(child, anns));
            }
            return anns;
        }

        private ASTNode AnnotatePragmaDecl(ASTNode node, AASTNode parent)
        {
            AASTNode pragmaDecl = new AASTNode(node, parent, no_type);
            pragmaDecl.Children.Add(AnnotateNode(node.Children[0], pragmaDecl)); // Identifier
            if (node.Children[2].Children.Count > 0)
            {
                pragmaDecl.Children.Add(AnnotateNode(node.Children[2].Children[1], pragmaDecl)); // Possible text (identifier actually)
            }
            return pragmaDecl;
        }

        private AASTNode AnnotatePrimary(ASTNode node, AASTNode parent)
        {
            AASTNode somePrim = new AASTNode(node, parent, no_type);
            Context ctx = FindParentContext(parent);

            // Identifier
            somePrim.Children.Add(AnnotateNode(node.Children[0], somePrim));
            ASTNode idLink = node.Children[0];

            // If constant, convert to number
            if (ctx.IsVarDeclared(idLink.Token) && ctx.IsVarConstant(idLink.Token))
            {
                int constValue = ctx.GetConstValue(idLink.Token);
                ASTNode number = new ASTNode(parent, new List<ASTNode>(), idLink.Token, "NUMBER");
                ASTNode opMinus = new ASTNode(number, new List<ASTNode>(), idLink.Token, "[ - ]");
                if (constValue < 0)
                {
                    opMinus.Children.Add(new ASTNode(opMinus, new List<ASTNode>(), idLink.Token, "OPERATOR"));
                    constValue *= -1;
                }
                number.Children.Add(opMinus);
                ASTNode literal = new ASTNode(number, new List<ASTNode>(), new Token(TokenType.NUMBER, constValue.ToString(), idLink.Token.Position), "SOME_LITERAL");
                number.Children.Add(literal);
                return AnnotateNode(number, parent);
            }
            else
            {
                // { '.' Identifier }
                if (node.Children[1].Children.Count > 0)
                {
                    foreach (ASTNode child in node.Children[1].Children)
                    {
                        if (!ctx.IsVarStruct(idLink.Token))
                        {
                            throw new SemanticErrorException(
                                "Trying to access non-struct variable via \'.\' notation!!!\r\n" +
                                "\tAt (Line: " + idLink.Token.Position.Line.ToString() +
                                ", Char: " + idLink.Token.Position.Char.ToString() + ")."
                                );
                        }
                        if (child.ASTType.Equals("IDENTIFIER"))
                            idLink = child;
                        somePrim.Children.Add(AnnotateNode(child, somePrim));
                    }
                }

                // [ ArrayAccess | CallArgs ]
                if (node.Children[2].Children.Count > 0)
                {
                    if (node.Children[2].Children[0].Children[0].ASTType.Equals("Call arguments"))
                    {
                        somePrim.Children.Add(AnnotateCallArgs(node.Children[2].Children[0].Children[0], somePrim));
                    }
                    else
                    {
                        if (!ctx.IsVarArray(idLink.Token))
                        {
                            throw new SemanticErrorException(
                                "Trying to access non-array variable via \'[]\' notation!!!\r\n" +
                                "\tAt (Line: " + idLink.Token.Position.Line.ToString() +
                                ", Char: " + idLink.Token.Position.Char.ToString() + ")."
                                );
                        }
                        // If expression is constant we can check for array boundaries
                        if (IsExprConstant(node.Children[2].Children[0].Children[0].Children[1], ctx))
                        {
                            int index = CalculateConstExpr(node.Children[2].Children[0].Children[0].Children[1], ctx);
                            int arrSize = ctx.GetArrSize(idLink.Token);
                            if (index < 0)
                                throw new SemanticErrorException(
                                "Negative array index!!!\r\n" +
                                "\tAt (Line: " + idLink.Token.Position.Line.ToString() +
                                    ", Char: " + idLink.Token.Position.Char.ToString() + ")."
                                );
                            // If we know the size of the array already (arrSize != 0 indicates this)
                            if (arrSize != 0 && index >= arrSize)
                                throw new SemanticErrorException(
                                "Accessing element with index higher than array the size!!!\r\n" +
                                "\tAt (Line: " + idLink.Token.Position.Line.ToString() +
                                    ", Char: " + idLink.Token.Position.Char.ToString() + ")."
                                );
                        }
                        somePrim.Children.Add(AnnotateExpr(node.Children[2].Children[0].Children[0].Children[1], somePrim));
                    }
                }
            }

            return somePrim;
        }

        private AASTNode AnnotateLiteral(ASTNode node, AASTNode parent)
        {
            AASTNode literal = new AASTNode(node, parent, no_type)
            {
                AASTValue = int.Parse(node.Children[1].Token.Value) * (node.Children[0].Children.Count > 0 ? -1 : 1)
            };
            return literal;
        }

        private AASTNode AnnotateModule(ASTNode node, AASTNode parent)
        {
            Context ctx = FindParentContext(parent);
            AASTNode module = new AASTNode(node, parent, new VarType(VarType.ERAType.MODULE))
            {
                Context = new Context(node.Children[1].Token.Value, ctx)
            };
            foreach (ASTNode child in node.Children[2].Children)
            {
                module.Children.Add(AnnotateNode(child, module));
            }
            ctx.AddVar(module, node.Children[1].Token.Value);
            return module;
        }

        private AASTNode AnnotateVarDeclaration(ASTNode node, AASTNode parent)
        {
            VarType type = IdentifyType(node.Children[0], node.Children[1].Children[0].ASTType.Equals("Constant"));
            if (node.Children[1].Children[0].ASTType.Equals("Array")) 
                type = new ArrayType(type);            
            AASTNode varDecl = new AASTNode(node, parent, no_type);
            varDecl.Children.AddRange(IdentifyVarDecl(node.Children[1].Children[0], varDecl, type));
            return varDecl;
        }
        
        private List<AASTNode> IdentifyVarDecl(ASTNode node, AASTNode parent, VarType type)
        {
            List<AASTNode> lst = new List<AASTNode>();
            Context ctx = FindParentContext(parent);

            switch (node.ASTType)
            {
                case "Variable":
                    {
                        // VarDefinition { , VarDefinition } ;
                        AASTNode firstDef = new AASTNode(node.Children[0], parent, type);
                        lst.Add(firstDef);
                        ctx.AddVar(firstDef, node.Children[0].Children[0].Token.Value); // VarDef's identifier
                                                                                        // Check expr if exists
                        if (node.Children[0].Children[1].Children.Count > 0)
                        {
                            AASTNode firstExpr = AnnotateNode(node.Children[0].Children[1].Children[0].Children[1], firstDef);
                            firstDef.Children.Add(firstExpr);
                        }
                        // Repeat for { , VarDefinition }
                        foreach (ASTNode varDef in node.Children[1].Children)
                        {
                            if (varDef.ASTType.Equals("Variable definition")) // Skip comma rule
                            {
                                AASTNode def = new AASTNode(varDef, parent, type);
                                lst.Add(def);
                                ctx.AddVar(def, varDef.Children[0].Token.Value); // VarDef's identifier
                                if (varDef.Children[1].Children.Count > 0)
                                {
                                    AASTNode expr = AnnotateNode(varDef.Children[1].Children[0].Children[1], def);
                                    def.Children.Add(expr);
                                }
                            }
                        }
                        break;
                    }

                case "Constant":
                    {
                        // 'const' ConstDefinition { , ConstDefinition } ;                        
                        AASTNode firstDef = new AASTNode(node.Children[1], parent, type);
                        lst.Add(firstDef);
                        ctx.AddVar(firstDef, node.Children[1].Children[0].Token.Value); // ConstDef's identifier

                        if (!IsExprConstant(node.Children[1].Children[2], ctx))
                        {
                            throw new SemanticErrorException(
                                "Expression for a constant definition is not constant!!!\r\n" +
                                "\t At (Line: " + node.Children[1].Children[2].Token.Position.Line.ToString() +
                                ", Char: " + node.Children[1].Children[2].Token.Position.Char.ToString() + ")."
                                );
                        }
                        firstDef.AASTValue = CalculateConstExpr(node.Children[1].Children[2], ctx);
                        // Repeat for { , ConstDefinition }
                        foreach (ASTNode varDef in node.Children[2].Children)
                        {
                            if (varDef.ASTType.Equals("Constant definition")) // Skip comma rule
                            {
                                AASTNode def = new AASTNode(varDef, parent, type);
                                lst.Add(def);
                                ctx.AddVar(def, varDef.Children[0].Token.Value); // ConstDef's identifier

                                if (!IsExprConstant(varDef.Children[2], ctx))
                                {
                                    throw new SemanticErrorException(
                                        "Expression for a constant definition is not constant!!!\r\n" +
                                        "\t At (Line: " + varDef.Children[2].Token.Position.Line.ToString() +
                                        ", Char: " + varDef.Children[2].Token.Position.Char.ToString() + ")."
                                        );
                                }
                                def.AASTValue = CalculateConstExpr(varDef.Children[2], ctx);
                            }
                        }
                        break;
                    }

                case "Array": 
                    {
                        // '[' ']' ArrDefinition { , ArrDefinition } ;
                        AASTNode firstDef = new AASTNode(node.Children[2], parent, type);
                        lst.Add(firstDef);
                        ctx.AddVar(firstDef, node.Children[2].Children[0].Token.Value); // ArrDef's identifier

                        //CheckVariablesForExistance(node.Children[2].Children[2], ctx); // Expression of ArrDefinition
                        if (IsExprConstant(node.Children[2].Children[2], ctx))
                        {
                            int arrSize = CalculateConstExpr(node.Children[2].Children[2], ctx);
                            if (arrSize <= 0) 
                                throw new SemanticErrorException(
                                "Incorrect array size!!!\r\n" +
                                "\t At (Line: " + node.Children[2].Children[2].Token.Position.Line.ToString() +
                                    ", Char: " + node.Children[2].Children[2].Token.Position.Char.ToString() + ")."
                                );
                            ((ArrayType)type).Size = arrSize;
                        }
                        else
                        {
                            // If size is not constant, just pass the expression
                            firstDef.Children.Add(AnnotateExpr(node.Children[2].Children[2], firstDef));
                        }
                        // Repeat for { , ArrDefinition }
                        foreach (ASTNode arrDef in node.Children[3].Children)
                        {
                            ArrayType arrType = new ArrayType(((ArrayType)type).ElementType); // Each array can have it's own size
                            if (arrDef.ASTType.Equals("Array definition")) // Skip comma rule
                            {
                                AASTNode def = new AASTNode(arrDef, parent, arrType);
                                lst.Add(def);
                                ctx.AddVar(def, arrDef.Children[0].Token.Value); // ArrDef's identifier

                                //CheckVariablesForExistance(arrDef.Children[2], ctx); // Expression of ArrDefinition
                                if (IsExprConstant(arrDef.Children[2], ctx))
                                {
                                    int _arrSize = CalculateConstExpr(arrDef.Children[2], ctx);
                                    if (_arrSize <= 0) 
                                        throw new SemanticErrorException(
                                        "Incorrect array size!!!\r\n" +
                                        "\t At (Line: " + arrDef.Children[2].Token.Position.Line.ToString() +
                                            ", Char: " + arrDef.Children[2].Token.Position.Char.ToString() + ")."
                                        );
                                    arrType.Size = _arrSize;
                                }
                                else
                                {
                                    // If size is not constant, just pass the expression
                                    def.Children.Add(AnnotateExpr(arrDef.Children[2], def));
                                }
                            }
                        }
                        break;
                    }
                default:
                    break;
            }

            return lst;
        }

        private AASTNode AnnotateExpr(ASTNode node, AASTNode parent) 
        {
            AASTNode expr = new AASTNode(node, parent, no_type);            
            List<ASTNode> children = node.Children;
            Context ctx = FindParentContext(parent);

            // Special case -1: if we have constant expression - calculate it and return literal instead
            // ATTENTION: need to be tested
            if (IsExprConstant(node, ctx))
            {
                int exprValue = CalculateConstExpr(node, ctx);             
                ASTNode number = new ASTNode(expr, new List<ASTNode>(), expr.Token, "NUMBER");                                
                ASTNode opMinus = new ASTNode(number, new List<ASTNode>(), expr.Token, "[ - ]");
                if (exprValue < 0)
                {
                    opMinus.Children.Add(new ASTNode(opMinus, new List<ASTNode>(), expr.Token, "OPERATOR"));
                    exprValue *= -1;
                }
                number.Children.Add(opMinus);
                ASTNode literal = new ASTNode(number, new List<ASTNode>(), new Token(TokenType.NUMBER, exprValue.ToString(), expr.Token.Position), "SOME_LITERAL");
                number.Children.Add(literal);
                expr.Children.Add(AnnotateNode(number, expr));
                return expr;
            }

            // Special case 0: if we have "legal" or initial Expression from Syntax Analyzer
            if (node.Children.Count == 2 && node.Children[1].ASTType.Equals("{ Operator Operand }"))
            {
                children = node.Children[1].Children;
                children.Insert(0, node.Children[0]);
            }

            // Special case 1: only one operand
            if (children.Count == 1)
            {
                expr.Children.Add(AnnotateNode(children[0], expr));
                return expr;
            }

            // Special case 2: operand, operator, and operand
            if (children.Count == 3)
            {
                foreach (var child in children)
                    expr.Children.Add(AnnotateNode(child, expr));
                return expr;
            }

            // If more, we need to rearrange the operands and operators to follow the operation priority
            // --  Gospod' dast - srabotaet  -- 
            // Priority list
            List<string> ops = new List<string>() { "*", "+", "-", ">=", "<=", ">", "<", "=", "/=", "&", "^", "|", "?" };

            foreach (string op in ops)
            {
                if (children.Count <= 3)
                {
                    break;
                }
                for (int i = 1; i < children.Count; i += 2) // Iterate over operators
                {
                    if (children[i].ASTType.Equals("Operator") && children[i].Token.Value.Equals(op))
                    {
                        ASTNode child_expr = new ASTNode(node, new List<ASTNode>(), node.Token, "Expression"); // Create additional expression
                        child_expr.Children.Add(children[i - 1]);
                        child_expr.Children.Add(children[i]);
                        child_expr.Children.Add(children[i + 1]);
                        children.RemoveRange(i - 1, 3);
                        children.Insert(i - 1, child_expr);
                        i -= 2;                       
                    }
                }
            }
            
            // Annotate modified AST and put it to expression
            foreach (ASTNode child in children) expr.Children.Add(AnnotateNode(child, expr));

            return expr;
        }

        private AASTNode AnnotateCodeBlock(ASTNode node, AASTNode parent)
        {
            AASTNode code = new AASTNode(node, parent, new VarType(VarType.ERAType.MODULE))
            {
                Context = new Context("code", FindParentContext(parent))
            };

            foreach (ASTNode child in node.Children[1].Children)
            {
                code.Children.Add(AnnotateNode(child, code));
            }

            FindParentContext(parent).AddVar(code, "code");

            return code;
        }

        private AASTNode AnnotateProgram(ASTNode node, AASTNode parent)
        {
            AASTNode program = new AASTNode(node, parent, no_type)
            {
                Context = new Context("Program", null)
            };

            foreach (ASTNode child in node.Children)
            {
                program.Children.Add(AnnotateNode(child, program));
            }

            return program;
        }

        /// <summary>
        /// Performs additional checks after the AAST is constructed.
        /// </summary>
        /// <param name="node">Expected ASTType - Any node (preferrable Program)</param>
        /// <param name="ctx">Current context</param>
        private void PostChecks(AASTNode node, Context ctx)
        {
            int lword = 4;
            int word = 4; // ATTENTION: Since ST rewrites the whole 32-bit word
            int sword = 4; // ATTENTION: Since ST rewrites the whole 32-bit word

            if (node.Context != null) ctx = node.Context;

            if (node.ASTType.Equals("IDENTIFIER") && !node.Parent.ASTType.Equals("Pragma declaration"))
            {
                if (node.Parent.ASTType.Equals("For"))
                {
                    ctx = ((AASTNode)node.Parent.Children[node.Parent.Children.Count - 1].Children[0]).Context;
                }
                if (!ctx.IsVarDeclared(node.Token))
                    throw new SemanticErrorException(
                        "A variable with name \"" + node.Token.Value + "\" has been never declared in this context!!!\r\n" +
                        "\tAt (Line: " + node.Token.Position.Line.ToString() + ", Char: " + node.Token.Position.Char.ToString() + ")."
                        );
            }
            else if (node.ASTType.Equals("Primary") && node.Children.Count > 1 && node.Children[1].ASTType.Equals("Call arguments"))
            {
                int paramNum = ctx.GetRoutineParamNum(node.Children[0].Token);
                if (node.Children[1].Children.Count != paramNum)
                {
                    throw new SemanticErrorException(
                        "Incorrect number of arguments when calling routine \"" + node.Children[0].Token.Value + "\"!!!\r\n" +
                        "Expected: " + paramNum.ToString() + ", received: " + node.Children[1].Children.Count.ToString() + ".\r\n" +
                        "  At (Line: " + node.Children[1].Token.Position.Line.ToString() +
                        ", Char: " + node.Children[1].Token.Position.Char.ToString() + ")."
                        );
                }
            }
            else if (node.ASTType.Equals("Call"))
            {
                if (ctx.GetRoutineReturnType(node.Children[0].Token).Type != VarType.ERAType.NO_TYPE)
                {
                    throw new SemanticErrorException(
                        "Calling a routine \"" + node.Children[0].Token.Value + "\" without using the return value!!!\r\n" +
                        "  At (Line: " + node.Children[1].Token.Position.Line.ToString() +
                        ", Char: " + node.Children[1].Token.Position.Char.ToString() + ")."
                        );
                }

            }
            else if (node.ASTType.Equals("Code") || node.ASTType.Equals("Routine body") || node.ASTType.Equals("Block body"))
            {
                // Numbering the statements and variable declarations for Linear Scan
                int num = 0;
                foreach(AASTNode child in node.Children)
                {
                    child.BlockPosition = ++num;
                }

                // Calculate offsets relative to Frame Pointer
                int i = 0;
                foreach (AASTNode var in ctx.GetDeclaredVars())
                {
                    var.FrameOffset = i;
                    switch (var.AASTType.Type)
                    {
                        case VarType.ERAType.INT:
                        case VarType.ERAType.INT_ADDR:
                        case VarType.ERAType.SHORT_ADDR:
                        case VarType.ERAType.BYTE_ADDR:
                            i += lword;
                            break;
                        case VarType.ERAType.SHORT:
                            i += word;
                            break;
                        case VarType.ERAType.BYTE:
                            i += sword;
                            break;
                        case VarType.ERAType.STRUCTURE:
                            // TODO: compute the structure size and put it to the stack
                            break;
                        case VarType.ERAType.ARRAY:
                            ArrayType arrType = (ArrayType)var.AASTType;
                            if (arrType.Size == 0) // Dynamic allocation
                            {
                                i += lword;
                            }
                            else
                            {
                                switch (arrType.ElementType.Type)
                                {
                                    case VarType.ERAType.INT:
                                    case VarType.ERAType.INT_ADDR:
                                    case VarType.ERAType.SHORT_ADDR:
                                    case VarType.ERAType.BYTE_ADDR:
                                        i += lword * arrType.Size;
                                        break;
                                    case VarType.ERAType.SHORT:
                                        i += word * arrType.Size;
                                        break;
                                    case VarType.ERAType.BYTE:
                                        i += sword * arrType.Size;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            break;
                        case VarType.ERAType.LABEL:
                            // TODO: get the statement number and generate... (?) question mark
                            break;
                        case VarType.ERAType.NO_TYPE:
                            break;
                        default:
                            break;
                    }
                }
            }
            else if (node.ASTType.Equals("Variable declaration"))
            {                
                if (node.Parent.ASTType.Equals("Statement"))
                {
                    foreach (AASTNode child in node.Children)
                    {
                        ctx.SetLIStart(child.Token, ((AASTNode)node.Parent).BlockPosition);
                    }
                }
                else
                {
                    foreach (AASTNode child in node.Children)
                    {
                        ctx.SetLIStart(child.Token, node.BlockPosition);
                    }
                }
            }
            else if (node.ASTType.Equals("Statement"))
            {
                foreach (string var in GetAllUsedVars(node))
                {
                    if (ctx.IsVarDeclaredInThisContext(var))
                        ctx.SetLIEnd(var, node.BlockPosition);
                }
            }
            else if (node.ASTType.Equals("Program")) // Global data
            {
                // Calculate offsets relative to Static Base
                int i = 0;
                foreach (AASTNode var in ctx.GetDeclaredVars())
                {
                    var.IsGlobal = true;
                    var.StaticOffset = i;
                    switch (var.AASTType.Type)
                    {
                        case VarType.ERAType.INT:
                        case VarType.ERAType.INT_ADDR:
                        case VarType.ERAType.SHORT_ADDR:
                        case VarType.ERAType.BYTE_ADDR:
                        case VarType.ERAType.ROUTINE:
                        case VarType.ERAType.MODULE:
                            //if (!var.Token.Value.Equals("code")) // ATTENTION: may cause troubles
                            i += lword;
                            break;
                        case VarType.ERAType.SHORT:
                            i += word;
                            break;
                        case VarType.ERAType.BYTE:
                            i += sword;
                            break;
                        case VarType.ERAType.STRUCTURE:
                            // TODO: compute the structure size and put it to the stack
                            break;
                        case VarType.ERAType.ARRAY:
                            ArrayType arrType = (ArrayType)var.AASTType;
                            if (arrType.Size == 0) // Dynamic allocation
                            {
                                i += lword;
                            }
                            else
                            {
                                switch (arrType.ElementType.Type)
                                {
                                    case VarType.ERAType.INT:
                                    case VarType.ERAType.INT_ADDR:
                                    case VarType.ERAType.SHORT_ADDR:
                                    case VarType.ERAType.BYTE_ADDR:
                                        i += lword * arrType.Size;
                                        break;
                                    case VarType.ERAType.SHORT:
                                        i += word * arrType.Size;
                                        break;
                                    case VarType.ERAType.BYTE:
                                        i += sword * arrType.Size;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            break;
                        case VarType.ERAType.DATA:
                            // TODO: what to do with this stuff???
                            break;
                        default:
                            break;
                    }
                }

                // Store the length of the Static Data inside Program node
                node.AASTValue = i;
            }

            foreach (AASTNode child in node.Children)
            {
                PostChecks(child, ctx);
            }
        }

        /*
         Helper functions
        */

        /// <summary>
        /// Calculates a numerical value of a given constant expression
        /// </summary>
        /// <param name="node">Expected ASTType - Expression</param>
        /// <param name="ctx">Current context</param>
        /// <returns>Calculated value of an expression</returns>
        private int CalculateConstExpr(ASTNode node, Context ctx)
        {
            // Construct a list of operands and operators
            List<int> operands = new List<int>();
            List<string> operators = new List<string>();

            if (node.Children.Count == 2 && node.Children[1].ASTType.Equals("{ Operator Operand }")) // Standart Expression
            {
                operands.Add(GetOperandValue(node.Children[0], ctx));
                if (node.Children[1].Children.Count > 0)
                {
                    for (int i = 0; i < node.Children[1].Children.Count; i += 2)
                    {
                        operators.Add(node.Children[1].Children[i].Token.Value);
                        operands.Add(GetOperandValue(node.Children[1].Children[i + 1], ctx));
                    }

                    // Execute higher order operators (*) 
                    for (int i = 0; i < operators.Count; i++)
                    {
                        if (operators[i].Equals("*"))
                        {
                            operators.RemoveAt(i);
                            int res = operands[i] * operands[i + 1];
                            operands.RemoveRange(i, 2);
                            operands.Insert(i, res);
                            i--;
                        }
                    }

                    // Execute higher order operators (+, -) 
                    for (int i = 0; i < operators.Count; i++)
                    {
                        if (operators[i].Equals("+"))
                        {
                            operators.RemoveAt(i);
                            int res = operands[i] + operands[i + 1];
                            operands.RemoveRange(i, 2);
                            operands.Insert(i, res);
                            i--;
                        }
                        else if (operators[i].Equals("-"))
                        {
                            operators.RemoveAt(i);
                            int res = operands[i] - operands[i + 1];
                            operands.RemoveRange(i, 2);
                            operands.Insert(i, res);
                            i--;
                        }
                    }

                    // Execute higher order operators (<=, >=) 
                    for (int i = 0; i < operators.Count; i++)
                    {
                        if (operators[i].Equals("<="))
                        {
                            operators.RemoveAt(i);
                            int res = operands[i] << operands[i + 1];
                            operands.RemoveRange(i, 2);
                            operands.Insert(i, res);
                            i--;
                        }
                        else if (operators[i].Equals(">="))
                        {
                            operators.RemoveAt(i);
                            int res = operands[i] >> operands[i + 1];
                            operands.RemoveRange(i, 2);
                            operands.Insert(i, res);
                            i--;
                        }
                    }

                    // Execute lower order operators (<, >, =, /=) 
                    for (int i = 0; i < operators.Count; i++)
                    {
                        if (operators[i].Equals(">"))
                        {
                            operators.RemoveAt(i);
                            int res = operands[i] > operands[i + 1] ? 1 : 0;
                            operands.RemoveRange(i, 2);
                            operands.Insert(i, res);
                            i--;
                        }
                        else if (operators[i].Equals("<"))
                        {
                            operators.RemoveAt(i);
                            int res = operands[i] < operands[i + 1] ? 1 : 0;
                            operands.RemoveRange(i, 2);
                            operands.Insert(i, res);
                            i--;
                        }
                        else if (operators[i].Equals("="))
                        {
                            operators.RemoveAt(i);
                            int res = operands[i] == operands[i + 1] ? 1 : 0;
                            operands.RemoveRange(i, 2);
                            operands.Insert(i, res);
                            i--;
                        }
                        else if (operators[i].Equals("/="))
                        {
                            operators.RemoveAt(i);
                            int res = operands[i] != operands[i + 1] ? 1 : 0;
                            operands.RemoveRange(i, 2);
                            operands.Insert(i, res);
                            i--;
                        }
                    }

                    // Execute lower order operators (&) 
                    for (int i = 0; i < operators.Count; i++)
                    {
                        if (operators[i].Equals("&"))
                        {
                            operators.RemoveAt(i);
                            int res = operands[i] & operands[i + 1];
                            operands.RemoveRange(i, 2);
                            operands.Insert(i, res);
                            i--;
                        }
                    }

                    // Execute lower order operators (^) 
                    for (int i = 0; i < operators.Count; i++)
                    {
                        if (operators[i].Equals("^"))
                        {
                            operators.RemoveAt(i);
                            int res = operands[i] ^ operands[i + 1];
                            operands.RemoveRange(i, 2);
                            operands.Insert(i, res);
                            i--;
                        }
                    }

                    // Execute lower order operators (|) 
                    for (int i = 0; i < operators.Count; i++)
                    {
                        if (operators[i].Equals("|"))
                        {
                            operators.RemoveAt(i);
                            int res = operands[i] | operands[i + 1];
                            operands.RemoveRange(i, 2);
                            operands.Insert(i, res);
                            i--;
                        }
                    }

                    // Execute lower order operators (?)
                    for (int i = 0; i < operators.Count; i++)
                    {
                        if (operators[i].Equals("?"))
                        {
                            operators.RemoveAt(i);
                            int res = operands[i] > operands[i + 1] ? 1 : operands[i] == operands[i + 1] ? 4 : 2;
                            operands.RemoveRange(i, 2);
                            operands.Insert(i, res);
                            i--;
                        }
                    }

                    return operands[0];
                }
                else
                {
                    return operands[0];
                }
            }
            else // Compressed Expression
            {
                operands.Add(GetOperandValue(node.Children[0], ctx));
                if (node.Children.Count > 1)
                {
                    for (int i = 1; i < node.Children.Count; i += 2)
                    {
                        operators.Add(node.Children[i].Token.Value);
                        operands.Add(GetOperandValue(node.Children[i + 1], ctx));
                    }

                    // Execute higher order operators (*) 
                    for (int i = 0; i < operators.Count; i++)
                    {
                        if (operators[i].Equals("*"))
                        {
                            operators.RemoveAt(i);
                            int res = operands[i] * operands[i + 1];
                            operands.RemoveRange(i, 2);
                            operands.Insert(i, res);
                            i--;
                        }
                    }

                    // Execute higher order operators (+, -) 
                    for (int i = 0; i < operators.Count; i++)
                    {
                        if (operators[i].Equals("+"))
                        {
                            operators.RemoveAt(i);
                            int res = operands[i] + operands[i + 1];
                            operands.RemoveRange(i, 2);
                            operands.Insert(i, res);
                            i--;
                        }
                        else if (operators[i].Equals("-"))
                        {
                            operators.RemoveAt(i);
                            int res = operands[i] - operands[i + 1];
                            operands.RemoveRange(i, 2);
                            operands.Insert(i, res);
                            i--;
                        }
                    }

                    // Execute lower order operators (<, >, =, /=) 
                    for (int i = 0; i < operators.Count; i++)
                    {
                        if (operators[i].Equals(">"))
                        {
                            operators.RemoveAt(i);
                            int res = operands[i] > operands[i + 1] ? 1 : 0;
                            operands.RemoveRange(i, 2);
                            operands.Insert(i, res);
                            i--;
                        }
                        else if (operators[i].Equals("<"))
                        {
                            operators.RemoveAt(i);
                            int res = operands[i] < operands[i + 1] ? 1 : 0;
                            operands.RemoveRange(i, 2);
                            operands.Insert(i, res);
                            i--;
                        }
                        else if (operators[i].Equals("="))
                        {
                            operators.RemoveAt(i);
                            int res = operands[i] == operands[i + 1] ? 1 : 0;
                            operands.RemoveRange(i, 2);
                            operands.Insert(i, res);
                            i--;
                        }
                        else if (operators[i].Equals("/="))
                        {
                            operators.RemoveAt(i);
                            int res = operands[i] != operands[i + 1] ? 1 : 0;
                            operands.RemoveRange(i, 2);
                            operands.Insert(i, res);
                            i--;
                        }
                    }

                    // Execute lower order operators (&) 
                    for (int i = 0; i < operators.Count; i++)
                    {
                        if (operators[i].Equals("&"))
                        {
                            operators.RemoveAt(i);
                            int res = operands[i] & operands[i + 1];
                            operands.RemoveRange(i, 2);
                            operands.Insert(i, res);
                            i--;
                        }
                    }

                    // Execute lower order operators (^) 
                    for (int i = 0; i < operators.Count; i++)
                    {
                        if (operators[i].Equals("^"))
                        {
                            operators.RemoveAt(i);
                            int res = operands[i] ^ operands[i + 1];
                            operands.RemoveRange(i, 2);
                            operands.Insert(i, res);
                            i--;
                        }
                    }

                    // Execute lower order operators (|) 
                    for (int i = 0; i < operators.Count; i++)
                    {
                        if (operators[i].Equals("|"))
                        {
                            operators.RemoveAt(i);
                            int res = operands[i] | operands[i + 1];
                            operands.RemoveRange(i, 2);
                            operands.Insert(i, res);
                            i--;
                        }
                    }

                    // Execute lower order operators (?)
                    for (int i = 0; i < operators.Count; i++)
                    {
                        if (operators[i].Equals("?"))
                        {
                            operators.RemoveAt(i);
                            int res = operands[i] > operands[i + 1] ? 1 : operands[i] == operands[i + 1] ? 4 : 2;
                            operands.RemoveRange(i, 2);
                            operands.Insert(i, res);
                            i--;
                        }
                    }

                    return operands[0];
                }
                else
                {
                    return operands[0];
                }
            }
        }
        
        /// <summary>
        /// Returns a value of a single operand.
        /// </summary>
        /// <param name="node">Expected ASTType - Operand</param>
        /// <param name="ctx">Current context</param>
        /// <returns>The value of an operand.</returns>
        private int GetOperandValue(ASTNode node, Context ctx)
        {
            return node.Children[0].ASTType switch
            {
                "( Expression )" => CalculateConstExpr(node.Children[0].Children[1], ctx),
                "Primary" => ctx.GetConstValue(node.Children[0].Children[0].Token), // Identifier
                "NUMBER" => int.Parse(node.Children[0].Children[1].Token.Value) * (node.Children[0].Children[0].Children.Count > 0 ? -1 : 1),
                _ => -1,
            };
        }
       
        /// <summary>
        /// Checks whether an expression is constant.
        /// </summary>
        /// <param name="node">Expected ASTType - Expression</param>
        /// <param name="ctx">Current context</param>
        /// <returns>True if constant, false otherwise</returns>
        private bool IsExprConstant(ASTNode node, Context ctx)
        {
            // Regular Expression
            if (node.Children[1].ASTType.Equals("{ Operator Operand }"))
            {
                if (!IsOperandConstant(node.Children[0], ctx)) return false;
                foreach (ASTNode child in node.Children[1].Children)
                {
                    if (child.ASTType.Equals("Operand") && !IsOperandConstant(child, ctx)) return false;
                }
            }
            else
            {
                foreach (ASTNode child in node.Children)
                {
                    if (child.ASTType.Equals("Operand") && !IsOperandConstant(child, ctx)) return false;
                    if (child.ASTType.Equals("Expression") && !IsExprConstant(child, ctx)) return false;
                }
            }

            return true;
        }
        
        private bool IsOperandConstant(ASTNode node, Context ctx)
        {
            string type = node.Children[0].ASTType; // The child of Operand
            
            if (type.Equals("Explicit address") || type.Equals("Dereference") || type.Equals("Reference"))
            {
                return false;
            }

            if (type.Equals("( Expression )")) return IsExprConstant(node.Children[0].Children[1], ctx);

            // Check whether primary is constant
            if (type.Equals("Primary"))
            {
                if (node.Children[0].Children[1].Children.Count > 0) return false; // { . Identitfier } TODO: dot descent
                if (node.Children[0].Children[2].Children.Count > 0) return false; // [ ArrayAccess | CallArgs ] Always non-constant
                if (!ctx.IsVarConstant(node.Children[0].Children[0].Token)) // Just check the first operand
                {
                    return false;
                }
            }

            return true;
        }

        private VarType IdentifyType(ASTNode node, bool isConst = false)
        {
            VarType vt;
            
            if (node.Children[0].ASTType.Equals("IDENTIFIER"))
            {
                vt = new StructType(node.Children[0].Token.Value);
            }
            else
            {
                if (isConst)
                {
                    if (node.Children[0].Children[1].Children.Count == 0)
                    {
                        vt = node.Children[0].Children[0].Children[0].Token.Value switch
                        {
                            "int" => new VarType(VarType.ERAType.CONST_INT),
                            "byte" => new VarType(VarType.ERAType.CONST_BYTE),
                            "short" => new VarType(VarType.ERAType.CONST_SHORT),
                            _ => new VarType(VarType.ERAType.NO_TYPE),
                        };
                    }
                    else
                    {
                        vt = node.Children[0].Children[0].Children[0].Token.Value switch
                        {
                            "int" => new VarType(VarType.ERAType.CONST_INT_ADDR),
                            "byte" => new VarType(VarType.ERAType.CONST_BYTE_ADDR),
                            "short" => new VarType(VarType.ERAType.CONST_SHORT_ADDR),
                            _ => new VarType(VarType.ERAType.NO_TYPE),
                        };
                    }
                }
                else
                {
                    if (node.Children[0].Children[1].Children.Count == 0)
                    {
                        vt = node.Children[0].Children[0].Children[0].Token.Value switch
                        {
                            "int" => new VarType(VarType.ERAType.INT),
                            "byte" => new VarType(VarType.ERAType.BYTE),
                            "short" => new VarType(VarType.ERAType.SHORT),
                            _ => new VarType(VarType.ERAType.NO_TYPE),
                        };
                    }
                    else
                    {
                        vt = node.Children[0].Children[0].Children[0].Token.Value switch
                        {
                            "int" => new VarType(VarType.ERAType.INT_ADDR),
                            "byte" => new VarType(VarType.ERAType.BYTE_ADDR),
                            "short" => new VarType(VarType.ERAType.SHORT_ADDR),
                            _ => new VarType(VarType.ERAType.NO_TYPE),
                        };
                    }
                }
            }
            return vt;           
        }
        
        private HashSet<string> GetAllUsedVars(AASTNode node)
        {
            HashSet<string> set = new HashSet<string>();
            foreach (AASTNode child in node.Children)
            {
                if (child.ASTType.Equals("IDENTIFIER"))
                    set.Add(child.Token.Value);
                set.UnionWith(GetAllUsedVars(child));
            }
            return set;
        }
        
        private int GetMaxDepth(AASTNode node)
        {            
            int maxDepth = 1;
            foreach (AASTNode child in node.Children)
            {
                int childDepth = GetMaxDepth(child);
                if (childDepth > maxDepth)
                    maxDepth = childDepth; 
            }
            if (node.ASTType.Equals("Block body"))
            {
                if (node.Children.Count > maxDepth)
                    maxDepth = node.Children.Count;
            }
            return maxDepth;
        }

        /// <summary>
        /// Used for current context retrieval
        /// </summary>
        /// <param name="parent">Parent (or current) node from which to start the search</param>
        /// <returns>Nearest context (may return global Program context)</returns>
        public static Context FindParentContext(AASTNode parent)
        {
            while (true)
            {
                if (parent == null) break;
                if (parent.Context != null) return parent.Context;
                parent = (AASTNode)parent.Parent;
            }
            return null;
        }
        
    }
}
