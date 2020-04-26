using System.IO;
using ERACompiler.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ERACompilerUnitTests
{
    [TestClass]
    public class SyntaxAnalyzerUnitTests
    {
        [TestMethod]
        public void CodeRuleTests()
        {
            int testsNum = 1;
            for (int i = 1; i < testsNum + 1; i++)
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/code_rule_t_" + i + ".era");
                string expectedCode = File.ReadAllText("tests/syntax_analyzer/code_rule_e_" + i + ".era");
                string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode, actualCode, false, i.ToString());
            }
        }

        [TestMethod]
        public void RoutineRuleTests()
        {
            int testsNum = 6;
            for (int i = 1; i < testsNum + 1; i++)
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/routine_rule_t_" + i + ".era");
                string expectedCode = File.ReadAllText("tests/syntax_analyzer/routine_rule_e_" + i + ".era");
                string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode, actualCode, false, i.ToString());
            }
        }

        [TestMethod]
        public void DataRuleTests()
        {
            int testsNum = 1;
            for (int i = 1; i < testsNum + 1; i++)
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/data_rule_t_" + i + ".era");
                string expectedCode = File.ReadAllText("tests/syntax_analyzer/data_rule_e_" + i + ".era");
                string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode, actualCode, false, i.ToString());
            }
        }

        [TestMethod]
        public void ModuleRuleTests()
        {
            int testsNum = 1;
            for (int i = 1; i < testsNum + 1; i++)
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/module_rule_t_" + i + ".era");
                string expectedCode = File.ReadAllText("tests/syntax_analyzer/module_rule_e_" + i + ".era");
                string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode, actualCode, false, i.ToString());
            }
        }

        [TestMethod]
        public void PragmaRuleTests()
        {
            int testsNum = 2;
            for (int i = 1; i < testsNum + 1; i++)
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/pragma_rule_t_" + i + ".era");
                string expectedCode = File.ReadAllText("tests/syntax_analyzer/pragma_rule_e_" + i + ".era");
                string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode, actualCode, false, i.ToString());
            }            
        }

        [TestMethod]
        public void VariableDeclarationRuleTests()
        {
            int testsNum = 2;
            for (int i = 1; i < testsNum + 1; i++)
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/variable_declaration_rule_t_" + i + ".era");
                string expectedCode = File.ReadAllText("tests/syntax_analyzer/variable_declaration_rule_e_" + i + ".era");
                string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode, actualCode, false, i.ToString());
            }
        }
    }
}
