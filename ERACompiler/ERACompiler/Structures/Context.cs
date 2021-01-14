using System.Text;
using System.Collections.Generic;
using ERACompiler.Utilities;
using ERACompiler.Utilities.Errors;
using ERACompiler.Structures.Types;
using System.Linq;
using System;

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
        public int Level { get; set; }

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
                TokenPosition pos = st[identifier].Token.Position;
                TokenPosition dPos = variable.Token.Position;
                Logger.LogError(new SemanticError(
                    "A variable with name \"" + identifier + "\" is already declared!!!\r\n" +
                    "\tAt (Line: " + pos.Line + ", Char: " + pos.Char + ").\r\n" +
                    "\tAt (Line: " + dPos.Line + ", Char: " + dPos.Char + ") - duplicate."
                    ));
            }
        }

        /// <summary>
        /// Returns a value of a constant variable. Used for compile-time constant expression calculation and for retrieving intial values.
        /// </summary>
        /// <param name="identifier">The variable name.</param>
        /// <returns>Value of a constant variable.</returns>
        public int GetConsValue(Token identifier)
        {
            AASTNode? var = LocateVar(identifier.Value);

            if (var == null)
            {
                Logger.LogError(new SemanticError(
                    "A variable with name \"" + identifier.Value + "\" has been never declared in this context!!!\r\n" +
                    "\tAt (Line: " + identifier.Position.Line.ToString() + ", Char: " + identifier.Position.Char.ToString() + ")."
                    ));
                return -1;
            }
            else
            {
                return var.AASTValue;
            }
        }

        public bool IsVarStruct(Token token)
        {
            return LocateVar(token.Value).AASTType.IsStruct();
        }

        /// <summary>
        /// Checks if a variable with given name is constant
        /// </summary>
        /// <param name="identifier">A variable to check</param>
        /// <returns>True if var is constant, false otherwise</returns>
        public bool IsVarConstant(Token identifier)
        {
            AASTNode? var = LocateVar(identifier.Value);
            if (var == null)
            {
                Logger.LogError(new SemanticError(
                    "A variable with name \"" + identifier.Value + "\" has been never declared in this context!!!\r\n" +
                    "\tAt (Line: " + identifier.Position.Line.ToString() + ", Char: " + identifier.Position.Char.ToString() + ")."
                    ));
                return false;
            }
            else
            {
                return var.AASTType.IsConst();
            }
        }

        public int GetRoutineParamNum(Token identifier)
        {
            return ((RoutineType)LocateVar(identifier.Value).AASTType).ParamTypes.Count;
        }

        public int GetArrSize(Token identifier)
        {
            return ((ArrayType) LocateVar(identifier.Value).AASTType).Size;
        }

        /// <summary>
        /// Checks if a variable with given identifier exists in current context
        /// </summary>
        /// <param name="identifier">The name of a variable.</param>
        /// <returns>True if it exists, false otherwise</returns>
        public bool IsVarDeclared(Token identifier)
        {
            return LocateVar(identifier.Value) != null;
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

            sb.Append(string.Concat(Enumerable.Repeat("\t", Level + 1)))
                .Append("{\r\n");

            sb.Append(string.Concat(Enumerable.Repeat("\t", Level + 2)))
                .Append("\"name\": \"").Append(Name).Append("\",\r\n");

            sb.Append(string.Concat(Enumerable.Repeat("\t", Level + 2)))
                .Append("\"symbol_table\": [");

            foreach (KeyValuePair<string, AASTNode> pair in st)
            {
                string varName = pair.Key;
                AASTNode var = pair.Value;

                sb.Append("\r\n").Append(string.Concat(Enumerable.Repeat("\t", Level + 3)))
                    .Append("{\r\n");

                sb.Append(string.Concat(Enumerable.Repeat("\t", Level + 4)))
                    .Append("\"var_type\": \"").Append(var.AASTType.ToString()).Append("\",\r\n");
                sb.Append(string.Concat(Enumerable.Repeat("\t", Level + 4)))
                    .Append("\"var_name\": \"").Append(varName).Append("\",\r\n");
                sb.Append(string.Concat(Enumerable.Repeat("\t", Level + 4)))
                    .Append("\"var_value\": \"").Append(var.AASTValue.ToString()).Append("\"\r\n");

                sb.Append(string.Concat(Enumerable.Repeat("\t", Level + 3)))
                    .Append("},");
            }

            if (st.Count > 0)
            {
                sb.Remove(sb.Length - 1, 1).Append("\r\n");
                sb.Append(string.Concat(Enumerable.Repeat("\t", Level + 2)));
            }

            sb.Append("]\r\n");

            sb.Append(string.Concat(Enumerable.Repeat("\t", Level + 1)))
                .Append("}");

            return sb.ToString();
        }
    }
}
