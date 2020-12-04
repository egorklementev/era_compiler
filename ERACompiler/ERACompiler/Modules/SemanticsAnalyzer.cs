using ERACompiler.Structures;
using ERACompiler.Structures.Types;
using ERACompiler.Utilities;
using ERACompiler.Utilities.Errors;
using System.Collections.Generic;

namespace ERACompiler.Modules
{
    class SemanticsAnalyzer
    {
        public AASTNode BuildAAST(ASTNode ASTRoot)
        {
            return new AASTNode(ASTRoot, new VarType(VarType.VarTypeType.NO_TYPE)); //AnnotateNode(ASTRoot, null);
        }
    }
}
