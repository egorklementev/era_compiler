using System.Text;
using System.Collections.Generic;
using ERACompiler.Structures;
using ERACompiler.Structures.Rules;

namespace ERACompiler.Modules
{
    ///<summary> 
    ///The main abstraction class of the compiler. 
    ///</summary>
    public class Compiler
    {
        private readonly LexicalAnalyzer lexis;         // Used to retrieve tokens from the source code.
        private readonly SyntaxAnalyzer syntax;         // Used to build AST.
        private readonly SemanticsAnalyzer semantics;   // Used to check semantics of AST and more.
        private readonly Generator generator;           // Used to generate assembly code given AAST.

        /// <summary>
        /// Constructor for the compiler. Initializates all the modules such as Lexical Analyzer, etc.
        /// </summary>
        public Compiler()
        {
            lexis = new LexicalAnalyzer();
            syntax = new SyntaxAnalyzer();
            semantics = new SemanticsAnalyzer();
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
                // Only Lexical Analyzer.
                // Returns a string with all tokens listed.
                case CompilationMode.LEXIS:
                    {
                        List<Token> lst = lexis.GetTokenList(sourceCode);
                        StringBuilder sb = new StringBuilder();
                        foreach (Token t in lst)
                            sb.Append(t.ToString()).Append("\r\n");
                        return sb.ToString();
                    }

                // Lexical and Syntax Analyzers.
                // Returns a string with AST traversion listed.
                case CompilationMode.SYNTAX:
                    {
                        List<Token> lst = lexis.GetTokenList(sourceCode);
                        SyntaxRule.SyntaxResponse synResp = syntax.CheckSyntax(lst);
                        ASTNode ast = synResp.AstNode;
                        return ast.ToString();
                    }

                // Lexical, Syntax, and Semantic Analyzers.
                // Returns a string with AAST traversion listed.
                case CompilationMode.SEMANTICS:
                    {
                        List<Token> lst = lexis.GetTokenList(sourceCode);
                        SyntaxRule.SyntaxResponse synResp = syntax.CheckSyntax(lst);
                        ASTNode ast = synResp.AstNode;
                        AASTNode aast = semantics.BuildAAST(ast);
                        return aast.ToString();
                    }

                // Full compilation.
                // Returns actual assembly generated code.
                case CompilationMode.GENERATION:
                    {
                        List<Token> lst = lexis.GetTokenList(sourceCode);
                        SyntaxRule.SyntaxResponse synResp = syntax.CheckSyntax(lst);
                        ASTNode ast = synResp.AstNode;
                        AASTNode aast = semantics.BuildAAST(ast);                        
                        return generator.GetAssemblyCode(aast);
                    }

                default:
                    return "";
            }
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
            SEMANTICS,
            GENERATION
        }

    }
}
