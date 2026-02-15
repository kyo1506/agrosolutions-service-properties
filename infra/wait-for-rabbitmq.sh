#!/bin/sh

# Script robusto para aguardar RabbitMQ estar completamente pronto
# Aguarda até que o RabbitMQ responda ao health check E aceite conexões

set -e

RABBITMQ_HOST="${RABBITMQ_HOST:-rabbitmq}"
RABBITMQ_PORT="${RABBITMQ_PORT:-5672}"
RABBITMQ_USER="${RABBITMQ_USER:-guest}"
RABBITMQ_PASS="${RABBITMQ_PASS:-guest}"
MAX_ATTEMPTS="${MAX_ATTEMPTS:-30}"
SLEEP_INTERVAL="${SLEEP_INTERVAL:-2}"

echo "🐰 Aguardando RabbitMQ estar pronto em ${RABBITMQ_HOST}:${RABBITMQ_PORT}..."

# Função para verificar se RabbitMQ está respondendo
check_rabbitmq() {
    # Verifica se a porta AMQP está acessível
    if ! nc -z "$RABBITMQ_HOST" "$RABBITMQ_PORT" 2>/dev/null; then
        return 1
    fi
    
    return 0
}

attempt=1
while [ $attempt -le $MAX_ATTEMPTS ]; do
    echo "⏳ Tentativa ${attempt}/${MAX_ATTEMPTS}..."
    
    if check_rabbitmq; then
        echo "✅ RabbitMQ está pronto!"
        
        # Aguarda mais 3 segundos para garantir estabilidade completa
        echo "⏱️  Aguardando estabilização (3s)..."
        sleep 3
        
        echo "🚀 Iniciando aplicação..."
        exit 0
    fi
    
    echo "⚠️  RabbitMQ ainda não está pronto. Aguardando ${SLEEP_INTERVAL}s..."
    sleep $SLEEP_INTERVAL
    attempt=$((attempt + 1))
done

echo "❌ ERRO: RabbitMQ não ficou pronto após ${MAX_ATTEMPTS} tentativas"
echo "   Verifique os logs: docker logs agrosolutions-rabbitmq"
exit 1
