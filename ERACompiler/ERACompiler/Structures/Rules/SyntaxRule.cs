using System.Collections.Generic;
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

        /// <summary>
        /// To store all possible syntax errors
        /// </summary>
        private static readonly List<string> errorList = 
            new List<string>();

        /// <summary>
        /// For getting the error token for debug needs
        /// </summary>
        private static TokenPosition lastTokenPos = new TokenPosition(0, 0);
        private readonly Token noToken = new Token(TokenType.NO_TOKEN, "no_token", new TokenPosition(0, 0));

        /// <summary>
        /// Constructor. Sets default rule name and initializes new rule list.
        /// </summary>
        public SyntaxRule() 
        {
            ruleName = "dummy rule";
            rules = new List<SyntaxRule>();          
        }

        /// <summary>
        /// Sets a name for a rule. Used when printing the AST or AAST.
        /// </summary>
        /// <param name="name">A new name for a rule.</param>
        /// <returns>The object itself.</returns>
        public SyntaxRule SetName(string name)
        {
            ruleName = name;
            return this;
        }

        /// <summary>
        /// Sets the type of a rule (e.g. OR, ONE_OR_MANY, etc.) 
        /// </summary>
        /// <returns>The object itself.</returns>
        public SyntaxRule SetType(SyntaxRuleType type)
        {
            this.type = type;
            return this;
        }

        /// <summary>
        /// Adds a rule to the rule's children.
        /// How these children are processed depends on parent type.
        /// </summary>
        /// <param name="rule">A rule to add.</param>
        /// <returns>The object itself.</returns>
        public SyntaxRule AddRule(SyntaxRule rule)
        {
            rules.Add(rule);
            return this;
        }

        /// <summary>
        /// Main function of this class. Given a list of tokens and parent AST node it returns
        /// a SyntaxResponse instance which contains information about success of failure of syntax
        /// checking, the number of tokens consumed (in the best case scenario should be equal to the 
        /// number of tokens received), and the parent AST node (that was given) filled with appropriate 
        /// AST children nodes according to this rule.
        /// </summary>
        /// <param name="parentTokens"></param>
        /// <param name="parentNode"></param>
        /// <returns></returns>
        public SyntaxResponse Verify(List<Token> parentTokens, ASTNode parentNode)
        {
            // Copy tokens to overcome their removal at upper levels
            List<Token> tokens = new List<Token>();
            foreach (var token in parentTokens)
            {
                tokens.Add(new Token(token));
            }

            // Process according to the rule type
            switch (type)
            {
                // If terminal - just check token type and, if necessary, token value
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

                // If sequence - all children rules should be satisfied by tokens in sequential order
                case SyntaxRuleType.SEQUENCE:
                    int tokensConsumed = 0;
                    ASTNode seqNode = new ASTNode(
                        parentNode, 
                        new List<ASTNode>(), 
                        tokens.Count > 0 ? tokens[0] : noToken, 
                        ruleName
                        );
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

                // If zero or one - all children rules should be satisfied by tokens zero or exactly one time (sequentilly)
                case SyntaxRuleType.ZERO_OR_ONE:
                    int zooNum = 0;
                    int zooTokensConsumed = 0;
                    ASTNode zooNode = new ASTNode(
                        parentNode, 
                        new List<ASTNode>(), 
                        tokens.Count > 0 ? tokens[0] : noToken, 
                        ruleName
                        );
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

                // If zero or more - all children rules should be satisfied by tokens zero or more times (sequentially)
                case SyntaxRuleType.ZERO_OR_MORE:
                    int zomNum = 0;
                    int zomTokensConsumed = 0;
                    ASTNode zomNode = new ASTNode(
                        parentNode, 
                        new List<ASTNode>(), 
                        tokens.Count > 0 ? tokens[0] : noToken, 
                        ruleName
                        );
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

                // If one or more - all children rules should be satisfied by tokens one or more times (sequentially).
                // Used for main (Program) rule only, where we have a set of Units and we do not want empty file to be
                // valid.
                case SyntaxRuleType.ONE_OR_MORE:
                    int oomNum = 0;
                    int oomTokensConsumed = 0;
                    ASTNode oomNode = new ASTNode(
                        parentNode, 
                        new List<ASTNode>(),
                        tokens.Count > 0 ? tokens[0] : noToken,
                        ruleName
                        );
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

                // If or - one or more child rules should be satisfied by the tokens. Rule order does not matter.
                case SyntaxRuleType.OR:
                    ASTNode orNode = new ASTNode(
                        parentNode, 
                        new List<ASTNode>(), 
                        tokens.Count > 0 ? tokens[0] : noToken, 
                        ruleName
                        );
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
            private ASTNode? ast_node;

            /// <summary>
            /// Constructor. Sets success or failure, consumed token number, and oprionally ast node to return.
            /// </summary>
            /// <param name="success">Success or failure of rule checking</param>
            /// <param name="tokensConsumed">How many tokens were consumed</param>
            /// <param name="astNode">The AST node to return</param>
            public SyntaxResponse(bool success, int tokensConsumed, ASTNode? astNode = null)
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
