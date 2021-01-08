using System;
using System.Collections.Generic;
using System.Text;

namespace ERACompiler.Structures.Types
{
    public class RoutineType : VarType
    {
        public VarType ReturnType { get; }
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
            sb.Remove(sb.Length - 2, 2);
            return "Routine with return type \"" + ReturnType.ToString() + "\", Parameters: (" + sb.ToString() + ")";
        }
    }
}
