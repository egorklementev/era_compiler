using System.IO;
using ERACompiler.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ERACompilerUnitTests
{
    [TestClass]
    public class SyntaxAnalyzerUnitTests
    {
        [TestMethod]
        public void CodeRuleTest1()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("tests/syntax_analyzer/code_rule_test_1.era");
            string expectedCode = File.ReadAllText("tests/syntax_analyzer/code_rule_expected_1.era");
            string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
            Assert.AreEqual(expectedCode, actualCode);
        }

        [TestMethod]
        public void RoutineRuleTest1()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("tests/syntax_analyzer/routine_rule_test_1.era");
            string expectedCode = File.ReadAllText("tests/syntax_analyzer/routine_rule_expected_1.era");
            string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
            Assert.AreEqual(expectedCode, actualCode);
        }

        [TestMethod]
        public void DataRuleTest1()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("tests/syntax_analyzer/data_rule_test_1.era");
            string expectedCode = File.ReadAllText("tests/syntax_analyzer/data_rule_expected_1.era");
            string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
            Assert.AreEqual(expectedCode, actualCode);
        }

        [TestMethod]
        public void ModuleRuleTest1()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("tests/syntax_analyzer/module_rule_test_1.era");
            string expectedCode = File.ReadAllText("tests/syntax_analyzer/module_rule_expected_1.era");
            string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
            Assert.AreEqual(expectedCode, actualCode);
        }

        [TestMethod]
        public void PragmaRuleTest1()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText("tests/syntax_analyzer/pragma_rule_test_1.era");
            string expectedCode = File.ReadAllText("tests/syntax_analyzer/pragma_rule_expected_1.era");
            string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
            Assert.AreEqual(expectedCode, actualCode);
        }
    }
}
