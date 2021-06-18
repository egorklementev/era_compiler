namespace ERACompiler.Structures.Types
{
    /// <summary>
    /// Represents array type of ERA language.
    /// </summary>
    public class ArrayType : VarType
    {
        /// <summary>
        /// The type of elements inside the array.
        /// </summary>
        public VarType ElementType { get; }

        /// <summary>
        /// The size of the array.
        /// </summary>
        public int Size { get; set; } = 0;

        public ArrayType(VarType elementsType) : base(ERAType.ARRAY)
        {
            ElementType = elementsType;            
        }

        /// <summary>
        /// Returns array size on the stack (in bytes)
        /// </summary>
        /// <returns></returns>
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
