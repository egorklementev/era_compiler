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
            Assert.IsTrue(CompileFiles("nested_for"));
        }

        [TestMethod]
        public void NestedWhileTest()
        {
            Assert.IsTrue(CompileFiles("nested_while"));
        }

        [TestMethod]
        public void NestedLoopWhileTest()
        {
            Assert.IsTrue(CompileFiles("nested_loop_while"));
        }

        [TestMethod]
        public void NestedIfTest()
        {
            Assert.IsTrue(CompileFiles("nested_if"));
        }

        [TestMethod]
        public void IfTest()
        {
            Assert.IsTrue(CompileFiles("if"));
        }

        [TestMethod]
        public void ArrDefTest()
        {
           Assert.IsTrue(CompileFiles("arr_def"));
        }

        [TestMethod]
        public void ForTest()
        {
            Assert.IsTrue(CompileFiles("for"));
        }

        [TestMethod]
        public void ExpressionTest()
        {
            Assert.IsTrue(CompileFiles("expression"));
        }
        
        [TestMethod]
        public void RoutineTest()
        {
            Assert.IsTrue(CompileFiles("routine"));
        }

        [TestMethod]
        public void PrintTest()
        {
            Assert.IsTrue(CompileFiles("print"));
        }

        [TestMethod]
        public void ComplexTest()
        {
            Assert.IsTrue(CompileFiles("complex"));
        }

        [TestMethod]
        public void ComplexASMTest()
        {
            Assert.IsTrue(CompileFiles("complex", true));
        }

        private bool CompileFiles(string test_name, bool asm = false)
        {
            int i = 1;
            while (File.Exists(pathPrefix + test_name + "_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string asmSuf = "";
                if (asm)
                {
                    asmSuf = "asm_";
                    Program.config.ConvertToAsmCode = true;
                }
                string sourceCode = File.ReadAllText(pathPrefix + test_name + "_" + i + ".era");
                byte[] expectedCode = File.ReadAllBytes(pathPrefix + "expected_compiled_" + asmSuf + test_name + "_" + i + ".bin");
                byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.GENERATION);
                // Store the compiler output in a file
                File.WriteAllBytes(pathPrefix + "actual_compiled_" + asmSuf + test_name + "_" + i + ".bin", actualCode);
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
