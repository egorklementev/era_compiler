namespace ERACompiler.Utilities.Errors
{
    class LexicalErrorException : CompilationErrorException
    {
        public LexicalErrorException(string message) : base(message) { }
    }
}
