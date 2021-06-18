using ERACompiler.Structures;
using ERACompiler.Utilities.Errors;
using System;
using System.Collections.Generic;

namespace ERACompiler.Modules.Semantics
{
    class PragmaDeclarationAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            AASTNode pragmaNode = new AASTNode(astNode, parent, SemanticAnalyzer.no_type);
            string pragmaName = astNode.Children[0].Token.Value;
            string pragmaParam = "none";
            if (astNode.Children[2].Children.Count > 0)
            {
                pragmaParam = astNode.Children[2].Children[1].Token.Value;
            }
            switch (pragmaName)
            {
                case "memory": // Tells the compiler how much memory to allocate for stack and heap combined
                    {
                        // Usage: (b | kb | mb | gb) *number (ulong)*. Example:  memory("mb 16")
                        List<string> expectedArgs = new List<string>() { "b", "kb", "mb", "gb" };
                        string[] args = pragmaParam.Split(' ');
                        if (args.Length != 2 || !expectedArgs.Contains(args[0]))
                        {
                            throw new SemanticErrorException("Wrong 'memory' pragma usage!!!\r\nUsage: (b | kb | mb | gb) (number (ulong)). Example: memory(\"mb 16\")");
                        }
                        ulong mem;
                        try
                        {
                            mem = ulong.Parse(args[1]) * (ulong) Math.Pow(1024, expectedArgs.IndexOf(args[0]));
                        } 
                        catch (Exception)
                        {
                            throw new SemanticErrorException("Wrong 'memory' pragma number!!! 'ulong' expected!!!");
                        }
                        Program.config.MemorySize = mem;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            return pragmaNode;
        }
    }
}
