namespace ERACompiler.Utilities.Errors
{
    public class LexicalErrorException : CompilationErrorException
    {
        public LexicalErrorException(string message) : base(message) { }
    }
}
