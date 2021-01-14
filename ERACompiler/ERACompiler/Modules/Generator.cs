using ERACompiler.Structures;
using System.Text;

namespace ERACompiler.Modules
{
    /// <summary>
    /// Generates the actual assembly code
    /// </summary>
    public class Generator
    {       
        public Generator()
        {

        }

        public byte[] GetAssemblyCode(AASTNode root)
        {
            return ConstructCode(root);
        }

        private byte[] ConstructCode(AASTNode node)
        {
            StringBuilder asc = new StringBuilder();

            return new byte[] {
                        0x01, // Executable version
                        
                        0x00, // Padding (just skips it)
                        
                        0x00, // Static data start address
                        0x00,
                        0x00,
                        0x12,

                        0x00, // Static data length
                        0x00,
                        0x00,
                        0x01,

                        0x00, // Code block start address
                        0x00,
                        0x00,
                        0x14,

                        0x00, // Code block length address
                        0x00,
                        0x00,
                        0x01,

                        0x00, // Static data block itself
                        0x00,

                        // Code block itself
                        0xc7, // R0 := ->R29;
                        0xa0
                    };
            // Expected: R0 == SB addresss


            //return Encoding.ASCII.GetBytes(asc.ToString());
        }

    }
}
