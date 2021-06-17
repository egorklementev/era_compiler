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
        public readonly LexicalAnalyzer lexis;         // Used to retrieve tokens from the source code.
        public readonly SyntaxAnalyzer syntax;         // Used to build AST.
        public readonly SemanticAnalyzer semantics;    // Used to check semantics of AST and more.
        public readonly Generator generator;           // Used to generate assembly code given AAST.

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
        public byte[] Compile(string sourceCode, CompilationMode mode)
        {
            switch (mode)
            {
                // Only Lexical Analyzer.
                // Returns a string with all tokens listed.
                case CompilationMode.LEXIS:
                    {
                        List<Token> lst = lexis.GetTokenList(sourceCode);
                        StringBuilder sb = new StringBuilder();
                        sb.Append("{\r\n\t[\r\n");
                        foreach (Token t in lst)
                            sb.Append(t.ToString()).Append(",\r\n");
                        sb.Remove(sb.ToString().LastIndexOf(','), 1);
                        sb.Append("\t]\r\n}");
                        return Encoding.ASCII.GetBytes(sb.ToString());
                    }

                // Lexical and Syntax Analyzers.
                // Returns a string with AST traversion listed.
                case CompilationMode.SYNTAX:
                    {
                        List<Token> lst = lexis.GetTokenList(sourceCode);
                        SyntaxRule.SyntaxResponse synResp = syntax.CheckSyntax(lst);
                        ASTNode ast = synResp.AstNode;
                        return Encoding.ASCII.GetBytes(ast.ToString());
                    }

                // Lexical, Syntax, and Semantic Analyzers.
                // Returns a string with AAST traversion listed.
                case CompilationMode.SEMANTICS:
                    {
                        List<Token> lst = lexis.GetTokenList(sourceCode);
                        SyntaxRule.SyntaxResponse synResp = syntax.CheckSyntax(lst);
                        ASTNode ast = synResp.AstNode;
                        AASTNode aast = semantics.BuildAAST(ast);
                        return Encoding.ASCII.GetBytes(aast.ToString());
                    }

                // Full compilation.
                // Returns actual assembly generated code.
                case CompilationMode.GENERATION:
                    {
                        List<Token> lst = lexis.GetTokenList(sourceCode);
                        SyntaxRule.SyntaxResponse synResp = syntax.CheckSyntax(lst);
                        ASTNode ast = synResp.AstNode;
                        AASTNode aast = semantics.BuildAAST(ast);
                        if (Program.config.ConvertToAsmCode)
                        {
                            return Encoding.ASCII.GetBytes(Generator.GetProgramCodeNodeRoot(aast).ToString());
                        }
                        LinkedList<byte> bytes = CollectBytes(Generator.GetProgramCodeNodeRoot(aast));
                        byte[] toReturn = new byte[bytes.Count];
                        int i = 0;
                        foreach (byte b in bytes)
                        {
                            toReturn[i] = b;
                            i++;
                        }
                        return toReturn;
                    }

                default:
                    return System.Array.Empty<byte>();
            }
        }

        /// <summary>
        /// General compilation function. Returns generated assembly code.
        /// </summary>
        /// <param name="sourceCode">A source code from a file.</param>
        /// <returns>Generated string of assembly commands.</returns>
        public byte[] Compile(string sourceCode)
        {
            return Compile(sourceCode, CompilationMode.GENERATION);
        }

        /// <summary>
        /// Simple DFS to convert the CodeNode tree to the flat linked list of bytes
        /// </summary>
        /// <param name="cn"></param>
        /// <returns></returns>
        private LinkedList<byte> CollectBytes(CodeNode cn)
        {
            if (cn.IsLeaf())
            {
                return cn.Bytes;
            }
            LinkedList<byte> toReturn = new LinkedList<byte>();
            foreach (CodeNode child in cn.Children)
            {
                foreach (byte b in CollectBytes(child))
                {
                    toReturn.AddLast(b);
                }   
            }
            return toReturn;
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
