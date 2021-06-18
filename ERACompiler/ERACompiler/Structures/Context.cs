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

        public Context? Parent { get => parent; }

        public AASTNode AASTLink { get; }

        /// <summary>
        /// The name of the context
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Creates a context instance.
        /// </summary>
        /// <param name="name">The name of the context (may not be uniqie).</param>
        /// <param name="parent">Parent context (may be null).</param>
        public Context(string name, Context? parent, AASTNode aastLink)
        {
            Name = name;
            this.parent = parent;
            AASTLink = aastLink;
            st = new Dictionary<string, AASTNode>();
        }

        /// <summary>
        /// Tries to add a new variable to the context. Raises error if the variable already exists.
        /// </summary>
        /// <param name="variable">The variable node.</param>
        /// <param name="identifier">The name of the variable node.</param>
        public void AddVar(AASTNode variable, string identifier)
        {
            AASTNode? var = LocateVar(identifier);

            if (var == null)
            {
                st.Add(identifier, variable);
            }
            else
            {
                TokenPosition pos = var.Token.Position;
                TokenPosition dPos = variable.Token.Position;
                throw new SemanticErrorException(
                    "A variable with name \"" + identifier + "\" is already declared!!!\r\n" +
                    "  At (Line: " + pos.Line + ", Char: " + pos.Char + ").\r\n" +
                    "  At (Line: " + dPos.Line + ", Char: " + dPos.Char + ") - duplicate."
                    );
            }
        }

        /// <summary>
        /// Returns a value of a constant variable. Used for compile-time constant expression calculation and for retrieving intial values.
        /// </summary>
        /// <param name="identifier">The variable name.</param>
        /// <returns>Value of a constant variable.</returns>
        public int GetConstValue(Token identifier)
        {
            AASTNode? var = LocateVar(identifier.Value);

            if (var == null)
            {
                throw new SemanticErrorException(
                    "A variable with name \"" + identifier.Value + "\" has been never declared in this context!!!\r\n" +
                    "  At (Line: " + identifier.Position.Line.ToString() + ", Char: " + identifier.Position.Char.ToString() + ")."
                    );
            }
            else
            {
                return var.AASTValue;
            }
        }

        /// <summary>
        /// Checks if a variable with given name is constant
        /// </summary>
        /// <param name="identifier">A variable to check</param>
        /// <returns>True if var is constant, false otherwise</returns>

        public int GetRoutineParamNum(Token identifier)
        {
            return ((RoutineType)LocateVar(identifier.Value).AASTType).ParamTypes.Count;
        }

        public VarType GetRoutineReturnType(Token identifier)
        {
            return ((RoutineType)LocateVar(identifier.Value).AASTType).ReturnType;
        }

        public int GetArrSize(Token identifier)
        {
            AASTNode? var = LocateVar(identifier.Value);
            if (var?.AASTType is ArrayType a_type)
            {
                return a_type.Size;
            }
            else if (var?.AASTType is DataType d_type)
            {
                return d_type.Size;
            }
            throw new SemanticErrorException("Incorrect array check!!! Ask developers.");
        }

        /// <summary>
        /// Checks if a variable with given identifier exists in current context
        /// </summary>
        /// <param name="identifier">The name of a variable.</param>
        /// <returns>True if it exists, false otherwise</returns>
        public bool IsVarDeclared(Token identifier)
        {
            return IsVarDeclared(identifier.Value);
        }

        public bool IsVarDeclared(string identifier)
        {
            return LocateVar(identifier) != null;
        }

        public bool IsVarDeclaredInThisContext(string identifier)
        {
            return LocateVar(identifier, true) != null;
        }
        
        /// <returns>Returns how many context above this context given variable was declared.</returns>
        public int GetVarDeclarationBlockOffset(string identifier)
        {
            int distance = 0;
            Context iter = this;
            while (iter != null)
            {
                if (iter.st.ContainsKey(identifier))
                {
                    return distance;
                }
                else
                {
                    iter = iter.parent;
                    distance++;
                }
            }
            return -1;
        }

        public bool IsVarData(string identifier)
        {
            return LocateVar(identifier).AASTType.IsData();
        }

        public bool IsVarStruct(Token identifier)
        {
            return LocateVar(identifier.Value).AASTType.IsStruct();
        }

        public bool IsVarArray(Token identifier)
        {
            return IsVarArray(identifier.Value);
        }

        public bool IsVarArray(string identifier)
        {
            return LocateVar(identifier).AASTType.IsArray();
        }

        public bool IsVarRoutine(Token identifier)
        {
            return IsVarRoutine(identifier.Value);
        }

        public bool IsVarRoutine(string identifier)
        {
            return LocateVar(identifier).AASTType.Type == VarType.ERAType.ROUTINE;
        }

        public bool IsVarDynamicArray(string identifier)
        {
            VarType type = LocateVar(identifier).AASTType;
            return type.IsArray() && ((ArrayType)type).Size == 0;
        }

        public bool IsVarLabel(Token identifier)
        {
            return IsVarLabel(identifier.Value);
        }

        public bool IsVarLabel(string identifier)
        {
            return LocateVar(identifier).AASTType.Type == VarType.ERAType.LABEL;
        }

        public bool IsVarConstant(Token identifier)
        {
            AASTNode? var = LocateVar(identifier.Value);
            if (var == null)
            {
                throw new SemanticErrorException(
                    "A variable with name \"" + identifier.Value + "\" has been never declared in this context!!!\r\n" +
                    "  At (Line: " + identifier.Position.Line.ToString() + ", Char: " + identifier.Position.Char.ToString() + ")."
                    );
            }
            else
            {
                return var.AASTType.IsConst();
            }
        }

        public bool IsVarConstant(string identifier)
        {
            AASTNode? var = LocateVar(identifier);
            if (var == null)
            {
                throw new SemanticErrorException(
                    "A variable with name \"" + identifier + "\" has been never declared in this context!!!\r\n"
                    );
            }
            else
            {
                return var.AASTType.IsConst();
            }
        }
        
        public bool IsVarGlobal(string identifier)
        {
            return LocateVar(identifier).IsGlobal;
        }

        /// <summary>
        /// Sets the LI start value of a variable
        /// </summary>
        /// <param name="identifier">Variable identifier token</param>
        /// <param name="blockPosition">Position in the block</param>
        public void SetLIStart(Token identifier, int blockPosition)
        {
            AASTNode var = LocateVar(identifier.Value);
            var.LIStart = blockPosition;
            var.LIEnd = blockPosition;
        }

        public int GetLIStart(string identifier)
        {
            return LocateVar(identifier).LIStart;
        }

        /// <summary>
        /// Sets a new value (previous one increased by 1) for the LI end value
        /// </summary>
        /// <param name="identifier">Variable identifier</param>
        /// <param name="blockPosition">Statement position in current block</param>
        public void SetLIEnd(string identifier, int blockPosition)
        {
            AASTNode var = LocateVar(identifier);
            if (var.LIStart != 0 && var.LIEnd < blockPosition) 
                var.LIEnd = blockPosition;
        }

        public int GetLIEnd(string identifier)
        {
            return LocateVar(identifier).LIEnd;
        }

        public int GetStaticOffset(string identifier)
        {
            return LocateVar(identifier).StaticOffset;
        }

        public int GetFrameOffset(string identifier)
        {
            return LocateVar(identifier).FrameOffset;
        }

        public VarType GetVarType(Token identifier)
        {
            return GetVarType(identifier.Value);
        }

        public VarType GetVarType(string identifier)
        {
            return LocateVar(identifier).AASTType;
        }

        public int GetArrayOffsetSize(string identifier)
        {
            int lword = 4;
            int word = 2;
            int sword = 1;
            switch (((ArrayType)LocateVar(identifier).AASTType).ElementType.Type)
            {
                case VarType.ERAType.INT:
                case VarType.ERAType.INT_ADDR:
                case VarType.ERAType.SHORT_ADDR:
                case VarType.ERAType.BYTE_ADDR:
                    return lword;
                case VarType.ERAType.SHORT:
                    return word;
                case VarType.ERAType.BYTE:
                    return sword;
                case VarType.ERAType.STRUCTURE:
                    return 0; // TODO: calculate and return struct size
                default:
                    return 0;
            }
        }
        
        public Dictionary<string, AASTNode>.ValueCollection GetDeclaredVars()
        {
            return st.Values;
        }

        public HashSet<string> GetAllVisibleVars()
        {
            HashSet<string> set = new HashSet<string>();
            Context ctx = this;
            do
            {
                set.UnionWith(ctx.st.Keys);
                ctx = ctx.parent;
            }
            while (ctx != null);
            return set;
        }

        /* --- */

        /// <summary>
        /// Searches for the variable recursively up in the context tree.
        /// </summary>
        /// <param name="identifier">Identifier of the variable to be found.</param>
        /// <param name="onlyInCurrentContext">If true, searches for a declaration only in this context.</param>
        /// <returns>Null if there is no such variable in this context, AAST node with the variable if it exists.</returns>
        private AASTNode? LocateVar(string identifier, bool onlyInCurrentContext = false)
        {
            if (!st.ContainsKey(identifier))
            {
                if (onlyInCurrentContext)
                    return null;

                if (parent != null)
                    return parent.LocateVar(identifier);
            }
            else
            {
                return st[identifier];
            }

            return null;
        }

        private string SymbolTableToString(Dictionary<string, AASTNode> st)
        {
            StringBuilder sb = new StringBuilder();

            string tabs_lvl3 = string.Concat(Enumerable.Repeat("\t", Level + 3));
            string tabs_lvl4 = tabs_lvl3 + "\t";

            foreach (KeyValuePair<string, AASTNode> pair in st)
            {
                string varName = pair.Key;
                AASTNode var = pair.Value;

                sb.Append("\r\n").Append(tabs_lvl3)
                    .Append("{\r\n");

                sb.Append(tabs_lvl4)
                    .Append("\"var_type\": \"").Append(var.AASTType.ToString()).Append("\",\r\n");
                sb.Append(tabs_lvl4)
                    .Append("\"var_name\": \"").Append(varName).Append("\",\r\n");

                if (var.FrameOffset != 0 || Program.config.ExtendedSemanticMessages)
                {
                    sb.Append(tabs_lvl4)
                        .Append("\"var_frame_offset\": \"").Append(var.FrameOffset).Append("\",\r\n");
                }

                if (var.StaticOffset != 0 || Program.config.ExtendedSemanticMessages)
                {
                    sb.Append(tabs_lvl4)
                        .Append("\"var_static_offset\": \"").Append(var.StaticOffset).Append("\",\r\n");
                }

                if (var.LIStart != 0 || Program.config.ExtendedSemanticMessages)
                {
                    sb.Append(tabs_lvl4)
                        .Append("\"li_start\": \"").Append(var.LIStart).Append("\",\r\n");
                    sb.Append(tabs_lvl4)
                        .Append("\"li_end\": \"").Append(var.LIEnd).Append("\",\r\n");
                }

                if (var.AASTValue != 0 || Program.config.ExtendedSemanticMessages)
                    sb.Append(tabs_lvl4)
                        .Append("\"var_value\": \"").Append(var.AASTValue.ToString()).Append("\"\r\n");
                else
                    sb.Remove(sb.Length - 3, 1);

                sb.Append(tabs_lvl3)
                    .Append("},");
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            string tabs_lvl1 = string.Concat(Enumerable.Repeat("\t", Level + 1));
            string tabs_lvl2 = tabs_lvl1 + "\t";

            sb.Append(tabs_lvl1)
                .Append("{\r\n");

            sb.Append(tabs_lvl2)
                .Append("\"name\": \"").Append(Name).Append("\",\r\n");

            sb.Append(tabs_lvl2)
                .Append("\"symbol_table\": [");

            // Show all variables visible from this context
            if (Program.config.ExtendedSemanticMessages)
            {
                Context? prnt = parent;
                while (prnt != null)
                {
                    sb.Append(SymbolTableToString(prnt.st));
                    prnt = prnt.parent;
                }
            }

            sb.Append(SymbolTableToString(st));

            if (st.Count > 0 || Program.config.ExtendedSemanticMessages)
            {
                sb.Remove(sb.Length - 1, 1).Append("\r\n");
                sb.Append(tabs_lvl2);
            }

            sb.Append("]\r\n");

            sb.Append(tabs_lvl1)
                .Append("}");

            return sb.ToString();
        }


    }
}
