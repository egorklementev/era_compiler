using System.Text;

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

        public override string ToString()
        {
            // To print newline symbols conveniently.
            string finalValue = Value;
            if (finalValue.Equals("\t"))
                finalValue = "\\t";
            if (finalValue.Equals("\n"))
                finalValue = "\\n";
            if (finalValue.Equals("\r\n"))
                finalValue = "\\r\\n";
            if (finalValue.Equals(" "))
                finalValue = "\" \"";

            StringBuilder sb = new StringBuilder();
            sb.Append("Token type: ")
                .Append(Type.ToString())
                .Append("\n")
                .Append("Value: ")
                .Append(finalValue)
                .Append("\n")
                .Append("Position: ")
                .Append(Position.ToString())
                .Append("\n");
            return sb.ToString();
        }

    }

    /// <summary>
    /// Represents a type of a token. Used by Syntax Analyzer.
    /// </summary>
    /// <remarks>
    /// DO NOT CHANGE ORDER, ADD TO THE BOTTOM IF NEEDED!!!
    /// </remarks>
    public enum TokenType
    {
        KEYWORD,
        OPERATOR,
        REGISTER,
        DELIMITER,
        WHITESPACE,
        IDENTIFIER,        
        NUMBER       
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

        public override string ToString()
        {
            return "Line: " + Line + ", Char: " + Character;
        }
    }
}
