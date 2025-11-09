using System;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace DisplayRotator
{
    internal static class Logger
    {
        private static readonly object _sync = new object();
        private static readonly string _logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DisplayRotator");
        private static readonly string _logPath = Path.Combine(_logDir, "displayrotator.log");

        public static void Info(string message) => Write("INFO", message);
        public static void Error(string message) => Write("ERROR", message);

        private static void Write(string level, string message)
        {
            try
            {
                lock (_sync)
                {
                    Directory.CreateDirectory(_logDir);
                    var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}{Environment.NewLine}";
                    File.AppendAllText(_logPath, line, Encoding.UTF8);

                    // Output to Visual Studio Output window / debugger
                    Debug.Write(line);

                    // Also write to console if possible (safe to ignore if unattached, e.g., in WinForms)
                    try
                    {
                        if (level == "ERROR")
                            Console.Error.Write(line);
                        else
                            Console.Out.Write(line);
                    }
                    catch { /* Ignore if console is not attached or unavailable */ }
                }
            }
            catch
            {
                // Ignore logging failures
            }
        }
    }
}
