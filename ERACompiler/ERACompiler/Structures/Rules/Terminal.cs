namespace ERACompiler.Structures.Rules
{
    class Terminal : SyntaxRule
    {
        private readonly Token token;

        public Terminal(Token token) 
        {
            this.token = token;
            SetType(SyntaxRuleType.TERMINAL);
        }

        public Token GetToken()
        {
            return token;
        }
    }
}
