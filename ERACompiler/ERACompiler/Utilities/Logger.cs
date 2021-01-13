using System;
using System.IO;
using System.Collections.Generic;
using ERACompiler.Utilities.Errors;

namespace ERACompiler.Utilities
{
    /// <summary>
    /// Used to log the compilation process and display all the errors in compilation.
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// 0 for file logging, 1 for console logging
        /// </summary>
        public static int LoggingMode { get; set; } = 1;
        
        private static bool isLoggingEnabled = false;
        private static string logFilePath = "";

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="path">Output file for logs.</param>
        /// <param name="enableLogging">Whether or not to log anything.</param>
        public Logger(string path, bool enableLogging = false)
        {
            isLoggingEnabled = enableLogging;
            logFilePath = path;
        }

        /// <summary>
        /// Constuctor without specification of the output file.
        /// </summary>
        /// <param name="enableLogging">Whether or not to log anything.</param>
        public Logger(bool enableLogging = false) : this("compilation_log " + DateTime.Now.ToString("MM_dd_yyyy HH_mm_ss") + ".log", enableLogging)
        {           
        }

        /// <summary>
        /// Logs exception to the specified log file.
        /// </summary>
        /// <param name="ex">Exception to be logged.</param>
        public static void LogException(Exception ex)
        {
            if (isLoggingEnabled)
            {
                
                if (isLoggingEnabled)
                {
                    switch (LoggingMode)
                    {
                        case 0:
                            {
                                File.AppendAllText(logFilePath, ex.Message + "\r\n\r\n");
                                break;
                            }
                        case 1:
                            {
                                Console.WriteLine(ex.ToString() + "\r\n\r\n");
                                break;
                            }
                    }
                }
            }
        }

        /// <summary>
        /// Logs the compilation error to the specified log file and exits the program exection.
        /// </summary>
        /// <param name="err">Error to be logged.</param>
        public static void LogError(CompilationError err)
        {
            if (isLoggingEnabled)
            {
                switch(LoggingMode)
                {
                    case 0:
                        {
                            File.AppendAllText(logFilePath, "\r\nIn file \"\r\n" + Program.currentFile + "\":\r\n" + err.ToString() + "\r\n");
                            break;
                        }
                    case 1:
                        {
                            Console.WriteLine("\r\nIn file \"" + Program.currentFile + "\":\r\n" + err.ToString() + "\r\n");
                            break;
                        }
                }
            }
            Environment.Exit(0);
        }

        /// <summary>
        /// Logs the compilation errors to the specified log file and exits the program exection.
        /// </summary>
        /// <param name="errList">Error list to be logged.</param>
        public static void LogErrors(List<CompilationError> errList)
        {
            foreach (var err in errList)
            {
                if (isLoggingEnabled)
                {
                    switch (LoggingMode)
                    {
                        case 0:
                            {
                                File.AppendAllText(logFilePath, "\r\nIn file \"" + Program.currentFile + "\":\r\n" + err.ToString() + "\r\n");
                                break;
                            }
                        case 1:
                            {
                                Console.WriteLine("\r\nIn file \"" + Program.currentFile + "\":\r\n" + err.ToString() + "\r\n");
                                break;
                            }
                    }
                }
            }
            Environment.Exit(0);
        }

        /// <summary>
        /// Enables the Logger to log compilation process to the file.
        /// </summary>
        public static void EnableLogging()
        {
            isLoggingEnabled = true;
        }
        /// <summary>
        /// Disables the Logger from logging compilation process to the file.
        /// </summary>
        public static void DisableLogging()
        {
            isLoggingEnabled = false;
        }
    }
}
