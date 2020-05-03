using System.Collections.Generic;
using ERACompiler.Utilities;
using ERACompiler.Utilities.Errors;

namespace ERACompiler.Structures
{
    /// <summary>
    /// Represents a single context in the program.
    /// Used to check declaration issues and variable resolution.
    /// </summary>
    public class Context
    {
        private readonly Dictionary<string, AASTNode> st; // Symbol Table
        private readonly Context? parent;

        /// <summary>
        /// The name of the context
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Creates a context instance.
        /// </summary>
        /// <param name="name">The name of the context (may not be uniqie).</param>
        /// <param name="parent">Parent context (may be null).</param>
        public Context(string name, Context? parent)
        {
            Name = name;
            this.parent = parent;
            st = new Dictionary<string, AASTNode>();
        }

        /// <summary>
        /// Searches for the variable and tries to return the AAST node corresponding to it. If there is no such variable, raises error.
        /// </summary>
        /// <param name="identifier">The identifier AAST node that refers to the variable.</param>
        /// <returns>Returns AAST node of the variable with the given identifier.</returns>
        public AASTNode? GetVarValue(AASTNode identifier)
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

        /// <summary>
        /// Tries to add a new variable to the context. Raises error if the variable already exists.
        /// </summary>
        /// <param name="variable">The variable node.</param>
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

        /// <summary>
        /// Searches for the variable recursively up in the context tree.
        /// </summary>
        /// <param name="identifier">Identifier of the variable to be found.</param>
        /// <returns>Null if there is no such variable in this context, AAST node with the variable if it exists.</returns>
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

        public override string ToString()
        {
            return Name;
        }
    }
}
