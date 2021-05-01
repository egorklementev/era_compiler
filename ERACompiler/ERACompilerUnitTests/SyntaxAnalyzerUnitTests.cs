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
        public void PrintRuleTests()
        {
            CompileFiles("print");
        }

        [TestMethod]
        public void GotoRuleTests()
        {
            CompileFiles("goto");
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

        [TestMethod]
        [ExpectedException(typeof(SyntaxErrorException), "Syntax error occured.")]
        public void MissingSemicolonTest()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText(pathPrefix + "syntax_error_1.era");
            c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
        }

        private void CompileFiles(string test_name)
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
        }
    }
}
