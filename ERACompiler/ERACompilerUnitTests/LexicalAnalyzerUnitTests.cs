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
            string sourceCode = File.ReadAllText("tests/lexical_analyzer/keywords_1.era");
            string expectedCode = File.ReadAllText("tests/lexical_analyzer/compiled_keywords_1.era");
            string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode, actualCode);
        }
        [TestMethod]
        public void KeywordsTest2()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("tests/lexical_analyzer/keywords_2.era");
            string expectedCode = File.ReadAllText("tests/lexical_analyzer/compiled_keywords_2.era");
            string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode, actualCode);
        }

        // ----------------------------------------------------------------------------------

        [TestMethod]
        public void OperatorsTest1()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("tests/lexical_analyzer/operators_1.era");
            string expectedCode = File.ReadAllText("tests/lexical_analyzer/compiled_operators_1.era");
            string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode, actualCode);
        }
        [TestMethod]
        public void OperatorsTest2()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("tests/lexical_analyzer/operators_2.era");
            string expectedCode = File.ReadAllText("tests/lexical_analyzer/compiled_operators_2.era");
            string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode, actualCode);
        }

        // ----------------------------------------------------------------------------------

        [TestMethod]
        public void RegistersTest1()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("tests/lexical_analyzer/registers_1.era");
            string expectedCode = File.ReadAllText("tests/lexical_analyzer/compiled_registers_1.era");
            string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode, actualCode);
        }
        [TestMethod]
        public void RegistersTest2()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("tests/lexical_analyzer/registers_2.era");
            string expectedCode = File.ReadAllText("tests/lexical_analyzer/compiled_registers_2.era");
            string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode, actualCode);
        }

        // ----------------------------------------------------------------------------------

        [TestMethod]
        public void DelimitersTest1()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("tests/lexical_analyzer/delimiters_1.era");
            string expectedCode = File.ReadAllText("tests/lexical_analyzer/compiled_delimiters_1.era");
            string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode, actualCode);
        }
        [TestMethod]
        public void DelimitersTest2()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("tests/lexical_analyzer/delimiters_2.era");
            string expectedCode = File.ReadAllText("tests/lexical_analyzer/compiled_delimiters_2.era");
            string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode, actualCode);
        }

        // ----------------------------------------------------------------------------------

        [TestMethod]
        public void WhitespacesTest1()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("tests/lexical_analyzer/whitespaces_1.era");
            string expectedCode = File.ReadAllText("tests/lexical_analyzer/compiled_whitespaces_1.era");
            string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode, actualCode);
        }

        // ----------------------------------------------------------------------------------

        [TestMethod]
        public void CodeTest1()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("tests/lexical_analyzer/code_1.era");
            string expectedCode = File.ReadAllText("tests/lexical_analyzer/compiled_code_1.era");
            string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode, actualCode);
        }
    }
}
