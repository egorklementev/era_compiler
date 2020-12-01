using System.Collections.Generic;
using ERACompiler.Utilities;
using ERACompiler.Utilities.Errors;

namespace ERACompiler.Structures.Rules
{
    /// <summary>
    /// Represents a single syntax rule presented in the ERA language.
    /// Implements Builder pattern.
    /// </summary>
    class SyntaxRule
    {
        private SyntaxRule? parentRule = null;
        private SyntaxRuleType type;
        private List<SyntaxRule> rules;

        private static TokenPosition lastTokenPos = new TokenPosition(0, 0);

        public SyntaxRule() 
        {
            rules = new List<SyntaxRule>();
        }
        
        public SyntaxRule SetParentRule(SyntaxRule parentRule)
        {
            this.parentRule = parentRule;
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

        public SyntaxRule AddTerminalRule(Token token)
        {
            rules.Add(new Terminal(token));
            return this;
        }

        public bool Verify(List<Token> tokens)
        {
            foreach (SyntaxRule s in rules)
            {
                switch (s.type)
                {
                    case SyntaxRuleType.TERMINAL:
                        // If terminal, look at the first token and compare it with the rule token
                        if (tokens.Count == 0)
                        {
                            Logger.LogError(new SyntaxError(
                                "Missing keyword or special symbol!\n" + 
                                "\tAt (Line: " + lastTokenPos.Line.ToString() 
                                + ", Char: " + lastTokenPos.Char.ToString() + ")."
                                ));
                            return false;
                        }
                        else
                        {
                            lastTokenPos = tokens[0].Position;
                            if (!((Terminal) s).GetToken().Value.Equals(tokens[0].Value))
                            {
                                Logger.LogError(new SyntaxError(
                                    "Unexpected keyword or special symbol!\n" +
                                    "\tAt (Line: " + lastTokenPos.Line.ToString()
                                    + ", Char: " + lastTokenPos.Char.ToString() + ")."
                                    ));
                                return false;
                            }
                        }

                        // If everything is alright, delete token from the list
                        tokens.RemoveAt(0);
                        break;
                    case SyntaxRuleType.ZERO_OR_ONE:
                        return false;
                    case SyntaxRuleType.ZERO_OR_MORE:
                        return false;
                    case SyntaxRuleType.EXACTLY_ONE:
                        return false;
                    case SyntaxRuleType.OR:
                        return false;
                    default:
                        return false;
                }
            }
            return true;
        }

        public enum SyntaxRuleType
        {
            TERMINAL,
            COMPOUND,
            ZERO_OR_ONE,
            ZERO_OR_MORE,
            EXACTLY_ONE,
            OR
        }

    }
}
