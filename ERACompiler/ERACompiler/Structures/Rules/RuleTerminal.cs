namespace ERACompiler.Structures.Rules
{
    class RuleTerminal : SyntaxRule
    {
        private readonly Token token;

        public RuleTerminal(Token token) 
        {
            this.token = token;
            SetType(SyntaxRuleType.TERMINAL);
            SetName("Terminal: \"" + token.Value + "\"");
        }

        public Token GetToken()
        {
            return token;
        }
    }
}
