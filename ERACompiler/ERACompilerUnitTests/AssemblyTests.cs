using ERACompiler;
using ERACompiler.Modules;
using ERACompiler.Utilities;
using ERACompiler.Utilities.Errors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace ERACompilerUnitTests
{
    [TestClass]
    public class AssemblyTests
    {
        private readonly string pathPrefix = "../../../tests/asm/";

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
        public void BasicTest()
        {
            Assert.IsTrue(CompileFiles("basic"));
        }

        [TestMethod]
        public void RoutineCallTest()
        {
            Assert.IsTrue(CompileFiles("routine_call"));
        }

        [TestMethod]
        public void NestedTest()
        {
            Assert.IsTrue(CompileFiles("nested"));
        }

        [TestMethod]
        public void SortTest()
        {
            Assert.IsTrue(CompileFiles("sort"));
        }

        [TestMethod]
        public void LoopTest()
        {
            Assert.IsTrue(CompileFiles("loop"));
        }

        [TestMethod]
        public void AsmTest()
        {
            Assert.IsTrue(CompileFiles("asm", true));
        }

        [TestMethod]
        [ExpectedException(typeof(CompilationErrorException), "Compilation error occured.")]
        public void RoutineAssignmentTest()
        {
            string sourceCode = File.ReadAllText(pathPrefix + "compilation_error_1.era");
            Program.currentCompiler.Compile(sourceCode, Compiler.CompilationMode.GENERATION);
        }

        private bool CompileFiles(string test_name, bool toAsm = false)
        {
            int i = 1;
            while (File.Exists(pathPrefix + test_name + "_" + i + ".era"))
            {
                Program.currentCompiler = new Compiler();
                if (toAsm)
                    Program.config.ConvertToAsmCode = true;
                string sourceCode = File.ReadAllText(pathPrefix + test_name + "_" + i + ".era");
                byte[] expectedOutput = File.ReadAllBytes(pathPrefix + "expected_simulated_" + test_name + "_" + i + ".eralog");
                byte[] actualCode = Program.currentCompiler.Compile(sourceCode, Compiler.CompilationMode.GENERATION);
                // Store the compiler output in a file
                string actualBin = "actual_compiled_" + test_name + "_" + i + ".bin";
                File.WriteAllBytes(pathPrefix + actualBin, actualCode);

                // Execute the simulator
                if (!toAsm)
                {
                    string actualSim = "actual_simulated_" + actualBin[16..^4] + ".eralog";
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        CreateNoWindow = false,
                        UseShellExecute = false,
                        FileName = "../../../tests/simulator/ERASimulator.exe",
                        WindowStyle = ProcessWindowStyle.Hidden,
                        Arguments = "-s " + pathPrefix + actualBin + " --op " + pathPrefix + actualSim + " --mb 16 --nodump"
                    };
                    try
                    {
                        using Process exeProcess = Process.Start(startInfo);
                        string errors = exeProcess.StandardError.ReadToEnd();
                        string output = exeProcess.StandardOutput.ReadToEnd();
                        exeProcess.WaitForExit();
                        Thread.Sleep(10);
                    }
                    catch
                    {
                        return false; 
                    }

                    byte[] actualOutput = File.ReadAllBytes(pathPrefix + "actual_simulated_" + test_name + "_" + i + ".eralog");
                    Assert.AreEqual(expectedOutput.Length, actualOutput.Length);
                    for (int j = 0; j < expectedOutput.Length; j++)
                    {
                        Assert.AreEqual(expectedOutput[j], actualOutput[j]);
                    }
                }
                else
                {
                    Assert.AreEqual(expectedOutput.Length, actualCode.Length);
                    for (int j = 0; j < expectedOutput.Length; j++)
                    {
                        Assert.AreEqual(expectedOutput[j], actualCode[j]);
                    }
                }
                i++;
            }
            return true;
        }
    }
}
