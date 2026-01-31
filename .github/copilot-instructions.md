# AgroSolutions Properties Service - AI Agent Instructions

## Architecture Overview

This is a **CQRS Write Side** microservice implementing Clean Architecture for managing agricultural properties (Produtores → Fazendas → Talhões → Sensores hierarchy). Uses .NET 10, PostgreSQL with strong ACID guarantees, and publishes domain events via MassTransit/RabbitMQ.

### Layer Responsibilities
- **Domain**: Entities (`Produtor`, `Fazenda`, `Talhao`, `Sensor`) inherit from `BaseEntity` (Id, CreatedAt, UpdatedAt, IsActive). Events live here.
- **Application**: CQRS with MediatR. Commands return `Guid` or `Unit`. Queries return DTOs. FluentValidation for each command. AutoMapper profiles in `Mappings/`.
- **Infrastructure**: EF Core repositories, `PropertiesDbContext`, MassTransit consumers (`StatusChangedEventConsumer`, `ProdutorEventsConsumer`).
- **API**: Minimal controllers delegate to MediatR. Version prefix: `v{version:apiVersion}/`. Auth via JWT/Keycloak.

## Critical Patterns

### CQRS Command/Query Pattern
Commands and Queries follow strict naming:
```csharp
// Command: IRequest<Guid> for creates, IRequest<Unit> for updates/deletes
public class CreateSensorCommand : IRequest<Guid> { ... }

// Handler: Primary constructors for DI (C# 12)
public class CreateSensorCommandHandler(ISensorRepository repo, IEventPublisher publisher) 
    : IRequestHandler<CreateSensorCommand, Guid> { ... }

// Validator: AbstractValidator<TCommand>
public class CreateSensorCommandValidator : AbstractValidator<CreateSensorCommand> { ... }
```

### Primary Constructors (C# 12)
**ALL** classes use primary constructors. Never use field injection or traditional constructors:
```csharp
// ✅ Correct
public class MyService(IDependency dep) { ... }

// ❌ Wrong - Don't use
public class MyService 
{
    private readonly IDependency _dep;
    public MyService(IDependency dep) { _dep = dep; }
}
```

### AutoMapper Registration
**CRITICAL**: AutoMapper requires lambda configuration in DI setup:
```csharp
// ✅ Correct - AddAutoMapper extension method
services.AddAutoMapper(typeof(MappingProfile).Assembly);

// ❌ Wrong - Manual registration causes CS1503 error
services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
```
Profiles are in `Application/Mappings/MappingProfile.cs`. Use `CreateMap<Entity, Dto>()` for entity-to-DTO only.

### Event Publishing
After create/update operations, publish domain events:
```csharp
var sensor = new Sensor { ... };
await repository.AddAsync(sensor);

var @event = new SensorUpdatedEvent 
{ 
    SensorId = sensor.Id, 
    TipoSensor = sensor.Tipo.ToString(),
    Timestamp = DateTime.UtcNow 
};
await eventPublisher.PublishAsync(@event);
```
Events are in `Domain/Events/`. MassTransit publishes to RabbitMQ topic exchanges automatically.

### EF Core Configuration
Fluent API configurations in `PropertiesDbContext.OnModelCreating()`:
- Use `HasMaxLength()` for strings
- `HasColumnType("decimal(18,2)")` for decimals
- `HasIndex().IsUnique()` for business keys (e.g., CPF)
- Cascade deletes for parent-child relationships

### API Controller Pattern
```csharp
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
[Authorize]
public class EntityController(IMediator mediator, ILogger<EntityController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EntityDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<EntityDto>>>> GetAll(CancellationToken ct)
    {
        var query = new GetAllQuery();
        var result = await mediator.Send(query, ct);
        return Ok(ApiResponse<IEnumerable<EntityDto>>.SuccessResponse(result));
    }
}
```

## Development Workflows

### Build & Run
```bash
# Build solution (from root)
dotnet build AgroSolutions.Properties.sln

# Run API (auto-runs migrations on startup)
cd src/AgroSolutions.Properties.Api
dotnet run

# Docker Compose (includes PostgreSQL, RabbitMQ, Keycloak)
docker-compose up -d
```

### Common Build Errors
1. **CS1503 AutoMapper error**: Use `AddAutoMapper(typeof(Profile).Assembly)` not lambda config
2. **Missing migration**: Add via `dotnet ef migrations add <Name> -p Infrastructure -s Api`
3. **RabbitMQ connection**: Ensure `RabbitMQ__Host` env var matches docker-compose service name

### Testing
Tests in `tests/AgroSolutions.Properties.Tests/`. Structure mirrors `src/` folders.
```bash
dotnet test
```

## Domain Model Hierarchy
```
Produtor (CPF unique, cascades deletes)
  ├── Fazenda (AreaTotal decimal(18,2), Lat/Long coordinates)
      ├── Talhao (Cultura, Status enum, FK to Fazenda)
          └── Sensor (CodigoIdentificacao, Tipo/Status enums, FK to Talhao)
```

## Messaging Integration

### Outbound Events (Published by this service)
- `SensorUpdatedEvent`: On sensor create/update → consumed by worker-alerts
- `SensorDeletedEvent`: On sensor delete → consumed by worker-alerts for cleanup
- `TalhaoCreatedEvent`: On talhao create → consumed by analytics services
- `Produtor*Event`: Lifecycle events for sync

### Inbound Events (Consumed by this service)
- `StatusChangedEvent` from worker-alerts → updates `Talhao.Status`
- Produtor events from identity service → syncs user data

Consumers in `Infrastructure/Messaging/Consumers/`. Queues configured in `InfrastructureConfiguration.cs` with MassTransit.

### Resilience Patterns

**Outbox Pattern**: All events are saved to `OutboxMessages` table and processed by `OutboxProcessorService` background worker. Guarantees exactly-once delivery even if RabbitMQ is down.

**Circuit Breaker**: `ResilientEventPublisher` uses Polly v8 circuit breaker (50% failure ratio, 30s break duration). When open, events are automatically saved to outbox.

**Retry Policy**: MassTransit configured with exponential backoff (5 retries, 2s-5min intervals). Per-endpoint retries for consumers (3 retries, 1s-1min).

**Dead Letter Queue**: Messages that fail after all retries are automatically routed to DLQ by RabbitMQ.

**OpenTelemetry Metrics**: 
- `events.published` - Counter of successfully published events
- `events.failed` - Counter of failed event publications
- `events.publish.duration` - Histogram of publish latency in milliseconds

## Configuration

Key `appsettings.json` sections:
- `ConnectionStrings:DefaultConnection`: PostgreSQL connection
- `Jwt:Authority` / `Jwt:Audience`: Keycloak realm URL
- `RabbitMQ:Host/Username/Password`: Message broker config

Environment-specific overrides: `appsettings.{Environment}.json`

## File Organization Rules

1. **One class per file** matching filename (e.g., `CreateSensorCommand.cs`)
2. **Commands/Queries**: Group by entity in `Application/Commands/{Entity}/` and `Application/Queries/{Entity}/`
3. **Validators**: Co-locate with command: `CreateSensorCommandValidator.cs` next to `CreateSensorCommand.cs`
4. **Events**: `Domain/Events/{EventName}Event.cs`

## Code Style Conventions

- Use **file-scoped namespaces** (`namespace X;` not `namespace X { }`)
- Prefer **nullable reference types** (`string?` for optional)
- Use **expression bodies** for simple methods: `public async Task Method() => await ...;`
- **Enum-to-string**: Map explicitly in AutoMapper: `.ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))`

## Security & Observability

- All endpoints require `[Authorize]` except health checks
- `CorrelationIdMiddleware` injects X-Correlation-ID for distributed tracing
- Serilog structured logging: `logger.LogError(ex, "Message with {Property}", value)`
- OpenTelemetry metrics/tracing configured in `ObservabilityConfiguration.cs`
