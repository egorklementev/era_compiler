using System.IO;

namespace ERACompiler.Utilities
{
    /// <summary>
    /// Used to configurate the program.
    /// </summary>
    public class Config
    {
        public ulong MemorySize { get; set; } = 2 * 1024 * 1024;
        public bool ExtendedErrorMessages { get; set; } = false;
        public bool ExtendedSemanticMessages { get; set; } = false;
        public bool ConvertToAsmCode { get; set; } = false;
    }
}
