using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.Json;
using ERACompiler.Modules;
using ERACompiler.Utilities.Errors;
using ERACompiler.Utilities;


namespace ERACompiler
{
    /// <summary>
    /// Used for console allocation.
    /// </summary>
    internal sealed class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        public static extern bool FreeConsole();
    }

    /// <summary>
    /// The entrance point of the compiler.
    /// </summary>
    public class Program
    {
        private static List<string> sourceFilenames; // File/Files to be compiled
        private static List<string> outputFilenames; // File/Files for compiled code
        private static Compiler.CompilationMode cmode = Compiler.CompilationMode.GENERATION;
        private static bool ignoreConfigFile = false;
        private static bool forceFolderCreation = false;
        private static bool extendedErrorMessages = false; // For syntax errors better display
        private static bool extendedSemanticMessages = false; // For more detailed semantic messages
        private static bool convertToAssemblyCode = false; // To get assembly code instead of binary code
        private static string currentPref = null;

        public static Compiler currentCompiler;
        public static string currentFile = "none";
        public static Config config;

        static void Main(string[] args)
        {
            NativeMethods.AllocConsole();

            //args = new string[] { "-s", "to_compile/fast_sort.era", "--sem", "--ignconf"};
            //args = new string[] { "-s", "to_compile/fast_sort.era", "--asm", "--ignconf"};

            //args = new string[] { "-s", "to_compile/merge_sort.era", "--syn", "--ignconf"};
            //args = new string[] { "-s", "to_compile/merge_sort.era", "--sem", "--ignconf"};
            //args = new string[] { "-s", "to_compile/merge_sort.era", "--asm", "--ignconf"};

            bool error = true;
            try
            {
                error = ProcessArguments(args);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine(ex.Message);
            }                 
            if (error)
            {
                NativeMethods.FreeConsole();
                return;
            }

            if (File.Exists("compiler.config"))
            {
                if (ignoreConfigFile)
                {
                    config = new Config
                    {
                        ConvertToAsmCode = convertToAssemblyCode,
                        ExtendedErrorMessages = extendedErrorMessages,
                        ExtendedSemanticMessages = extendedSemanticMessages
                    };
                }
                else
                {
                    string configJson = File.ReadAllText("compiler.config");
                    config = JsonSerializer.Deserialize<Config>(configJson);
                }
            }
            else
            {
                config = new Config
                {
                    ConvertToAsmCode = convertToAssemblyCode,
                    ExtendedErrorMessages = extendedErrorMessages,
                    ExtendedSemanticMessages = extendedSemanticMessages
                };
                string configJson = JsonSerializer.Serialize(config, new JsonSerializerOptions() { WriteIndented = true });
                File.WriteAllText("compiler.config", configJson);
            }

            for (int i = 0; i < sourceFilenames.Count; i++)
            {
                try
                {
                    currentFile = sourceFilenames[i];

                    // Loading of sample code
                    string sourceCode = File.ReadAllText(sourceFilenames[i]);

                    // For time tracking
                    Stopwatch stopWatch = new Stopwatch();

                    // Create instance of the era compiler and get the compiled code 
                    // It is fresh everytime to refresh all the nodes (may be optimized obviously)                    
                    stopWatch.Start();
                    byte[] compiledCode = new byte[0];
                    currentCompiler = new Compiler();
                    compiledCode = currentCompiler.Compile(sourceCode, cmode);
                    stopWatch.Stop();

                    TimeSpan ts = stopWatch.Elapsed;
                    string elapsedTime = string.Format("{0:00}m {1:00}.{2:00}s",
                    ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

                    // Create a new file with the compiled code
                    if (i >= outputFilenames.Count)
                    {
                        string defaultFilename = sourceFilenames[i];
                        // If it is a path
                        if (defaultFilename.Contains("/") || defaultFilename.Contains("\\"))
                        {
                            int j = defaultFilename.LastIndexOfAny(new char[] { '\\', '/' });
                            defaultFilename = defaultFilename.Insert(j + 1, (currentPref ?? "compiled_"));
                        }
                        else
                        {
                            defaultFilename = (currentPref ?? "compiled_") + defaultFilename;
                        }
                        int dot = defaultFilename.LastIndexOf('.');
                        defaultFilename = defaultFilename.Remove(dot); // Remove .* and add .bin
                        defaultFilename += ".bin";

                        outputFilenames.Add(defaultFilename);
                    }
                    
                    if (outputFilenames[i].Contains("/") || outputFilenames[i].Contains("\\"))
                    {
                        // Check if directory exists
                        string folder = outputFilenames[i].Remove(outputFilenames[i].LastIndexOfAny(new char[] { '\\', '/' }));
                        if (Directory.Exists(folder))
                        {
                            File.WriteAllBytes(outputFilenames[i], compiledCode);
                            Console.WriteLine("\"" + sourceFilenames[i] + "\" has been compiled (" + elapsedTime + ").");
                        }
                        else
                        {
                            if (forceFolderCreation)
                            {
                                Directory.CreateDirectory(folder);
                                File.WriteAllBytes(outputFilenames[i], compiledCode);
                                Console.WriteLine("\"" + sourceFilenames[i] + "\" has been compiled (" + elapsedTime + ").");
                            }
                            else
                            {
                                Console.WriteLine("Folder \"" + folder + "\" does not exists!!!");
                            }
                        }
                    }
                    else
                    {
                        File.WriteAllBytes(outputFilenames[i], compiledCode);
                        Console.WriteLine("\"" + sourceFilenames[i] + "\" has been compiled (" + elapsedTime + ").");
                    }
                }
                catch (IOException ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
                catch (CompilationErrorException ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
            }            

            NativeMethods.FreeConsole();
        }

        private static bool ProcessArguments(string[] args)
        {
            sourceFilenames = new List<string>();
            outputFilenames = new List<string>();

            for (int i = 0; i < args.Length; i++)
            {
                if (IsFlag(args[i]))
                {
                    switch (args[i])
                    {
                        case "-s":
                            {
                                // No files found
                                if (i == args.Length - 1 || IsFlag(args[i + 1]))
                                {
                                    Console.Error.WriteLine("No source files specified!!!");
                                    return true;
                                }
                                i++;
                                while (i < args.Length && !IsFlag(args[i]))
                                {
                                    sourceFilenames.Add(args[i]);
                                    i++;
                                }
                                i--;
                                break;
                            }
                        case "-d":
                            {
                                // No folders found
                                if (i == args.Length - 1 || IsFlag(args[i + 1]))
                                {
                                    Console.Error.WriteLine("No source folders specified!!!");
                                    return true;
                                }
                                i++;
                                while (i < args.Length && !IsFlag(args[i]))
                                {
                                    string[] files = Directory.GetFiles(args[i]); 
                                    foreach (string filename in files)
                                    {
                                        sourceFilenames.Add(filename);
                                    }
                                    i++;
                                }
                                i--;
                                break;
                            }
                        case "-o":
                            {
                                // No files found
                                if (i == args.Length - 1 || IsFlag(args[i + 1]))
                                {
                                    Console.Error.WriteLine("No output files specified!!!");
                                    return true;
                                }
                                i++;
                                while (i < args.Length && !IsFlag(args[i]))
                                {
                                    outputFilenames.Add(args[i]);
                                    i++;
                                }
                                i--;
                                break;
                            }
                        case "-p":
                            {
                                forceFolderCreation = true;
                                break;
                            }
                        case "-h":
                            {
                                Console.Error.WriteLine(
                                    "      ERA COMPILER\r\n" +
                                    "  INNOPOLIS UNIVERSITY\r\n" +
                                    "\r\n" +
                                    "  Possible arguments:\r\n" +
                                    "  '-s' { filepath }  :  specify input file to be compiled\r\n" +
                                    "  '-d' { path }  :  specify the folders with files to be compiled\r\n" +
                                    "  '-o' { filepath }  :  specify output file for compiled source code\r\n" +
                                    "  '-p'  :  force to create folders if they do not exist\r\n" +
                                    "  '--lex'  :  compile in lexis mode (separate into tokens only)\r\n" +
                                    "  '--syn'  :  compile in syntax mode (build AST only)\r\n" +
                                    "  '--sem'  :  compile in semantic mode (build AAST only)\r\n" +
                                    "  '--semext'  :  same as \"--sem\" with extended debug information\r\n" +
                                    "  '--asm'  :  full compilation with the assembly code output\r\n" +
                                    "  '--err'  :  displays more detailed error messages\r\n" +
                                    "  '--prefix'  :  custom name prefix for output binary file/files\r\n" +
                                    "  '--ignconf'  :  ignore config file and only rely on command-line arguments\r\n" +
                                    "  '-h'  :  show manual\r\n" +
                                    "  Default source code file is 'code.era'.\r\n" +
                                    "  Default output file is 'compiled_' + source code file name\r\n" +
                                    "  Default configuration file is 'compiler.config'\r\n"
                                    );
                                break;
                            }
                        case "--lex":
                            {
                                cmode = Compiler.CompilationMode.LEXIS;
                                break;
                            }
                        case "--syn":
                            {
                                cmode = Compiler.CompilationMode.SYNTAX;
                                break;
                            }
                        case "--sem":
                            {
                                cmode = Compiler.CompilationMode.SEMANTICS;
                                break;
                            }
                        case "--semext":
                            {
                                cmode = Compiler.CompilationMode.SEMANTICS;
                                extendedSemanticMessages = true;
                                break;
                            }
                        case "--ignconf":
                            {
                                ignoreConfigFile = true;
                                break;
                            }
                        case "--asm":
                            {
                                convertToAssemblyCode = true;
                                break;
                            }
                        case "--err":
                            {
                                extendedErrorMessages = true;
                                break;
                            }
                        case "--prefix":
                            {
                                // No prefix found
                                if (i == args.Length - 1 || IsFlag(args[i + 1]))
                                {
                                    Console.Error.WriteLine("No prefix specified!!!");
                                    return true;
                                }
                                i++;
                                currentPref = args[i];
                                break;
                            }
                        default:
                            {
                                throw new CompilationErrorException("Unknown parameter\"" + args[i] +"\" !!!");                                
                            }
                    }
                }
            }

            return false;
        }

        private static bool IsFlag(string s)
        {
            return s.StartsWith("-") || s.StartsWith("--");
        }
    }
}
