using ERACompiler.Structures;
using System.Text;

namespace ERACompiler.Modules
{
    /// <summary>
    /// Generates the actual assembly code
    /// </summary>
    public class Generator
    {
        
        /// <summary>
        /// Generator constructor
        /// </summary>
        public Generator()
        {

        }

        /// <summary>
        /// Constructs actual assembly code from the annotated AST.
        /// </summary>
        /// <param name="root">The root node of the AAST.</param>
        /// <returns>A string containing assembly code build using AAST.</returns>
        public byte[] GetAssemblyCode(AASTNode root)
        {
            return ConstructCode(root);
        }

        private byte[] ConstructCode(AASTNode node)
        {
            StringBuilder asc = new StringBuilder();

            return Encoding.ASCII.GetBytes(asc.ToString());
        }

    }
}
