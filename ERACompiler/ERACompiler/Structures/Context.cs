using System.Collections.Generic;
using ERACompiler.Utilities;
using ERACompiler.Utilities.Errors;

namespace ERACompiler.Structures
{
    public class Context
    {
        private readonly Dictionary<string, AASTNode> st; // Symbol Table
        private readonly Context parent;

        public string Name { get; set; } // The name of the context

        public Context(string name, Context parent)
        {
            Name = name;
            this.parent = parent;
            st = new Dictionary<string, AASTNode>();
        }

        public ASTNode? GetVarValue(AASTNode identifier)
        {
            AASTNode? var = LocateVar(identifier);

            if (var == null)
            {
                TokenPosition pos = identifier.CrspToken.Position;
                Logger.LogError(new SemanticError(
                    "Reference to the undeclared variable at (" + pos.Line + ", " + pos.Character + ")!!!"
                    ));
                return null;
            }
            else
            {
                return var;
            }
        }

        public void AddVar(AASTNode variable)
        {
            string varName = variable.Children[0].CrspToken.Value;

            AASTNode? var = LocateVar(variable);

            if (var == null)
            {
                st.Add(varName, variable);
            }
            else
            {
                TokenPosition pos = st[varName].Children[0].CrspToken.Position;
                TokenPosition dPos = variable.Children[0].CrspToken.Position;
                Logger.LogError(new SemanticError(
                    "The name " + varName + " is already declared at (" + pos.Line + ", " + pos.Character + ")!!!\r\n" +
                    "The duplicate is at (" + dPos.Line + ", " + dPos.Character + ")."
                    ));
            }
        }

        private AASTNode? LocateVar(AASTNode identifier)
        {
            string varName = identifier.CrspToken.Value;

            if (!st.ContainsKey(varName))
            {
                if (parent != null)
                    return parent.LocateVar(identifier);
            }
            else
            {
                return st[varName];
            }

            return null;
        }
    }
}
