using System.Collections.Generic;
using ERACompiler.Structures;
using ERACompiler.Utilities.Errors;

namespace ERACompiler.Structures.Rules
{
    /// <summary>
    /// Represents a single syntax rule presented in the ERA language.
    /// Implements Builder pattern.
    /// </summary>
    public class SyntaxRule
    {
        private const bool FAILED = false;
        private const bool SUCCESS = true;

        private string ruleName;
        private SyntaxRuleType type;
        private readonly List<SyntaxRule> rules;
        
        // To store all possible syntax errors
        private static readonly List<string> errorList = 
            new List<string>();

        /// <summary>
        /// For getting the error token for debug needs
        /// </summary>
        private static TokenPosition lastTokenPos = new TokenPosition(0, 0);
        private readonly Token noToken = new Token(TokenType.NO_TOKEN, "no_token", new TokenPosition(0, 0));

        public SyntaxRule() 
        {
            ruleName = "dummy rule";
            rules = new List<SyntaxRule>();          
        }

        public SyntaxRule SetName(string name)
        {
            ruleName = name;
            return this;
        }

        /// <summary>
        /// Makes this rule terminal.
        /// It means 'rules' contains only one Terminal object
        /// </summary>
        /// <returns></returns>
        public SyntaxRule SetType(SyntaxRuleType type)
        {
            this.type = type;
            return this;
        }

        public SyntaxRule AddRule(SyntaxRule rule)
        {
            rules.Add(rule);
            return this;
        }

        public SyntaxResponse Verify(List<Token> parentTokens, ASTNode parentNode)
        {
            // Copy tokens to overcome their removal at upper levels
            List<Token> tokens = new List<Token>();
            foreach (var token in parentTokens)
            {
                tokens.Add(new Token(token));
            }
            switch (type)
            {
                case SyntaxRuleType.TERMINAL:
                    Token expected = ((RuleTerminal)this).GetToken();
                    if (tokens.Count == 0) 
                    {
                        LogSyntaxError();
                        return new SyntaxResponse(FAILED, 0);
                    }
                    switch (expected.Type)
                    {
                        case TokenType.KEYWORD:
                        case TokenType.OPERATOR:
                        case TokenType.DELIMITER:
                            lastTokenPos = tokens[0].Position;
                            return new SyntaxResponse(
                                expected.Value.Equals(tokens[0].Value),
                                1,
                                new ASTNode(parentNode, new List<ASTNode>(), tokens[0], expected.Type.ToString())
                                );
                        default:
                            lastTokenPos = tokens[0].Position;
                            return new SyntaxResponse(
                                expected.Type == tokens[0].Type, 
                                1,
                                new ASTNode(parentNode, new List<ASTNode>(), tokens[0], expected.Type.ToString())
                                );
                    }
                case SyntaxRuleType.SEQUENCE:
                    int tokensConsumed = 0;
                    ASTNode seqNode = new ASTNode(parentNode, new List<ASTNode>(), noToken, ruleName);
                    foreach (var rule in rules)
                    {
                        SyntaxResponse response = rule.Verify(tokens, seqNode);
                        if (response.Success)
                        {
                            tokensConsumed += response.TokensConsumed;
                            tokens.RemoveRange(0, response.TokensConsumed);
                            seqNode.Children.Add(response.AstNode);
                        }
                        else
                        {
                            return new SyntaxResponse(FAILED, 0);
                        }
                    }
                    errorList.Clear();
                    return new SyntaxResponse(SUCCESS, tokensConsumed, seqNode);
                case SyntaxRuleType.ZERO_OR_ONE:
                    int zooNum = 0;
                    int zooTokensConsumed = 0;
                    ASTNode zooNode = new ASTNode(parentNode, new List<ASTNode>(), noToken, ruleName);
                    while (true)
                    {
                        bool toBreak = false;
                        foreach (var rule in rules)
                        {
                            SyntaxResponse response = rule.Verify(tokens, zooNode);
                            if (response.Success)
                            {
                                zooTokensConsumed += response.TokensConsumed;
                                tokens.RemoveRange(0, response.TokensConsumed);
                                zooNode.Children.Add(response.AstNode);
                            }
                            else
                            {
                                toBreak = true;
                                break;
                            }
                        }
                        if (toBreak) break;
                        zooNum++;
                    }
                    return new SyntaxResponse(zooNum < 2, zooTokensConsumed, zooNode);
                case SyntaxRuleType.ZERO_OR_MORE:
                    int zomNum = 0;
                    int zomTokensConsumed = 0;
                    ASTNode zomNode = new ASTNode(parentNode, new List<ASTNode>(), noToken, ruleName);
                    while (true)
                    {
                        bool toBreak = false;
                        foreach (var rule in rules)
                        {
                            SyntaxResponse response = rule.Verify(tokens, zomNode);
                            if (response.Success)
                            {
                                zomTokensConsumed += response.TokensConsumed;
                                tokens.RemoveRange(0, response.TokensConsumed);
                                zomNode.Children.Add(response.AstNode);
                            }
                            else
                            {
                                toBreak = true;
                                break;
                            }
                        }
                        if (toBreak) break;
                        zomNum++;
                    }
                    return new SyntaxResponse(zomNum >= 0, zomTokensConsumed, zomNode);
                case SyntaxRuleType.ONE_OR_MORE:
                    int oomNum = 0;
                    int oomTokensConsumed = 0;
                    ASTNode oomNode = new ASTNode(parentNode, new List<ASTNode>(), noToken, ruleName);
                    while (true)
                    {
                        bool toBreak = false;
                        foreach (var rule in rules)
                        {
                            SyntaxResponse response = rule.Verify(tokens, oomNode);
                            if (response.Success)
                            {
                                oomTokensConsumed += response.TokensConsumed;
                                tokens.RemoveRange(0, response.TokensConsumed);
                                oomNode.Children.Add(response.AstNode);
                            }
                            else
                            {
                                toBreak = true;
                                break;
                            }
                        }
                        if (toBreak) break;
                        oomNum++;
                    }
                    if (tokens.Count > 0)
                    {
                        return new SyntaxResponse(FAILED, 0);
                    }
                    else
                    {
                        return new SyntaxResponse(oomNum >= 1, oomTokensConsumed, oomNode);
                    }
                case SyntaxRuleType.OR:
                    ASTNode orNode = new ASTNode(parentNode, new List<ASTNode>(), noToken, ruleName);
                    foreach (var rule in rules)
                    {
                        SyntaxResponse response = rule.Verify(tokens, orNode);
                        if (response.Success)
                        {
                            orNode.Children.Add(response.AstNode);
                            return new SyntaxResponse(SUCCESS, response.TokensConsumed, orNode);
                        }
                    }
                    LogSyntaxError();
                    return new SyntaxResponse(FAILED, 0);
                default:
                    break;
            }
            LogSyntaxError();
            return new SyntaxResponse(FAILED, 0);
        }


        private void LogSyntaxError(string errorDescription = "")
        {
            errorList.Add(
                "\tAt (Line: " + lastTokenPos.Line.ToString() + ", Char: " +
                lastTokenPos.Char.ToString() + ").  " +
                "Error at \"" + ruleName + "\".\n" + errorDescription
                );
        }

        /// <summary>
        /// Used for communication with the parent rules.
        /// </summary>
        public class SyntaxResponse
        {
            private bool success = false;
            private int tokensConsumed = 0;
            private ASTNode ast_node;

            public SyntaxResponse(bool success, int tokensConsumed, ASTNode astNode = null)
            {
                Success = success;
                TokensConsumed = tokensConsumed;
                AstNode = astNode;
            }

            public bool Success { get => success; set => success = value; }
            public int TokensConsumed { get => tokensConsumed; set => tokensConsumed = value; }

            public ASTNode AstNode { get => ast_node; set => ast_node = value; }
        }

        public SyntaxError GetErrors()
        {
            string errorMsg = "";
            errorList.Reverse();
            foreach (var err in errorList)
            {
                errorMsg += err;
            }
            return new SyntaxError(errorMsg);
        }

        public enum SyntaxRuleType
        {
            TERMINAL,
            SEQUENCE,
            ZERO_OR_ONE,
            ONE_OR_MORE,           
            ZERO_OR_MORE,           
            OR
        }

    }
}
