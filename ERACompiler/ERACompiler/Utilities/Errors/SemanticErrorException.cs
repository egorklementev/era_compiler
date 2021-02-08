namespace ERACompiler.Utilities.Errors
{
    class SemanticErrorException : CompilationErrorException
    {
        public SemanticErrorException(string message) : base(message) { }
    }
}
