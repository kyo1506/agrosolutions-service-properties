# 🚀 CI/CD Pipeline - AgroSolutions Properties Service

## 📋 Visão Geral

O pipeline de CI/CD é executado automaticamente via **GitHub Actions** definido em `.github/workflows/deploy.yml`.

---

## 🔄 Trigger Automático

O workflow é disparado quando:
- ✅ Push na branch `main` ou `develop`
- ✅ Pull Request para `main`
- ✅ Execução manual via `workflow_dispatch`

---

## 📦 Jobs do Pipeline

### 1️⃣ Build and Test

**Execução**: Sempre (em todos os eventos)

**Passos**:
1. Checkout do código
2. Setup .NET SDK 10.0
3. Restore de dependências
4. Build da solution
5. Execução de testes unitários

**Resultado**: Valida que o código compila e testes passam.

---

### 2️⃣ Deploy to EKS

**Execução**: Apenas quando push em `main`

**Dependência**: `build-and-test` deve passar

**Passos**:
1. **Build Docker Image**
   - Build multi-stage com .NET 10 Alpine
   - Tag com `$(github.sha)` e `latest`
   
2. **Push para ECR**
   - Login no Amazon ECR
   - Push da imagem para `316295889438.dkr.ecr.sa-east-1.amazonaws.com/agrosolutions-properties-api`

3. **Deploy Kubernetes**
   ```bash
   kubectl apply -f k8s/production/namespace.yaml
   kubectl apply -f k8s/production/configmaps.yaml
   kubectl apply -f k8s/production/storage-class.yaml
   kubectl apply -f k8s/production/volumes.yaml
   kubectl apply -f k8s/production/databases.yaml
   kubectl apply -f k8s/production/services.yaml
   kubectl apply -f k8s/production/deployment.yaml
   kubectl apply -f k8s/production/hpa.yaml
   kubectl apply -f k8s/production/ingress-aws.yaml
   kubectl apply -f k8s/production/observability.yaml
   kubectl apply -f k8s/production/resource-configs.yaml
   ```

4. **Criação de Secrets Kubernetes**
   - `database-secrets` (PostgreSQL credentials + connection string)
   - `aws-credentials` (AWS Access Key ID/Secret)

5. **Verificação do Deploy**
   - Wait for rollout: `properties-api`
   - Health checks
   - Status dos pods, services, ingress

6. **Rollback Automático**
   - Se houver falha, executa `kubectl rollout undo`

---

## 🔒 Secrets Necessários

Configure em: `Settings > Secrets and variables > Actions`

| Secret Name | Descrição | Valor Exemplo |
|-------------|-----------|---------------|
| `AWS_ACCESS_KEY_ID` | Chave de acesso AWS | `AKIAIOSFODNN7EXAMPLE` |
| `AWS_SECRET_ACCESS_KEY` | Secret key AWS | `wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY` |
| `POSTGRES_PASSWORD` | Senha do PostgreSQL | `strong-password-here` |

---

## 📊 Fluxo Completo

```
┌─────────────────────────────────────────────────────────────┐
│  1. Developer: git push origin main                         │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│  2. GitHub Actions: Inicia Workflow                         │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│  3. Job: build-and-test                                     │
│     - dotnet restore, build, test                           │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│  4. Job: deploy-to-eks                                      │
│     - Build Docker                                          │
│     - Push ECR                                              │
│     - kubectl apply                                         │
│     - Health check                                          │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│  5. Ambiente de Produção Atualizado                         │
│     - Properties API rodando no EKS                         │
│     - PostgreSQL com persistência                           │
│     - HPA configurado (2-10 réplicas)                       │
│     - Métricas sendo coletadas                              │
└─────────────────────────────────────────────────────────────┘
```

---

## 🧪 Testes Locais do Workflow

### Pré-requisitos
```bash
# Instalar act (GitHub Actions local runner)
brew install act  # macOS
# ou
curl https://raw.githubusercontent.com/nektos/act/master/install.sh | sudo bash  # Linux
```

### Executar workflow localmente
```bash
# Test build-and-test job
act -j build-and-test

# Test com secrets (crie arquivo .secrets)
act -j build-and-test --secret-file .secrets

# Dry-run do deploy (sem executar)
act -j deploy-to-eks --dry-run
```

### Arquivo `.secrets` exemplo:
```env
AWS_ACCESS_KEY_ID=your-key-id
AWS_SECRET_ACCESS_KEY=your-secret-key
POSTGRES_PASSWORD=your-db-password
```

---

## 🌐 Infraestrutura Kubernetes

### Recursos Criados

| Recurso | Nome | Descrição |
|---------|------|-----------|
| **Namespace** | `agrosolutions-properties` | Isola recursos do serviço |
| **Deployment** | `properties-api` | API principal (2-10 réplicas) |
| **Deployment** | `properties-db` | PostgreSQL 17 Alpine |
| **Service** | `properties-api-service` | ClusterIP para API (porta 80) |
| **Service** | `properties-db-service` | ClusterIP para DB (porta 5432) |
| **PVC** | `properties-db-pvc` | 20Gi EBS gp3 para PostgreSQL |
| **HPA** | `properties-api-hpa` | Auto-scaling (CPU 70%, Mem 80%) |
| **Ingress** | `properties-api-ingress` | ALB para acesso externo |
| **ConfigMap** | `properties-api-config` | Variáveis de ambiente |
| **Secret** | `database-secrets` | Credenciais PostgreSQL |
| **Secret** | `aws-credentials` | AWS Access Keys |

### Observabilidade

- **ServiceMonitor**: Prometheus scraping da API e PostgreSQL
- **PrometheusRule**: 8 alertas críticos configurados
  - Alta latência (P95 > 1000ms)
  - Taxa de erro alta (>5%)
  - Pods não ready
  - Problemas de conexão com DB
  - Outbox messages stuck
  - HPA maxed out
  - Database storage filling
  - etc.

### Resource Management

- **PodDisruptionBudget**: Mínimo de 1 pod sempre disponível
- **ResourceQuota**: Limites de namespace (10 CPU, 20Gi RAM)
- **LimitRange**: Limites padrão para pods
- **NetworkPolicy**: Restringe tráfego entre pods

---

## 🔧 Manutenção e Troubleshooting

### Ver logs do deployment
```bash
kubectl logs -f deployment/properties-api -n agrosolutions-properties
```

### Verificar status dos pods
```bash
kubectl get pods -n agrosolutions-properties -o wide
```

### Verificar eventos
```bash
kubectl get events -n agrosolutions-properties --sort-by='.lastTimestamp'
```

### Forçar restart
```bash
kubectl rollout restart deployment/properties-api -n agrosolutions-properties
```

### Rollback manual
```bash
# Ver histórico de deployments
kubectl rollout history deployment/properties-api -n agrosolutions-properties

# Rollback para versão anterior
kubectl rollout undo deployment/properties-api -n agrosolutions-properties

# Rollback para versão específica
kubectl rollout undo deployment/properties-api -n agrosolutions-properties --to-revision=3
```

### Escalar manualmente
```bash
# Escalar para 5 réplicas
kubectl scale deployment/properties-api -n agrosolutions-properties --replicas=5

# Ver status do HPA
kubectl get hpa -n agrosolutions-properties
```

### Debug de secrets
```bash
# Verificar se secrets existem
kubectl get secrets -n agrosolutions-properties

# Ver conteúdo do secret (base64 encoded)
kubectl get secret database-secrets -n agrosolutions-properties -o yaml

# Decodificar valor
kubectl get secret database-secrets -n agrosolutions-properties -o jsonpath='{.data.postgres-password}' | base64 -d
```

### Acessar PostgreSQL
```bash
# Port-forward do PostgreSQL
kubectl port-forward -n agrosolutions-properties svc/properties-db-service 5432:5432

# Conectar via psql
psql -h localhost -U postgres -d agrosolutions_properties
```

---

## 🚨 Alertas Configurados

Os seguintes alertas são enviados automaticamente via Prometheus Alertmanager:

1. **PropertiesAPIHighLatency** - P95 > 1000ms por 5min
2. **PropertiesAPIHighErrorRate** - Taxa erro > 5% por 5min
3. **PropertiesAPIPodNotReady** - Pod não ready por 5min
4. **PropertiesDatabaseConnectionIssues** - Erros de conexão com DB
5. **PropertiesOutboxMessagesStuck** - >1000 mensagens pendentes por 10min
6. **PropertiesAPIHPAMaxedOut** - HPA no máximo por 15min
7. **PropertiesDatabaseStorageFilling** - Storage > 80%

---

## 📈 Métricas Monitoradas

### API Metrics
- `http_server_requests_total` - Total de requisições
- `http_server_duration_ms` - Latência de requisições
- `properties_outbox_pending_messages` - Mensagens pendentes no outbox
- `properties_db_connection_errors_total` - Erros de conexão com DB

### PostgreSQL Metrics (via postgres-exporter)
- `pg_up` - Database disponível
- `pg_stat_database_*` - Estatísticas de queries
- `pg_stat_activity_*` - Conexões ativas
- `pg_locks_*` - Locks no banco

### Kubernetes Metrics
- `kube_pod_status_ready` - Status dos pods
- `kube_horizontalpodautoscaler_*` - Métricas do HPA
- `kubelet_volume_stats_*` - Uso de storage

---

## 🎯 Próximos Passos

- [ ] Configurar Certificate Manager (ACM) para HTTPS
- [ ] Configurar DNS no Route53
- [ ] Implementar Blue/Green deployment
- [ ] Adicionar testes de integração no pipeline
- [ ] Configurar backup automático do PostgreSQL
- [ ] Implementar canary deployments
- [ ] Adicionar smoke tests pós-deploy

---

## 📚 Referências

- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [AWS EKS Best Practices](https://aws.github.io/aws-eks-best-practices/)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Prometheus Operator](https://prometheus-operator.dev/)
