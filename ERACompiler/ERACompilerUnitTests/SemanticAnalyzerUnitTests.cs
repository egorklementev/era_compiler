using System.IO;
using ERACompiler;
using ERACompiler.Modules;
using ERACompiler.Utilities;
using ERACompiler.Utilities.Errors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ERACompilerUnitTests
{
    [TestClass]
    public class SemanticAnalyzerUnitTests
    {
        private readonly string pathPrefix = "../../../tests/semantic_analyzer/";

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
        public void ComplexTests()
        {
            CompileFiles("complex");
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void MultipleDeclarationTest()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_1.era");
            c.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void NoDeclarationTest()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_2.era");
            c.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void NoCodeSegmentTest()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_3.era");
            c.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void BreakIsNotInPlaceTest()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_4.era");
            c.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void ReturnIsNotInPlaceTest()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_5.era");
            c.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void IncorrectFormatTest()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_6.era");
            c.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void DynamicLDATest()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_7.era");
            c.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void NonArrayAccessTest()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_8.era");
            c.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void NegativeIndexTest()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_9.era");
            c.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void IncorrectIndexTest()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_10.era");
            c.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void DynamicConstDefTest1()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_11.era");
            c.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void DynamicConstDefTest2()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_12.era");
            c.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void IncorrectArraySizeTest1()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_13.era");
            c.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void IncorrectArraySizeTest2()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_14.era");
            c.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void IncorrectNumberOfArgumentsTest()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_15.era");
            c.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void ConstantModificationTest()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_16.era");
            c.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void ReturnNoTypeRoutineTest()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_17.era");
            c.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void DynamicLDCTest()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_18.era");
            c.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }
        
        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void NonStructAccessTest()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_19.era");
            c.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void UndeclaredVariableTest()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_20.era");
            c.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        [TestMethod]
        [ExpectedException(typeof(SemanticErrorException), "Semantic error occured.")]
        public void IncorrectNumberOfParamsTest()
        {
            Compiler c = new Compiler();
            string sourceCode = File.ReadAllText(pathPrefix + "semantic_error_21.era");
            c.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
        }

        private void CompileFiles(string test_name)
        {
            int i = 1;
            while (File.Exists(pathPrefix + test_name + "_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText(pathPrefix + test_name + "_" + i + ".era");
                byte[] expectedCode = File.ReadAllBytes(pathPrefix + "expected_compiled_" + test_name + "_" + i + ".bin");
                byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SEMANTICS);
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
