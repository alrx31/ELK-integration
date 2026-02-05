#!/bin/bash

# Скрипт для генерации SSL сертификатов для ELK стека

set -e

CERT_DIR="./certs"
CA_DIR="${CERT_DIR}/ca"
ELASTICSEARCH_DIR="${CERT_DIR}/elasticsearch"
LOGSTASH_DIR="${CERT_DIR}/logstash"
KIBANA_DIR="${CERT_DIR}/kibana"

# Создание директорий
mkdir -p ${CA_DIR}
mkdir -p ${ELASTICSEARCH_DIR}
mkdir -p ${LOGSTASH_DIR}
mkdir -p ${KIBANA_DIR}

echo "Генерация CA сертификата..."

# Генерация CA приватного ключа
openssl genrsa -out ${CA_DIR}/ca.key 4096

# Генерация CA сертификата
openssl req -new -x509 -days 3650 -key ${CA_DIR}/ca.key -out ${CA_DIR}/ca.crt \
  -subj "/C=RU/ST=Moscow/L=Moscow/O=ELK/CN=ELK-CA"

echo "Генерация сертификата для Elasticsearch..."

# Генерация приватного ключа для Elasticsearch
openssl genrsa -out ${ELASTICSEARCH_DIR}/elasticsearch.key 4096

# Генерация CSR для Elasticsearch
openssl req -new -key ${ELASTICSEARCH_DIR}/elasticsearch.key \
  -out ${ELASTICSEARCH_DIR}/elasticsearch.csr \
  -subj "/C=RU/ST=Moscow/L=Moscow/O=ELK/CN=elasticsearch"

# Создание конфигурации для SAN
cat > ${ELASTICSEARCH_DIR}/elasticsearch.conf <<EOF
[req]
distinguished_name = req_distinguished_name
req_extensions = v3_req

[req_distinguished_name]

[v3_req]
basicConstraints = CA:FALSE
keyUsage = nonRepudiation, digitalSignature, keyEncipherment
subjectAltName = @alt_names

[alt_names]
DNS.1 = elasticsearch
DNS.2 = localhost
IP.1 = 127.0.0.1
EOF

# Подписание сертификата CA
openssl x509 -req -days 3650 -in ${ELASTICSEARCH_DIR}/elasticsearch.csr \
  -CA ${CA_DIR}/ca.crt -CAkey ${CA_DIR}/ca.key -CAcreateserial \
  -out ${ELASTICSEARCH_DIR}/elasticsearch.crt \
  -extensions v3_req -extfile ${ELASTICSEARCH_DIR}/elasticsearch.conf

echo "Генерация сертификата для Logstash..."

# Генерация приватного ключа для Logstash
openssl genrsa -out ${LOGSTASH_DIR}/logstash.key 4096

# Генерация CSR для Logstash
openssl req -new -key ${LOGSTASH_DIR}/logstash.key \
  -out ${LOGSTASH_DIR}/logstash.csr \
  -subj "/C=RU/ST=Moscow/L=Moscow/O=ELK/CN=logstash"

# Создание конфигурации для SAN
cat > ${LOGSTASH_DIR}/logstash.conf <<EOF
[req]
distinguished_name = req_distinguished_name
req_extensions = v3_req

[req_distinguished_name]

[v3_req]
basicConstraints = CA:FALSE
keyUsage = nonRepudiation, digitalSignature, keyEncipherment
subjectAltName = @alt_names

[alt_names]
DNS.1 = logstash
DNS.2 = localhost
IP.1 = 127.0.0.1
EOF

# Подписание сертификата CA
openssl x509 -req -days 3650 -in ${LOGSTASH_DIR}/logstash.csr \
  -CA ${CA_DIR}/ca.crt -CAkey ${CA_DIR}/ca.key -CAcreateserial \
  -out ${LOGSTASH_DIR}/logstash.crt \
  -extensions v3_req -extfile ${LOGSTASH_DIR}/logstash.conf

echo "Генерация сертификата для Kibana..."

# Генерация приватного ключа для Kibana
openssl genrsa -out ${KIBANA_DIR}/kibana.key 4096

# Генерация CSR для Kibana
openssl req -new -key ${KIBANA_DIR}/kibana.key \
  -out ${KIBANA_DIR}/kibana.csr \
  -subj "/C=RU/ST=Moscow/L=Moscow/O=ELK/CN=kibana"

# Создание конфигурации для SAN
cat > ${KIBANA_DIR}/kibana.conf <<EOF
[req]
distinguished_name = req_distinguished_name
req_extensions = v3_req

[req_distinguished_name]

[v3_req]
basicConstraints = CA:FALSE
keyUsage = nonRepudiation, digitalSignature, keyEncipherment
subjectAltName = @alt_names

[alt_names]
DNS.1 = kibana
DNS.2 = localhost
IP.1 = 127.0.0.1
EOF

# Подписание сертификата CA
openssl x509 -req -days 3650 -in ${KIBANA_DIR}/kibana.csr \
  -CA ${CA_DIR}/ca.crt -CAkey ${CA_DIR}/ca.key -CAcreateserial \
  -out ${KIBANA_DIR}/kibana.crt \
  -extensions v3_req -extfile ${KIBANA_DIR}/kibana.conf

# Установка правильных прав доступа
# Elasticsearch требует права 644 для чтения сертификатов и ключей
# (в контейнере Elasticsearch работает от root или имеет доступ через volume)
chmod 644 ${CA_DIR}/ca.key
chmod 644 ${CA_DIR}/ca.crt
chmod 644 ${ELASTICSEARCH_DIR}/elasticsearch.key
chmod 644 ${ELASTICSEARCH_DIR}/elasticsearch.crt
chmod 644 ${LOGSTASH_DIR}/logstash.key
chmod 644 ${LOGSTASH_DIR}/logstash.crt
chmod 644 ${KIBANA_DIR}/kibana.key
chmod 644 ${KIBANA_DIR}/kibana.crt

# Удаление временных файлов
rm -f ${ELASTICSEARCH_DIR}/*.csr ${ELASTICSEARCH_DIR}/*.conf
rm -f ${LOGSTASH_DIR}/*.csr ${LOGSTASH_DIR}/*.conf
rm -f ${KIBANA_DIR}/*.csr ${KIBANA_DIR}/*.conf
rm -f ${CA_DIR}/*.srl

echo "Сертификаты успешно сгенерированы!"
echo ""
echo ""
echo "Важно:"
echo "1. По умолчанию пароль для пользователя 'elastic': test_password_123 (из docker-compose.override.yml)"
echo "2. Для production измените пароль через .env файл или свой override файл"
echo "3. См. PASSWORDS.md для подробной информации о настройке паролей"
