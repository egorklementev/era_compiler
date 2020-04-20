using System.Collections.Generic;
using ERACompiler.Structures;

namespace ERACompiler.Modules
{
    /// <summary>
    /// The purpose of this class it to check the code for syntax errors and build Abstract Syntax Tree that can be used by Semantic Analyzer.
    /// </summary>
    public class SyntaxAnalyzer
    {
        /// <summary>
        /// Used for initialization of some variables.
        /// </summary>
        public SyntaxAnalyzer()
        {

        }

        /// <summary>
        /// Main function of the class. It checks the structure of the token list for syntax errors and if it 
        /// is correct, returns the root node of the constructed Abstract Syntax Tree.
        /// </summary>
        /// <param name="tokens">The list of tokens from Lexical Analyzer.</param>
        /// <returns>Constructed Abstract Syntax Tree.</returns>
        public ASTEntry BuildAST(List<Token> tokens)
        {
            return null;
        }
    }
}
