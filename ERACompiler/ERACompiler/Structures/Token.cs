using System.Text;

namespace ERACompiler.Structures
{
    /// <summary>
    /// Represents a token which is used by Syntax Analyzer.
    /// </summary>
    public class Token
    {
        /// <summary>
        /// A type that token prepresents.
        /// </summary>
        public TokenType Type { get; set; }
        /// <summary>
        /// A token value. For example, identifier name "arg1", or number 528.
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// A position of the token in the source code in terms of lines and characters.
        /// </summary>
        public TokenPosition Position { get; }

        /// <summary>
        /// Allows to create token instances.
        /// </summary>
        /// <param name="type">The type of the token.</param>
        /// <param name="value">The value token has.</param>
        /// <param name="position">The position of the token in the source code.</param>
        public Token(TokenType type, string value, TokenPosition position)
        {
            Type = type;
            Value = value;
            Position = position;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="other">Token to copy from</param>
        public Token(Token other)
        {
            Type = other.Type;
            Value = other.Value;
            Position = new TokenPosition(other.Position.Line, other.Position.Char);
        }

        /// <summary>
        /// To string
        /// </summary>
        /// <returns>A string representation of a token</returns>
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
    /// Represents a type of a token.
    /// </summary>
    /// <remarks>
    /// DO NOT CHANGE ORDER, ADD TO THE BOTTOM IF NEEDED!!!
    /// </remarks>
    public enum TokenType
    {
        KEYWORD,
        OPERATOR,        
        DELIMITER,
        WHITESPACE,
        REGISTER,
        IDENTIFIER,        
        NUMBER,
        NO_TOKEN
    }

    /// <summary>
    /// Position (in terms of line and characters on the line) of the token in the source code.
    /// </summary>
    public class TokenPosition
    {
        public int Line { get; }       // # of the line
        public int Char { get; }  // # of the char

        public TokenPosition(int line, int charachter)
        {
            Line = line;
            Char = charachter;
        }

        public override string ToString()
        {
            return "Line: " + Line + ", Char: " + Char;
        }
    }
}
