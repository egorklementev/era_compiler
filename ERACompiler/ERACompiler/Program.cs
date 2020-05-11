using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ERACompiler.Modules;
using ERACompiler.Utilities;
using ERACompiler.Utilities.Errors;

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

namespace ERACompiler
{
    /// <summary>
    /// The entrance point of the compiler.
    /// </summary>
    class Program
    {
        private static List<string> sourceFilenames; // File/Files to be compiled
        private static List<string> outputFilenames; // File/Files for compiled code
        private static Compiler.CompilationMode cmode = Compiler.CompilationMode.GENERATION;

        static void Main(string[] args)
        {
            NativeMethods.AllocConsole();

            //args = new string[] { "-s", "debug" };
            //args = new string[] { "-s", "debug", "--semantics" };

            // Logging
            new Logger(true);

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

            try
            {
                for (int i = 0; i < sourceFilenames.Count; i++)
                {
                    // Loading of sample code
                    string sourceCode = File.ReadAllText(sourceFilenames[i]);

                    // Create instance of the era compiler and get the compiled code                    
                    string compiledCode = new Compiler().Compile(sourceCode, cmode); // New everytime to refresh all the nodes (may be optimized obviously)

                    // Create a new file with the compiled code
                    if (i >= outputFilenames.Count)
                    {
                        string defaultFilename = sourceFilenames[i];
                        // If it is a path
                        if (defaultFilename.Contains("/") || defaultFilename.Contains("\\"))
                        {
                            int j = defaultFilename.LastIndexOfAny(new char[] {'\\', '/'});
                            defaultFilename = defaultFilename.Insert(j + 1, "compiled_");
                        }
                        else
                        {
                            defaultFilename = "compiled_" + defaultFilename;
                        }
                        outputFilenames.Add(defaultFilename);
                    }
                    File.WriteAllText(outputFilenames[i], compiledCode);

                    Console.WriteLine("\"" + sourceFilenames[i] + "\" has been compiled.");
                    //Console.ReadLine();
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
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
                        case "-h":
                            {
                                Console.Error.WriteLine(
                                    "      ERA COMPILER\r\n" +
                                    "  INNOPOLIS UNIVERSITY\r\n" +
                                    "\r\n" +
                                    "  Possible arguments:\r\n" +
                                    "  '-s' {filepath}  :  specify input file to be compiled\r\n" +
                                    "  '-d' {path}  :  specify the folders with files to be compiled\r\n" +
                                    "  '-o' {filepath}  :  specify output file for compiled source code\r\n" +
                                    "  '--lexis'  :  compile in lexis mode (separate into tokens only)\r\n" +
                                    "  '--syntax'  :  compile in syntax mode (build AST only)\r\n" +
                                    "  '--semantics'  :  compile in semantic mode (build AAST only)\r\n" +
                                    "  '--flog'  :  put compilation logs into file\r\n" +
                                    "  '-h'  :  show manual\r\n" +
                                    "  Default source code file is 'code.era'.\r\n" +
                                    "  Default output file is 'compiled_' + source code file name\r\n"
                                    );
                                break;
                            }
                        case "--lexis":
                            {
                                cmode = Compiler.CompilationMode.LEXIS;
                                break;
                            }
                        case "--syntax":
                            {
                                cmode = Compiler.CompilationMode.SYNTAX;
                                break;
                            }
                        case "--semantics":
                            {
                                cmode = Compiler.CompilationMode.SEMANTICS;
                                break;
                            }
                        case "--flog":
                            {
                                Logger.LoggingMode = 0;
                                break;
                            }
                        default:
                            {
                                Logger.LogError(new CompilationError("Unknown parameter\"" + args[i] +"\" !!!"));
                                break;
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
