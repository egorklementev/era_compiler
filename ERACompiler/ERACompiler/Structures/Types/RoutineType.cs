using System;
using System.Collections.Generic;
using System.Text;

namespace ERACompiler.Structures.Types
{
    /// <summary>
    /// Represents routine type of ERA language
    /// </summary>
    public class RoutineType : VarType
    {
        /// <summary>
        /// The type of return value. Can be NO_TYPE.
        /// </summary>
        public VarType ReturnType { get; }

        /// <summary>
        /// Parameter types.
        /// </summary>
        public List<VarType> ParamTypes { get; }

        public RoutineType(List<VarType> paramTypes, VarType returnType) : base(ERAType.ROUTINE)
        {
            ParamTypes = paramTypes;
            ReturnType = returnType;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (VarType v in ParamTypes) sb.Append(v.ToString()).Append(", ");
            if (sb.Length > 2) 
                sb.Remove(sb.Length - 2, 2);
            return ReturnType.ToString() + " routine, (" + sb.ToString() + ")";
        }
    }
}
