# .NET интеграция с ELK стеком

## Требования

- .NET 8.0 SDK или выше
- Visual Studio 2022 или VS Code

## Установка зависимостей

```bash
dotnet restore
```

**Примечание:** Проект использует кастомный `TcpSink` для отправки логов в Logstash через TCP, так как стандартные пакеты Serilog.Sinks.Tcp имеют ограничения. Кастомный sink находится в файле `TcpSink.cs`.

## Настройка SSL сертификата

Перед запуском приложения убедитесь, что у вас есть CA сертификат:

1. Скопируйте CA сертификат из директории `src/certs/ca/ca.crt` в `certs/ca/`
2. Или укажите путь к сертификату как аргумент командной строки

## Запуск приложения

### Базовый пример (без SSL валидации)

```bash
dotnet run
```

### Пример с SSL валидацией

```bash
dotnet run -- certs/ca/ca.crt
```

## Конфигурация

Основная конфигурация находится в `appsettings.json`:

- **Console** - вывод логов в консоль
- **Tcp** - отправка логов в Logstash через TCP

## Использование в вашем проекте

1. Добавьте пакеты NuGet (см. `ElkIntegration.csproj`)
2. Настройте Serilog в `Program.cs` или `Startup.cs`
3. Укажите адрес и порт Logstash
4. Настройте SSL валидацию для production

### Пример для ASP.NET Core

```csharp
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Настройка Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.Run();
```

## Примеры логирования

```csharp
using Serilog;
using Serilog.Context;

// Простое логирование
Log.Information("Сообщение");

// С параметрами
Log.Information("Пользователь {UserId} выполнил действие {Action}", 
    userId, "login");

// С контекстом
using (LogContext.PushProperty("UserId", userId))
using (LogContext.PushProperty("RequestId", requestId))
{
    Log.Information("Запрос обработан");
}

// Структурированные данные
var data = new { UserId = userId, Action = "login", IP = "192.168.1.1" };
Log.Information("Действие выполнено {@Data}", data);

// Ошибки
try
{
    // код
}
catch (Exception ex)
{
    Log.Error(ex, "Ошибка при выполнении операции");
}
```

## Структура логов

Логи отправляются в формате JSON (Compact):

```json
{
  "@t": "2026-02-04T10:30:00.000Z",
  "@l": "Information",
  "@m": "Приложение запущено",
  "Application": "my-dotnet-app",
  "Environment": "development",
  "UserId": "123",
  "SourceContext": "ElkIntegration.Program"
}
```

## SSL настройка для production

В production обязательно настройте правильную валидацию SSL сертификатов:

```csharp
// Загрузка CA сертификата
var caCert = new X509Certificate2("certs/ca/ca.crt");

ServicePointManager.ServerCertificateValidationCallback = 
    (sender, certificate, chain, sslPolicyErrors) =>
    {
        if (chain != null && chain.ChainElements.Count > 0)
        {
            var rootCert = chain.ChainElements[chain.ChainElements.Count - 1].Certificate;
            return rootCert.Thumbprint == caCert.Thumbprint;
        }
        return false;
    };
```

## Интеграция с ASP.NET Core

В `Program.cs`:

```csharp
using ElkIntegration;

builder.Host.UseSerilog((context, services, configuration) => 
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Sink(new TcpSink("localhost", 5000, 
            new Serilog.Formatting.Compact.CompactJsonFormatter()))
);
```

В `appsettings.json`:

```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "Tcp",
        "Args": {
          "host": "localhost",
          "port": 5000
        }
      }
    ]
  }
}
```
