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

    public void Write(LogEntry entry)
    {
        var payload = LogstashJsonFormatter.Format(entry);
        _connection.WriteLine(payload);
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}
