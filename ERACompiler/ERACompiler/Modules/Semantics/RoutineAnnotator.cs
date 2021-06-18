using ERACompiler.Structures;
using ERACompiler.Structures.Types;
using ERACompiler.Utilities.Errors;
using System.Collections.Generic;

namespace ERACompiler.Modules.Semantics
{
    class RoutineAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            string routineName = astNode.Children[1].Token.Value;
            Context? ctx = SemanticAnalyzer.FindParentContext(parent)
                ?? throw new SemanticErrorException("No parent context found!!!\r\n  At line " + astNode.Token.Position.Line);
            List<VarType> paramTypes = new List<VarType>();
            VarType returnType = SemanticAnalyzer.no_type; // Default value
            // Determine parameter types and return type
            if (astNode.Children[3].Children.Count > 0)
            {
                paramTypes.AddRange(SemanticAnalyzer.RetrieveParamTypes(astNode.Children[3].Children[0])); // Parameters
            }
            if (astNode.Children[5].Children.Count > 0)
            {
                returnType = SemanticAnalyzer.IdentifyType(astNode.Children[5].Children[1]);
            }
            AASTNode routine = new AASTNode(astNode, parent, new RoutineType(paramTypes, returnType));
            ctx?.AddVar(routine, routineName);
            routine.Context = new Context(routineName, ctx, routine);
            // Add params to the context if any
            if (astNode.Children[3].Children.Count > 0)
            {
                AASTNode firstParam = new AASTNode(astNode.Children[3].Children[0].Children[0], routine, paramTypes[0]);
                firstParam.Token.Type = TokenType.IDENTIFIER;
                firstParam.Token.Value = astNode.Children[3].Children[0].Children[0].Children[1].Token.Value;
                firstParam.LIStart = 1;
                routine.Context.AddVar(firstParam, astNode.Children[3].Children[0].Children[0].Children[1].Token.Value);
                int i = 1;
                foreach (ASTNode child in astNode.Children[3].Children[0].Children[1].Children)
                {
                    if (child.ASTType.Equals("Parameter")) // Skip comma rule
                    {
                        AASTNode param = new AASTNode(child, routine, paramTypes[i]);
                        param.Token.Type = TokenType.IDENTIFIER;
                        param.Token.Value = child.Children[1].Token.Value;
                        param.LIStart = 1;
                        routine.Context.AddVar(param, child.Children[1].Token.Value);
                        i++;
                    }
                }
            }            
            // Annotate routine body
            routine.Children.Add(base.Annotate(astNode.Children[6], routine));

            // Check if return statement exists
            if(!CheckForReturn(astNode, ctx.GetRoutineReturnType(astNode.Children[1].Token).Type == VarType.ERAType.NO_TYPE))
            {
                throw new SemanticErrorException("A routine has no 'return' statement!!!\r\n" +
                    "  At (Line: " + astNode.Children[1].Token.Position.Line.ToString() +
                    ", Char: " + astNode.Children[1].Token.Position.Char.ToString() + ")."
                    );
            }

            // Set LI end of parameters
            int maxDepth = SemanticAnalyzer.GetMaxDepth(routine);
            Dictionary<string, AASTNode>.ValueCollection parameters = routine.Context.GetDeclaredVars();
            int j = 0;
            foreach (AASTNode param in parameters)
            {
                if (j >= paramTypes.Count) break;
                routine.Context.SetLIEnd(param.Token.Value, maxDepth);
                j++;
            }
            return routine;
        }

        private bool CheckForReturn(ASTNode node, bool withNoReturn)
        {
            if (node.ASTType.Equals("Return"))
            {
                if (!withNoReturn)
                {
                    if (node.Children.Count == 0)
                    {
                        throw new SemanticErrorException("A 'return' statement returns nothing!!!\r\n" +
                            "  At (Line: " + node.Token.Position.Line.ToString() +
                            ", Char: " + node.Token.Position.Char.ToString() + ")."
                    );
                    }
                    return true;
                } 
                else
                {
                    return true;
                }
            }
            foreach (ASTNode child in node.Children)
            {
                if (CheckForReturn(child, withNoReturn))
                {
                    return true;
                }
            }
            return withNoReturn;
        }
    }
}
