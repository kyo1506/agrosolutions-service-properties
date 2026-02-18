# Kubernetes Manifests - Properties Service

Arquivos de configuração Kubernetes para deploy do AgroSolutions Properties Service no AWS EKS.

## 📁 Estrutura

```
k8s/
├── CI_CD_SUMMARY.md          # Documentação completa do pipeline
├── README.md                 # Este arquivo
└── production/               # Manifestos de produção
    ├── namespace.yaml        # Namespace agrosolutions-properties
    ├── configmaps.yaml       # Variáveis de ambiente da API
    ├── storage-class.yaml    # EBS gp3 StorageClass
    ├── volumes.yaml          # PVC para PostgreSQL (20Gi)
    ├── databases.yaml        # PostgreSQL 17 + Exporter
    ├── services.yaml         # ClusterIP services
    ├── deployment.yaml       # Deployment da API (2-10 réplicas)
    ├── hpa.yaml              # Horizontal Pod Autoscaler
    ├── ingress-aws.yaml      # AWS ALB Ingress
    ├── observability.yaml    # ServiceMonitor + PrometheusRules
    └── resource-configs.yaml # NetworkPolicy, PDB, Quotas
```

## 🚀 Deploy Automático (CI/CD)

O deploy é automático via GitHub Actions quando há push na branch `main`.

Ver: [CI_CD_SUMMARY.md](./CI_CD_SUMMARY.md)

## 🔧 Deploy Manual

### Pré-requisitos

1. **AWS CLI** configurado com credenciais
2. **kubectl** instalado
3. **Cluster EKS** provisionado
4. **ECR Repository** criado

### Configurar kubeconfig

```bash
aws eks update-kubeconfig --name agrosolutions-eks-cluster --region sa-east-1
```

### Criar secrets

```bash
# Database secrets
kubectl create secret generic database-secrets \
  --from-literal=postgres-user=postgres \
  --from-literal=postgres-password=YOUR_POSTGRES_PASSWORD \
  --from-literal=connection-string="Host=properties-db-service;Port=5432;Database=agrosolutions_properties;Username=postgres;Password=YOUR_POSTGRES_PASSWORD" \
  --from-literal=postgres-exporter-dsn="postgresql://postgres:YOUR_POSTGRES_PASSWORD@properties-db-service:5432/agrosolutions_properties?sslmode=disable" \
  -n agrosolutions-properties

# AWS credentials
kubectl create secret generic aws-credentials \
  --from-literal=access-key-id=YOUR_AWS_ACCESS_KEY_ID \
  --from-literal=secret-access-key=YOUR_AWS_SECRET_ACCESS_KEY \
  -n agrosolutions-properties
```

### Aplicar manifestos

```bash
# Na raiz do projeto
cd k8s/production

# Aplicar na ordem
kubectl apply -f namespace.yaml
kubectl apply -f configmaps.yaml
kubectl apply -f storage-class.yaml
kubectl apply -f volumes.yaml
kubectl apply -f databases.yaml
kubectl apply -f services.yaml

# Aguardar DB ficar pronto
kubectl wait --for=condition=ready pod -l app=properties-db -n agrosolutions-properties --timeout=300s

# Deploy da API e configurações
kubectl apply -f deployment.yaml
kubectl apply -f hpa.yaml
kubectl apply -f ingress-aws.yaml
kubectl apply -f observability.yaml
kubectl apply -f resource-configs.yaml
```

### Verificar deploy

```bash
# Status dos pods
kubectl get pods -n agrosolutions-properties

# Status dos deployments
kubectl get deployments -n agrosolutions-properties

# Status do HPA
kubectl get hpa -n agrosolutions-properties

# Logs da API
kubectl logs -f deployment/properties-api -n agrosolutions-properties

# Health check interno
kubectl port-forward -n agrosolutions-properties svc/properties-api-service 8080:80
curl http://localhost:8080/health
```

## 🔄 Atualizações

### Atualizar imagem Docker

```bash
# Editar deployment.yaml ou usar kubectl set image
kubectl set image deployment/properties-api \
  properties-api=316295889438.dkr.ecr.sa-east-1.amazonaws.com/agrosolutions-properties-api:NEW_TAG \
  -n agrosolutions-properties

# Acompanhar rollout
kubectl rollout status deployment/properties-api -n agrosolutions-properties
```

### Atualizar ConfigMap

```bash
# Editar configmaps.yaml e aplicar
kubectl apply -f configmaps.yaml

# Restart para carregar novas configs
kubectl rollout restart deployment/properties-api -n agrosolutions-properties
```

### Escalar manualmente

```bash
# Escalar para 5 réplicas (sobrescreve HPA temporariamente)
kubectl scale deployment/properties-api -n agrosolutions-properties --replicas=5
```

## 🗑️ Cleanup

### Remover tudo

```bash
# Deletar namespace (remove todos os recursos)
kubectl delete namespace agrosolutions-properties
```

### Remover apenas a aplicação (manter banco)

```bash
kubectl delete -f deployment.yaml
kubectl delete -f hpa.yaml
kubectl delete -f ingress-aws.yaml
kubectl delete -f observability.yaml
kubectl delete -f resource-configs.yaml
```

## 📊 Monitoramento

### Métricas do Prometheus

Os ServiceMonitors são automaticamente descobertos pelo Prometheus Operator:

```bash
# Verificar ServiceMonitors
kubectl get servicemonitor -n agrosolutions-properties

# Verificar PrometheusRules
kubectl get prometheusrule -n agrosolutions-properties
```

### Acessar métricas diretamente

```bash
# Port-forward para a API
kubectl port-forward -n agrosolutions-properties svc/properties-api-service 8080:80

# Acessar /metrics
curl http://localhost:8080/metrics
```

### Logs centralizados

Se estiver usando EFK/ELK stack:

```bash
# Ver logs de todos os pods
kubectl logs -l app=properties-api -n agrosolutions-properties --tail=100

# Seguir logs em tempo real
kubectl logs -f deployment/properties-api -n agrosolutions-properties
```

## 🔐 Segurança

### NetworkPolicy

O `resource-configs.yaml` inclui NetworkPolicy que:
- ✅ Permite tráfego do API Gateway
- ✅ Permite health checks do Load Balancer
- ✅ Permite Prometheus scraping
- ✅ Permite acesso ao PostgreSQL interno
- ✅ Permite acesso ao Keycloak (validação JWT)
- ✅ Permite tráfego externo (AWS SQS/SNS)
- ❌ Bloqueia todo o resto

### Secrets

⚠️ **NUNCA** commite secrets nos manifestos YAML. Use apenas:
- Kubernetes Secrets (criados via kubectl)
- AWS Secrets Manager
- HashiCorp Vault

## 🏗️ Arquitetura

```
┌────────────────────────────────────────────────────┐
│             Internet (443/80)                      │
└─────────────────┬──────────────────────────────────┘
                  │
          ┌───────▼───────┐
          │  AWS ALB      │ (ingress-aws.yaml)
          └───────┬───────┘
                  │
     ┌────────────▼────────────┐
     │  properties-api-service │ (ClusterIP :80)
     └────────────┬────────────┘
                  │
     ┌────────────▼────────────┐
     │  properties-api Pod(s)  │ (2-10 réplicas via HPA)
     │  - .NET 10 Alpine       │
     │  - Port 8080            │
     │  - /health endpoint     │
     └────────────┬────────────┘
                  │
                  │ (PostgreSQL connection)
                  │
     ┌────────────▼────────────┐
     │  properties-db-service  │ (ClusterIP :5432)
     └────────────┬────────────┘
                  │
     ┌────────────▼────────────┐
     │  properties-db Pod      │
     │  - PostgreSQL 17        │
     │  - 20Gi EBS gp3 volume  │
     │  - Postgres Exporter    │
     └─────────────────────────┘
```

## 📚 Documentação Adicional

- [CI/CD Pipeline](./CI_CD_SUMMARY.md)
- [GitHub Secrets Setup](../.github/SECRETS_SETUP.md)
- [AWS EKS Best Practices](https://aws.github.io/aws-eks-best-practices/)
- [Kubernetes Documentation](https://kubernetes.io/docs/)

## 🆘 Troubleshooting

Ver seção **Manutenção e Troubleshooting** em [CI_CD_SUMMARY.md](./CI_CD_SUMMARY.md#-manutenção-e-troubleshooting).
