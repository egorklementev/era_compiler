namespace ERACompiler.Utilities.Errors
{
    public class SemanticErrorException : CompilationErrorException
    {
        public SemanticErrorException(string message) : base(message) { }
    }
}
