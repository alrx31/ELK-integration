using Logs.Domain;

namespace Logs.Application;

public interface ILogWriter
{
    void Write(LogEntry entry);
}
