using System.IO;
using ERACompiler;
using ERACompiler.Modules;
using ERACompiler.Utilities;
using ERACompiler.Utilities.Errors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ERACompilerUnitTests
{
    [TestClass]
    public class SyntaxAnalyzerUnitTests
    {
        private readonly string pathPrefix = "../../../tests/syntax_analyzer/";


        [TestInitialize]
        public void InitTests()
        {
            Program.config = new Config
            {
                ConvertToAsmCode = false,
                ExtendedErrorMessages = false,
                ExtendedSemanticMessages = false
            };
        }

        [TestMethod]
        public void LabelRuleTests()
        {
            Assert.IsTrue(CompileFiles("label"));
        }

        [TestMethod]
        public void CodeRuleTests()
        {
            Assert.IsTrue(CompileFiles("code"));
        }

        [TestMethod]
        public void RoutineRuleTests()
        {
            Assert.IsTrue(CompileFiles("routine"));
        }

        [TestMethod]
        public void DataRuleTests()
        {
            Assert.IsTrue(CompileFiles("data"));
        }

        [TestMethod]
        public void ModuleRuleTests()
        {
            Assert.IsTrue(CompileFiles("module"));
        }

        [TestMethod]
        public void StructRuleTests()
        {
            Assert.IsTrue(CompileFiles("struct"));
        }

        [TestMethod]
        public void PragmaRuleTests()
        {
            Assert.IsTrue(CompileFiles("pragma"));
        }

        [TestMethod]
        public void PrintRuleTests()
        {
            Assert.IsTrue(CompileFiles("print"));
        }

        [TestMethod]
        public void GotoRuleTests()
        {
            Assert.IsTrue(CompileFiles("goto"));
        }

        [TestMethod]
        public void VariableDeclarationRuleTests()
        {
            Assert.IsTrue(CompileFiles("variable_declaration"));
        }

        [TestMethod]
        public void AssemblyStatementRuleTests()
        {
            Assert.IsTrue(CompileFiles("assembly_statement"));
        }

        [TestMethod]
        public void ExpressionRuleTests()
        {
            Assert.IsTrue(CompileFiles("expression"));
        }

        [TestMethod]
        public void AssignmentRuleTests()
        {
            Assert.IsTrue(CompileFiles("assignment"));
        }

        [TestMethod]
        public void SwapRuleTests()
        {
            Assert.IsTrue(CompileFiles("swap"));
        }

        [TestMethod]
        public void CallRuleTests()
        {
            Assert.IsTrue(CompileFiles("call"));
        }

        [TestMethod]
        public void IfRuleTests()
        {
            Assert.IsTrue(CompileFiles("if"));
        }

        [TestMethod]
        public void LoopRuleTests()
        {
            Assert.IsTrue(CompileFiles("loop"));
        }

        [TestMethod]
        public void BreakRuleTests()
        {
            Assert.IsTrue(CompileFiles("break"));
        }

        [TestMethod]
        public void ComplexTests()
        {
            Assert.IsTrue(CompileFiles("complex"));
        }

        [TestMethod]
        [ExpectedException(typeof(SyntaxErrorException), "Syntax error occured.")]
        public void MissingSemicolonTest()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText(pathPrefix + "syntax_error_1.era");
            c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
        }

        private bool CompileFiles(string test_name)
        {
            int i = 1;
            while (File.Exists(pathPrefix + test_name + "_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText(pathPrefix + test_name + "_" + i + ".era");
                byte[] expectedCode = File.ReadAllBytes(pathPrefix + "expected_compiled_" + test_name + "_" + i + ".bin");
                byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                // Store the compiler output in a file
                File.WriteAllBytes(pathPrefix + "actual_compiled_" + test_name + "_" + i + ".bin", actualCode);
                Assert.AreEqual(expectedCode.Length, actualCode.Length);
                for (int j = 0; j < expectedCode.Length; j++)
                {
                    Assert.AreEqual(expectedCode[j], actualCode[j]);
                }
                i++;
            }
            return true;
        }
    }
}
