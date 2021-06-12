namespace ERACompiler.Structures.Types
{
    public class DataType : ArrayType
    {
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
