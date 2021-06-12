using System;

namespace ERACompiler.Structures.Types
{
    public class VarType
    {
        // Used to get type sizes for all ERA types. Arrays and structures have only 4 bytes that is their address basically.
        private int[] typeSizes = new int[] { 4, 2, 1, 4, 4, 4, 0, 0, 0, 4, 4, 4, 4, 0, 0, 0, 4, 0, 0 };

        public ERAType Type { get; set; }

        public VarType(ERAType type)
        {
            Type = type;
        }

        public bool IsArray()
        {
            return Type == ERAType.ARRAY;
        }

        public bool IsStruct()
        {
            return Type == ERAType.STRUCTURE;
        }
        
        public bool IsConst()
        {
            return Type >= ERAType.CONST_INT && Type <= ERAType.CONST_BYTE_ADDR;
        }

        /// <summary>
        /// Returns the size of the variable in bytes (e.g. int == 4 bytes)
        /// </summary>
        /// <returns>Size in bytes</returns>
        public virtual int GetSize()
        {
            return typeSizes[(int)Type];
        }

        public enum ERAType
        {
            INT,
            SHORT,
            BYTE,
            INT_ADDR,
            SHORT_ADDR,
            BYTE_ADDR,
            CONST_INT,
            CONST_SHORT,
            CONST_BYTE,
            CONST_INT_ADDR,
            CONST_SHORT_ADDR,
            CONST_BYTE_ADDR,
            STRUCTURE,
            ROUTINE,
            MODULE,
            DATA,
            ARRAY,
            LABEL,
            NO_TYPE
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }
}
