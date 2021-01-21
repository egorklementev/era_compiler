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
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/code_rule_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/semantic_analyzer/complex_" + i + ".era");
                byte[] expectedCode = File.ReadAllBytes("tests/semantic_analyzer/compiled_complex_" + i + ".bin");
                byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
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
