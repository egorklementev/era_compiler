using System;
using System.IO;
using ERACompiler.Modules;
using ERACompiler.Utilities;

namespace ERACompiler
{
    /// <summary>
    /// The entrance point of the compiler.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: use special folders for compilation of the files in it.

            // Name of the file with the source code
            string fileName = "sample.era";

            // Logging
            Logger logger = new Logger(true);

            try
            {
                // Loading of sample code
                string sourceCode = File.ReadAllText(fileName);

                // Create instance of the era compiler and get the compiled code
                Compiler eraCompiler = new Compiler();
                string compiledCode = eraCompiler.Compile(sourceCode, Compiler.CompilationMode.LEXIS);

                Console.WriteLine("Compilation has been finished.");
                Console.ReadLine();

                // Create a new file with the compiled code
                File.WriteAllText("compiled_" + fileName, compiledCode);
            }
            catch (IOException)
            {
                throw;
            }
        }
    }
}
