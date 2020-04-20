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
            string sourceCode = File.ReadAllText("tests/lexical_analyzer/keywords_test_1.era");
            string expectedCode = File.ReadAllText("tests/lexical_analyzer/keywords_expected_1.era");
            string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode, actualCode);
        }
        [TestMethod]
        public void KeywordsTest2()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("tests/lexical_analyzer/keywords_test_2.era");
            string expectedCode = File.ReadAllText("tests/lexical_analyzer/keywords_expected_2.era");
            string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode, actualCode);
        }

        // ----------------------------------------------------------------------------------

        [TestMethod]
        public void OperatorsTest1()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("tests/lexical_analyzer/operators_test_1.era");
            string expectedCode = File.ReadAllText("tests/lexical_analyzer/operators_expected_1.era");
            string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode, actualCode);
        }
        [TestMethod]
        public void OperatorsTest2()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("tests/lexical_analyzer/operators_test_2.era");
            string expectedCode = File.ReadAllText("tests/lexical_analyzer/operators_expected_2.era");
            string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode, actualCode);
        }

        // ----------------------------------------------------------------------------------

        [TestMethod]
        public void RegistersTest1()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("tests/lexical_analyzer/registers_test_1.era");
            string expectedCode = File.ReadAllText("tests/lexical_analyzer/registers_expected_1.era");
            string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode, actualCode);
        }
        [TestMethod]
        public void RegistersTest2()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("tests/lexical_analyzer/registers_test_2.era");
            string expectedCode = File.ReadAllText("tests/lexical_analyzer/registers_expected_2.era");
            string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode, actualCode);
        }

        // ----------------------------------------------------------------------------------

        [TestMethod]
        public void DelimitersTest1()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("tests/lexical_analyzer/delimiters_test_1.era");
            string expectedCode = File.ReadAllText("tests/lexical_analyzer/delimiters_expected_1.era");
            string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode, actualCode);
        }
        [TestMethod]
        public void DelimitersTest2()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("tests/lexical_analyzer/delimiters_test_2.era");
            string expectedCode = File.ReadAllText("tests/lexical_analyzer/delimiters_expected_2.era");
            string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode, actualCode);
        }

        // ----------------------------------------------------------------------------------

        [TestMethod]
        public void WhitespacesTest1()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("tests/lexical_analyzer/whitespaces_test_1.era");
            string expectedCode = File.ReadAllText("tests/lexical_analyzer/whitespaces_expected_1.era");
            string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode, actualCode);
        }

        // ----------------------------------------------------------------------------------

        [TestMethod]
        public void CodeTest1()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("tests/lexical_analyzer/code_test_1.era");
            string expectedCode = File.ReadAllText("tests/lexical_analyzer/code_expected_1.era");
            string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode, actualCode);
        }
    }
}
