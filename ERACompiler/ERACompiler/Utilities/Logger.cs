using System;
using System.IO;
using ERACompiler.Utilities.Errors;

namespace ERACompiler.Utilities
{
    /// <summary>
    /// Used to log the compilation process and display all the errors in compilation.
    /// </summary>
    public class Logger
    {
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
                File.AppendAllText(logFilePath, ex.Message + "\r\n\r\n");
        }

        /// <summary>
        /// Logs the compilation error to the specified log file.
        /// </summary>
        /// <param name="er">Error to be logged.</param>
        public static void LogError(CompilationError er)
        {
            if (isLoggingEnabled)
                File.AppendAllText(logFilePath, er.ToString() + "\r\n\r\n");
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
