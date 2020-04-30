using System.Collections.Generic;

namespace ERACompiler.Structures.Types
{
    public class StructType : VarType
    {
        public List<VarType> ChildrenTypes { get; set; }

        public StructType() : base(VarTypeType.STRUCTURE)
        {
            ChildrenTypes = new List<VarType>();
        }

    }
}
