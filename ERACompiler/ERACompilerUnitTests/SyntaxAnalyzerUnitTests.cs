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
            while(File.Exists("tests/syntax_analyzer/code_rule_t_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/code_rule_t_" + i + ".era");
                string expectedCode = File.ReadAllText("tests/syntax_analyzer/code_rule_e_" + i + ".era");
                string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode, actualCode, false, i.ToString());
                i++;
            }
        }

        [TestMethod]
        public void RoutineRuleTests()
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/routine_rule_t_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/routine_rule_t_" + i + ".era");
                string expectedCode = File.ReadAllText("tests/syntax_analyzer/routine_rule_e_" + i + ".era");
                string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode, actualCode, false, i.ToString());
                i++;
            }
        }

        [TestMethod]
        public void DataRuleTests()
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/data_rule_t_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/data_rule_t_" + i + ".era");
                string expectedCode = File.ReadAllText("tests/syntax_analyzer/data_rule_e_" + i + ".era");
                string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode, actualCode, false, i.ToString());
                i++;
            }
        }

        [TestMethod]
        public void ModuleRuleTests()
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/module_rule_t_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/module_rule_t_" + i + ".era");
                string expectedCode = File.ReadAllText("tests/syntax_analyzer/module_rule_e_" + i + ".era");
                string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode, actualCode, false, i.ToString());
                i++;
            }
        }

        [TestMethod]
        public void StructRuleTests()
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/struct_rule_t_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/struct_rule_t_" + i + ".era");
                string expectedCode = File.ReadAllText("tests/syntax_analyzer/struct_rule_e_" + i + ".era");
                string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode, actualCode, false, i.ToString());
                i++;
            }
        }

        [TestMethod]
        public void PragmaRuleTests()
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/pragma_rule_t_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/pragma_rule_t_" + i + ".era");
                string expectedCode = File.ReadAllText("tests/syntax_analyzer/pragma_rule_e_" + i + ".era");
                string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode, actualCode, false, i.ToString());
                i++;
            }
        }

        [TestMethod]
        public void VariableDeclarationRuleTests()
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/variable_declaration_rule_t_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/variable_declaration_rule_t_" + i + ".era");
                string expectedCode = File.ReadAllText("tests/syntax_analyzer/variable_declaration_rule_e_" + i + ".era");
                string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode, actualCode, false, i.ToString());
                i++;
            }
        }

        [TestMethod]
        public void AssemblyStatementRuleTests()
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/assembly_statement_rule_t_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/assembly_statement_rule_t_" + i + ".era");
                string expectedCode = File.ReadAllText("tests/syntax_analyzer/assembly_statement_rule_e_" + i + ".era");
                string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode, actualCode, false, i.ToString());
                i++;
            }
        }

        [TestMethod]
        public void ExpressionRuleTests()
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/expression_rule_t_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/expression_rule_t_" + i + ".era");
                string expectedCode = File.ReadAllText("tests/syntax_analyzer/expression_rule_e_" + i + ".era");
                string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode, actualCode, false, i.ToString());
                i++;
            }
        }

        [TestMethod]
        public void AssignmentRuleTests()
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/assignment_rule_t_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/assignment_rule_t_" + i + ".era");
                string expectedCode = File.ReadAllText("tests/syntax_analyzer/assignment_rule_e_" + i + ".era");
                string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode, actualCode, false, i.ToString());
                i++;
            }
        }

        [TestMethod]
        public void SwapRuleTests()
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/swap_rule_t_" + i + ".era"))
            {                
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/swap_rule_t_" + i + ".era");
                string expectedCode = File.ReadAllText("tests/syntax_analyzer/swap_rule_e_" + i + ".era");
                string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode, actualCode, false, i.ToString());
                i++;
            }
        }

        [TestMethod]
        public void CallRuleTests()
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/call_rule_t_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/call_rule_t_" + i + ".era");
                string expectedCode = File.ReadAllText("tests/syntax_analyzer/call_rule_e_" + i + ".era");
                string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode, actualCode, false, i.ToString());
                i++;
            }
        }

        [TestMethod]
        public void IfRuleTests()
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/if_rule_t_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/if_rule_t_" + i + ".era");
                string expectedCode = File.ReadAllText("tests/syntax_analyzer/if_rule_e_" + i + ".era");
                string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode, actualCode, false, i.ToString());
                i++;
            }
        }

        [TestMethod]
        public void LoopRuleTests()
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/loop_rule_t_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/loop_rule_t_" + i + ".era");
                string expectedCode = File.ReadAllText("tests/syntax_analyzer/loop_rule_e_" + i + ".era");
                string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode, actualCode, false, i.ToString());
                i++;
            }
        }

        [TestMethod]
        public void BreakRuleTests()
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/break_rule_t_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/break_rule_t_" + i + ".era");
                string expectedCode = File.ReadAllText("tests/syntax_analyzer/break_rule_e_" + i + ".era");
                string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode, actualCode, false, i.ToString());
                i++;
            }
        }

        [TestMethod]
        public void GotoRuleTests()
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/goto_rule_t_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/goto_rule_t_" + i + ".era");
                string expectedCode = File.ReadAllText("tests/syntax_analyzer/goto_rule_e_" + i + ".era");
                string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode, actualCode, false, i.ToString());
                i++;
            }
        }

        [TestMethod]
        public void ComplexTests()
        {
            int i = 1;
            while (File.Exists("tests/syntax_analyzer/complex_t_" + i + ".era"))
            {
                Compiler c = new Compiler();
                string sourceCode = File.ReadAllText("tests/syntax_analyzer/complex_t_" + i + ".era");
                string expectedCode = File.ReadAllText("tests/syntax_analyzer/complex_e_" + i + ".era");
                string actualCode = c.Compile(sourceCode, Compiler.CompilationMode.SYNTAX);
                Assert.AreEqual(expectedCode, actualCode, false, i.ToString());
                i++;
            }
        }
    }
}
