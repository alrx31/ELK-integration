using Logs.Domain;

namespace Logs.Application;

public interface ILogWriter
{
    void Log(LogEntry entry);
    void Log(string message);
    void Log(string message, LogLevel logLevel);
}
