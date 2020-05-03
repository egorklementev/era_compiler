namespace ERACompiler.Structures.Types
{
    public class ArrayType : VarType
    {
        public int Size { get; }

        public ArrayType(int size) : base(VarTypeType.ARRAY)
        {
            Size = size;
        }
    }
}
