using System;

namespace ERACompiler.Structures.Types
{
    public class VarType
    {
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
            return 4; // TODO: this is ridiculous, however we have to work in this way for now.
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
