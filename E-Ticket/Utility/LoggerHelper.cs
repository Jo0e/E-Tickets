using System.Text.Json;
using System.Xml.Linq;

namespace E_Ticket.Utility
{
    public static class LoggerHelper
    {
        private static readonly string LogDirectory = "Logs";
        private static readonly string LogFilePath = Path.Combine(LogDirectory, "AdminActions.json");
        public static void LogAdminAction(ILogger logger, string adminName, string action, string entityName)
        {
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }
            var logEntry = new LogEntry
            {
                AdminName = adminName,
                Action = action,
                EntityName = entityName,
                Timestamp = DateTime.UtcNow
            };

            List<LogEntry> logEntries;


            if (File.Exists(LogFilePath))
            {
                var existingLogs = File.ReadAllText(LogFilePath);
                logEntries = JsonSerializer.Deserialize<List<LogEntry>>(existingLogs) ?? new List<LogEntry>();
            }
            else
            {
                logEntries = new List<LogEntry>();
            }

            logEntries.Add(logEntry);
            var jsonLogs = JsonSerializer.Serialize(logEntries, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(LogFilePath, jsonLogs);

            // Regular logging
            string logMessage = $"Admin: {adminName}, Action: {action}, Entity: {entityName}, Timestamp: {DateTime.UtcNow}";
            logger.LogInformation(logMessage);

        }
    }
    public class LogEntry
    {
        public string AdminName { get; set; }
        public string Action { get; set; }
        public string EntityName { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
