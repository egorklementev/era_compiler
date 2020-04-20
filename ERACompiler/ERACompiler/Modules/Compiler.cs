using System;
using System.Collections.Generic;
using ERACompiler.Structures;

namespace ERACompiler.Modules
{
    ///<summary> 
    ///The main abstraction class of the compiler. 
    ///</summary>
    class Compiler
    {
        private LexicalAnalyzer lexis; // Used to retrieve tokens from the source code.
        private SyntaxAnalyzer syntax; // Used to build AST.
        private SemanticAnalyzer semantics; // Used to check semantics of AST and more.
        private Generator generator; // Used to generate assembly code given AAST.

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
        public string Compile(string sourceCode)
        {

            // Lexical analyzer
            List<Token> lst = lexis.GetTokenList(sourceCode);
            foreach (Token t in lst)
            {
                Console.WriteLine(t.ToString());
            }

            // Syntax analyzer
            // Semantic analyzer
            // Generator

            return "";
        }
    }
}
