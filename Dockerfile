ARG DOTNET_VERSION=10.0

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}-alpine AS build

ARG BUILD_DATE
ARG VERSION=1.0.0
ARG REVISION=dev

WORKDIR /src

# Copiar todos os arquivos do projeto
COPY . .

# Restore e Publish (sem build separado para evitar problemas com glob patterns)
RUN dotnet restore src/AgroSolutions.Properties.Api/AgroSolutions.Properties.Api.csproj
RUN dotnet publish src/AgroSolutions.Properties.Api/AgroSolutions.Properties.Api.csproj \
    -c Release \
    -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-alpine AS final

# Labels para metadata
LABEL maintainer="AgroSolutions Team" \
      org.opencontainers.image.title="AgroSolutions Properties Service" \
      org.opencontainers.image.description="Core domain service for managing producers, farms, plots and sensors" \
      org.opencontainers.image.version="${VERSION}" \
      org.opencontainers.image.created="${BUILD_DATE}" \
      org.opencontainers.image.revision="${REVISION}"

# Instalar dependências
RUN apk add --no-cache \
    icu-libs \
    ca-certificates \
    tzdata \
    krb5-libs \
    && update-ca-certificates

# Criar usuário não-root
RUN addgroup -g 1001 -S appgroup && \
    adduser -u 1001 -S appuser -G appgroup

WORKDIR /app

# Copiar arquivos publicados
COPY --from=build --chown=appuser:appgroup /app/publish .

# Configurar variáveis de ambiente
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_EnableDiagnostics=0 \
    ASPNETCORE_URLS=http://+:8080 \
    TZ=America/Sao_Paulo

# Trocar para usuário não-root
USER appuser

# Expor porta
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "AgroSolutions.Properties.Api.dll"]
