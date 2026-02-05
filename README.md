# ELK Integration

Универсальный шаблон для подключения ELK стека (Elasticsearch, Logstash, Kibana) с поддержкой HTTPS и примерами интеграции для Java и .NET.

## Особенности

- ✅ Полная настройка ELK стека через Docker Compose
- ✅ HTTPS/SSL поддержка для всех компонентов
- ✅ Примеры интеграции для Java и .NET
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
docker-compose up -d
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
curl -k -u elastic:changeme https://localhost:9200/_cluster/health

# Просмотр логов контейнеров
docker logs logstash
```

## Интеграция с приложением

### Java

См. подробное руководство: [src/examples/java/README.md](src/examples/java/README.md)

```java
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.MDC;

Logger logger = LoggerFactory.getLogger(MyClass.class);
MDC.put("userId", "123");
logger.info("Сообщение");
```

### .NET

См. подробное руководство: [src/examples/dotnet/README.md](src/examples/dotnet/README.md)

```csharp
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Tcp("localhost", 5000)
    .CreateLogger();

Log.Information("Сообщение");
```

## Документация

- [Полное руководство по интеграции](src/INTEGRATION_GUIDE.md)
- [Примеры для Java](src/examples/java/)
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
│       ├── java/                   # Java примеры
│       └── dotnet/                 # .NET примеры
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
3. Перезапустите контейнеры: `docker-compose restart`

### Просмотр логов

```bash
# Все сервисы
docker-compose logs -f

# Конкретный сервис
docker logs logstash
docker logs elasticsearch
docker logs kibana
```

## Лицензия

См. файл [LICENSE](LICENSE)
