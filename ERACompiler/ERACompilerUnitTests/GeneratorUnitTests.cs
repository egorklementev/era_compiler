using System.IO;
using ERACompiler;
using ERACompiler.Modules;
using ERACompiler.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ERACompilerUnitTests
{
    [TestClass]
    public class GeneratorUnitTests
    {
        private readonly string pathPrefix = "../../../tests/generator/";

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
        public void NestedForTest()
        {
            CompileFiles("nested_for");
        }

        [TestMethod]
        public void NestedWhileTest()
        {
            CompileFiles("nested_while");
        }

        [TestMethod]
        public void NestedLoopWhileTest()
        {
            CompileFiles("nested_loop_while");
        }

        [TestMethod]
        public void NestedIfTest()
        {
            CompileFiles("nested_if");
        }

        [TestMethod]
        public void IfTest()
        {
            CompileFiles("if");
        }

        [TestMethod]
        public void ArrDefTest()
        {
            CompileFiles("arr_def");
        }

        [TestMethod]
        public void ForTest()
        {
            CompileFiles("for");
        }

        [TestMethod]
        public void ExpressionTest()
        {
            CompileFiles("expression");
        }
        
        [TestMethod]
        public void RoutineTest()
        {
            CompileFiles("routine");
        }

        [TestMethod]
        public void PrintTest()
        {
            CompileFiles("print");
        }

        private void CompileFiles(string test_name, bool asm = false)
        {
            int i = 1;
            while (File.Exists(pathPrefix + test_name + "_" + i + ".era"))
            {
                Compiler c = new Compiler();
                if (asm)
                    Program.config.ConvertToAsmCode = true;
                string sourceCode = File.ReadAllText(pathPrefix + test_name + "_" + i + ".era");
                byte[] expectedCode = File.ReadAllBytes(pathPrefix + "expected_compiled_" + test_name + "_" + i + ".bin");
                byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.GENERATION);
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
