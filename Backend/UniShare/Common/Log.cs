namespace UniShare.Common
{
    public static class Log
    {
        private static readonly string LogFilePath;
        private static readonly object Lock = new object();

        static Log()
        {
            try
            {
                // Go up from bin/Debug/net9.0 to the project root
                var projectRoot = Directory.GetParent(AppContext.BaseDirectory)?.Parent?.Parent?.Parent?.FullName;
                var logDirectory = projectRoot != null ? Path.Combine(projectRoot, "logs") : "logs";
                LogFilePath = Path.Combine(logDirectory, "log.txt");

                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create log directory: {ex.Message}");
                // Fallback to a local log file if directory creation fails
                LogFilePath = "log.txt";
            }
        }

        public static void Info(string message)
        {
            WriteLog("INFO", message);
        }

        public static void Warning(string message)
        {
            WriteLog("WARNING", message);
        }

        public static void Error(string message, Exception? ex = null)
        {
            var errorMessage = ex != null ? $"{message} - Exception: {ex}" : message;
            WriteLog("ERROR", errorMessage);
        }

        private static void WriteLog(string level, string message)
        {
            lock (Lock)
            {
                try
                {
                    var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
                    Console.WriteLine(logMessage);
                    File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to write to log file: {ex.Message}");
                }
            }
        }
    }
}
