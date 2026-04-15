# Swift-Marketplace
### Tech
- .NET 10
- PostgreSQL
- ElasticSearch
- Dapper
- FluentMigrator
- xUnit
- Kafka
- Redis
- gRPC
- Prometheus (future)
- Grafana (future)
### Architecture
- Microservices
- SAGA
- Clean Architecture [for services]


## Service
- ### ApiService
- - gRPC relationships with services data
- - RestAPI for Auth
- ### CatalogService
- - gRPC relationships with services data
- ### IdentityService
- - RestAPI for Auth
- ### PersonalService
- - gRPC relationships with services data ??
- ### PaymentService
- - gRPC relationships with services data
- ### NotificationService
- - gRPC relationships with services data
- ### OrderService
- - gRPC relationships with services data

#### Clean Architecture {HINT}:
1. ##### Domain Layer (Entities)
   Contains business models and rules.
   Pure C# classes with no external dependencies.
2. ##### Application Layer (Use Cases)
   Defines application logic (e.g., CQRS, MediatR).
   Contains interfaces for repositories and services.
3. ##### Infrastructure Layer
   Implements external concerns:
   Database (EF Core, Dapper)
   APIs, Email, File Storage
   Depends on the Application Layer, not the other way around.
4. ##### Presentation Layer (UI)
   ASP.NET Core Web API / MVC / Blazor
   Minimal logic; delegates work to the Application Layer.