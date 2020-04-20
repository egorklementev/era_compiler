namespace ERACompiler.Utilities.Errors
{
    public class CompilationError
    {
        protected string message;

        public CompilationError(string message)
        {
            this.message = message;
        }

        public override string ToString()
        {
            return GetType().ToString() + ":\r\n" + message + "!";
        }
    }
}
