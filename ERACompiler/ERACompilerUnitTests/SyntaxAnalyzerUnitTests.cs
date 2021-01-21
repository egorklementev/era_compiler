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
            int i = 1;
            while(File.Exists("tests/syntax_analyzer/code_rule_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/code_rule_" + i + ".era");
                byte[] expectedCode = File.ReadAllBytes("tests/syntax_analyzer/compiled_code_rule_" + i + ".bin");
                byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode.Length, actualCode.Length);
                for (int j = 0; j < expectedCode.Length; j++)
                {
                    Assert.AreEqual(expectedCode[j], actualCode[j]);
                }
                i++;
            }
        }

        [TestMethod]
        public void RoutineRuleTests()
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/routine_rule_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/routine_rule_" + i + ".era");
                byte[] expectedCode = File.ReadAllBytes("tests/syntax_analyzer/compiled_routine_rule_" + i + ".bin");
                byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode.Length, actualCode.Length);
                for (int j = 0; j < expectedCode.Length; j++)
                {
                    Assert.AreEqual(expectedCode[j], actualCode[j]);
                }
                i++;
            }
        }

        [TestMethod]
        public void DataRuleTests()
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/data_rule_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/data_rule_" + i + ".era");
                byte[] expectedCode = File.ReadAllBytes("tests/syntax_analyzer/compiled_data_rule_" + i + ".bin");
                byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode.Length, actualCode.Length);
                for (int j = 0; j < expectedCode.Length; j++)
                {
                    Assert.AreEqual(expectedCode[j], actualCode[j]);
                }
                i++;
            }
        }

        [TestMethod]
        public void ModuleRuleTests()
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/module_rule_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/module_rule_" + i + ".era");
                byte[] expectedCode = File.ReadAllBytes("tests/syntax_analyzer/compiled_module_rule_" + i + ".bin");
                byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode.Length, actualCode.Length);
                for (int j = 0; j < expectedCode.Length; j++)
                {
                    Assert.AreEqual(expectedCode[j], actualCode[j]);
                }
                i++;
            }
        }

        [TestMethod]
        public void StructRuleTests()
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/struct_rule_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/struct_rule_" + i + ".era");
                byte[] expectedCode = File.ReadAllBytes("tests/syntax_analyzer/compiled_struct_rule_" + i + ".bin");
                byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode.Length, actualCode.Length);
                for (int j = 0; j < expectedCode.Length; j++)
                {
                    Assert.AreEqual(expectedCode[j], actualCode[j]);
                }
                i++;
            }
        }

        [TestMethod]
        public void PragmaRuleTests()
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/pragma_rule_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/pragma_rule_" + i + ".era");
                byte[] expectedCode = File.ReadAllBytes("tests/syntax_analyzer/compiled_pragma_rule_" + i + ".bin");
                byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode.Length, actualCode.Length);
                for (int j = 0; j < expectedCode.Length; j++)
                {
                    Assert.AreEqual(expectedCode[j], actualCode[j]);
                }
                i++;
            }
        }

        [TestMethod]
        public void VariableDeclarationRuleTests()
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/variable_declaration_rule_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/variable_declaration_rule_" + i + ".era");
                byte[] expectedCode = File.ReadAllBytes("tests/syntax_analyzer/compiled_variable_declaration_rule_" + i + ".bin");
                byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode.Length, actualCode.Length);
                for (int j = 0; j < expectedCode.Length; j++)
                {
                    Assert.AreEqual(expectedCode[j], actualCode[j]);
                }
                i++;
            }
        }

        [TestMethod]
        public void AssemblyStatementRuleTests()
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/assembly_statement_rule_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/assembly_statement_rule_" + i + ".era");
                byte[] expectedCode = File.ReadAllBytes("tests/syntax_analyzer/compiled_assembly_statement_rule_" + i + ".bin");
                byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode.Length, actualCode.Length);
                for (int j = 0; j < expectedCode.Length; j++)
                {
                    Assert.AreEqual(expectedCode[j], actualCode[j]);
                }
                i++;
            }
        }

        [TestMethod]
        public void ExpressionRuleTests()
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/expression_rule_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/expression_rule_" + i + ".era");
                byte[] expectedCode = File.ReadAllBytes("tests/syntax_analyzer/compiled_expression_rule_" + i + ".bin");
                byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode.Length, actualCode.Length);
                for (int j = 0; j < expectedCode.Length; j++)
                {
                    Assert.AreEqual(expectedCode[j], actualCode[j]);
                }
                i++;
            }
        }

        [TestMethod]
        public void AssignmentRuleTests()
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/assignment_rule_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/assignment_rule_" + i + ".era");
                byte[] expectedCode = File.ReadAllBytes("tests/syntax_analyzer/compiled_assignment_rule_" + i + ".bin");
                byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode.Length, actualCode.Length);
                for (int j = 0; j < expectedCode.Length; j++)
                {
                    Assert.AreEqual(expectedCode[j], actualCode[j]);
                }
                i++;
            }
        }

        [TestMethod]
        public void SwapRuleTests()
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/swap_rule_" + i + ".era"))
            {                
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/swap_rule_" + i + ".era");
                byte[] expectedCode = File.ReadAllBytes("tests/syntax_analyzer/compiled_swap_rule_" + i + ".bin");
                byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode.Length, actualCode.Length);
                for (int j = 0; j < expectedCode.Length; j++)
                {
                    Assert.AreEqual(expectedCode[j], actualCode[j]);
                }
                i++;
            }
        }

        [TestMethod]
        public void CallRuleTests()
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/call_rule_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/call_rule_" + i + ".era");
                byte[] expectedCode = File.ReadAllBytes("tests/syntax_analyzer/compiled_call_rule_" + i + ".bin");
                byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode.Length, actualCode.Length);
                for (int j = 0; j < expectedCode.Length; j++)
                {
                    Assert.AreEqual(expectedCode[j], actualCode[j]);
                }
                i++;
            }
        }

        [TestMethod]
        public void IfRuleTests()
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/if_rule_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/if_rule_" + i + ".era");
                byte[] expectedCode = File.ReadAllBytes("tests/syntax_analyzer/compiled_if_rule_" + i + ".bin");
                byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode.Length, actualCode.Length);
                for (int j = 0; j < expectedCode.Length; j++)
                {
                    Assert.AreEqual(expectedCode[j], actualCode[j]);
                }
                i++;
            }
        }

        [TestMethod]
        public void LoopRuleTests()
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/loop_rule_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/loop_rule_" + i + ".era");
                byte[] expectedCode = File.ReadAllBytes("tests/syntax_analyzer/compiled_loop_rule_" + i + ".bin");
                byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode.Length, actualCode.Length);
                for (int j = 0; j < expectedCode.Length; j++)
                {
                    Assert.AreEqual(expectedCode[j], actualCode[j]);
                }
                i++;
            }
        }

        [TestMethod]
        public void BreakRuleTests()
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/break_rule_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/break_rule_" + i + ".era");
                byte[] expectedCode = File.ReadAllBytes("tests/syntax_analyzer/compiled_break_rule_" + i + ".bin");
                byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode.Length, actualCode.Length);
                for (int j = 0; j < expectedCode.Length; j++)
                {
                    Assert.AreEqual(expectedCode[j], actualCode[j]);
                }
                i++;
            }
        }

        [TestMethod]
        public void ComplexTests()
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/complex_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/complex_" + i + ".era");
                byte[] expectedCode = File.ReadAllBytes("tests/syntax_analyzer/compiled_complex_" + i + ".bin");
                byte[] actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
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
