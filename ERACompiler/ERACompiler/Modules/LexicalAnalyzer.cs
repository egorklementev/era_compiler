﻿using System.Collections.Generic;
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
        private string remembered; // Used for storing previously read characters
        private int lineChar; // Used for token position
        private int lineNumber; // Used for token position

        // Lists that contain terminals
        private readonly List<string> registers; // Used separately since registers are initially identified as 
                                                 // identifiers and then are converted to registers
        private readonly List<List<string>> allLists;

        public LexicalAnalyzer()
        {
            remembered = "";
            lineChar = 1;
            lineNumber = 1;

            registers = RegistersInitialization();
            List<string> keywords = KeywordsInitialization();
            List<string> operators = OperatorsInitialization();
            List<string> delimiters = DelimitersInitialization();
            List<string> whitespaces = WhitespacesInitialization();

            allLists = new List<List<string>>() {
                keywords,
                operators,
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
            try
            {
                List<Token> finalList = new List<Token>();

                if (sourceCode.Length <= 2)
                {
                    throw new LexicalErrorException("The source code length should be at least more than two characters!!!");
                }

                sourceCode = sourceCode.ToLower();

                remembered = sourceCode[0] + ""; // Will start traversal from the second character

                bool quoteOccurred = false;

                // Traversing through the characters of the source code
                for (int i = 1; i < sourceCode.Length; i++)
                {
                    char c = sourceCode[i]; // Current character

                    if (IsAbleToDetermineToken(c))
                    {
                        Token t = DetermineToken(c);

                        // Determine line and character numbers
                        if (t.Type == TokenType.WHITESPACE)
                        {
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
                            while (c != '\n' && i < sourceCode.Length)
                            {
                                c = sourceCode[i++];
                            }
                            i--;
                            remembered = "\r\n";
                        }
                        else if (t.Type == TokenType.DELIMITER && t.Value.Equals("\""))
                        {
                            t.Value = "\\\"";
                            if (!quoteOccurred)
                            {
                                quoteOccurred = true;
                                string text = "";
                                int lc = i;
                                finalList.Add(t);
                                c = sourceCode[i++];
                                while (c != '\"' && i < sourceCode.Length)
                                {
                                    text += c;
                                    c = sourceCode[i++];
                                }
                                finalList.Add(new Token(TokenType.TEXT, text, new TokenPosition(lineNumber, lc)));
                                i--;
                                remembered = c.ToString();
                            }
                            else
                            {
                                quoteOccurred = false;
                                finalList.Add(t);
                                remembered = c.ToString();
                            }
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

                return new List<Token>(finalList);
            }
            catch (System.NullReferenceException)
            {
                throw new LexicalErrorException("In file \"" + Program.currentFile + "\": input file malformed!");
            }
        }

        /// <summary>
        /// Used for correction and analysis of the token list.
        /// </summary>
        /// <param name="list">List with tokens to be analyzed.</param>
        /// <returns>Compressed and improved token list.</returns>
        private List<Token> Analyze(List<Token> list)
        {
            LinkedList<Token> llist = new LinkedList<Token>(list);
            var anchor = llist.First;

            while(true)
            {
                if (anchor == null) break;

                Token ti = anchor.Value;                     

                // Special case for ':=' operator.
                if (anchor.Next != null && ti.Type == TokenType.DELIMITER && ti.Value.Equals(":"))
                {
                    if (anchor.Next.Value.Type == TokenType.OPERATOR && anchor.Next.Value.Value.Equals("="))
                    {
                        TokenPosition pos = ti.Position;
                        llist.AddAfter(anchor.Next, new Token(TokenType.OPERATOR, ":=", pos));
                        var next_anchor = anchor.Next.Next;
                        llist.Remove(anchor.Next);
                        llist.Remove(anchor);
                        anchor = next_anchor;
                    }
                }

                // Combine identifier/register/keyword/number tokens into a single token if any
                if (ti.Type == TokenType.IDENTIFIER || ti.Type == TokenType.KEYWORD)
                {
                    var ti_node_iter = anchor;
                    TokenPosition pos = ti_node_iter.Value.Position;
                    TokenType savedType = ti_node_iter.Value.Type;
                    int n = 0;
                    string value = "";
                    while (ti_node_iter != null &&
                        (ti_node_iter.Value.Type == TokenType.NUMBER ||
                        ti_node_iter.Value.Type == TokenType.IDENTIFIER ||
                        ti_node_iter.Value.Type == TokenType.KEYWORD))
                    {
                        value += ti_node_iter.Value.Value;
                        n++;
                        ti_node_iter = ti_node_iter.Next;
                    }
                    for (int k = 0; k < n - 1; k++)
                    {
                        llist.Remove(anchor.Next);
                    }

                    // Special case for registers
                    bool regFound = false;
                    foreach (string reg in registers)
                    {
                        if (value.Equals(reg))
                        {
                            regFound = true;
                            savedType = TokenType.REGISTER;
                        }
                    }

                    llist.AddAfter(anchor, new Token(n > 1 && !regFound ? TokenType.IDENTIFIER : savedType, value, pos));
                    var next_anchor = anchor.Next;
                    llist.Remove(anchor);
                    anchor = next_anchor;
                }

                // Special case for numbers
                if (ti.Type == TokenType.NUMBER)
                {
                    var ti_node_iter = anchor;
                    TokenPosition pos = ti_node_iter.Value.Position;
                    TokenType savedType = ti_node_iter.Value.Type;
                    int n = 0;
                    string value = "";
                    while (ti_node_iter != null && ti_node_iter.Value.Type == TokenType.NUMBER)
                    {
                        value += ti_node_iter.Value.Value;
                        n++;
                        ti_node_iter = ti_node_iter.Next;
                    }
                    for (int k = 0; k < n - 1; k++)
                    {
                        llist.Remove(anchor.Next);
                    }
                    llist.AddAfter(anchor, new Token(n > 1 ? TokenType.NUMBER : savedType, value, pos));
                    var next_anchor = anchor.Next;
                    llist.Remove(anchor);
                    anchor = next_anchor;
                }

                // Special case for '/=' operator
                if (ti.Type == TokenType.OPERATOR && ti.Value.Equals("/="))
                {
                    llist.Remove(anchor.Next);
                }

                anchor = anchor.Next;
            }

            return new List<Token>(llist);
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

            // If it is number or identifier or register
            char fc = remembered[0];
            if ((fc >= 'a' && fc <= 'z') || (fc == '_'))
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
        private List<string> KeywordsInitialization()
        {
            return new List<string>() {
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
        private List<string> OperatorsInitialization()
        {
            return new List<string>() {
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
        private List<string> RegistersInitialization()
        {
            List<string> registers = new List<string>();
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
        private List<string> DelimitersInitialization()
        {
            return new List<string>()
            {
                ":",
                ";",
                ",",
                ".",
                "(",
                ")",
                "[",
                "]",
                "\"",
                "//",
                "@"
            };
        }
        private List<string> WhitespacesInitialization()
        {
            return new List<string>() {
                " ",
                "\t",
                "\n",
                "\r\n"
            };
        }
        
    }
}
