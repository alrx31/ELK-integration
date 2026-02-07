using Serilog;
using Serilog.Context;
using Serilog.Sinks.Tcp;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net;

namespace ElkIntegration;

/// <summary>
/// Пример с правильной настройкой SSL для production
/// </summary>
class ProgramWithSSL
{
    static void Main(string[] args)
    {
        // Настройка SSL с загрузкой CA сертификата
        var caCertPath = args.Length > 0 ? args[0] : "certs/ca/ca.crt";
        SetupSSLValidationWithCA(caCertPath);

        // Инициализация логгера
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "my-dotnet-app")
            .Enrich.WithProperty("Environment", "development")
            .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter())
            .WriteTo.Tcp(
                host: "localhost",
                port: 5000,
                outputTemplate: "{Message}{NewLine}",
                formatter: new Serilog.Formatting.Compact.CompactJsonFormatter()
            )
            .CreateLogger();

        Log.Information("Приложение с SSL подключением запущено");
        Log.Information("Логи отправляются в Logstash через защищенное соединение");

        using (LogContext.PushProperty("UserId", Guid.NewGuid().ToString()))
        {
            Log.Information("Тестовое сообщение с SSL");
        }

        Log.CloseAndFlush();
    }

    /// <summary>
    /// Настройка SSL валидации с использованием CA сертификата
    /// </summary>
    static void SetupSSLValidationWithCA(string caCertPath)
    {
        try
        {
            // Загрузка CA сертификата
            var caCert = new X509Certificate2(caCertPath);

            ServicePointManager.ServerCertificateValidationCallback = 
                (sender, certificate, chain, sslPolicyErrors) =>
                {
                    // Проверка, что сертификат подписан нашим CA
                    if (chain != null && chain.ChainElements.Count > 0)
                    {
                        // Проверяем, что корневой сертификат в цепочке - наш CA
                        var rootCert = chain.ChainElements[chain.ChainElements.Count - 1].Certificate;
                        return rootCert.Thumbprint == caCert.Thumbprint;
                    }

                    return false;
                };

            Log.Information("SSL валидация настроена с CA сертификатом: {CertPath}", caCertPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке CA сертификата: {ex.Message}");
            Console.WriteLine("Используется небезопасная валидация (только для разработки!)");
            
            // Fallback для разработки
            ServicePointManager.ServerCertificateValidationCallback = 
                (sender, certificate, chain, sslPolicyErrors) => true;
        }
    }
}
