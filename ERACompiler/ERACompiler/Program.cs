using System;
using System.IO;
using ERACompiler.Modules;

namespace ERACompiler
{
    /**
     * Entrance point of the compiler
     */
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: use special folders for compilation of the files in it

            // Name of the file with the source code
            string fileName = "sample_code.era";

            try
            {
                // Loading of sample code
                string sourceCode = File.ReadAllText(fileName);

                // Create instance of the era compiler and get the compiled code
                Compiler eraCompiler = new Compiler();
                string compiledCode = eraCompiler.Compile(sourceCode);

                Console.WriteLine("Compilation has been finished.");

                // Create a new file with the compiled code
                File.WriteAllText("compiled_" + fileName, compiledCode);
            }
            catch (FileNotFoundException)
            {
                throw;
            }
        }
    }
}
