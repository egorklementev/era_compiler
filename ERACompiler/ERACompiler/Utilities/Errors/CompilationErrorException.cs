using System;

namespace ERACompiler.Utilities.Errors
{
    public class CompilationErrorException : Exception
    {
        protected string message;

        public CompilationErrorException(string message)
        {
            this.message = message;
        }

        public override string ToString()
        {
            return message;
        }
    }
}
