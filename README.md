# AgroSolutions - Properties Service

Serviço de gestão de propriedades (Core Domain) responsável pelo cadastro de produtores, fazendas, talhões e sensores.

## 📋 Visão Geral

Este é o **coração cadastral** da plataforma AgroSolutions, implementando o padrão **CQRS Write Side** com SQL Server/PostgreSQL para garantir integridade referencial forte (ACID).

### Responsabilidades

- ✅ CRUD completo de **Produtores** (proprietários)
- ✅ CRUD completo de **Fazendas** (propriedades rurais)
- ✅ CRUD completo de **Talhões** (subdivisões com culturas)
- ✅ CRUD completo de **Sensores** (dispositivos IoT)
- ✅ **Publicação de eventos de domínio** (sensor.updated, talhao.created)
- ✅ **Consumo de eventos de status** (status.changed do worker-alerts)

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
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│            Domain Layer                     │
│  Entities | Enums | Interfaces | Events     │
└─────────────────────────────────────────────┘
```

### Modelo de Dados

```
Produtor (CPF, Nome, Contato)
    └── Fazenda (Nome, Área, Localização)
            └── Talhao (Nome, Cultura, Status)
                    └── Sensor (Código, Tipo, Status)
```

## 🚀 Tecnologias

- **.NET 10** - Framework principal
- **C# 12** - Linguagem (Primary Constructors)
- **Entity Framework Core 10** - ORM
- **PostgreSQL** - Banco de dados relacional
- **MassTransit + AWS SQS/SNS** - Mensageria assíncrona
- **MediatR** - CQRS pattern
- **FluentValidation** - Validações
- **AutoMapper** - Mapeamento de objetos
- **Serilog** - Logging estruturado
- **OpenTelemetry** - Observabilidade
- **Scalar** - Documentação de API
- **JWT Bearer** - Autenticação via Keycloak

## 📦 Estrutura do Projeto

```
agrosolutions-service-properties/
├── src/
│   ├── AgroSolutions.Properties.Domain/
│   │   ├── Entities/          # Produtor, Fazenda, Talhao, Sensor
│   │   ├── Enums/             # Status, Tipos
│   │   ├── Interfaces/        # Repositórios, IEventPublisher
│   │   └── Events/            # Eventos de domínio
│   ├── AgroSolutions.Properties.Application/
│   │   ├── Commands/          # CreateProdutorCommand, CreateSensorCommand
│   │   ├── Queries/           # GetAllProdutoresQuery
│   │   ├── Handlers/          # Command/Query Handlers
│   │   ├── Mappings/          # AutoMapper Profiles
│   │   └── Validators/        # FluentValidation
│   ├── AgroSolutions.Properties.Infrastructure/
│   │   ├── Data/              # DbContext
│   │   ├── Repositories/      # Implementações
│   │   └── Messaging/         # EventPublisher, Consumers
│   ├── AgroSolutions.Properties.Api/
│   │   ├── Controllers/       # REST APIs
│   │   ├── Middlewares/       # CorrelationId, ExceptionHandling
│   │   └── Configurations/    # DI, Auth, Database
│   └── AgroSolutions.Properties.Shared/
│       ├── DTOs/              # Data Transfer Objects
│       └── Models/            # ApiResponse
├── tests/
│   └── AgroSolutions.Properties.Tests/
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

# AWS Configuration
AWS__Region=sa-east-1
AWS__SQS__Queues__StatusChangedQueue=agrosolutions-status-changed-queue
AWS__SQS__Queues__ProutorSyncQueue=agrosolutions-produtor-sync-queue
AWS__SNS__Topics__PropertiesEventsTopic=arn:aws:sns:sa-east-1:405114419969:agrosolutions-property-events
```

### Credenciais AWS

Para conectar às filas SQS e tópicos SNS reais, configure suas credenciais AWS:

#### Opção 1: Arquivo .env (recomendado para desenvolvimento)

```bash
# Copiar o exemplo
cp .env.example .env

# Editar com suas credenciais
AWS_ACCESS_KEY_ID=sua-access-key
AWS_SECRET_ACCESS_KEY=sua-secret-key
AWS_SESSION_TOKEN=seu-session-token  # opcional
```

#### Opção 2: Variáveis de ambiente do sistema

```bash
export AWS_ACCESS_KEY_ID=sua-access-key
export AWS_SECRET_ACCESS_KEY=sua-secret-key
export AWS_SESSION_TOKEN=seu-session-token  # opcional
```

⚠️ **Nunca commite o arquivo `.env` com credenciais reais!** O `.env.example` deve conter apenas placeholders.

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

## 📊 Endpoints Principais

### Produtores

- `GET /v1/produtores` - Lista todos os produtores
- `POST /v1/produtores` - Cria um novo produtor

### Sensores

- `GET /v1/sensores/talhao/{talhaoId}` - Lista sensores de um talhão
- `POST /v1/sensores` - Cria um novo sensor (publica evento)

### Health Checks

- `GET /health` - Status geral
- `GET /health/ready` - Readiness probe
- `GET /health/live` - Liveness probe

## 🔄 Fluxo de Eventos

### Publicação (Outbound)

Quando um sensor é criado/atualizado:

```
API → CreateSensorCommand → Handler → Repository.Add() 
    → EventPublisher.Publish(SensorUpdatedEvent) → RabbitMQ (Topic Exchange)
```

### Consumo (Inbound)

Worker de alertas detecta anomalia e publica:

```
RabbitMQ (status.changed) → StatusChangedEventConsumer 
    → TalhaoRepository.Update(Status)
```

## 🧪 Testes

```bash
dotnet restore
dotnet build
dotnet test
```

## 🔒 Segurança

- ✅ **Autenticação JWT** via Keycloak
- ✅ **Container não-root** (usuário 1001)
- ✅ **Secrets via variáveis de ambiente**
- ✅ **Validação de entrada** (FluentValidation)

## 📈 Observabilidade

- **Logs estruturados** com Serilog
- **Métricas** com OpenTelemetry/Prometheus
- **Tracing distribuído** com OpenTelemetry
- **Health checks** integrados
- **Correlation ID** para rastreamento

## 🎯 Princípios SOLID

- **Single Responsibility**: Cada classe tem uma única responsabilidade
- **Open/Closed**: Extensível via interfaces
- **Liskov Substitution**: Implementações respeitam contratos
- **Interface Segregation**: Interfaces específicas e coesas
- **Dependency Inversion**: Dependências via abstrações

## 🚦 Próximos Passos

1. Implementar endpoints de Fazendas e Talhões
2. Adicionar paginação nas queries
3. Implementar cache com Redis
4. Adicionar testes de integração
5. Configurar CI/CD pipeline

## 📄 Licença

Projeto desenvolvido para o Hackathon AgroSolutions - Agricultura 4.0

---

**AgroSolutions** - Transformando a agricultura através da tecnologia 🌱
