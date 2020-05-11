namespace ERACompiler.Structures.Types
{
    public class ArrayType : VarType
    {
        public int Size { get; }
        public VarType ElementType { get; } 

        public ArrayType(VarType elementsType, int size) : base(VarTypeType.ARRAY)
        {
            Size = size;
            ElementType = elementsType;
        }

        public override string ToString()
        {
            return "Array of " + Size.ToString() + " elements of type " + ElementType.ToString();
        }
    }
}
