namespace ERACompiler.Structures.Types
{
    public class VarType
    {
        public VarTypeType Type { get; set; }
        
        public VarType(VarTypeType type)
        {
            Type = type;
        }

        public enum VarTypeType
        {
            INTEGER,
            SHORT,
            BYTE,
            CONSTANT,
            STRUCTURE,
            ROUTINE,
            MODULE,
            DATA,
            ARRAY,
            NO_TYPE
        }
    }
}
