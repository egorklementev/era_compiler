using ERACompiler.Modules.Semantics;
using ERACompiler.Structures;
using ERACompiler.Structures.Types;
using ERACompiler.Utilities.Errors;
using System.Collections.Generic;

namespace ERACompiler.Modules
{
    public class SemanticAnalyzer
    {
        public static readonly VarType no_type = new VarType(VarType.ERAType.NO_TYPE); // Placeholder
        public readonly Dictionary<string, NodeAnnotator> nodeAnnotators;
        public AASTNode varToAddToCtx = null; // For "for" loops

        public SemanticAnalyzer()
        {
            nodeAnnotators = new Dictionary<string, NodeAnnotator>
            {
                { "Default", new DefaultAnnotator() },
                { "Program", new ProgramAnnotator() },
                { "Annotations", new AnnotationsAnnotator() },
                { "Data", new DataBlockAnnotator() },
                { "Structure", new StructureAnnotator() },
                { "Routine", new RoutineAnnotator() },
                { "Routine body", new RoutineBodyAnnotator() },
                { "Assembly block", new AssemblyBlockAnnotator() },
                { "Assembly statement", new AssemblyStatementAnnotator() },
                { "Expression", new ExpressionAnnotator() },
                { "Assignment", new AssignmentAnnotator() },
                { "Call arguments", new CallArumentsAnnotator() },
                { "NUMBER", new LiteralAnnotator() },
                { "Primary", new PrimaryAnnotator() },
                { "( Expression )", new ParenthesisExpressionAnnotator() },
                { "Variable declaration", new VariableDeclarationAnnotator() },
                { "Code", new CodeBlockAnnotator() },
                { "Module", new ModuleAnnotator() },
                { "Call", new CallAnnotator() },
                { "Swap", new SwapAnnotator() },
                { "If", new IfAnnotator() },
                { "Block body", new BlockBodyAnnotator() },
                { "For", new ForAnnotator() },
                { "While", new WhileAnnotator() },
                { "Loop While", new LoopWhileAnnotator() },
                { "Loop body", new LoopBodyAnnotator() },
                { "Break", new BreakAnnotator() },
                { "Return", new ReturnAnnotator() },
                { "Statement", new StatementAnnotator() },
                { "Goto", new GotoAnnotator() },
                { "Label", new LabelAnnotator() },
                { "KEYWORD", new NoProcessingAnnotator() },
                { "OPERATOR", new NoProcessingAnnotator() },
                { "REGISTER", new NoProcessingAnnotator() },
                { "DELIMITER", new NoProcessingAnnotator() },
                { "IDENTIFIER", new NoProcessingAnnotator() },
                { "Loop", new FirstChildAnnotator() },
                { "Call ;", new FirstChildAnnotator() },
                { "Operand", new FirstChildAnnotator() },
                { "Operator", new FirstChildAnnotator() },
                { "Receiver", new FirstChildAnnotator() },
                { "Some unit", new FirstChildAnnotator() },
                { "Extension statement", new FirstChildAnnotator() },
                { "Some module statement", new FirstChildAnnotator() },
                { "Identifier | Register", new FirstChildAnnotator() },
                { "VarDeclaration | Statement", new FirstChildAnnotator() },
                { "Print", new AllChildrenAnnotator() },
                { "Reference", new AllChildrenAnnotator() },
                { "Dereference", new AllChildrenAnnotator() }
            };
        }

        public AASTNode BuildAAST(ASTNode ASTRoot)
        {
            AASTNode program = nodeAnnotators[ASTRoot.ASTType].Annotate(ASTRoot, null);

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

        public static VarType IdentifyType(ASTNode node, bool isConst = false)
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
        
        public static HashSet<string> GetAllUsedVars(AASTNode node)
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
        
        public static int GetMaxDepth(AASTNode node)
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
        public static Context? FindParentContext(AASTNode? parent)
        {
            while (true)
            {
                if (parent == null) break;
                if (parent.Context != null) return parent.Context;
                parent = (AASTNode?)parent.Parent;
            }
            return null;
        }

        public static List<VarType> RetrieveParamTypes(ASTNode node)
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

        /// <summary>
        /// Performs additional checks after the AAST is constructed.
        /// </summary>
        /// <param name="node">Expected ASTType - Any node (preferrable Program)</param>
        /// <param name="ctx">Current context</param>
        private void PostChecks(AASTNode node, Context? ctx)
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
                if (ctx.GetRoutineReturnType(node.Children[0].Token).Type == VarType.ERAType.NO_TYPE)
                {
                    throw new SemanticErrorException(
                        "Trying to use no-return routine in an expression!!!\r\n" +
                        "\tAt (Line: " + node.Children[0].Token.Position.Line.ToString() +
                        ", Char: " + node.Children[0].Token.Position.Char.ToString() + ")."
                        );
                }
            }
            else if (node.ASTType.Equals("Call"))
            {
                if (ctx.GetRoutineReturnType(node.Children[0].Token).Type != VarType.ERAType.NO_TYPE)
                {
                    throw new SemanticErrorException(
                        "Calling a routine \"" + node.Children[0].Token.Value + "\" without using the return value!!!\r\n" +
                        "  At (Line: " + node.Children[0].Token.Position.Line.ToString() +
                        ", Char: " + node.Children[0].Token.Position.Char.ToString() + ")."
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
            else if (node.ASTType.Equals("Routine"))
            {
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
        public static int CalculateConstExpr(ASTNode node, Context ctx)
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
        public static int GetOperandValue(ASTNode node, Context ctx)
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
        public static bool IsExprConstant(ASTNode node, Context ctx)
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
        
        public static bool IsOperandConstant(ASTNode node, Context ctx)
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

    }
}
