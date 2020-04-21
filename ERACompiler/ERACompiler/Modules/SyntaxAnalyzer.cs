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
            // Remove all whitespaces since we don't need them
            tokens.RemoveAll(IsTokenWhitespace);

            // If something wrong, compilation stops
            CheckBrackets(tokens);

            ASTNode root = new ASTNode(null, new List<ASTNode>(), emptyToken, ASTNode.ASTNodeType.PROGRAM); // The root node of the tree

            while (HasNextUnit(tokens))
            {
                root.Children.Add(GetNextUnit(tokens, root));
            }

            if (tokens.Count > 0)
            {
                Logger.LogError(new SyntaxError("Unknown units are found!!!"));
                Environment.Exit(0);
            }

            // When no units available - there is nothing to be compiled
            if (root.Children.Count == 0)
            {
                Logger.LogError(new SyntaxError("No units in the program!!!"));
                Environment.Exit(0);
            }

            return root;
        }

        private bool IsTokenWhitespace(Token token)
        {
            return token.Type == TokenType.WHITESPACE;
        }

        private void CheckBrackets(List<Token> tokens)
        {
            List<string> opening = new List<string>() { "[", "(", "<" };
            List<string> closing = new List<string>() { "]", ")", ">" };

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
                        Environment.Exit(0);
                    }
                }
                if (current > 0)
                {
                    Logger.LogError(new SyntaxError("Missing close statement for some \"" + opening[i] + "\" token!!!"));
                    Environment.Exit(0);
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
                tokens[0].Value.Equals("routine");
        }

        // All tokens
        private ASTNode GetNextUnit(List<Token> tokens, ASTNode parent)
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
                                tokens[i].Value.Equals("if")
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
                            Environment.Exit(0);
                        }

                        // Pass only the 'code' related part of the source code down
                        ASTNode codeNode = new ASTNode(
                            parent, 
                            new List<ASTNode>(), 
                            tokens[0], 
                            ASTNode.ASTNodeType.CODE
                            );
                        codeNode.Children.AddRange(GetCodeChildren(tokens.GetRange(1, end_i - 1), codeNode));
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
                            Environment.Exit(0);
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
                                Environment.Exit(0);
                            }
                            i++;
                        }
                        tokens.RemoveRange(0, i + 1);

                        return dataNode;
                    }
                case "module":
                    {
                        return null;
                    }
                case "start":
                case "entry":
                case "routine":
                    {
                        return null;
                    }
                case "pragma":
                    {
                        return null;
                    }
                default:
                    {
                        Logger.LogError(new SyntaxError(
                            "Bad source code structure!!! No annotation, code, data, module, or routine found!"
                            ));
                        Environment.Exit(0);
                        return null;
                    }
            }
        }

        // Only the part of tokens inside 'code' 'end' block
        private List<ASTNode> GetCodeChildren(List<Token> tokens, ASTNode parent)
        {
            List<ASTNode> lst = new List<ASTNode>();

            while(tokens.Count > 0)
            {
                // Make a decision about what rule to follow
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
                                    Environment.Exit(0);
                                }
                                i++;
                            }

                            lst.Add(GetVarDeclaration(tokens.GetRange(0, i), parent));
                            tokens.RemoveRange(0, i + 1); // Including ';'
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

            return lst;
        }

        // Tokens from type or 'const' to ';' 
        private ASTNode GetVarDeclaration(List<Token> tokens, ASTNode parent)
        {
            ASTNode varDeclNode = new ASTNode(parent, new List<ASTNode>(), tokens[0], ASTNode.ASTNodeType.VARIABLE_DECLARATION);

            if (tokens[0].Value.Equals("const"))
            {
                ASTNode constantNode = new ASTNode(varDeclNode, new List<ASTNode>(), tokens[0], ASTNode.ASTNodeType.CONSTANT);
                varDeclNode.Children.Add(constantNode);

                // TODO
                
            }
            else
            {
                ASTNode variableNode = new ASTNode(varDeclNode, new List<ASTNode>(), tokens[0], ASTNode.ASTNodeType.VARIABLE);
                varDeclNode.Children.Add(variableNode);

                ASTNode typeNode = new ASTNode(variableNode, new List<ASTNode>(), tokens[0], ASTNode.ASTNodeType.TYPE);
                variableNode.Children.Add(typeNode);

                if (tokens.Count < 2 || tokens[1].Type != TokenType.IDENTIFIER)
                {
                    Logger.LogError(new SyntaxError(
                        "Missing variable definition at (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")!!!"
                        ));
                    Environment.Exit(0);
                }

                variableNode.Children.AddRange(GetVarDefinitions(tokens.GetRange(1, tokens.Count - 1), variableNode));
            }

            return varDeclNode;
        }

        // Tokens: VarDefinition {, VarDefinition}
        private List<ASTNode> GetVarDefinitions(List<Token> tokens, ASTNode parent)
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

        // Tokens: Identifier [ := Expression ] | Identifier [ Expression ]
        private ASTNode GetVarDefinition(List<Token> tokens, ASTNode parent)
        {
            if (tokens[0].Type != TokenType.IDENTIFIER)
            {
                Logger.LogError(new SyntaxError(
                        "Missing identifier at (" + tokens[0].Position.Line + ", " + tokens[0].Position.Character + ")!!!"
                        ));
                Environment.Exit(0);
            }

            ASTNode varDefNode = new ASTNode(parent, new List<ASTNode>(), tokens[0], ASTNode.ASTNodeType.VAR_DEFINITION);
            ASTNode id = new ASTNode(varDefNode, new List<ASTNode>(), tokens[0], ASTNode.ASTNodeType.IDENTIFIER);
            varDefNode.Children.Add(id);

            if (tokens.Count < 2) // If no definition
            {
                return varDefNode;
            }
            else
            {
                if (tokens[1].Type == TokenType.DELIMITER) // Identifier [ Expression ]
                {
                    varDefNode.Children.Add(GetExpression(tokens.GetRange(2, tokens.Count - 3), varDefNode)); // Pass just expression tokens
                }
                else // Identifier [ := Expression ]
                {

                }

                return varDefNode;
            }
        }

        // Operand [ Operator Operand ]
        private ASTNode GetExpression(List<Token> tokens, ASTNode parent)
        {
            // TODO

            return new ASTNode(parent, new List<ASTNode>(), tokens[0], ASTNode.ASTNodeType.EXPRESSION);
        }
    }
}
