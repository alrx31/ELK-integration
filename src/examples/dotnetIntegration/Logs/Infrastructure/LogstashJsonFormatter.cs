using System.Text.Json;
using Logs.Domain;

namespace Logs.Infrastructure;

internal static class LogstashJsonFormatter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static string Format(LogEntry entry)
    {
        var payload = new Dictionary<string, object?>
        {
            ["@timestamp"] = entry.Timestamp.UtcDateTime,
            ["level"] = entry.Level.ToString().ToUpperInvariant(),
            ["message"] = entry.Message,
            ["application"] = entry.Application,
            ["environment"] = entry.Environment
        };

        if (!string.IsNullOrWhiteSpace(entry.CorrelationId))
        {
            payload["correlationId"] = entry.CorrelationId;
        }

        if (!string.IsNullOrWhiteSpace(entry.UserId))
        {
            payload["userId"] = entry.UserId;
        }

        if (entry.Exception != null)
        {
            payload["exception"] = new
            {
                type = entry.Exception.GetType().FullName,
                message = entry.Exception.Message,
                stackTrace = entry.Exception.StackTrace
            };
        }

        if (entry.Properties != null)
        {
            foreach (var kvp in entry.Properties)
            {
                payload[kvp.Key] = kvp.Value;
            }
        }

        return JsonSerializer.Serialize(payload, SerializerOptions);
    }
}
