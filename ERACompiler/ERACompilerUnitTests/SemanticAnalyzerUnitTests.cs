using System.IO;
using ERACompiler;
using ERACompiler.Modules;
using ERACompiler.Utilities;
using ERACompiler.Utilities.Errors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

//[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]
namespace ERACompilerUnitTests
{
    [TestClass]
    public class SemanticAnalyzerUnitTests
    {
        private readonly string pathPrefix = "../../../tests/semantic_analyzer/";

        [ClassInitialize]
        public static void AssemblyInit(TestContext context)
        {
            Program.config = new Config
            {
                ConvertToAsmCode = false,
                ExtendedErrorMessages = false,
                ExtendedSemanticMessages = false
            };
        }

        [TestInitialize]
        public void InitTests()
        {
            Program.currentCompiler = new Compiler();
        }

        [TestMethod]
        public void ComplexTests()
        {
            Assert.IsTrue(CompileFiles("complex"));
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void MultipleDeclarationTest()
        {
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_1.era");
            Program.currentCompiler.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void NoDeclarationTest()
        {
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_2.era");
            Program.currentCompiler.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void NoCodeSegmentTest()
        {
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_3.era");
            Program.currentCompiler.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void BreakIsNotInPlaceTest()
        {
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_4.era");
            Program.currentCompiler.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void ReturnIsNotInPlaceTest()
        {
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_5.era");
            Program.currentCompiler.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void IncorrectFormatTest()
        {
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_6.era");
            Program.currentCompiler.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void DynamicLDATest()
        {
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_7.era");
            Program.currentCompiler.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void NonArrayAccessTest()
        {
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_8.era");
            Program.currentCompiler.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void NegativeIndexTest()
        {
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_9.era");
            Program.currentCompiler.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void IncorrectIndexTest()
        {
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_10.era");
            Program.currentCompiler.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void DynamicConstDefTest1()
        {
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_11.era");
            Program.currentCompiler.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void DynamicConstDefTest2()
        {
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_12.era");
            Program.currentCompiler.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void IncorrectArraySizeTest1()
        {
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_13.era");
            Program.currentCompiler.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void IncorrectArraySizeTest2()
        {
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_14.era");
            Program.currentCompiler.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void IncorrectNumberOfArgumentsTest()
        {
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_15.era");
            Program.currentCompiler.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void ConstantModificationTest()
        {
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_16.era");
            Program.currentCompiler.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void ReturnNoTypeRoutineTest()
        {
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_17.era");
            Program.currentCompiler.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void DynamicLDCTest()
        {
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_18.era");
            Program.currentCompiler.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }
        
        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void NonStructAccessTest()
        {
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_19.era");
            Program.currentCompiler.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void UndeclaredVariableTest()
        {
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_20.era");
            Program.currentCompiler.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void IncorrectNumberOfParamsTest()
        {
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_21.era");
            Program.currentCompiler.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void NoReturnStatementTest()
        {
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_22.era");
            Program.currentCompiler.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        private bool CompileFiles(string test_name)
        {
            int i = 1;
            while (File.Exists(pathPrefix + test_name + "_" + i + ".era"))
            {
                Program.currentCompiler = new Compiler();
                string sourceCode = File.ReadAllText(pathPrefix + test_name + "_" + i + ".era");
                byte[] expectedCode = File.ReadAllBytes(pathPrefix + "expected_compiled_" + test_name + "_" + i + ".bin");
                byte[] actualCode = Program.currentCompiler.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
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
