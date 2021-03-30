using System.IO;
using ERACompiler.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ERACompilerUnitTests
{
    [TestClass]
    public class LexicalAnalyzerUnitTests
    {
        [TestMethod, TestCategory("Lexis")]
        public void KeywordsTest1()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("../../../tests/lexical_analyzer/keywords_1.era");
            byte[] expectedCode = File.ReadAllBytes("../../../tests/lexical_analyzer/compiled_keywords_1.bin");
            byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode.Length, actualCode.Length);
            for (int i = 0; i < expectedCode.Length; i++)
            {
                Assert.AreEqual(expectedCode[i], actualCode[i]);
            }
        }
        [TestMethod, TestCategory("Lexis")]
        public void KeywordsTest2()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("../../../tests/lexical_analyzer/keywords_2.era");
            byte[] expectedCode = File.ReadAllBytes("../../../tests/lexical_analyzer/compiled_keywords_2.bin");
            byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode.Length, actualCode.Length);
            for (int i = 0; i < expectedCode.Length; i++)
            {
                Assert.AreEqual(expectedCode[i], actualCode[i]);
            }
        }

        // ----------------------------------------------------------------------------------

        [TestMethod, TestCategory("Lexis")]
        public void OperatorsTest1()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("../../../tests/lexical_analyzer/operators_1.era");
            byte[] expectedCode = File.ReadAllBytes("../../../tests/lexical_analyzer/compiled_operators_1.bin");
            byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode.Length, actualCode.Length);
            for (int i = 0; i < expectedCode.Length; i++)
            {
                Assert.AreEqual(expectedCode[i], actualCode[i]);
            }
        }
        [TestMethod, TestCategory("Lexis")]
        public void OperatorsTest2()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("../../../tests/lexical_analyzer/operators_2.era");
            byte[] expectedCode = File.ReadAllBytes("../../../tests/lexical_analyzer/compiled_operators_2.bin");
            byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode.Length, actualCode.Length);
            for (int i = 0; i < expectedCode.Length; i++)
            {
                Assert.AreEqual(expectedCode[i], actualCode[i]);
            }
        }

        // ----------------------------------------------------------------------------------

        [TestMethod, TestCategory("Lexis")]
        public void RegistersTest1()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("../../../tests/lexical_analyzer/registers_1.era");
            byte[] expectedCode = File.ReadAllBytes("../../../tests/lexical_analyzer/compiled_registers_1.bin");
            byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode.Length, actualCode.Length);
            for (int i = 0; i < expectedCode.Length; i++)
            {
                Assert.AreEqual(expectedCode[i], actualCode[i]);
            }
        }
        [TestMethod, TestCategory("Lexis")]
        public void RegistersTest2()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("../../../tests/lexical_analyzer/registers_2.era");
            byte[] expectedCode = File.ReadAllBytes("../../../tests/lexical_analyzer/compiled_registers_2.bin");
            byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode.Length, actualCode.Length);
            for (int i = 0; i < expectedCode.Length; i++)
            {
                Assert.AreEqual(expectedCode[i], actualCode[i]);
            }
        }

        // ----------------------------------------------------------------------------------

        [TestMethod, TestCategory("Lexis")]
        public void DelimitersTest1()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("../../../tests/lexical_analyzer/delimiters_1.era");
            byte[] expectedCode = File.ReadAllBytes("../../../tests/lexical_analyzer/compiled_delimiters_1.bin");
            byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode.Length, actualCode.Length);
            for (int i = 0; i < expectedCode.Length; i++)
            {
                Assert.AreEqual(expectedCode[i], actualCode[i]);
            }
        }
        [TestMethod, TestCategory("Lexis")]
        public void DelimitersTest2()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("../../../tests/lexical_analyzer/delimiters_2.era");
            byte[] expectedCode = File.ReadAllBytes("../../../tests/lexical_analyzer/compiled_delimiters_2.bin");
            byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode.Length, actualCode.Length);
            for (int i = 0; i < expectedCode.Length; i++)
            {
                Assert.AreEqual(expectedCode[i], actualCode[i]);
            }
        }

        // ----------------------------------------------------------------------------------

        [TestMethod, TestCategory("Lexis")]
        public void WhitespacesTest1()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("../../../tests/lexical_analyzer/whitespaces_1.era");
            byte[] expectedCode = File.ReadAllBytes("../../../tests/lexical_analyzer/compiled_whitespaces_1.bin");
            byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode.Length, actualCode.Length);
            for (int i = 0; i < expectedCode.Length; i++)
            {
                Assert.AreEqual(expectedCode[i], actualCode[i]);
            }
        }

        // ----------------------------------------------------------------------------------

        [TestMethod, TestCategory("Lexis")]
        public void AllRulesTest1()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("../../../tests/lexical_analyzer/all_rules.era");
            byte[] expectedCode = File.ReadAllBytes("../../../tests/lexical_analyzer/compiled_all_rules.bin");
            byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.LEXIS);
            Assert.AreEqual(expectedCode.Length, actualCode.Length);
            for (int i = 0; i < expectedCode.Length; i++)
            {
                Assert.AreEqual(expectedCode[i], actualCode[i]);
            }
        }
    }
}
