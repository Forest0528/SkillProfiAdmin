using System;
using System.IO;

public static class Logger
{
    private static readonly string logFilePath = "App.log";

    public static void LogError(string message, Exception ex)
    {
        try
        {
            var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR: {message}\n{ex}\n";
            File.AppendAllText(logFilePath, logMessage);
        }
        catch
        {
            // Если логирование не удалось, пропустим исключение, чтобы приложение продолжило работу.
        }
    }

    public static void LogInfo(string message)
    {
        try
        {
            var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - INFO: {message}\n";
            File.AppendAllText(logFilePath, logMessage);
        }
        catch
        {
            // Если логирование не удалось, пропустим исключение.
        }
    }
}
