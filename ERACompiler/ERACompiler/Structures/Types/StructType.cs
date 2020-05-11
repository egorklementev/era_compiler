using System.Collections.Generic;

namespace ERACompiler.Structures.Types
{
    public class StructType : VarType
    {
        public string TypeName { get; set; }

        public StructType(string typeName) : base(VarTypeType.STRUCTURE)
        {
            TypeName = typeName;
        }

        public override string ToString()
        {
            return TypeName;
        }
    }
}
