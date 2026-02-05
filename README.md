# ELK Integration

Универсальный шаблон для подключения ELK стека (Elasticsearch, Logstash, Kibana) с поддержкой HTTPS и примером интеграции для .NET.

## Особенности

- ✅ Полная настройка ELK стека через Docker Compose
- ✅ HTTPS/SSL поддержка для всех компонентов
- ✅ Пример интеграции для .NET
- ✅ Автоматическая генерация SSL сертификатов
- ✅ Готовые конфигурации для production

## Быстрый старт

### 1. Генерация SSL сертификатов

```bash
cd src
./generate-certs.sh
```

### 2. Запуск ELK стека

```bash
docker compose up -d
```

### 3. Доступ к сервисам

- **Elasticsearch**: https://localhost:9200
  - Пользователь: `elastic`
  - Пароль: `test_password_123` (тестовый пароль из `docker-compose.override.yml`)

- **Kibana**: https://localhost:5601
  - Пользователь: `elastic` ⚠️ **Важно: для входа используйте `elastic`, не `kibana_system`!**
  - Пароль: `test_password_123` (тестовый пароль из `docker-compose.override.yml`)

**Примечание:** `kibana_system` - служебный пользователь только для внутреннего подключения. Для веб-интерфейса используйте `elastic`. См. [src/KIBANA_ACCESS.md](src/KIBANA_ACCESS.md)

**Настройка паролей:** См. [src/PASSWORDS.md](src/PASSWORDS.md) для подробной информации о настройке паролей.

### 4. Проверка работы

```bash
# Проверка Elasticsearch
curl -k -u elastic:test_password_123 https://localhost:9200/_cluster/health

# Просмотр логов контейнеров
docker compose logs logstash
```

## Интеграция с приложением

### .NET

См. пример: [src/examples/dotnet/](src/examples/dotnet/)  
Библиотека логирования: [src/examples/dotnetIntegration/Logs](src/examples/dotnetIntegration/Logs)

```csharp
using Logs.Domain;
using Logs.Infrastructure;

using var logger = new Logger(new LogstashOptions
{
    Host = "localhost",
    Port = 5000,
    UseSsl = true,
    CaCertificatePath = "certs/ca/ca.crt"
});

logger.Write(new LogEntry(
    Timestamp: DateTimeOffset.UtcNow,
    Level: LogLevel.Info,
    Message: "Сообщение",
    Application: "my-app",
    Environment: "development"));
```

## Документация

- [Полное руководство по интеграции](src/INTEGRATION_GUIDE.md)
- [Примеры для .NET](src/examples/dotnet/)

## Структура проекта

```
.
├── src/
│   ├── docker-compose.yml          # Конфигурация ELK стека
│   ├── generate-certs.sh           # Скрипт генерации сертификатов
│   ├── INTEGRATION_GUIDE.md        # Подробное руководство
│   ├── certs/                      # SSL сертификаты (генерируются)
│   ├── logstash/                   # Конфигурации Logstash
│   └── examples/                   # Примеры интеграции
│       ├── dotnet/                 # .NET пример (использует Logs)
│       └── dotnetIntegration/      # .NET библиотека логирования
└── README.md                       # Этот файл
```

## Порты

- **5000** - Logstash TCP (SSL)
- **5044** - Logstash Beats (SSL)
- **9200** - Elasticsearch HTTPS
- **5601** - Kibana HTTPS

## Безопасность

⚠️ **Важно для production:**

1. Измените тестовые пароли:
   ```bash
   # Создайте .env файл с production паролем
   cp .env.example .env
   # Отредактируйте .env и установите безопасный пароль
   ```

2. Используйте доверенные CA сертификаты вместо самоподписанных

3. Настройте правильную SSL валидацию в клиентских приложениях

4. Ограничьте доступ к файлам с паролями:
   ```bash
   chmod 600 .env
   ```

Подробнее о управлении паролями: [src/PASSWORDS.md](src/PASSWORDS.md)

## Troubleshooting

### Проблемы с SSL

Если возникают проблемы с SSL сертификатами:

1. Убедитесь, что сертификаты сгенерированы: `ls -la src/certs/`
2. Проверьте права доступа: `chmod 600 src/certs/**/*.key`
3. Перезапустите контейнеры: `docker compose restart`

### Просмотр логов

```bash
# Все сервисы
docker compose logs -f

# Конкретный сервис
docker compose logs logstash
docker compose logs elasticsearch
docker compose logs kibana
```

## Лицензия

См. файл [LICENSE](LICENSE)
