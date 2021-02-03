using System.IO;
using ERACompiler.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ERACompilerUnitTests
{
    [TestClass]
    public class SemanticAnalyzerUnitTests
    {
        [TestMethod]
        public void ComplexRuleTests()
        {
            CompileFiles("complex");
        }

        private void CompileFiles(string test_name)
        {
            int i = 1;
            while (File.Exists("tests/semantic_analyzer/" + test_name + "_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/semantic_analyzer/" + test_name + "_" + i + ".era");
                byte[] expectedCode = File.ReadAllBytes("tests/semantic_analyzer/expected_compiled_" + test_name + "_" + i + ".bin");
                byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
                // Store the compiler output in a file
                File.WriteAllBytes("tests/semantic_analyzer/actual_compiled_" + test_name + "_" + i + ".bin", actualCode);
                Assert.AreEqual(expectedCode.Length, actualCode.Length);
                for (int j = 0; j < expectedCode.Length; j++)
                {
                    Assert.AreEqual(expectedCode[j], actualCode[j]);
                }
                i++;
            }
        }
    }
}
