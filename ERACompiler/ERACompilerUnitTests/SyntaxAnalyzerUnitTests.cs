using System.IO;
using ERACompiler.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ERACompilerUnitTests
{
    [TestClass]
    public class SyntaxAnalyzerUnitTests
    {
        [TestMethod]
        public void LabelRuleTests()
        {
            CompileFiles("label");
        }

        [TestMethod]
        public void CodeRuleTests()
        {
            CompileFiles("code");
        }

        [TestMethod]
        public void RoutineRuleTests()
        {
            CompileFiles("routine");
        }

        [TestMethod]
        public void DataRuleTests()
        {
            CompileFiles("data");
        }

        [TestMethod]
        public void ModuleRuleTests()
        {
            CompileFiles("module");
        }

        [TestMethod]
        public void StructRuleTests()
        {
            CompileFiles("struct");
        }

        [TestMethod]
        public void PragmaRuleTests()
        {
            CompileFiles("pragma");
        }

        [TestMethod]
        public void VariableDeclarationRuleTests()
        {
            CompileFiles("variable_declaration");
        }

        [TestMethod]
        public void AssemblyStatementRuleTests()
        {
            CompileFiles("assembly_statement");
        }

        [TestMethod]
        public void ExpressionRuleTests()
        {
            CompileFiles("expression");
        }

        [TestMethod]
        public void AssignmentRuleTests()
        {
            CompileFiles("assignment");
        }

        [TestMethod]
        public void SwapRuleTests()
        {
            CompileFiles("swap");
        }

        [TestMethod]
        public void CallRuleTests()
        {
            CompileFiles("call");
        }

        [TestMethod]
        public void IfRuleTests()
        {
            CompileFiles("if");
        }

        [TestMethod]
        public void LoopRuleTests()
        {
            CompileFiles("loop");
        }

        [TestMethod]
        public void BreakRuleTests()
        {
            CompileFiles("break");
        }

        [TestMethod]
        public void ComplexTests()
        {
            CompileFiles("complex");
        }

        private void CompileFiles(string test_name)
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/" + test_name + "_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/" + test_name + "_" + i + ".era");
                byte[] expectedCode = File.ReadAllBytes("tests/syntax_analyzer/expected_compiled_" + test_name + "_" + i + ".bin");
                byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                // Store the compiler output in a file
                File.WriteAllBytes("tests/syntax_analyzer/actual_compiled_" + test_name + "_" + i + ".bin", actualCode);
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
