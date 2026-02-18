# GitHub Secrets Setup - Properties Service

Este documento detalha como configurar os secrets necessários no GitHub para o pipeline de CI/CD.

## 📍 Localização

`Settings > Secrets and variables > Actions > Repository secrets`

---

## 🔒 Secrets Obrigatórios

### 1. AWS_ACCESS_KEY_ID

**Descrição**: Chave de acesso da conta AWS para deploy no EKS e push no ECR.

**Como obter**:
```bash
aws configure get aws_access_key_id
```

**Permissões necessárias**:
- ECR: `ecr:GetAuthorizationToken`, `ecr:BatchCheckLayerAvailability`, `ecr:PutImage`, `ecr:InitiateLayerUpload`, `ecr:UploadLayerPart`, `ecr:CompleteLayerUpload`
- EKS: `eks:DescribeCluster`, `eks:ListClusters`
- IAM: `iam:GetRole` (para verificar roles)

**Adicionar ao GitHub**:
1. Acesse: `https://github.com/YOUR_USERNAME/agrosolutions-service-properties/settings/secrets/actions`
2. Clique em "New repository secret"
3. Name: `AWS_ACCESS_KEY_ID`
4. Secret: Cole o valor da access key
5. Clique em "Add secret"

---

### 2. AWS_SECRET_ACCESS_KEY

**Descrição**: Secret key correspondente à AWS Access Key ID.

**Como obter**:
```bash
aws configure get aws_secret_access_key
```

⚠️ **IMPORTANTE**: Nunca compartilhe ou commite esta chave! Ela tem acesso total aos recursos AWS.

**Adicionar ao GitHub**:
1. Name: `AWS_SECRET_ACCESS_KEY`
2. Secret: Cole o valor da secret access key
3. Clique em "Add secret"

---

### 3. POSTGRES_PASSWORD

**Descrição**: Senha do banco de dados PostgreSQL usado pelo serviço.

**Recomendações**:
- Mínimo 16 caracteres
- Mistura de letras maiúsculas, minúsculas, números e símbolos
- Não usar palavras de dicionário

**Gerar senha segura**:
```bash
# No Linux/macOS
openssl rand -base64 32

# Ou use um gerenciador de senhas (LastPass, 1Password, Bitwarden)
```

**Adicionar ao GitHub**:
1. Name: `POSTGRES_PASSWORD`
2. Secret: Cole a senha gerada
3. Clique em "Add secret"

---

## ✅ Verificação

Após adicionar os secrets, você deve ver algo como:

```
✓ AWS_ACCESS_KEY_ID           Updated 1 minute ago
✓ AWS_SECRET_ACCESS_KEY       Updated 1 minute ago
✓ POSTGRES_PASSWORD           Updated 1 minute ago
```

---

## 🧪 Testar Configuração

### 1. Verificar AWS Credentials localmente

```bash
# Testar acesso ao ECR
aws ecr describe-repositories --region sa-east-1

# Testar acesso ao EKS
aws eks describe-cluster --name agrosolutions-eks-cluster --region sa-east-1
```

### 2. Executar workflow manualmente

1. Vá para: `Actions` tab no GitHub
2. Selecione: `AgroSolutions Properties - CI/CD Pipeline`
3. Clique em: `Run workflow`
4. Selecione a branch: `main`
5. Clique em: `Run workflow`

Se os secrets estiverem corretos, o workflow deve:
- ✅ Fazer build da aplicação
- ✅ Executar testes
- ✅ Fazer push da imagem Docker para ECR
- ✅ Aplicar manifestos no EKS cluster

---

## 🔄 Rotação de Secrets

### AWS Credentials

Recomenda-se rotacionar a cada **90 dias**.

**Passos**:
1. Criar nova Access Key no IAM Console
2. Atualizar secrets no GitHub
3. Testar deploy com novas credenciais
4. Desativar/deletar Access Key antiga

```bash
# Criar nova access key (AWS CLI)
aws iam create-access-key --user-name github-actions-user

# Deletar access key antiga
aws iam delete-access-key --access-key-id OLD_KEY_ID --user-name github-actions-user
```

### Database Password

Se precisar rotacionar, siga este procedimento:

1. **Atualizar secret no GitHub** primeiro
2. **Deploy da aplicação** (ela não vai conectar ainda, mas está OK)
3. **Atualizar senha no PostgreSQL**:
   ```bash
   kubectl exec -it -n agrosolutions-properties deployment/properties-db -- psql -U postgres
   ALTER USER postgres WITH PASSWORD 'new-password-here';
   \q
   ```
4. **Restart da API**:
   ```bash
   kubectl rollout restart deployment/properties-api -n agrosolutions-properties
   ```

---

## 🚨 Troubleshooting

### Erro: "Error: Image pull failed"

**Causa**: AWS credentials inválidas ou sem permissão para ECR.

**Solução**:
```bash
# Verificar se consegue fazer login no ECR
aws ecr get-login-password --region sa-east-1 | docker login --username AWS --password-stdin 316295889438.dkr.ecr.sa-east-1.amazonaws.com
```

### Erro: "error: You must be logged in to the server (Unauthorized)"

**Causa**: Credenciais AWS sem permissão para acessar EKS cluster.

**Solução**:
1. Verificar IAM user/role tem política `AmazonEKSClusterPolicy`
2. Adicionar user ao ConfigMap `aws-auth`:
   ```bash
   kubectl edit -n kube-system configmap/aws-auth
   ```

### Erro: "Connection to database failed"

**Causa**: Senha do PostgreSQL incorreta no secret.

**Solução**:
1. Verificar secret no Kubernetes:
   ```bash
   kubectl get secret database-secrets -n agrosolutions-properties -o jsonpath='{.data.postgres-password}' | base64 -d
   ```
2. Se estiver diferente da senha do GitHub Secret, re-deploy:
   ```bash
   kubectl delete secret database-secrets -n agrosolutions-properties
   # Triggere novo deploy do GitHub Actions
   ```

---

## 📋 Checklist de Setup

- [ ] AWS_ACCESS_KEY_ID configurado
- [ ] AWS_SECRET_ACCESS_KEY configurado
- [ ] POSTGRES_PASSWORD configurado (senha forte)
- [ ] Testado acesso AWS via CLI local
- [ ] Executado workflow manualmente e passou
- [ ] Verificado pods rodando no EKS: `kubectl get pods -n agrosolutions-properties`
- [ ] Verificado health endpoint respondendo
- [ ] Documentado senha do PostgreSQL no gerenciador de senhas do time

---

## 📞 Suporte

Se encontrar problemas:

1. **Verifique o log do workflow** no GitHub Actions
2. **Verifique os eventos do Kubernetes**:
   ```bash
   kubectl get events -n agrosolutions-properties --sort-by='.lastTimestamp'
   ```
3. **Verifique os logs dos pods**:
   ```bash
   kubectl logs -f deployment/properties-api -n agrosolutions-properties
   ```
4. **Consulte a documentação**: [k8s/CI_CD_SUMMARY.md](./CI_CD_SUMMARY.md)
