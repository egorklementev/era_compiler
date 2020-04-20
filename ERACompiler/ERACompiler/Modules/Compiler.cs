﻿using System;
using System.Text;
using System.Collections.Generic;
using ERACompiler.Structures;

namespace ERACompiler.Modules
{
    ///<summary> 
    ///The main abstraction class of the compiler. 
    ///</summary>
    public class Compiler
    {
        private LexicalAnalyzer lexis; // Used to retrieve tokens from the source code.
        private SyntaxAnalyzer syntax; // Used to build AST.
        private SemanticAnalyzer semantics; // Used to check semantics of AST and more.
        private Generator generator; // Used to generate assembly code given AAST.

        /// <summary>
        /// Constructor for the compiler. Initializates all the modules such as Lexical Analyzer, etc.
        /// </summary>
        public Compiler()
        {
            lexis = new LexicalAnalyzer();
            syntax = new SyntaxAnalyzer();
            semantics = new SemanticAnalyzer();
            generator = new Generator();
        }

        ///<summary> 
        /// Main compiler function.       
        ///</summary>
        ///<returns>
        /// A string with the compiled source code. It is ready to be written to the file.
        ///</returns>
        public string Compile(string sourceCode, CompilationMode mode)
        {
            switch (mode)
            {
                case CompilationMode.LEXIS:
                    List<Token> lst = lexis.GetTokenList(sourceCode);
                    StringBuilder sb = new StringBuilder();
                    foreach (Token t in lst)                    
                        sb.Append(t.ToString()).Append("\r\n");
                    return sb.ToString();
                case CompilationMode.SYNTAX:
                    break;
                case CompilationMode.SEMANTIC:
                    break;
                case CompilationMode.GENERATION:
                    break;
                default:
                    break;
            }

            // Lexical analyzer           
            // Syntax analyzer
            // Semantic analyzer
            // Generator

            return "";
        }

        /// <summary>
        /// General compilation function. Returns generated assembly code.
        /// </summary>
        /// <param name="sourceCode">A source code from a file.</param>
        /// <returns>Generated string of assembly commands.</returns>
        public string Compile(string sourceCode)
        {
            return Compile(sourceCode, CompilationMode.GENERATION);
        }

        /// <summary>
        /// Represents a level of compilation.
        /// For example syntax means lexical analysis plus syntax analysis.
        /// </summary>
        public enum CompilationMode
        {
            LEXIS,
            SYNTAX,
            SEMANTIC,
            GENERATION
        }

    }
}
