namespace ERACompiler.Structures.Types
{
    public class ArrayType : VarType
    {
        public VarType ElementType { get; }

        public int Size { get; set; } = 0;

        public ArrayType(VarType elementsType) : base(ERAType.ARRAY)
        {
            ElementType = elementsType;            
        }

        public override int GetSize()
        {
            if (Size == 0) return base.GetSize();
            return ElementType.GetSize() * Size;
        }

        public override string ToString()
        {
            return "Array of type " + ElementType.ToString() + " of size " + Size.ToString();
        }
    }
}
