namespace ERACompiler.Utilities.Errors
{
    public class SyntaxErrorException : CompilationErrorException
    {
        public SyntaxErrorException(string message) : base(message) { }
    }
}
