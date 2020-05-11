using System.Text;
using System.Collections.Generic;
using ERACompiler.Utilities;
using ERACompiler.Utilities.Errors;
using System.Linq;
using System.Collections;

namespace ERACompiler.Structures
{
    /// <summary>
    /// Represents a single context in the program.
    /// Used to check declaration issues and variable resolution.
    /// </summary>
    public class Context
    {
        /// <summary>
        /// Used in ToString()
        /// </summary>
        public int level { get; set; }

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
            AASTNode? var = LocateVar(identifier.CrspToken.Value);

            if (var == null)
            {
                TokenPosition pos = identifier.CrspToken.Position;
                Logger.LogError(new SemanticsError(
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
        /// <param name="identifier">The name of the variable node.</param>
        public void AddVar(AASTNode variable, string identifier)
        {
            AASTNode? var = LocateVar(identifier); // First is identifier

            if (var == null)
            {
                st.Add(identifier, variable);
            }
            else
            {
                TokenPosition pos = st[identifier].Children[0].CrspToken.Position;
                TokenPosition dPos = variable.CrspToken.Position;
                Logger.LogError(new SemanticsError(
                    "The name " + identifier + " is already declared at (" + pos.Line + ", " + pos.Character + ")!!!\r\n" +
                    "The duplicate is at (" + dPos.Line + ", " + dPos.Character + ")."
                    ));
            }
        }

        /// <summary>
        /// Searches for the variable recursively up in the context tree.
        /// </summary>
        /// <param name="identifier">Identifier of the variable to be found.</param>
        /// <returns>Null if there is no such variable in this context, AAST node with the variable if it exists.</returns>
        private AASTNode? LocateVar(string identifier)
        {
            if (!st.ContainsKey(identifier))
            {
                if (parent != null)
                    return parent.LocateVar(identifier);
            }
            else
            {
                return st[identifier];
            }

            return null;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(string.Concat(Enumerable.Repeat("\t", level + 1)))
                .Append("{\r\n");

            sb.Append(string.Concat(Enumerable.Repeat("\t", level + 2)))
                .Append("\"name\": \"").Append(Name).Append("\",\r\n");

            sb.Append(string.Concat(Enumerable.Repeat("\t", level + 2)))
                .Append("\"symbol_table\": [");

            foreach (KeyValuePair<string, AASTNode> pair in st)
            {
                string varName = pair.Key;
                AASTNode var = pair.Value;

                sb.Append("\r\n").Append(string.Concat(Enumerable.Repeat("\t", level + 3)))
                    .Append("{\r\n");

                sb.Append(string.Concat(Enumerable.Repeat("\t", level + 4)))
                    .Append("\"var_type\": \"").Append(var.Type.ToString()).Append("\",\r\n");
                sb.Append(string.Concat(Enumerable.Repeat("\t", level + 4)))
                    .Append("\"var_name\": \"").Append(varName).Append("\"\r\n");

                sb.Append(string.Concat(Enumerable.Repeat("\t", level + 3)))
                    .Append("},");
            }

            if (st.Count > 0)
            {
                sb.Remove(sb.Length - 1, 1).Append("\r\n");
                sb.Append(string.Concat(Enumerable.Repeat("\t", level + 2)));
            }

            sb.Append("]\r\n");

            sb.Append(string.Concat(Enumerable.Repeat("\t", level + 1)))
                .Append("}");

            return sb.ToString();
        }
    }
}
