namespace ERACompiler.Structures
{
    /// <summary>
    /// Represents a token which is used by Syntax Analyzer.
    /// </summary>
    class Token
    {
        public TokenType Type { get; }
        public string Value { get; }
        public TokenPosition Position { get; }

        public Token(TokenType type, string value, TokenPosition position)
        {
            Type = type;
            Value = value;
            Position = position;
        }

    }

    /// <summary>
    /// Represents a type of a token. Used by Syntax Analyzer.
    /// </summary>
    public enum TokenType
    {
        KEYWORD,
        IDENTIFIER,
        DELIMITER,
        OPERATOR,
        NUMBER,
        REGISTER
    }

    /// <summary>
    /// Position (in terms of line and characters on the line) of the token in the source code.
    /// </summary>
    class TokenPosition
    {
        public int Line { get; }       // # of the line
        public int Character { get; }  // # of the char

        public TokenPosition(int line, int charachter)
        {
            Line = line;
            Character = charachter;
        }
    }
}
