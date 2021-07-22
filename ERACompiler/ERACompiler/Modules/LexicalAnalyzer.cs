using System;
using System.Collections.Generic;
using ERACompiler.Structures;
using ERACompiler.Utilities;
using ERACompiler.Utilities.Errors;

namespace ERACompiler.Modules
{
    /// <summary>
    /// Contains functionality for conversion of source code of the program to the list of tokens.
    /// </summary>
    public class LexicalAnalyzer
    {
        private const uint IDENTIFIER = 1;
        private const uint LITERAL = 2;
        private const uint OPERATOR = 4;
        private const uint DELIMITER = 8;
        private const uint WHITESPACE = 16;

        private uint lastGuess = 0; // By comparing last token type guess with the current one, we can decide on the exact token type

        private string remembered; // Used for storing previously read characters
        private int lineChar; // Used for token position
        private int lineNumber; // Used for token position

        private HashSet<char> identifierSymbols;
        private HashSet<char> literalSymbols;
        private HashSet<char> operatorSymbols;
        private HashSet<char> delimiterSymbols;
        private HashSet<char> whitespaceSymbols;

        public LexicalAnalyzer()
        {
            remembered = "";
            lineChar = 0;
            lineNumber = 1;

            identifierSymbols = new HashSet<char>();
            for (char c = 'a'; c <= 'z'; c++)
            {
                identifierSymbols.Add(c);
            }

            literalSymbols = new HashSet<char>();
            for (char c = '0'; c <= '9'; c++)
            {
                identifierSymbols.Add(c);
                literalSymbols.Add(c);
            }

            identifierSymbols.Add('_');

            operatorSymbols = new HashSet<char>
            { 
                '+', '-', '*', ':', '&', '|', '^', '<', '>', '?', '/', '='
            };

            delimiterSymbols = new HashSet<char>
            {
                ':', ';', ',', '.', '(', ')', '[', ']', '\"', '/', '@'
            };

            whitespaceSymbols = new HashSet<char>
            {
                ' ', '\t', '\n', '\r'
            };
        }

        /// <summary>
        /// Converts a program source code to the list of tokens.
        /// </summary>
        /// <param name="sourceCode">A source code of a program loaded from the file.</param>
        /// <returns>A list of tokens constructed from the source code.</returns>
        public List<Token> GetTokenList(string sourceCode)
        {
            List<Token> finalList = new List<Token>();

            sourceCode = sourceCode.ToLower();
            sourceCode += Environment.NewLine; // For convenience

            if (sourceCode.Length <= 2)
            {
                throw new LexicalErrorException("Empty file received!!!");
            }

            int rememberedLineNum = 0;
            int rememberedCharNum = 0;
            bool startQuoteFound = false;
            bool commentLineSkip = false;
            foreach (char c in sourceCode)
            {
                lineChar++;

                if (commentLineSkip)
                {
                    if (c == '\n')
                    {
                        lineNumber++;
                        lineChar = 0;
                        commentLineSkip = false;
                        lastGuess = 0;
                        remembered = "";
                    }
                    continue;
                }

                if (startQuoteFound)
                {
                    rememberedCharNum++;
                    if (c == '\n')
                    {
                        lineNumber++;
                        lineChar = 0;
                        remembered += " ";
                        rememberedCharNum -= 2; // ??? donknowwhy
                    }
                    if (c == '\"')
                    {
                        finalList.Add(new Token(TokenType.TEXT, remembered[1..], new TokenPosition(rememberedLineNum, rememberedCharNum - remembered.Length + 1)));
                        finalList.Add(new Token(TokenType.DELIMITER, "\\\"", new TokenPosition(lineNumber, lineChar)));
                        startQuoteFound = false;
                        remembered = "";
                        lastGuess = 0;
                        continue;
                    }
                    else
                    {
                        if (c != '\n' && c != '\r' && c != '\t')
                        {
                            remembered += c;
                        }
                        continue;
                    }
                }

                if (!literalSymbols.Contains(c) && 
                    !identifierSymbols.Contains(c) && 
                    ((lastGuess & LITERAL) > 0 || 
                    (lastGuess & IDENTIFIER) > 0))
                {
                    if (literalSymbols.Contains(remembered[0]))
                    {
                        finalList.Add(new Token(TokenType.NUMBER, remembered, new TokenPosition(lineNumber, lineChar - remembered.Length)));
                        remembered = "";
                        lastGuess = 0;
                    } 
                    else
                    {
                        finalList.Add(new Token(TokenType.IDENTIFIER, remembered, new TokenPosition(lineNumber, lineChar - remembered.Length)));
                        remembered = "";
                        lastGuess = 0;
                    }
                }

                if (!whitespaceSymbols.Contains(c) && (lastGuess & WHITESPACE) > 0)
                {
                    // We do not need whitespace tokens
                    //finalList.Add(new Token(TokenType.WHITESPACE, remembered, new TokenPosition(lineNumber, lineChar)));
                    /*if (remembered == Environment.NewLine)
                    {
                        lineNumber++;
                        lineChar = 1;
                    }*/
                    remembered = "";
                    lastGuess = 0;
                }

                if (!delimiterSymbols.Contains(c) && (lastGuess & DELIMITER) > 0)
                {
                    if (remembered + c == "/=" || remembered + c == ":=")
                    {
                        finalList.Add(new Token(TokenType.OPERATOR, remembered + c, new TokenPosition(lineNumber, lineChar - remembered.Length - 1)));
                        remembered = "";
                        lastGuess = 0;
                        continue;
                    } 
                    else if (remembered == ":")
                    {
                        finalList.Add(new Token(TokenType.DELIMITER, remembered, new TokenPosition(lineNumber, lineChar)));
                        remembered = "";
                        lastGuess = 0;
                    }
                    else
                    {
                        if (remembered == "//")
                        {
                            // We don't need that in the token list
                            //finalList.Add(new Token(TokenType.DELIMITER, remembered, new TokenPosition(lineNumber, lineChar - remembered.Length)));
                            commentLineSkip = true;
                        }
                        remembered = "";
                        lastGuess = 0;
                    }
                }

                if (!operatorSymbols.Contains(c) && (lastGuess & OPERATOR) > 0)
                {
                    finalList.Add(new Token(TokenType.OPERATOR, remembered, new TokenPosition(lineNumber, lineChar - remembered.Length)));
                    remembered = "";
                    lastGuess = 0;
                }

                if (identifierSymbols.Contains(c))
                {
                    lastGuess |= IDENTIFIER;
                }
                if (literalSymbols.Contains(c))
                {
                    lastGuess |= LITERAL;
                }
                if (operatorSymbols.Contains(c))
                {
                    lastGuess |= OPERATOR;
                }
                if (delimiterSymbols.Contains(c))
                {
                    if (c != '/' && c != ':')
                    {
                        finalList.Add(new Token(TokenType.DELIMITER, c.ToString(), new TokenPosition(lineNumber, lineChar)));
                        remembered = "";
                        lastGuess = 0;
                    }
                    if (c == '\"')
                    {
                        finalList[^1].Value = "\\\"";
                        startQuoteFound = !startQuoteFound;
                        rememberedLineNum = lineNumber;
                        rememberedCharNum = lineChar;
                    }
                    lastGuess |= DELIMITER;
                }
                if (whitespaceSymbols.Contains(c))
                {
                    if (c == '\n')
                    {
                        lineNumber++;
                        lineChar = 0;
                    }
                    lastGuess |= WHITESPACE;
                }

                remembered += c;
            }

            // Verify constructed tokens
            HashSet<string> operators = OperatorsInitialization();
            HashSet<string> delimiters = DelimitersInitialization();
            HashSet<string> registers = RegistersInitialization();
            HashSet<string> keywords = KeywordsInitialization();
            foreach (Token t in finalList)
            {
                if (t.Type == TokenType.OPERATOR)
                {
                    if (!operators.Contains(t.Value))
                    {
                        throw new LexicalErrorException("Unexpected token!!!" + Environment.NewLine + $"  At (Line: {t.Position.Line}, Char: {t.Position.Char}).");
                    }
                }
                else if (t.Type == TokenType.DELIMITER)
                {
                    if (!delimiters.Contains(t.Value))
                    {
                        throw new LexicalErrorException("Unexpected token!!!" + Environment.NewLine + $"  At (Line: {t.Position.Line}, Char: {t.Position.Char}).");
                    }
                }
                else if (t.Type == TokenType.IDENTIFIER)
                {
                    if (keywords.Contains(t.Value))
                    {
                        t.Type = TokenType.KEYWORD;
                    }
                    else if (registers.Contains(t.Value))
                    {
                        t.Type = TokenType.REGISTER;
                    }
                }
            }

            return finalList;
        }

        private HashSet<string> KeywordsInitialization()
        {
            return new HashSet<string>() {
                "pragma",
                "module",
                "data",
                "code",
                "routine",
                "return",
                "if",
                "else",
                "do",
                "end",
                "const",
                "int",
                "short",
                "byte",
                "asm",
                "format",
                "skip",
                "stop",
                "for",
                "from",
                "to",
                "step",
                "while",
                "loop",
                "break",
                "goto",
                "struct",
                "print"
            };
        }
        private HashSet<string> OperatorsInitialization()
        {
            return new HashSet<string>() {
                "+",
                "-",
                "*",
                "&",
                "|",
                "^",
                "=",
                "<",
                ">",
                "?",
                "/=",
                ":=",
                "+=",
                "-=",
                ">>=",
                "<<=",
                "|=",
                "&=",
                "^=",
                "<=",
                ">=",
                "?=",
                "<=>",
                "<-",
                "->"
            };
        }
        private HashSet<string> RegistersInitialization()
        {
            HashSet<string> registers = new HashSet<string>();
            for (int i = 0; i < 28; i++)
            {
                registers.Add("r" + i.ToString());
            }
            registers.Add("fp");
            registers.Add("sp");
            registers.Add("sb");
            registers.Add("pc");
            return registers;
        }
        private HashSet<string> DelimitersInitialization()
        {
            return new HashSet<string>()
            {
                ":",
                ";",
                ",",
                ".",
                "(",
                ")",
                "[",
                "]",
                "\\\"",
                "//",
                "@"
            };
        }
    }
}
