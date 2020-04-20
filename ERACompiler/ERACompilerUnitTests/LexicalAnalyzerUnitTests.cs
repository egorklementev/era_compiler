using System.IO;
using ERACompiler.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ERACompilerUnitTests
{
    [TestClass]
    public class LexicalAnalyzerUnitTests
    {
        [TestMethod]
        public void KeywordsTest1()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("tests/lexical_analyzer/test_1.era");
            string expectedCode = File.ReadAllText("tests/lexical_analyzer/expected_1.era");
            string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode, actualCode);
        }

        [TestMethod]
        public void KeywordsTest2()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("tests/lexical_analyzer/test_2.era");
            string expectedCode = File.ReadAllText("tests/lexical_analyzer/expected_2.era");
            string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode, actualCode);
        }
    }
}
