using System;
using System.Collections.Generic;
using ERACompiler.Structures;
using ERACompiler.Utilities;
using ERACompiler.Utilities.Errors;

namespace ERACompiler.Modules
{
    /// <summary>
    /// The purpose of this class it to check the code for syntax errors and build Abstract Syntax Tree that can be used by Semantic Analyzer.
    /// </summary>
    public class SyntaxAnalyzer
    {
        private Token emptyToken; // Used for generalization purposes

        /// <summary>
        /// Used for initialization of some variables.
        /// </summary>
        public SyntaxAnalyzer()
        {
            emptyToken = new Token(TokenType.NO_TOKEN, "No token", new TokenPosition(-1, -1));
        }

        /// <summary>
        /// Main function of the class. It checks the structure of the token list for syntax errors and if it 
        /// is correct, returns the root node of the constructed Abstract Syntax Tree.
        /// </summary>
        /// <param name="tokens">The list of tokens from Lexical Analyzer.</param>
        /// <returns>Constructed Abstract Syntax Tree.</returns>
        public ASTNode BuildAST(List<Token> tokens)
        {
            // Remove unnecessary whitespaces
            tokens = RemoveWhitespaces(tokens);

            // If something wrong, compilation stops
            CheckBrackets(tokens);

            ASTNode root = new ASTNode(null, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.PROGRAM); // The root node of the tree

            try
            {
                bool codeBlockFound = false;
                while (HasNextUnit(tokens))
                {
                    root.Children.Add(GetNextUnit(tokens, root));
                    if (root.Children[root.Children.Count - 1].Type == ASTNode.ASTNodeType.CODE)
                    {
                        if (codeBlockFound)
                        {
                            Logger.LogError(new SyntaxError(
                                "Multiple 'code' block found, only a single block allowed!!!"
                                ));
                        }
                        else
                        {
                            codeBlockFound = true;
                        }
                    }
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                Logger.LogError(new SyntaxError("Unknown syntax error found!!!"));
                throw;
            }

            if (tokens.Count > 0)
            {
                Logger.LogError(new SyntaxError("Unknown units are found!!!"));
                
            }

            // When no units available - there is nothing to be compiled
            if (root.Children.Count == 0)
            {
                Logger.LogError(new SyntaxError("No units in the program!!!"));
            }

            return root;
        }

        private List<Token> RemoveWhitespaces(List<Token> tokens)
        {
            List<Token> newTokens = new List<Token>();
            List<int> toBeSaved = new List<int>();
            for (int i = 0; i < tokens.Count; i++)
            {
                if (
                    tokens[i].Type == TokenType.WHITESPACE && 
                    i > 0 && 
                    tokens[i - 1].Type == TokenType.OPERATOR && 
                    (tokens[i - 1].Value.Equals("*") || tokens[i - 1].Value.Equals("&"))
                    )                    
                {
                    toBeSaved.Add(i);
                }
            }
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].Type != TokenType.WHITESPACE || toBeSaved.Contains(i))
                {
                    newTokens.Add(tokens[i]);
                }
            }

            return newTokens;
        }

        private void CheckBrackets(List<Token> tokens)
        {
            List<string> opening = new List<string>() { "[", "(" };
            List<string> closing = new List<string>() { "]", ")" };

            for (int i = 0; i < opening.Count; i++)
            {
                int current = 0;
                foreach (Token t in tokens)
                {
                    if (t.Value.Equals(opening[i]))
                    {
                        current++;
                    }
                    if (t.Value.Equals(closing[i]))
                    {
                        current--;
                    }
                    if (current < 0)
                    {
                        Logger.LogError(new SyntaxError(
                            "Missing opening statement for \"" + t.Value + "\" at (" + t.Position.Line.ToString() + ", " + t.Position.Character.ToString() + ")!!!"
                            ));
                    }
                }
                if (current > 0)
                {
                    Logger.LogError(new SyntaxError("Missing close statement for some \"" + opening[i] + "\" token!!!"));
                }
            }
        }

        private bool HasNextUnit(List<Token> tokens)
        {
            if (tokens.Count == 0)
            {
                return false;
            }
            return 
                tokens[0].Value.Equals("code") || 
                tokens[0].Value.Equals("data") ||
                tokens[0].Value.Equals("pragma") || 
                tokens[0].Value.Equals("module") ||
                tokens[0].Value.Equals("routine") ||
                tokens[0].Value.Equals("start") ||
                tokens[0].Value.Equals("entry");
        }
        
        private ASTNode GetNextUnit(List<Token> tokens, ASTNode parent) // Annotation | Data | Module | Routine | Code
        {
            // Make a decision about what rule to follow
            switch (tokens[0].Value)
            {
                case "code":
                    {
                        // Decide whether 'code' has corresponding 'end' and locate it
                        int end_i = -1;
                        int current = 1;
                        for (int i = 0; i < tokens.Count; i++)
                        {
                            // 'asm' keyword special case
                            if (tokens[i].Value.Equals("asm"))
                            {
                                int j = i;
                                while (true)
                                {
                                    if (tokens[j].Value.Equals("end") || tokens[j].Value.Equals(";"))
                                    {
                                        i = j + 1;
                                        break;
                                    }
                                    j++;
                                }
                                if (i >= tokens.Count)
                                {
                                    break;
                                }
                            }
                            if (
                                tokens[i].Value.Equals("loop") ||
                                tokens[i].Value.Equals("do")
                                )
                            {
                                current++;
                            }
                            if (tokens[i].Value.Equals("end"))
                            {
                                current--;
                            }
                            if (current == 0)
                            {
                                end_i = i;
                                break;
                            }
                        }
                        if (current > 0 || end_i == -1)
                        {
                            Logger.LogError(new SyntaxError(
                                "No 'end' statement for the 'code' at (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")!!!"
                                ));
                        }

                        // Pass only the 'code' related part of the source code down                        
                        ASTNode codeNode = GetCode(tokens.GetRange(1, end_i - 1), parent);
                        tokens.RemoveRange(0, end_i + 1);

                        return codeNode;
                    }
                case "data":
                    {
                        ASTNode dataNode = new ASTNode(
                            parent,
                            new List<ASTNode>(),
                            tokens[0],
                            ASTNode.ASTNodeType.DATA
                            );

                        // Check for identifier
                        if (tokens[1].Type != TokenType.IDENTIFIER)
                        {
                            Logger.LogError(new SyntaxError(
                                "Missing identifier for 'data' block at (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")!!!"
                                ));
                        }

                        // Data identifier
                        dataNode.Children.Add(new ASTNode(dataNode, new List<ASTNode>(), tokens[1], ASTNode.ASTNodeType.IDENTIFIER));

                        // Check if 'data' block is correct and collect all literals
                        int i = 2;
                        while (true)
                        {
                            if (tokens[i].Value.Equals("end"))
                            {
                                break;
                            }
                            if (tokens[i].Type == TokenType.NUMBER)
                            {
                                dataNode.Children.Add(new ASTNode(dataNode, new List<ASTNode>(), tokens[i], ASTNode.ASTNodeType.LITERAL));
                            }
                            if (tokens[i].Type != TokenType.NUMBER && !tokens[i].Value.Equals(","))
                            {
                                Logger.LogError(new SyntaxError(
                                    "Unknown symbols at 'data' block at (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")!!!"
                                    ));
                            }
                            i++;
                        }
                        tokens.RemoveRange(0, i + 1);

                        return dataNode;
                    }
                case "module":
                    {
                        // Decide whether 'module' has corresponding 'end' and locate it
                        int end_i = -1;
                        int current = 1;
                        for (int i = 0; i < tokens.Count; i++)
                        {
                            // 'asm' keyword special case
                            if (tokens[i].Value.Equals("asm"))
                            {
                                int j = i;
                                while (true)
                                {
                                    if (tokens[j].Value.Equals("end") || tokens[j].Value.Equals(";"))
                                    {
                                        i = j;
                                        break;
                                    }
                                    j++;
                                }
                                if (i >= tokens.Count)
                                {
                                    break;
                                }
                            }
                            if (
                                tokens[i].Value.Equals("loop") ||
                                tokens[i].Value.Equals("do")
                                )
                            {
                                current++;
                            }
                            if (tokens[i].Value.Equals("end"))
                            {
                                current--;
                            }
                            if (current == 0)
                            {
                                end_i = i;
                                break;
                            }
                        }
                        if (current > 0 || end_i == -1)
                        {
                            Logger.LogError(new SyntaxError(
                                "No 'end' statement for the 'module' at (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")!!!"
                                ));
                        }

                        // Pass only the 'code' related part of the source code down                        
                        ASTNode moduleNode = GetModule(tokens.GetRange(1, end_i - 1), parent);
                        tokens.RemoveRange(0, end_i + 1);

                        return moduleNode;
                    }
                case "start":
                case "entry":
                case "routine":
                    {
                        int end_i = LocateRoutineEnd(tokens);
                        ASTNode routineNode = GetRoutine(tokens.GetRange(0, end_i), parent);
                        tokens.RemoveRange(0, end_i + 1); // Including 'end'
                        return routineNode;
                    }
                case "pragma":
                    {
                        ASTNode annotationNode = new ASTNode(parent, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.ANNOTATION);

                        int end = 0;
                        int lastComma = 0;
                        while (!tokens[end].Value.Equals(";"))
                        {
                            if (tokens[end].Value.Equals(","))
                            {
                                annotationNode.Children.Add(GetPragmaDeclaration(tokens.GetRange(lastComma + 1, end - lastComma - 1), annotationNode));
                                lastComma = end;                                
                            }
                            end++;
                        }
                        annotationNode.Children.Add(GetPragmaDeclaration(tokens.GetRange(lastComma + 1, end - lastComma - 1), annotationNode));
                        tokens.RemoveRange(0, end + 1);

                        return annotationNode;
                    }
                default:
                    {
                        Logger.LogError(new SyntaxError(
                            "Bad source code structure!!! No annotation, code, data, module, or routine found!"
                            ));
                        return null;
                    }
            }
        }
        
        private ASTNode GetPragmaDeclaration(List<Token> tokens, ASTNode parent) // Identifier ( [ Text ] )
        {
            ASTNode prgmDeclNode = new ASTNode(parent, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.PRAGMA_DECLARATION);

            if (tokens.Count == 3)
            {
                prgmDeclNode.Children.Add(new ASTNode(prgmDeclNode, new List<ASTNode>(), tokens[0], ASTNode.ASTNodeType.IDENTIFIER));
            }
            else
            {
                prgmDeclNode.Children.Add(new ASTNode(prgmDeclNode, new List<ASTNode>(), tokens[0], ASTNode.ASTNodeType.IDENTIFIER));
                prgmDeclNode.Children.Add(new ASTNode(prgmDeclNode, new List<ASTNode>(), tokens[2], ASTNode.ASTNodeType.IDENTIFIER)); // TODO: Fix to 'Text' rule
            }

            return prgmDeclNode;
        }

        private ASTNode GetCode(List<Token> tokens, ASTNode parent) // code { VarDeclaration | Statement } end
        {
            ASTNode codeNode = new ASTNode(parent, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.CODE);

            while(tokens.Count > 0)
            {
                switch (tokens[0].Value)
                {
                    case "int":
                    case "byte":
                    case "short":
                    case "const":
                        {
                            // Locate the end of the variable declaration
                            int i = 1;
                            while (!tokens[i].Value.Equals(";"))
                            {
                                if (i == tokens.Count - 1)
                                {
                                    Logger.LogError(new SyntaxError(
                                        "Missing ';' at (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")!!!"
                                        ));
                                }
                                i++;
                            }

                            codeNode.Children.Add(GetVarDeclaration(tokens.GetRange(0, i), codeNode));
                            tokens.RemoveRange(0, i + 1); // Including ';'
                            break;
                        }
                    default:
                        {                            
                            int end_i = LocateStatementEnd(tokens); // Locate the end of the statement
                            codeNode.Children.Add(GetStatement(tokens.GetRange(0, end_i), codeNode));
                            tokens.RemoveRange(0, end_i + 1); // Including ';' or 'end'
                            break;
                        }
                }
            }

            return codeNode;
        }
        
        private ASTNode GetModule(List<Token> tokens, ASTNode parent) // module Identifier { VarDeclaration | Routine } end
        {
            ASTNode moduleNode = new ASTNode(parent, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.MODULE);
            moduleNode.Children.Add(new ASTNode(moduleNode, new List<ASTNode>(), tokens[0], ASTNode.ASTNodeType.IDENTIFIER));
            tokens.RemoveAt(0);

            while(tokens.Count > 0)
            {
                switch(tokens[0].Value)
                {
                    case "int":
                    case "byte":
                    case "short":
                    case "const": // VarDeclaration
                        {
                            // Locate the end of the declaration
                            int end = 0;
                            while (end < tokens.Count && !tokens[end].Value.Equals(";")) { end++; }
                            if (end == tokens.Count)
                            {
                                Logger.LogError(new SyntaxError(
                                    "Missing ';' at (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")!!!"
                                ));
                            }

                            moduleNode.Children.Add(GetVarDeclaration(tokens.GetRange(0, end), moduleNode));
                            tokens.RemoveRange(0, end + 1);
                            break;
                        }
                    case "entry":
                    case "start":
                    case "routine": // Routine
                        {
                            int end_i = LocateRoutineEnd(tokens); // Locate the end of the routine
                            moduleNode.Children.Add(GetRoutine(tokens.GetRange(0, end_i), moduleNode));
                            tokens.RemoveRange(0, end_i + 1);
                            break;
                         }
                    default:
                        {
                            Logger.LogError(new SyntaxError(
                                "Unexpected token at (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")!!!"
                                ));
                            break;
                        }
                }
            }

            return moduleNode;
        }

        private ASTNode GetRoutine(List<Token> tokens, ASTNode parent) // [ Attribute ] routine Identifier [ Parameters ] [ Results ] ( ; | RoutineBody )
        {
            ASTNode routineNode = new ASTNode(parent, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.ROUTINE);
            bool isInterfaceRoutine = false;
            Token errorAnchor = tokens[0];

            if (!tokens[0].Value.Equals("routine")) // 'entry' / 'start'
            {
                routineNode.Children.Add(new ASTNode(routineNode, new List<ASTNode>(), tokens[0], ASTNode.ASTNodeType.ATTRIBUTE));
                if (tokens[0].Value.Equals("start")) isInterfaceRoutine = true;
                tokens.RemoveAt(0);
            }

            routineNode.Children.Add(new ASTNode(routineNode, new List<ASTNode>(), tokens[1], ASTNode.ASTNodeType.IDENTIFIER));
            tokens.RemoveRange(0, 2);

            if (tokens.Count > 0)
            {
                if (tokens[0].Value.Equals("(")) // Has parameters
                {
                    int paramEndIndex = 0;
                    while (!tokens[++paramEndIndex].Value.Equals(")")) ;
                    routineNode.Children.Add(GetParameters(tokens.GetRange(1, paramEndIndex - 1), routineNode));
                    tokens.RemoveRange(0, paramEndIndex + 1);
                }

                if (tokens.Count > 0)
                {
                    if (tokens[0].Value.Equals(":")) // Has results
                    {
                        int resultsEndIndex = 0;
                        while (resultsEndIndex < tokens.Count && !tokens[resultsEndIndex].Value.Equals(";") && !tokens[resultsEndIndex].Value.Equals("do")) { resultsEndIndex++; }
                        routineNode.Children.Add(GetResults(tokens.GetRange(1, resultsEndIndex - 1), routineNode));
                        tokens.RemoveRange(0, resultsEndIndex);
                    }

                    if (!isInterfaceRoutine && tokens.Count > 0) // Has 'do' 'end' block
                    {
                        routineNode.Children.Add(GetRoutineBody(tokens.GetRange(1, tokens.Count - 1), routineNode));                        
                    }
                    else if (isInterfaceRoutine && tokens.Count > 0)
                    {
                        Logger.LogError(new SyntaxError(
                            "Routines with attribute 'start' may not have a routine body! At (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")"
                            ));
                    }
                    else if (!isInterfaceRoutine && tokens.Count == 0)
                    {
                        Logger.LogError(new SyntaxError(
                            "Routines without any attribute or with attribute 'entry' should have a routine body! At (" + errorAnchor.Position.Line + ", " + errorAnchor.Position.Character + ")"
                            ));
                    }
                }    
            }
            else if (!isInterfaceRoutine)
            {
                Logger.LogError(new SyntaxError(
                            "Routines without any attribute or with attribute 'entry' should have a routine body! At (" + errorAnchor.Position.Line + ", " + errorAnchor.Position.Character + ")"
                            ));
            }

            return routineNode;
        }

        private ASTNode GetParameters(List<Token> tokens, ASTNode parent) // ( Parameter { , Parameter } )
        {
            ASTNode paramsNode = new ASTNode(parent, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.PARAMETERS);

            int lastCommaIndex = -1;
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].Value.Equals(","))
                {
                    paramsNode.Children.Add(GetParameter(tokens.GetRange(lastCommaIndex + 1, i - lastCommaIndex - 1), paramsNode));
                    lastCommaIndex = i;
                }
            }
            paramsNode.Children.Add(GetParameter(tokens.GetRange(lastCommaIndex + 1, tokens.Count - lastCommaIndex - 1), paramsNode));

            return paramsNode;
        }

        private ASTNode GetParameter(List<Token> tokens, ASTNode parent) // Type Identifier | Register
        {
            ASTNode paramNode = new ASTNode(parent, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.PARAMETER);

            if (tokens[0].Type == TokenType.REGISTER)
            {
                paramNode.Children.Add(new ASTNode(paramNode, new List<ASTNode>(), tokens[0], ASTNode.ASTNodeType.REGISTER));
            }
            else
            {
                paramNode.Children.Add(new ASTNode(paramNode, new List<ASTNode>(), tokens[0], ASTNode.ASTNodeType.TYPE));
                paramNode.Children.Add(new ASTNode(paramNode, new List<ASTNode>(), tokens[1], ASTNode.ASTNodeType.IDENTIFIER));
            }

            return paramNode;
        }

        private ASTNode GetResults(List<Token> tokens, ASTNode parent) // : Register { , Register }
        {
            ASTNode resultsNode = new ASTNode(parent, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.RESULTS);

            foreach (Token t in tokens)
            {
                if (t.Type == TokenType.REGISTER)
                {
                    resultsNode.Children.Add(new ASTNode(resultsNode, new List<ASTNode>(), t, ASTNode.ASTNodeType.REGISTER));
                }
            }

            return resultsNode;
        }

        private ASTNode GetRoutineBody(List<Token> tokens, ASTNode parent) // do { VarDeclaration | Statement } end
        {
            ASTNode routineBodyNode = new ASTNode(parent, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.ROUTINE_BODY);

            while (tokens.Count > 0)
            {
                switch (tokens[0].Value)
                {
                    case "int":
                    case "byte":
                    case "short":
                    case "const":
                        {
                            // Locate the end of the variable declaration
                            int i = 1;
                            while (!tokens[i].Value.Equals(";"))
                            {
                                if (i == tokens.Count - 1)
                                {
                                    Logger.LogError(new SyntaxError(
                                        "Missing ';' at (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")!!!"
                                        ));
                                }
                                i++;
                            }

                            routineBodyNode.Children.Add(GetVarDeclaration(tokens.GetRange(0, i), routineBodyNode));
                            tokens.RemoveRange(0, i + 1); // Including ';'
                            break;
                        }
                    default:
                        {
                            int end_i = LocateStatementEnd(tokens); // Locate the end of the statement
                            routineBodyNode.Children.Add(GetStatement(tokens.GetRange(0, end_i), routineBodyNode));
                            tokens.RemoveRange(0, end_i + 1); // Including ';' or 'end'
                            break;
                        }
                }
            }

            return routineBodyNode;
        }

        private ASTNode GetVarDeclaration(List<Token> tokens, ASTNode parent) // Variable | Constant
        {
            ASTNode varDeclNode = new ASTNode(parent, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.VARIABLE_DECLARATION);

            if (tokens[0].Value.Equals("const"))
            {
                ASTNode constantNode = new ASTNode(varDeclNode, new List<ASTNode>(), tokens[0], ASTNode.ASTNodeType.CONSTANT);
                varDeclNode.Children.Add(constantNode);

                if (tokens.Count < 2 || tokens[1].Type != TokenType.IDENTIFIER)
                {
                    Logger.LogError(new SyntaxError(
                        "Missing constant definition at (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")!!!"
                        ));
                }

                constantNode.Children.AddRange(GetConstDefinitions(tokens.GetRange(1, tokens.Count - 1), constantNode));
            }
            else
            {
                ASTNode variableNode = new ASTNode(varDeclNode, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.VARIABLE);
                varDeclNode.Children.Add(variableNode);

                ASTNode typeNode = new ASTNode(variableNode, new List<ASTNode>(), tokens[0], ASTNode.ASTNodeType.TYPE);
                variableNode.Children.Add(typeNode);

                if (tokens.Count < 2 || tokens[1].Type != TokenType.IDENTIFIER)
                {
                    Logger.LogError(new SyntaxError(
                        "Missing variable definition at (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")!!!"
                        ));
                }

                variableNode.Children.AddRange(GetVarDefinitions(tokens.GetRange(1, tokens.Count - 1), variableNode));
            }

            return varDeclNode;
        }
        
        private List<ASTNode> GetVarDefinitions(List<Token> tokens, ASTNode parent) // VarDefinition {, VarDefinition}
        {
            List<ASTNode> lst = new List<ASTNode>();

            List<Token> tokensToPass = new List<Token>();
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].Value.Equals(","))
                {
                    lst.Add(GetVarDefinition(tokensToPass, parent));
                    tokensToPass.Clear();
                }
                else
                {
                    tokensToPass.Add(tokens[i]);
                }
            }
            lst.Add(GetVarDefinition(tokensToPass, parent));

            return lst;
        }        
        private ASTNode GetVarDefinition(List<Token> tokens, ASTNode parent) // Identifier [ := Expression ] | Identifier [ Expression ]
        {
            if (tokens[0].Type != TokenType.IDENTIFIER)
            {
                Logger.LogError(new SyntaxError(
                        "Missing identifier at (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")!!!"
                        ));
            }

            ASTNode varDefNode = new ASTNode(parent, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.VAR_DEFINITION);            

            if (tokens.Count < 2) // If no definition
            {
                varDefNode.Children.Add(new ASTNode(varDefNode, new List<ASTNode>(), tokens[0], ASTNode.ASTNodeType.IDENTIFIER));
                return varDefNode;
            }
            else
            {
                if (tokens[1].Type == TokenType.DELIMITER) // Identifier [ Expression ]
                {
                    ASTNode arrayDeclNode = new ASTNode(varDefNode, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.ARRAY_DECLARATION);
                    arrayDeclNode.Children.Add(new ASTNode(arrayDeclNode, new List<ASTNode>(), tokens[0], ASTNode.ASTNodeType.IDENTIFIER));
                    arrayDeclNode.Children.Add(GetExpression(tokens.GetRange(2, tokens.Count - 3), arrayDeclNode)); // Pass just expression tokens
                    varDefNode.Children.Add(arrayDeclNode);
                }
                else // Identifier [ := Expression ]
                {
                    varDefNode.Children.Add(new ASTNode(varDefNode, new List<ASTNode>(), tokens[0], ASTNode.ASTNodeType.IDENTIFIER));
                    varDefNode.Children.Add(GetExpression(tokens.GetRange(2, tokens.Count - 2), varDefNode)); // Pass just expression tokens
                }

                return varDefNode;
            }
        }
        
        private List<ASTNode> GetConstDefinitions(List<Token> tokens, ASTNode parent) // ConstDefinition {, ConstDefinition}
        {
            List<ASTNode> lst = new List<ASTNode>();

            List<Token> tokensToPass = new List<Token>();
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].Value.Equals(","))
                {
                    lst.Add(GetConstDefinition(tokensToPass, parent));
                    tokensToPass.Clear();
                }
                else
                {
                    tokensToPass.Add(tokens[i]);
                }
            }
            lst.Add(GetConstDefinition(tokensToPass, parent));

            return lst;
        }        
        private ASTNode GetConstDefinition(List<Token> tokens, ASTNode parent) // Identifier = Expression
        {
            if (tokens[0].Type != TokenType.IDENTIFIER)
            {
                Logger.LogError(new SyntaxError(
                        "Missing identifier at (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")!!!"
                        ));
            }

            ASTNode constDefNode = new ASTNode(parent, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.CONST_DEFINITION);
            ASTNode id = new ASTNode(constDefNode, new List<ASTNode>(), tokens[0], ASTNode.ASTNodeType.IDENTIFIER);
            constDefNode.Children.Add(id);

            if (!tokens[1].Value.Equals("="))
            {
                Logger.LogError(new SyntaxError(
                        "Missing '=' at (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")!!!"
                        ));
            }

            constDefNode.Children.Add(GetExpression(tokens.GetRange(2, tokens.Count - 2), constDefNode));

            return constDefNode;
        }
        
        private ASTNode GetExpression(List<Token> tokens, ASTNode parent) // Operand [ Operator Operand ]
        {
            ASTNode expNode = new ASTNode(parent, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.EXPRESSION);

            List<Token> temp = new List<Token>();
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].Type != TokenType.OPERATOR)
                {
                    if (tokens[i].Type != TokenType.WHITESPACE)
                        temp.Add(tokens[i]);
                }
                else
                {
                    if ((tokens[i].Value.Equals("&") || tokens[i].Value.Equals("*")) && tokens[i + 1].Type != TokenType.WHITESPACE) // Reference special case
                    {
                        expNode.Children.Add(GetOperand(tokens.GetRange(i, 2), expNode));
                        i++;
                    }
                    else
                    {
                        if (temp.Count > 0)
                        {
                            expNode.Children.Add(GetOperand(temp, expNode));
                            temp.Clear();
                        }
                        expNode.Children.Add(GetOperator(tokens[i], expNode));                        
                    }
                }
            }
            if (temp.Count > 0)
            {
                expNode.Children.Add(GetOperand(temp, expNode));
                temp.Clear();
            }

            return expNode;
        }

        private ASTNode GetOperator(Token token, ASTNode parent) // + | - | * | & | | | ^ | ? | CompOperator
        {
            if (token.Type != TokenType.OPERATOR)
            {
                Logger.LogError(new SyntaxError(
                        "Missing operator at (" + token.Position.Line + ", " + token.Position.Character + ")!!!"
                        ));
            }

            switch (token.Value)
            {
                case "+":
                case "-":
                case "*":
                case "&":
                case "|":
                case "^":
                case "?":
                    {
                        return new ASTNode(parent, new List<ASTNode>(), token, ASTNode.ASTNodeType.OPERATOR);                        
                    }
                case "=":
                case "/=":
                case "<":
                case ">":
                case "<=":
                case ">=":
                    {
                        ASTNode op = new ASTNode(parent, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.OPERATOR);
                        op.Children.Add(new ASTNode(op, new List<ASTNode>(), token, ASTNode.ASTNodeType.COMPARISON_OPERATOR));
                        return op;
                    }
                default:
                    {
                        Logger.LogError(new SyntaxError(
                            "Unknown operator at (" + token.Position.Line + ", " + token.Position.Character + ")!!!"
                            ));
                        return null;
                    }
            }
        }

        private ASTNode GetOperand(List<Token> tokens, ASTNode parent) // Receiver | Reference | Literal
        {
            ASTNode opNode = new ASTNode(parent, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.OPERAND);

            switch(tokens[0].Type)
            {
                case TokenType.OPERATOR: // Reference
                    {
                        opNode.Children.Add(GetReference(tokens, opNode));
                        return opNode;
                    }
                case TokenType.NUMBER: // Literal
                    {
                        opNode.Children.Add(new ASTNode(opNode, new List<ASTNode>(), tokens[0], ASTNode.ASTNodeType.LITERAL));
                        return opNode;
                    }
                case TokenType.IDENTIFIER: // Receiver
                case TokenType.REGISTER:
                    {
                        opNode.Children.Add(GetReceiver(tokens, opNode));
                        return opNode;
                    }
                default:
                    {
                        Logger.LogError(new SyntaxError(
                            "Syntax error at (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")!!!"
                            ));
                        return null;
                    }
            }
        }

        private ASTNode GetReceiver(List<Token> tokens, ASTNode parent) // Identifier | ArrayAccess | Register
        {
            ASTNode recNode = new ASTNode(parent, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.RECEIVER);

            switch (tokens[0].Type)
            {
                case TokenType.IDENTIFIER: // Identifier | ArrayAccess
                    {
                        if (tokens.Count > 1)
                        {
                            recNode.Children.Add(GetArrayAccess(tokens, recNode));
                        }
                        else
                        {
                            recNode.Children.Add(new ASTNode(recNode, new List<ASTNode>(), tokens[0], ASTNode.ASTNodeType.IDENTIFIER));
                        }

                        return recNode;
                    }
                case TokenType.REGISTER: // Register
                    {                        
                        recNode.Children.Add(new ASTNode(recNode, new List<ASTNode>(), tokens[0], ASTNode.ASTNodeType.REGISTER));
                        return recNode;
                    }
                default:
                    {
                        Logger.LogError(new SyntaxError(
                            "Syntax error at (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")!!!"
                            ));
                        return null;
                    }
            }
        }

        private ASTNode GetReference(List<Token> tokens, ASTNode parent) // & Identifier
        {
            if (tokens.Count != 2 || tokens[0].Type != TokenType.OPERATOR || tokens[1].Type != TokenType.IDENTIFIER)
            {
                Logger.LogError(new SyntaxError(
                    "Incorrect reference at (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")!!!"
                    ));
            }

            ASTNode refNode = new ASTNode(parent, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.REFERENCE);
            refNode.Children.Add(new ASTNode(refNode, new List<ASTNode>(), tokens[1], ASTNode.ASTNodeType.IDENTIFIER));

            return refNode;
        }

        private ASTNode GetArrayAccess(List<Token> tokens, ASTNode parent) // Identifier '[' Expression ']'
        {
            if (tokens.Count < 4)
            {
                Logger.LogError(new SyntaxError(
                    "Incorrect array access at (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")!!!"
                    ));
            }

            if (tokens[0].Type != TokenType.IDENTIFIER)
            {
                Logger.LogError(new SyntaxError(
                    "Missing identifier at (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")!!!"
                    ));
            }

            if (tokens[1].Type != TokenType.DELIMITER)
            {
                Logger.LogError(new SyntaxError(
                    "Missing '[' at (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")!!!"
                    ));
            }

            ASTNode arrAccessNode = new ASTNode(parent, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.ARRAY_ELEMENT);
            arrAccessNode.Children.Add(new ASTNode(arrAccessNode, new List<ASTNode>(), tokens[0], ASTNode.ASTNodeType.IDENTIFIER));
            arrAccessNode.Children.Add(GetExpression(tokens.GetRange(2, tokens.Count - 3), arrAccessNode));

            return arrAccessNode;
        }

        private ASTNode GetStatement(List<Token> tokens, ASTNode parent) // [ Label ] ( AssemblyBlock | ExtensionStatement ) 
        {
            ASTNode stmntNode = new ASTNode(parent, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.STATEMENT);

            if (tokens[0].Value.Equals("<")) // With label
            {
                ASTNode label = new ASTNode(stmntNode, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.LABEL);
                label.Children.Add(new ASTNode(label, new List<ASTNode>(), tokens[1], ASTNode.ASTNodeType.IDENTIFIER));
                stmntNode.Children.Add(label);
                tokens.RemoveRange(0, 3); // Remove '<' ID '>' tokens
            }

            if (tokens[0].Value.Equals("asm"))
            {
                stmntNode.Children.Add(GetAssemblyBlock(tokens, stmntNode));
            }
            else
            {
                stmntNode.Children.Add(GetExtensionStatement(tokens, stmntNode));
            }

            return stmntNode;
        }

        private ASTNode GetAssemblyBlock(List<Token> tokens, ASTNode parent) // asm ( AssemblyStatement; | AssemblyStatement {, AssemblyStatement} end)
        {
            ASTNode asmBlockNode = new ASTNode(parent, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.ASSEMBLER_BLOCK);

            List<Token> temp = new List<Token>();
            for (int i = 1; i < tokens.Count; i++)
            {
                if (tokens[i].Value.Equals(","))
                {
                    asmBlockNode.Children.Add(GetAssemblyStatement(temp, asmBlockNode));
                    temp.Clear();
                }
                else
                {
                    temp.Add(tokens[i]);
                }
            }
            if (temp.Count > 0)
            {
                asmBlockNode.Children.Add(GetAssemblyStatement(temp, asmBlockNode));
                temp.Clear();
            }
            else
            {
                Logger.LogError(new SyntaxError(
                    "Unexpected ',' at (" + tokens[tokens.Count - 1].Position.Line + ", " + tokens[tokens.Count - 1].Position.Character + ")!!!"
                    ));
            }

            return asmBlockNode;
        }

        private ASTNode GetAssemblyStatement(List<Token> tokens, ASTNode parent) // Huge rule
        {
            ASTNode asStmntNode = new ASTNode(parent, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.ASSEMBLER_STATEMENT);

            if (tokens.Count > 1)
            {
                switch (tokens[0].Type)
                {
                    case TokenType.REGISTER: // Basic register operation
                        {
                            if (tokens.Count == 3 && tokens[2].Type == TokenType.REGISTER) // The rest of the rules
                            {
                                asStmntNode.Children.Add(new ASTNode(asStmntNode, new List<ASTNode>(), tokens[0], ASTNode.ASTNodeType.REGISTER)); // Register 1
                                asStmntNode.Children.Add(new ASTNode(asStmntNode, new List<ASTNode>(), tokens[1], ASTNode.ASTNodeType.OPERATOR)); // := / += / ...
                                asStmntNode.Children.Add(new ASTNode(asStmntNode, new List<ASTNode>(), tokens[2], ASTNode.ASTNodeType.REGISTER)); // Register 2

                                return asStmntNode;
                            }
                            else if (tokens.Count > 3 && tokens[2].Value.Equals("*")) // Register := *Register
                            {
                                asStmntNode.Children.Add(new ASTNode(asStmntNode, new List<ASTNode>(), tokens[0], ASTNode.ASTNodeType.REGISTER)); // Register 1
                                asStmntNode.Children.Add(new ASTNode(asStmntNode, new List<ASTNode>(), tokens[1], ASTNode.ASTNodeType.OPERATOR)); // :=
                                asStmntNode.Children.Add(new ASTNode(asStmntNode, new List<ASTNode>(), tokens[2], ASTNode.ASTNodeType.OPERATOR)); // *
                                asStmntNode.Children.Add(new ASTNode(asStmntNode, new List<ASTNode>(), tokens[3], ASTNode.ASTNodeType.REGISTER)); // Register 2

                                return asStmntNode;
                            }
                            else
                            {
                                asStmntNode.Children.Add(new ASTNode(asStmntNode, new List<ASTNode>(), tokens[0], ASTNode.ASTNodeType.REGISTER)); // Register 1
                                asStmntNode.Children.Add(new ASTNode(asStmntNode, new List<ASTNode>(), tokens[1], ASTNode.ASTNodeType.OPERATOR)); // :=
                                asStmntNode.Children.Add(GetExpression(tokens.GetRange(2, tokens.Count - 2), asStmntNode)); // Expression

                                return asStmntNode;
                            }
                        }
                    case TokenType.KEYWORD: // format ( 8 | 16 | 32 ) or if Register goto Register
                        {
                            if (tokens[0].Value.Equals("if"))
                            {
                                asStmntNode.Children.Add(new ASTNode(asStmntNode, new List<ASTNode>(), tokens[1], ASTNode.ASTNodeType.REGISTER)); // Register 1
                                asStmntNode.Children.Add(new ASTNode(asStmntNode, new List<ASTNode>(), tokens[2], ASTNode.ASTNodeType.OPERATOR)); // goto
                                asStmntNode.Children.Add(new ASTNode(asStmntNode, new List<ASTNode>(), tokens[3], ASTNode.ASTNodeType.REGISTER)); // Register 2

                                return asStmntNode;
                            }
                            else
                            {
                                asStmntNode.Children.Add(new ASTNode(asStmntNode, new List<ASTNode>(), tokens[0], ASTNode.ASTNodeType.OPERATOR)); // format
                                if (tokens[2].Value.Equals("8") || tokens[2].Value.Equals("16") || tokens[2].Value.Equals("32"))
                                {
                                    asStmntNode.Children.Add(new ASTNode(asStmntNode, new List<ASTNode>(), tokens[2], ASTNode.ASTNodeType.LITERAL)); // number
                                }
                                else
                                {
                                    Logger.LogError(new SyntaxError(
                                        "Incorrect format at (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")!!! Only 8, 16, or 32 allowed."
                                        ));
                                }

                                return asStmntNode;
                            }
                        }
                    case TokenType.OPERATOR: // *Register := Register
                        {
                            if (tokens.Count != 4)
                            {
                                Logger.LogError(new SyntaxError(
                                    "Incorrect register assignment at (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")!!!"
                                    ));
                            }

                            asStmntNode.Children.Add(new ASTNode(asStmntNode, new List<ASTNode>(), tokens[0], ASTNode.ASTNodeType.OPERATOR)); // *
                            asStmntNode.Children.Add(new ASTNode(asStmntNode, new List<ASTNode>(), tokens[1], ASTNode.ASTNodeType.REGISTER)); // Register 1
                            asStmntNode.Children.Add(new ASTNode(asStmntNode, new List<ASTNode>(), tokens[2], ASTNode.ASTNodeType.OPERATOR)); // :=
                            asStmntNode.Children.Add(new ASTNode(asStmntNode, new List<ASTNode>(), tokens[3], ASTNode.ASTNodeType.REGISTER)); // Register 2

                            return asStmntNode;
                        }
                    default:
                        {
                            Logger.LogError(new SyntaxError(
                                        "Syntax error in register block at (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")!!!"
                                        ));
                            return null;
                        }
                }
            }
            else // 'skip' or 'stop'
            {
                return new ASTNode(parent, new List<ASTNode>(), tokens[0], ASTNode.ASTNodeType.ASSEMBLER_STATEMENT);
            }
        }

        private ASTNode GetExtensionStatement(List<Token> tokens, ASTNode parent) // Assignment | Swap | Call | If | Loop | Break | Goto
        {
            ASTNode extStmntNode = new ASTNode(parent, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.EXTENSION_STATEMENT);

            switch (tokens[0].Type)
            {
                case TokenType.IDENTIFIER: // Call | Assignment | Swap
                    {
                        // locate '(' or ':=' or '<=>'
                        bool openBracketFound = false;
                        bool closeBracketFound = false;
                        for (int i = 0; i < tokens.Count; i++)
                        {
                            if (tokens[i].Value.Equals(":="))
                            {
                                extStmntNode.Children.Add(GetAssignment(tokens, extStmntNode));
                                return extStmntNode;
                            }
                            if (tokens[i].Value.Equals("<=>"))
                            {
                                extStmntNode.Children.Add(GetSwap(tokens, extStmntNode));
                                return extStmntNode;
                            }
                            if (tokens[i].Value.Equals("("))
                            {
                                openBracketFound = true;
                            }
                            if (tokens[i].Value.Equals(")"))
                            {
                                closeBracketFound = true;
                            }
                        }

                        if (openBracketFound && closeBracketFound)
                        {
                            extStmntNode.Children.Add(GetCall(tokens, extStmntNode));
                        }
                        else
                        {
                            Logger.LogError(new SyntaxError(
                                "Unknown statement at (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")!!!"
                                ));
                        }

                        break;
                    }
                case TokenType.KEYWORD: // If | Loop | Break | Goto
                    {
                        switch (tokens[0].Value)
                        {
                            case "if":
                                {
                                    extStmntNode.Children.Add(GetIf(tokens, extStmntNode));
                                    break;
                                }
                            case "for":
                            case "while":
                            case "loop":
                                {
                                    extStmntNode.Children.Add(GetLoop(tokens, extStmntNode));
                                    break;
                                }                           
                            case "break":
                                {
                                    extStmntNode.Children.Add(GetBreak(tokens, extStmntNode));
                                    break;
                                }
                            case "goto":
                                {
                                    extStmntNode.Children.Add(GetGoto(tokens, extStmntNode));
                                    break;
                                }
                            default:
                                {
                                    Logger.LogError(new SyntaxError(
                                        "Unexpected keyword at (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")!!!"
                                        ));
                                    break;
                                }
                        }
                        break;
                    }
                case TokenType.OPERATOR: // Assignment | Swap
                case TokenType.REGISTER:
                    {
                        for (int i = 0; i < tokens.Count; i++)
                        {
                            if (tokens[i].Value.Equals(":="))
                            {
                                extStmntNode.Children.Add(GetAssignment(tokens, extStmntNode));
                                break;
                            }
                            if (tokens[i].Value.Equals("<=>"))
                            {
                                extStmntNode.Children.Add(GetSwap(tokens, extStmntNode));
                                break;
                            }
                        }
                        break;
                    }
                default:
                    {
                        Logger.LogError(new SyntaxError(
                            "Unexpected token type at (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")!!!"
                            ));
                        break;
                    }
            }

            return extStmntNode;
        }

        private ASTNode GetAssignment(List<Token> tokens, ASTNode parent) // Primary := Expression ;
        {
            ASTNode asgmntNode = new ASTNode(parent, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.ASSIGNMENT);

            int i = 0;
            while (i < tokens.Count && !tokens[++i].Value.Equals(":="));
            if (i >= tokens.Count)
            {
                Logger.LogError(new SyntaxError(
                    "Missing ':=' at (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")!!!"
                    ));
            }
            asgmntNode.Children.Add(GetPrimary(tokens.GetRange(0, i), asgmntNode));
            asgmntNode.Children.Add(GetExpression(tokens.GetRange(i + 1, tokens.Count - i - 1), asgmntNode));

            return asgmntNode;
        }

        private ASTNode GetSwap(List<Token> tokens, ASTNode parent) // Primary <=> Primary ;
        {
            ASTNode swapNode = new ASTNode(parent, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.SWAP);

            int i = 0;
            while (i < tokens.Count && !tokens[++i].Value.Equals("<=>"));
            if (i >= tokens.Count)
            {
                Logger.LogError(new SyntaxError(
                    "Missing ':=' at (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")!!!"
                    ));
            }

            swapNode.Children.Add(GetPrimary(tokens.GetRange(0, i), swapNode));
            swapNode.Children.Add(GetPrimary(tokens.GetRange(i + 1, tokens.Count - i - 1), swapNode));

            return swapNode;
        }

        private ASTNode GetCall(List<Token> tokens, ASTNode parent) // [ Identifier. ] Identifier CallArgs ;
        {
            ASTNode callNode = new ASTNode(parent, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.CALL);

            int idNum = 1;
            foreach (Token t in tokens)
            {
                if (t.Value.Equals("(")) break; // When reached arguments list
                if (t.Value.Equals(".")) idNum++;
            }

            for (int i = 0; i < idNum; i++)
            {
                callNode.Children.Add(new ASTNode(callNode, new List<ASTNode>(), tokens[i * 2], ASTNode.ASTNodeType.IDENTIFIER));
            }

            callNode.Children.Add(GetCallArgs(tokens.GetRange((idNum - 1) * 2 + 1, tokens.Count - ((idNum - 1) * 2 + 1)), callNode));

            return callNode;
        }

        private ASTNode GetIf(List<Token> tokens, ASTNode parent) // if Expression do BlockBody ( end | else BlockBody end )
        {
            ASTNode ifNode = new ASTNode(parent, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.IF);

            int i = 0; // index of 'do'
            while (i < tokens.Count && !tokens[++i].Value.Equals("do")) ;
            if (i >= tokens.Count)
            {
                Logger.LogError(new SyntaxError(
                    "Missing 'do' at (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")!!!"
                    ));
            }

            ifNode.Children.Add(GetExpression(tokens.GetRange(1, i - 1), ifNode));

            // Locate 'else' if any
            bool hasElse = false;
            int j; // index of 'else' if any
            for (j = i; j < tokens.Count; j++)
            {
                if (tokens[j].Value.Equals("else"))
                {
                    hasElse = true;
                    break;
                }
            }

            if (hasElse)
            {
                ifNode.Children.Add(GetBlockBody(tokens.GetRange(i + 1, j - i - 1), ifNode));
                ifNode.Children.Add(GetBlockBody(tokens.GetRange(j + 1, tokens.Count - j - 1), ifNode));
            }
            else
            {
                ifNode.Children.Add(GetBlockBody(tokens.GetRange(i + 1, tokens.Count - i - 1), ifNode));
            }

            return ifNode;
        }

        private ASTNode GetLoop(List<Token> tokens, ASTNode parent) // For | While | LoopBody
        {
            ASTNode loopNode = new ASTNode(parent, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.LOOP);

            switch (tokens[0].Value)
            {
                case "for": // for Identifier [ from Expression ] [ to Expression] [ step Expression ] LoopBody
                    {
                        ASTNode forNode = new ASTNode(loopNode, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.FOR);
                        loopNode.Children.Add(forNode);

                        // Locate possible keywords
                        bool hasFrom = false;
                        bool hasTo = false;
                        bool hasStep = false;

                        foreach (Token t in tokens)
                        {
                            if (t.Value.Equals("from")) hasFrom = true;
                            if (t.Value.Equals("to")) hasTo = true;
                            if (t.Value.Equals("step")) hasStep = true;
                            if (t.Value.Equals("loop")) break; // When reached loop
                        }

                        forNode.Children.Add(new ASTNode(forNode, new List<ASTNode>(), tokens[1], ASTNode.ASTNodeType.IDENTIFIER));

                        if (hasFrom)
                        {
                            // Locate 'from' and 'to'/'step'/'loop'
                            int fromIndex = 0;
                            while (fromIndex < tokens.Count && !tokens[++fromIndex].Value.Equals("from")) ;
                            forNode.Children.Add(new ASTNode(forNode, new List<ASTNode>(), tokens[fromIndex], ASTNode.ASTNodeType.OPERATOR)); // To distinguish expressions

                            if (hasTo)
                            {
                                int toIndex = fromIndex;
                                while (toIndex < tokens.Count && !tokens[++toIndex].Value.Equals("to")) ;
                                forNode.Children.Add(GetExpression(tokens.GetRange(fromIndex + 1, toIndex - fromIndex - 1), forNode));
                            }
                            else if (hasStep)
                            {
                                int stepIndex = fromIndex;
                                while (stepIndex < tokens.Count && !tokens[++stepIndex].Value.Equals("step")) ;
                                forNode.Children.Add(GetExpression(tokens.GetRange(fromIndex + 1, stepIndex - fromIndex - 1), forNode));
                            }
                            else
                            {
                                int loopIndex = fromIndex;
                                while (loopIndex < tokens.Count && !tokens[++loopIndex].Value.Equals("loop")) ;
                                forNode.Children.Add(GetExpression(tokens.GetRange(fromIndex + 1, loopIndex - fromIndex - 1), forNode));
                            }
                        }
                        if (hasTo)
                        {
                            // Locate 'to' and 'step'/'loop'
                            int toIndex = 0;
                            while (toIndex < tokens.Count && !tokens[++toIndex].Value.Equals("to")) ;
                            forNode.Children.Add(new ASTNode(forNode, new List<ASTNode>(), tokens[toIndex], ASTNode.ASTNodeType.OPERATOR)); // To distinguish expressions

                            if (hasStep)
                            {
                                int stepIndex = toIndex;
                                while (stepIndex < tokens.Count && !tokens[++stepIndex].Value.Equals("step")) ;
                                forNode.Children.Add(GetExpression(tokens.GetRange(toIndex + 1, stepIndex - toIndex - 1), forNode));
                            }
                            else
                            {
                                int loopIndex = toIndex;
                                while (loopIndex < tokens.Count && !tokens[++loopIndex].Value.Equals("loop")) ;
                                forNode.Children.Add(GetExpression(tokens.GetRange(toIndex + 1, loopIndex - toIndex - 1), forNode));
                            }
                        }
                        if (hasStep)
                        {
                            // Locate 'step' and 'loop'
                            int stepIndex = 0;
                            while (stepIndex < tokens.Count && !tokens[++stepIndex].Value.Equals("step")) ;
                            forNode.Children.Add(new ASTNode(forNode, new List<ASTNode>(), tokens[stepIndex], ASTNode.ASTNodeType.OPERATOR)); // To distinguish expressions

                            int loopIndex = stepIndex;
                            while (loopIndex < tokens.Count && !tokens[++loopIndex].Value.Equals("loop")) ;
                            forNode.Children.Add(GetExpression(tokens.GetRange(stepIndex + 1, loopIndex - stepIndex - 1), forNode));
                        }

                        int i = 0;
                        while (i < tokens.Count && !tokens[++i].Value.Equals("loop")) ;

                        ASTNode loopBody = new ASTNode(forNode, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.LOOP_BODY);
                        loopBody.Children.Add(GetBlockBody(tokens.GetRange(i + 1, tokens.Count - i - 1), loopBody));
                        forNode.Children.Add(loopBody);

                        break;
                    }
                case "while": // while Expression LoopBody
                    {
                        int i = 0;
                        while (i < tokens.Count && !tokens[++i].Value.Equals("loop")) ;

                        ASTNode whileNode = new ASTNode(loopNode, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.WHILE);
                        loopNode.Children.Add(whileNode);

                        whileNode.Children.Add(GetExpression(tokens.GetRange(1, i - 1), whileNode));

                        ASTNode loopBody = new ASTNode(loopNode, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.LOOP_BODY);
                        loopBody.Children.Add(GetBlockBody(tokens.GetRange(i + 1, tokens.Count - i - 1), loopBody));
                        loopNode.Children.Add(loopBody);

                        break;
                    }
                case "loop": // loop BlockBody end
                    {
                        ASTNode loopBody = new ASTNode(loopNode, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.LOOP_BODY);
                        loopBody.Children.Add(GetBlockBody(tokens.GetRange(1, tokens.Count - 1), loopBody));
                        loopNode.Children.Add(loopBody);
                        break;
                    }
                default:
                    {
                        Logger.LogError(new SyntaxError(
                            "Missing loop keyword at (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")!!!"
                            ));
                        break;
                    }
            }

            return loopNode;
        }

        private ASTNode GetBreak(List<Token> tokens, ASTNode parent) // break ;
        {
            return new ASTNode(parent, new List<ASTNode>(), tokens[0], ASTNode.ASTNodeType.BREAK);
        }

        private ASTNode GetGoto(List<Token> tokens, ASTNode parent) // goto Identifier ;
        {
            ASTNode gotoNode = new ASTNode(parent, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.GOTO);
            gotoNode.Children.Add(new ASTNode(gotoNode, new List<ASTNode>(), tokens[1], ASTNode.ASTNodeType.IDENTIFIER));
            return gotoNode;
        }

        private ASTNode GetPrimary(List<Token> tokens, ASTNode parent) // Receiver | Dereference | ExplicitAddress
        {
            ASTNode primaryNode = new ASTNode(parent, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.PRIMARY);

            if (tokens[0].Value.Equals("*")) // Dereference | ExplicitAddress
            {
                if (tokens[1].Type == TokenType.NUMBER) // ExplicitAddress
                {
                    ASTNode expAddrNode = new ASTNode(primaryNode, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.EXPLICIT_ADDRESS);
                    expAddrNode.Children.Add(new ASTNode(expAddrNode, new List<ASTNode>(), tokens[1], ASTNode.ASTNodeType.LITERAL));
                    primaryNode.Children.Add(expAddrNode);
                }
                else // Dereference
                {
                    if (tokens[1].Type == TokenType.REGISTER)
                    {
                        ASTNode derefNode = new ASTNode(primaryNode, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.DEREFERENCE);
                        derefNode.Children.Add(new ASTNode(derefNode, new List<ASTNode>(), tokens[1], ASTNode.ASTNodeType.REGISTER));
                        primaryNode.Children.Add(derefNode);
                    }
                    else
                    {
                        ASTNode derefNode = new ASTNode(primaryNode, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.DEREFERENCE);
                        derefNode.Children.Add(new ASTNode(derefNode, new List<ASTNode>(), tokens[1], ASTNode.ASTNodeType.IDENTIFIER));
                        primaryNode.Children.Add(derefNode);
                    }
                }
            }
            else // Receiver
            {
                primaryNode.Children.Add(GetReceiver(tokens, primaryNode));
            }

            return primaryNode;
        }

        private ASTNode GetCallArgs(List<Token> tokens, ASTNode parent) // ( [ Expression { , Expression } ] )
        {
            ASTNode callArgsNode = new ASTNode(parent, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.CALL_ARGUMENTS);

            if (tokens.Count == 2) // No args
            {
                return callArgsNode;
            }
            else
            {
                int lastComma = 0;
                for (int i = 0; i < tokens.Count; i++)
                {
                    if (tokens[i].Value.Equals(","))
                    {
                        callArgsNode.Children.Add(GetExpression(tokens.GetRange(lastComma + 1, i - lastComma - 1), callArgsNode));
                        lastComma = i;
                    }
                }
                callArgsNode.Children.Add(GetExpression(tokens.GetRange(lastComma + 1, tokens.Count - lastComma - 2), callArgsNode));
            }

            return callArgsNode;
        }

        private ASTNode GetBlockBody(List<Token> tokens, ASTNode parent) // { Statement }
        {
            ASTNode blockBodyNode = new ASTNode(parent, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.BLOCK_BODY);

            while (tokens.Count > 0)
            {
                int end_i = LocateStatementEnd(tokens); // Locate the end of the routine
                blockBodyNode.Children.Add(GetStatement(tokens.GetRange(0, end_i), blockBodyNode));
                tokens.RemoveRange(0, end_i + 1); // Including ';' or 'end'
            }

            return blockBodyNode;
        }

        /// <summary>
        /// Locates an index of the ending token of the statement
        /// </summary>
        /// <param name="tokens">Token list that starts with statement.</param>
        /// <returns>Returns an index of the ending token of the statement</returns>
        private int LocateStatementEnd(List<Token> tokens)
        {
            int end;

            switch (tokens[0].Value)
            {
                case "asm":
                    {
                        end = 0;
                        while (!tokens[end].Value.Equals(";") && !tokens[end].Value.Equals("end")) { end++; }
                        return end;
                    }
                case "<":
                    {
                        return LocateStatementEnd(tokens.GetRange(3, tokens.Count - 3)) + 3;
                    }
                case "break":
                    {
                        return 1; // break ;
                    }
                case "goto":
                    {
                        return 2; // goto Identifier ;
                    }
                case "if":                
                case "loop":               
                    {
                        end = 0;
                        int current = 1;
                        while (current > 0)
                        {
                            end++;
                            if (tokens[end].Value.Equals("asm"))
                            {
                                while (!tokens[end].Value.Equals(";") && !tokens[end].Value.Equals("end")) { end++; }
                                end++;
                            }
                            if (tokens[end].Value.Equals("end"))
                            {
                                current--;
                            }
                            if (
                                tokens[end].Value.Equals("if") ||
                                tokens[end].Value.Equals("loop")
                                )
                            {
                                current++;
                            }                            
                        }
                        return end;
                    }
                case "for":
                case "while":
                    {
                        end = 0;
                        while (!tokens[end].Value.Equals("loop")) { end++; } // Go to the 'loop'

                        int current = 1;
                        while (current > 0 && end < tokens.Count)
                        {
                            end++;
                            if (tokens[end].Value.Equals("asm"))
                            {
                                while (!tokens[end].Value.Equals(";") && !tokens[end].Value.Equals("end")) { end++; }
                                end++;
                            }
                            if (tokens[end].Value.Equals("end"))
                            {
                                current--;
                            }
                            if (
                                tokens[end].Value.Equals("if") ||
                                tokens[end].Value.Equals("loop")
                                )
                            {
                                current++;
                            }
                        }
                        return end;
                    }
                default:
                    {
                        end = 0;
                        while (!tokens[end].Value.Equals(";")) { end++; }
                        return end;
                    }
            }
        }

        /// <summary>
        /// Locates an index of the ending token of the routine
        /// </summary>
        /// <param name="tokens">Token list that starts with routine.</param>
        /// <returns>Returns an index of the ending token of the routine</returns>
        private int LocateRoutineEnd(List<Token> tokens)
        {
            int end = 0;

            while (!tokens[end].Value.Equals(";") && !tokens[end].Value.Equals("do")) { end++; }
            if (tokens[end].Value.Equals(";")) return end;
            
            int current = 1;
            while (current > 0)
            {
                end++;
                if (tokens[end].Value.Equals("asm"))
                {
                    while (!tokens[end].Value.Equals(";") && !tokens[end].Value.Equals("end")) { end++; }
                    end++;
                }
                if (tokens[end].Value.Equals("end"))
                {
                    current--;
                }
                if (
                    tokens[end].Value.Equals("if") ||
                    tokens[end].Value.Equals("loop")
                    )
                {
                    current++;
                }
            }
 
            return end;
        }
    }
}
