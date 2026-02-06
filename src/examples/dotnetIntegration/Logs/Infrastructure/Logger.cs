using Logs.Application;
using Logs.Domain;

namespace Logs.Infrastructure;

public sealed class Logger : ILogWriter, IDisposable
{
    private readonly LogstashConnection _connection;

    public Logger(LogstashOptions options)
    {
        _connection = new LogstashConnection(options);
    }

    public void Log(LogEntry entry)
    {
        var payload = LogstashJsonFormatter.Format(entry);
        _connection.WriteLine(payload);
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    public void Log(string message)
    {
        var logEntry = new LogEntry(
            DateTime.UtcNow,
            LogLevel.Info,
            message,
            null,
            null,
            null,
            null
        );

        var payload = LogstashJsonFormatter.Format(logEntry);
        _connection.WriteLine(payload);
    }

    public void Log(string message, LogLevel logLevel)
    {
        var logEntry = new LogEntry(
            DateTime.UtcNow,
            logLevel,
            message,
            null,
            null,
            null,
            null
        );

        var payload = LogstashJsonFormatter.Format(logEntry);
        _connection.WriteLine(payload);
    }
}
