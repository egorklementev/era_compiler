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
    class LexicalAnalyzer
    {
        private string remembered; // Used for storing previously read characters
        private int lineChar; // Used for token position
        private int lineNumber; // Used for token position

        // Lists that contain terminals
        private List<string> keywords;
        private List<string> operators;
        private List<string> registers;
        private List<string> delimiters;
        private List<string> whitespaces;
        private List<List<string>> allLists;

        public LexicalAnalyzer()
        {
            remembered = "";
            lineChar = 1;
            lineNumber = 1;

            KeywordsInitialization();
            OperatorsInitialization();
            RegistersInitialization();
            DelimitersInitialization();
            WhitespacesInitialization();

            allLists = new List<List<string>>() {
                keywords,
                operators,
                registers,
                delimiters,
                whitespaces
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
            
            if (sourceCode.Length <= 2)
            {
                Logger.LogError(new LexicalError("The source code length should be at least more than two characters!!!"));
                Environment.Exit(0);
            }

            remembered = sourceCode[0] + ""; // Will start traversal from the second character

            // Traversing through the characters of the source code
            for (int i = 1; i < sourceCode.Length; i++)
            {
                char c = sourceCode[i]; // Current character

                if (IsAbleToDetermineToken(c))
                {
                    Token t = DetermineToken(c);

                    // Determine line and character numbers
                    if (t.Type == TokenType.WHITESPACE) { 
                        if (t.Value == "\n" || t.Value == "\r\n")
                        {
                            lineChar = 0;
                            lineNumber++;
                        }
                    }

                    // Special case for comments
                    if (t.Type == TokenType.DELIMITER && t.Value.Equals("//"))
                    {
                        lineChar = 0;
                        lineNumber++;
                        while (c != '\n' && i < sourceCode.Length)
                        {
                            c = sourceCode[i++];                            
                        }
                        remembered = "\r\n";
                    }
                    else
                    {
                        finalList.Add(t);
                        remembered = c.ToString();
                    }
                }
                else
                {
                    remembered += c;                    
                }
                lineChar++;
            }

            finalList = Analyze(finalList);

            return finalList;
        }

        /// <summary>
        /// Used for correction and analysis of the token list.
        /// </summary>
        /// <param name="list">List with tokens to be analyzed.</param>
        /// <returns>Compressed and improved token list.</returns>
        private List<Token> Analyze(List<Token> list)
        {          
            for (int i = 0; i < list.Count; i++)
            {
                // Special case for ':=' operator.
                if (i < list.Count - 1 && list[i].Type == TokenType.DELIMITER && list[i].Value.Equals(":"))
                {
                    if (list[i + 1].Type == TokenType.OPERATOR && list[i + 1].Value.Equals("="))
                    {
                        TokenPosition pos = list[i].Position;
                        list.RemoveRange(i, 2);
                        list.Insert(i, new Token(TokenType.OPERATOR, ":=", pos));
                    }
                }
            
                // Combine identifier/register/keyword/number tokens into a single token if any
                if (list[i].Type == TokenType.IDENTIFIER || list[i].Type == TokenType.KEYWORD || list[i].Type == TokenType.REGISTER)
                {
                    int j = i;
                    TokenPosition pos = list[j].Position;
                    TokenType savedType = list[j].Type;
                    int n = 0;
                    string value = "";
                    while (j < list.Count && 
                        (list[j].Type == TokenType.NUMBER || 
                        list[j].Type == TokenType.IDENTIFIER || 
                        list[j].Type == TokenType.KEYWORD || 
                        list[j].Type == TokenType.REGISTER))
                    {
                        value += list[j].Value;
                        n++;
                        j++;
                    }
                    list.RemoveRange(i, n);
                    list.Insert(i, new Token(n > 1 ? TokenType.IDENTIFIER : savedType, value, pos));
                }
            }

            return list;
        }

        /// <summary>
        /// Used for token determination.
        /// </summary>
        /// <returns>Whether or not is it possible to understand what token has to be added to the list.</returns>
        private bool IsAbleToDetermineToken(char c)
        {
            bool matched = false;
            for (int i = 0; i < allLists.Count; i++) {
                bool prev = DoesMatch(remembered, allLists[i]);
                bool next = DoesMatch(remembered + c.ToString(), allLists[i]);
                if (prev != next)
                {
                    return true;
                }
                if (prev && next)
                    matched = true;
            }
            return !matched;
        }
        /// <summary>
        /// Constructs token judjing by the 'remembered' variable.
        /// </summary>
        /// <returns>Retruns the next token for the list.</returns>
        private Token DetermineToken(char c)
        {
            TokenPosition pos = new TokenPosition(lineNumber, lineChar - remembered.Length + 1);

            for (int i = 0; i < allLists.Count; i++)
            {
                if (DoesMatch(remembered, allLists[i]))
                {
                    for (int j = 0; j < allLists[i].Count; j++)
                    {
                        if (remembered.Equals(allLists[i][j]))
                        {
                            return new Token((TokenType)i, remembered, pos);
                        }
                    }
                }
            }

            // If it is number or identifier
            char fc = remembered[0];
            if ((fc >= 'a' && fc <= 'z') || (fc >= 'A' && fc <= 'Z') || (fc == '_'))
            {
                return new Token(TokenType.IDENTIFIER, remembered, pos);
            }
            else if (fc >= '0' && fc <= '9')
            {
                return new Token(TokenType.NUMBER, remembered, pos);
            }

            // If it is comment
            if ((remembered + c).Equals("//"))
            {
                return new Token(TokenType.DELIMITER, remembered + c, pos);
            }

            // Special case for '/=' operator
            if ((remembered + c).Equals("/="))
            {
                return new Token(TokenType.OPERATOR, remembered + c, pos);
            }

            return null;
        }

        /// <summary>
        /// Checks if any of the elements from the given list starts with the given sequence of charachters.
        /// </summary>
        /// <param name="list">A list with strings.</param>
        /// <param name="sequence">A string for checking.</param>
        /// <returns>Whether or not there is a match.</returns>
        private bool DoesMatch(string sequence, List<string> list)
        {
            foreach (string entry in list)            
                if (entry.StartsWith(sequence))                
                    return true;                           
            return false;
        }

        // For convenience
        private void KeywordsInitialization()
        {
            keywords = new List<string>() {
                "pragma",
                "module",
                "data",
                "code",
                "routine",
                "start",
                "entry",
                "if",
                "else",
                "elif",
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
                "goto"
            };
        }
        private void OperatorsInitialization()
        {
            operators = new List<string>() {
                "+",
                "-",
                "*",
                "&",
                "|",
                "^",
                "=",
                "<",
                ">",
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
                "<=>"
            };
        }
        private void RegistersInitialization()
        {
            registers = new List<string>();
            for (int i = 0; i < 32; i++)
            {
                registers.Add("R" + i.ToString());
            }
        }
        private void DelimitersInitialization()
        {
            delimiters = new List<string>()
            {
                ":",
                ";",
                ",",
                ".",
                "(",
                ")",
                "[",
                "]",
                "//"
            };
        }
        private void WhitespacesInitialization()
        {
            whitespaces = new List<string>() {
                " ",
                "\t",
                "\n",
                "\r\n"
            };
        }
        
    }
}
