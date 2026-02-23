# AgroSolutions - Properties Service

Serviço de gestão de propriedades (Core Domain) responsável pelo cadastro de produtores, fazendas, talhões e sensores.

## 📋 Visão Geral

Este é o **coração cadastral** da plataforma AgroSolutions, implementando o padrão **CQRS** com Clean Architecture sobre PostgreSQL para garantir integridade referencial forte (ACID).

### Responsabilidades

- ✅ CRUD completo de **Produtores** (sincronizados via eventos do serviço Identity)
- ✅ CRUD completo de **Fazendas** (propriedades rurais)
- ✅ CRUD completo de **Talhões** (subdivisões com culturas e status)
- ✅ CRUD completo de **Sensores** (dispositivos IoT)
- ✅ **Publicação de eventos de domínio** via AWS SNS (`agrosolutions-property-events`)
- ✅ **Consumo de eventos** do worker-alerts (`StatusChangedEvent`) e do serviço Identity (`UserCreatedEvent`, `UserUpdatedEvent`, `UserDeletedEvent`)
- ✅ **Outbox Pattern** para entrega garantida de eventos
- ✅ **Circuit Breaker** com Polly v8 para resiliência na publicação
- ✅ **Rate Limiting** por cliente via header `X-Client-Id`

## 🏗️ Arquitetura

### Clean Architecture

```
┌─────────────────────────────────────────────┐
│              API Layer                      │
│  Controllers | Middlewares | Configurations │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│          Application Layer                  │
│  Commands | Queries | Handlers | Validators │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│         Infrastructure Layer                │
│  DbContext | Repositories | MassTransit     │
│  ResilientEventPublisher | OutboxProcessor  │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│            Domain Layer                     │
│  Entities | Enums | Interfaces | Events     │
└─────────────────────────────────────────────┘
```

### Modelo de Dados

```
Produtor (Nome, Email — sincronizado do Identity)
    └── Fazenda (Nome, Área, Latitude, Longitude)
            └── Talhao (Nome, Cultura, Status, DataPlantio)
                    └── Sensor (CodigoIdentificacao, Tipo, Status)
```

### Hierarquia de Enums

| Enum | Valores |
|------|---------|
| `TipoSensor` | UmidadeSolo, Temperatura, Precipitacao, UmidadeAr, pH, Luminosidade, VelocidadeVento |
| `StatusSensor` | Ativo, Inativo, EmManutencao, Removido |
| `TalhaoStatus` | Normal, Atencao, Critico, EmManutencao |
| `TypeSensor` | Solo, Silos, Meteorologica |
| `TypeOperation` | Create, Update, Delete |

## 🚀 Tecnologias

| Tecnologia | Versão | Uso |
|-----------|--------|-----|
| .NET | 10 | Framework principal |
| C# | 12 | Primary Constructors, file-scoped namespaces |
| Entity Framework Core | 10 | ORM e migrações |
| PostgreSQL | 17 | Banco de dados relacional |
| MassTransit | latest | Abstração de mensageria |
| AWS SQS/SNS | — | Transporte de mensagens |
| MediatR | — | CQRS pattern |
| FluentValidation | — | Validação de comandos |
| AutoMapper | 12 | Mapeamento Entity → DTO |
| Polly | v8 | Circuit breaker e retry |
| Serilog | — | Logging estruturado |
| OpenTelemetry | — | Métricas e tracing distribuído |
| Scalar | — | Documentação interativa da API |
| JWT Bearer | — | Autenticação via Keycloak |

## 📦 Estrutura do Projeto

```
agrosolutions-service-properties/
├── src/
│   ├── AgroSolutions.Properties.Domain/
│   │   ├── Entities/          # BaseEntity, Produtor, Fazenda, Talhao, Sensor, OutboxMessage
│   │   ├── Enums/             # TipoSensor, StatusSensor, TalhaoStatus, TypeSensor, TypeOperation
│   │   ├── Interfaces/        # IRepository, IEventPublisher, repositórios específicos
│   │   └── Events/            # SensorEvent, StatusChangedEvent, UserCreatedEvent, UserUpdatedEvent, UserDeletedEvent
│   ├── AgroSolutions.Properties.Application/
│   │   ├── Commands/
│   │   │   ├── Fazendas/      # Create, Update, Delete + Validators
│   │   │   ├── Produtores/    # Create + Validator
│   │   │   ├── Sensores/      # Create, Update, Delete + Validators
│   │   │   └── Talhoes/       # Create, Update, Delete + Validators
│   │   ├── Queries/
│   │   │   ├── Fazendas/      # GetAll, GetById, GetByProdutor
│   │   │   ├── Produtores/    # GetAll
│   │   │   ├── Sensores/      # GetById, GetByTalhao
│   │   │   └── Talhoes/       # GetAll, GetById, GetByFazenda
│   │   └── Mappings/          # MappingProfile (AutoMapper)
│   ├── AgroSolutions.Properties.Infrastructure/
│   │   ├── Data/              # PropertiesDbContext, Migrations/
│   │   ├── Repositories/      # Fazenda, Produtor, Sensor, Talhao, Outbox, Repository base
│   │   └── Messaging/
│   │       ├── ResilientEventPublisher.cs   # Circuit breaker + métricas + Outbox fallback
│   │       ├── OutboxProcessorService.cs    # Worker de reprocessamento do Outbox
│   │       └── Consumers/
│   │           ├── StatusChangedEventConsumer.cs
│   │           └── UserEventsConsumer.cs
│   ├── AgroSolutions.Properties.Api/
│   │   ├── Controllers/V1/    # Fazendas, Produtores, Sensores, Talhoes
│   │   ├── Middlewares/       # CorrelationIdMiddleware, ExceptionHandlingMiddleware
│   │   └── Configurations/    # ApiDoc, Auth, Database, DI, HealthChecks, Infrastructure, Observability, RateLimiting
│   └── AgroSolutions.Properties.Shared/
│       ├── DTOs/              # FazendaDto, ProdutorDto, SensorDto, TalhaoDto
│       └── Models/            # ApiResponse<T>
├── tests/
│   └── AgroSolutions.Properties.Tests/
│       └── Application/Commands/   # CreateProdutorCommandHandlerTests
├── k8s/production/            # Manifests Kubernetes (EKS)
├── .github/workflows/         # deploy.yml (CI/CD)
├── Dockerfile
├── docker-compose.yml
└── README.md
```

## 🔧 Configuração

### Variáveis de Ambiente

```bash
# Database
ConnectionStrings__DefaultConnection=Host=postgres;Database=agrosolutions_properties;Username=postgres;Password=postgres

# Keycloak JWT
Jwt__Authority=http://keycloak:8080/realms/agrosolutions
Jwt__Audience=agrosolutions-api

# AWS
AWS__Region=sa-east-1
AWS__SQS__Queues__StatusChangedQueue=agrosolutions-status-changed-queue
AWS__SQS__Queues__ProdutorSyncQueue=agrosolutions-produtor-sync-queue
AWS__SNS__Topics__PropertiesEventsTopic=arn:aws:sns:sa-east-1:316295889438:agrosolutions-property-events

# Rate Limiting (opcional — padrão: habilitado)
RateLimiting__EnableRateLimiting=true
RateLimiting__DefaultLimit=100
RateLimiting__DefaultPeriodInSeconds=60
RateLimiting__PublicLimit=50
RateLimiting__AuthenticatedLimit=200
```

### Credenciais AWS

Para conectar às filas SQS e tópicos SNS, configure suas credenciais AWS:

#### Opção 1: Arquivo .env (recomendado para desenvolvimento)

```bash
cp .env.example .env
# Edite .env com suas credenciais
AWS_ACCESS_KEY_ID=sua-access-key
AWS_SECRET_ACCESS_KEY=sua-secret-key
AWS_SESSION_TOKEN=seu-session-token  # se usar credenciais temporárias
```

#### Opção 2: Variáveis de ambiente do sistema

```bash
export AWS_ACCESS_KEY_ID=sua-access-key
export AWS_SECRET_ACCESS_KEY=sua-secret-key
export AWS_SESSION_TOKEN=seu-session-token
```

#### Opção 3: IAM Role (produção em EKS)

Em produção, as credenciais são providas automaticamente via **IRSA** (IAM Roles for Service Accounts), sem necessidade de variáveis de ambiente.

⚠️ **Nunca commite o arquivo `.env` com credenciais reais!** O `.env.example` contém apenas placeholders.

## 🐳 Docker

### Build

```bash
docker build -t agrosolutions/properties-service:latest .
```

### Executar com Docker Compose

```bash
docker-compose up -d
```

O serviço estará disponível em `http://localhost:5002`

## 📊 Endpoints

### Produtores

> Produtores são gerenciados pelo serviço Identity e sincronizados via eventos. O endpoint abaixo é somente leitura.

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/v1/produtores` | Lista todos os produtores |

### Fazendas

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/v1/fazendas` | Lista todas as fazendas |
| GET | `/v1/fazendas/{id}` | Obtém fazenda por ID |
| GET | `/v1/fazendas/produtor/{produtorId}` | Lista fazendas de um produtor |
| POST | `/v1/fazendas` | Cria nova fazenda |
| PUT | `/v1/fazendas/{id}` | Atualiza fazenda |
| DELETE | `/v1/fazendas/{id}` | Remove fazenda (soft delete) |

### Talhões

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/v1/talhoes` | Lista todos os talhões |
| GET | `/v1/talhoes/{id}` | Obtém talhão por ID |
| GET | `/v1/talhoes/fazenda/{fazendaId}` | Lista talhões de uma fazenda |
| POST | `/v1/talhoes` | Cria novo talhão (publica `TalhaoCreatedEvent`) |
| PUT | `/v1/talhoes/{id}` | Atualiza talhão |
| DELETE | `/v1/talhoes/{id}` | Remove talhão (soft delete) |

### Sensores

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/v1/sensores/{id}` | Obtém sensor por ID |
| GET | `/v1/sensores/talhao/{talhaoId}` | Lista sensores de um talhão |
| POST | `/v1/sensores` | Cria novo sensor (publica `SensorEvent`) |
| PUT | `/v1/sensores/{id}` | Atualiza sensor (publica `SensorEvent`) |
| DELETE | `/v1/sensores/{id}` | Remove sensor (publica `SensorEvent` com `TypeOperation.Delete`) |

### Health Checks

| Rota | Descrição |
|------|-----------|
| `GET /health` | Status geral da aplicação |
| `GET /health/ready` | Readiness probe (verifica conexão ao banco) |
| `GET /health/live` | Liveness probe |

Todos os endpoints (exceto `/health*`) requerem autenticação JWT via Keycloak.

## 🔄 Fluxo de Eventos

### Publicação (Outbound) — Tópico `agrosolutions-property-events`

```
POST /v1/sensores
  → CreateSensorCommand (MediatR)
  → CreateSensorCommandHandler
  → SensorRepository.AddAsync()
  → ResilientEventPublisher.PublishAsync(SensorEvent { TypeOperation.Create })
      ├── [circuito fechado] → MassTransit → AWS SNS
      └── [circuito aberto]  → OutboxRepository.SaveAsync() → OutboxMessage (PostgreSQL)
                                        ↑
                               OutboxProcessorService (background worker)
                               reprocessa periodicamente até publicar com sucesso
```

### Consumo (Inbound)

| Fila SQS | Consumer | Ação |
|----------|----------|------|
| `agrosolutions-status-changed-queue` | `StatusChangedEventConsumer` | Atualiza `Talhao.Status` com base no evento do worker-alerts |
| `agrosolutions-produtor-sync-queue` | `UserEventsConsumer` | Cria/atualiza/desativa `Produtor` quando usuário é criado/atualizado/deletado no Identity |

### Resiliência de Mensageria

- **Circuit Breaker** (Polly v8): abre com 50% de falhas em 30s; duração do break: 30s
- **Retry** (MassTransit): exponential backoff com 5 tentativas (2s → 5min)
- **Dead Letter Queue**: mensagens que esgotam retries vão automaticamente para DLQ no SQS
- **Outbox Pattern**: garante exatamente-uma-vez mesmo com AWS indisponível momentaneamente

## 🔒 Segurança

- ✅ **Autenticação JWT** via Keycloak (todos os endpoints, exceto health)
- ✅ **Rate Limiting** por `X-Client-Id` (fallback: IP)
  - Padrão global: 100 req/min
  - Endponts públicos (`PublicApi`): 50 req/min
  - Endpoints autenticados (`AuthenticatedApi`): 200 req/min
  - Resposta ao exceder limite: `429 Too Many Requests` + header `Retry-After`
  - Desabilitar em desenvolvimento: `RateLimiting:EnableRateLimiting=false`
- ✅ **Container não-root** (usuário 1001)
- ✅ **Secrets via variáveis de ambiente** (nunca em código)
- ✅ **Validação de entrada** com FluentValidation em todos os comandos

## 🧪 Testes

```bash
dotnet restore
dotnet build
dotnet test
```

Os testes ficam em `tests/AgroSolutions.Properties.Tests/` espelhando a estrutura de `src/`. Atualmente cobrem handlers de comandos (`Application/Commands/`).

## 📈 Observabilidade

| Recurso | Implementação |
|---------|--------------|
| Logs estruturados | Serilog com `UseSerilogRequestLogging()` |
| Métricas | OpenTelemetry — contadores `events.published`, `events.failed`, histograma `events.publish.duration` |
| Tracing distribuído | OpenTelemetry |
| Correlation ID | `CorrelationIdMiddleware` injeta `X-Correlation-ID` em todas as requisições |
| Health Checks | `/health`, `/health/ready` (DB), `/health/live` |

## 🚀 Desenvolvimento

### Build & Run

```bash
# Build da solução
dotnet build AgroSolutions.Properties.sln

# Rodar a API (aplica migrações automaticamente ao iniciar)
cd src/AgroSolutions.Properties.Api
dotnet run

# Docker Compose (PostgreSQL + API)
docker-compose up -d
```

### Migrações EF Core

```bash
# Adicionar nova migration
dotnet ef migrations add <NomeDaMigration> \
  -p src/AgroSolutions.Properties.Infrastructure \
  -s src/AgroSolutions.Properties.Api

# Aplicar manualmente
dotnet ef database update \
  -p src/AgroSolutions.Properties.Infrastructure \
  -s src/AgroSolutions.Properties.Api
```

## 🚀 CI/CD & Deployment

### GitHub Actions

**Deploy** (`.github/workflows/deploy.yml`)
- Trigger: Push para `main` com mudanças em `k8s/**` ou execução manual
- Deploy automatizado no EKS
- Verificação de rollout e health checks

### Kubernetes (AWS EKS)

Manifests em `k8s/production/`:

| Manifest | Conteúdo |
|----------|---------|
| `deployment.yaml` | API deployment (1 réplica base) |
| `hpa.yaml` | HPA (1–2 réplicas, 70% CPU) |
| `databases.yaml` | PostgreSQL 17-alpine |
| `configmaps.yaml` | Configurações não-secretas |
| `infrastructure.yaml` | ServiceAccount com IRSA |
| `ingress-aws.yaml` | ALB Ingress |
| `observability.yaml` | ServiceMonitor Prometheus |
| `services.yaml` | Services internos |
| `volumes.yaml` / `storage-class.yaml` | EBS gp3 10Gi |

**Recursos por pod**:
- API: 256Mi RAM, 200m CPU
- Database: 256Mi RAM, 100m CPU

**Segurança**:
- IRSA para acesso ao SQS/SNS sem credenciais estáticas
- Container não-root (usuário 1001)
- Network Policies

## 🚦 Resumo de Features Implementadas

- ✅ Clean Architecture com CQRS (MediatR)
- ✅ CRUD completo para todas as entidades da hierarquia
- ✅ Sincronização de Produtores via eventos do serviço Identity
- ✅ Publicação de eventos de sensor para o tópico SNS `agrosolutions-property-events`
- ✅ Outbox Pattern para entrega garantida (PostgreSQL → SNS)
- ✅ Circuit Breaker com Polly v8 (50% falhas / 30s break)
- ✅ Retry com exponential backoff no MassTransit (5x, 2s–5min)
- ✅ Dead Letter Queue automático pelo SQS
- ✅ Rate Limiting por cliente (`X-Client-Id`)
- ✅ Métricas OpenTelemetry (`events.published`, `events.failed`, `events.publish.duration`)
- ✅ Logging estruturado com Serilog e Correlation ID
- ✅ Versionamento de API (`v1`)
- ✅ Auto-scaling com HPA no EKS
- ✅ CI/CD com GitHub Actions
- ✅ Health checks (liveness, readiness)

## 📄 Licença

Projeto desenvolvido para o Hackathon AgroSolutions - Agricultura 4.0

---

**AgroSolutions** - Transformando a agricultura através da tecnologia 🌱
