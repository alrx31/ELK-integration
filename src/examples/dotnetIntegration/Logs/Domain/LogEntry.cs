namespace Logs.Domain;

public sealed record LogEntry(
    DateTimeOffset Timestamp,
    LogLevel Level,
    string Message,
    string Application,
    string Environment,
    string? CorrelationId = null,
    string? UserId = null,
    IReadOnlyDictionary<string, object?>? Properties = null,
    Exception? Exception = null);
