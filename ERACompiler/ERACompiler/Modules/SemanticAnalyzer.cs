using ERACompiler.Structures;
using ERACompiler.Structures.Types;

namespace ERACompiler.Modules
{
    class SemanticAnalyzer
    {
        public AASTNode BuildAAST(ASTNode ASTRoot)
        {
            AASTNode root = new AASTNode(ASTRoot, new VarType(VarType.VarTypeType.NO_TYPE)); // Program node



            return root;
        }
    }
}
