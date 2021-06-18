namespace ERACompiler.Structures.Types
{
    /// <summary>
    /// Represents data type of ERA language.
    /// </summary>
    public class DataType : ArrayType
    {
        /// <summary>
        /// Data block is basically a global integere array.
        /// </summary>
        public DataType() : base(new VarType(ERAType.INT))
        {
            Type = ERAType.DATA;
        }

        public override string ToString()
        {
            return "Data block of size " + Size.ToString();
        }
    }
}
