using Logs.Domain;
using Logs.Infrastructure;

namespace ElkIntegration;

class Program
{
    static void Main(string[] args)
    {
        var caPath = ResolveCaPath();
        var acceptAnyCert = string.IsNullOrWhiteSpace(caPath);

        using var logger = new Logger(new LogstashOptions
        {
            Host = "localhost",
            Port = 5000,
            UseSsl = true,
            CaCertificatePath = caPath,
            AcceptAnyCertificate = acceptAnyCert
        });

        WriteLog(logger, LogLevel.Info, "Приложение запущено");

        var userId = Guid.NewGuid().ToString();
        var sessionId = Guid.NewGuid().ToString();
        WriteLog(logger, LogLevel.Info, "Пользователь выполнил действие. Action: login, IP: 192.168.1.100",
            properties: new Dictionary<string, object?>
            {
                ["userId"] = userId,
                ["sessionId"] = sessionId
            });

        try
        {
            PerformOperation();
        }
        catch (Exception ex)
        {
            WriteLog(logger, LogLevel.Error, "Ошибка при выполнении операции. Operation: dataProcessing, ErrorCode: ERR_001",
                exception: ex);
        }

        var transactionId = Guid.NewGuid().ToString();
        WriteLog(logger, LogLevel.Info,
            "Транзакция завершена. TransactionId: {TransactionId}, Amount: {Amount}, Currency: {Currency}, Status: {Status}",
            properties: new Dictionary<string, object?>
            {
                ["transactionId"] = transactionId,
                ["amount"] = 1500.50,
                ["currency"] = "USD",
                ["status"] = "completed"
            });

        WriteLog(logger, LogLevel.Info, "Транзакция",
            properties: new Dictionary<string, object?>
            {
                ["transactionId"] = transactionId,
                ["amount"] = 1500.50,
                ["currency"] = "USD",
                ["status"] = "completed",
                ["timestamp"] = DateTimeOffset.UtcNow
            });

        WriteLog(logger, LogLevel.Info, "Приложение завершено");
    }

    static void PerformOperation()
    {
        Thread.Sleep(100);
    }

    static void WriteLog(
        Logger logger,
        LogLevel level,
        string message,
        IReadOnlyDictionary<string, object?>? properties = null,
        Exception? exception = null)
    {
        logger.Write(new LogEntry(
            Timestamp: DateTimeOffset.UtcNow,
            Level: level,
            Message: message,
            Application: "my-dotnet-app",
            Environment: "Development",
            CorrelationId: Guid.NewGuid().ToString(),
            Properties: properties,
            Exception: exception));
    }

    static string? ResolveCaPath()
    {
        var baseDirPath = Path.Combine(AppContext.BaseDirectory, "certs", "ca", "ca.crt");
        var cwdPath = Path.Combine(Directory.GetCurrentDirectory(), "certs", "ca", "ca.crt");

        if (File.Exists(baseDirPath))
        {
            return baseDirPath;
        }

        return File.Exists(cwdPath) ? cwdPath : null;
    }
}
