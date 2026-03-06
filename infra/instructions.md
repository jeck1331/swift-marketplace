# Проверка токена KeyCloack
curl.exe -s -X POST "http://keycloak:8080/realms/swift-marketplace-ecom/protocol/openid-connect/token" `-H "Content-Type: application/x-www-form-urlencoded" ` -d "grant_type=password&client_id=swift-marketplace-ecom-api&username=alice&password=password1"
# Kafka
## Создание топиков
    docker exec -it kafka kafka-topics --bootstrap-server kafka:9092 --create \ --topic orders.v1 --partitions 3 --replication-factor 1
    
    docker exec -it kafka kafka-topics --bootstrap-server kafka:9092 --create \ --topic payments.v1 --partitions 3 --replication-factor 1
    
    docker exec -it kafka kafka-topics --bootstrap-server kafka:9092 --create \ --topic orders.dlq.v1 --partitions 3 --replication-factor 1

## Проверка
    docker exec -it kafka kafka-topics --bootstrap-server kafka:9092 --list

    docker exec -it kafka kafka-console-consumer \ --bootstrap-server kafka:9092 \ --topic orders.v1 \ --from-beginning
    docker exec -it kafka kafka-console-producer \ --bootstrap-server kafka:9092 \ --topic orders.v1


docker run --rm --network swiftmarketplaceinfra_default curlimages/curl:8.7.1 `-s http://keycloak:8080/realms/swift-marketplace-ecom/.well-known/openid-configuration

# Общие
## Part of appesettings.json
    "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=app;Username=app;Password=app"
    },
    "Auth": {
    "Authority": "http://keycloak:8080/realms/swif-marketplace-ecom"
    },

# CatalogService
## Добавить запись в Hosts
    127.0.0.1 keycloak