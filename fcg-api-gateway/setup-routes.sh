#!/bin/bash

set -e

KONG_ADMIN="http://localhost:8001"
CONSUMER_NAME="fcg-user"

echo "🚀 Configurando Kong API Gateway..."
echo ""

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

log_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

log_error() {
    echo -e "${RED}✗ $1${NC}"
}

log_info() {
    echo -e "${YELLOW}→ $1${NC}"
}

# Teste conexão Kong
log_info "Testando conexão com Kong..."
if curl -s "$KONG_ADMIN/status" > /dev/null 2>&1; then
    log_success "Kong está online"
else
    log_error "Kong não está respondendo em $KONG_ADMIN"
    exit 1
fi

echo ""
echo "═══════════════════════════════════════════════════════════"
echo "CRIANDO SERVIÇOS"
echo "═══════════════════════════════════════════════════════════"

# 1. Users API
log_info "Criando serviço: Users API"
curl -s -X POST "$KONG_ADMIN/services" \
  -d "name=users-api" \
  -d "url=http://users-api:3000" > /dev/null && log_success "Users API criado" || log_error "Falha ao criar Users API"

# 2. Catalog API
log_info "Criando serviço: Catalog API"
curl -s -X POST "$KONG_ADMIN/services" \
  -d "name=catalog-api" \
  -d "url=http://catalog-api:3000" > /dev/null && log_success "Catalog API criado" || log_error "Falha ao criar Catalog API"

# 3. Payments API
log_info "Criando serviço: Payments API"
curl -s -X POST "$KONG_ADMIN/services" \
  -d "name=payments-api" \
  -d "url=http://payments-api:3000" > /dev/null && log_success "Payments API criado" || log_error "Falha ao criar Payments API"

# 4. Orchestration
log_info "Criando serviço: Orchestration"
curl -s -X POST "$KONG_ADMIN/services" \
  -d "name=orchestration" \
  -d "url=http://orchestration:3000" > /dev/null && log_success "Orchestration criado" || log_error "Falha ao criar Orchestration"

echo ""
echo "═══════════════════════════════════════════════════════════"
echo "CRIANDO ROTAS"
echo "═══════════════════════════════════════════════════════════"

# Users API Routes
log_info "Criando rotas: Users API"
curl -s -X POST "$KONG_ADMIN/services/users-api/routes" \
  -d "paths[]=/api/users" > /dev/null && log_success "/api/users → Users API" || log_error "Falha"

curl -s -X POST "$KONG_ADMIN/services/users-api/routes" \
  -d "paths[]=/api/auth" > /dev/null && log_success "/api/auth → Users API" || log_error "Falha"

# Catalog API Routes
log_info "Criando rotas: Catalog API"
curl -s -X POST "$KONG_ADMIN/services/catalog-api/routes" \
  -d "paths[]=/api/catalog" > /dev/null && log_success "/api/catalog → Catalog API" || log_error "Falha"

curl -s -X POST "$KONG_ADMIN/services/catalog-api/routes" \
  -d "paths[]=/api/games" > /dev/null && log_success "/api/games → Catalog API" || log_error "Falha"

# Payments API Routes
log_info "Criando rotas: Payments API"
curl -s -X POST "$KONG_ADMIN/services/payments-api/routes" \
  -d "paths[]=/api/payments" > /dev/null && log_success "/api/payments → Payments API" || log_error "Falha"

# Orchestration Routes
log_info "Criando rotas: Orchestration"
curl -s -X POST "$KONG_ADMIN/services/orchestration/routes" \
  -d "paths[]=/api/orders" > /dev/null && log_success "/api/orders → Orchestration" || log_error "Falha"

echo ""
echo "═══════════════════════════════════════════════════════════"
echo "ADICIONANDO PLUGINS"
echo "═══════════════════════════════════════════════════════════"

# JWT Plugin para Users API
log_info "Adicionando JWT: Users API"
curl -s -X POST "$KONG_ADMIN/services/users-api/plugins" \
  -d "name=jwt" > /dev/null && log_success "JWT habilitado em Users API" || log_error "Falha"

# JWT Plugin para Payments API
log_info "Adicionando JWT: Payments API"
curl -s -X POST "$KONG_ADMIN/services/payments-api/plugins" \
  -d "name=jwt" > /dev/null && log_success "JWT habilitado em Payments API" || log_error "Falha"

# Rate Limiting - Users API
log_info "Adicionando Rate Limiting: Users API"
curl -s -X POST "$KONG_ADMIN/services/users-api/plugins" \
  -d "name=rate-limiting" \
  -d "config.minute=100" \
  -d "config.policy=local" > /dev/null && log_success "Rate limit: 100 req/min em Users API" || log_error "Falha"

# CORS Plugin - Todos os serviços
log_info "Adicionando CORS: Global"
curl -s -X POST "$KONG_ADMIN/plugins" \
  -d "name=cors" \
  -d "config.origins=*" \
  -d "config.methods=GET,POST,PUT,DELETE,OPTIONS,PATCH" \
  -d "config.headers=Content-Type,Authorization" > /dev/null && log_success "CORS habilitado globalmente" || log_error "Falha"

# Logging Plugin
log_info "Adicionando Logging: Global"
curl -s -X POST "$KONG_ADMIN/plugins" \
  -d "name=tcp-log" \
  -d "config.host=localhost" \
  -d "config.port=9999" > /dev/null && log_success "TCP logging configurado" || log_error "Falha"

echo ""
echo "═══════════════════════════════════════════════════════════"
echo "CRIANDO CONSUMER"
echo "═══════════════════════════════════════════════════════════"

# Criar consumer
log_info "Criando consumer: $CONSUMER_NAME"
curl -s -X POST "$KONG_ADMIN/consumers" \
  -d "username=$CONSUMER_NAME" > /dev/null && log_success "Consumer '$CONSUMER_NAME' criado" || log_error "Falha"

# Criar JWT credential
log_info "Criando JWT credential"
JWT_RESPONSE=$(curl -s -X POST "$KONG_ADMIN/consumers/$CONSUMER_NAME/jwt" \
  -d "algorithm=HS256")

JWT_KEY=$(echo $JWT_RESPONSE | grep -o '"key":"[^"]*' | cut -d'"' -f4)
JWT_SECRET=$(echo $JWT_RESPONSE | grep -o '"secret":"[^"]*' | cut -d'"' -f4)

if [ -n "$JWT_KEY" ] && [ -n "$JWT_SECRET" ]; then
    log_success "JWT credential criado"
    echo ""
    echo "╔═════════════════════════════════════════════════════════╗"
    echo "║           JWT CREDENTIALS PARA TESTES                   ║"
    echo "╠═════════════════════════════════════════════════════════╣"
    echo "║ Key:    $JWT_KEY"
    echo "║ Secret: $JWT_SECRET"
    echo "╚═════════════════════════════════════════════════════════╝"
    echo ""
else
    log_error "Falha ao criar JWT credential"
fi

echo ""
echo "═══════════════════════════════════════════════════════════"
echo "VERIFICAÇÃO FINAL"
echo "═══════════════════════════════════════════════════════════"

# Listar serviços
SERVICES_COUNT=$(curl -s "$KONG_ADMIN/services" | grep -o '"name"' | wc -l)
log_success "Total de serviços: $SERVICES_COUNT"

# Listar rotas
ROUTES_COUNT=$(curl -s "$KONG_ADMIN/routes" | grep -o '"id"' | wc -l)
log_success "Total de rotas: $ROUTES_COUNT"

# Listar plugins
PLUGINS_COUNT=$(curl -s "$KONG_ADMIN/plugins" | grep -o '"name"' | wc -l)
log_success "Total de plugins: $PLUGINS_COUNT"

echo ""
echo "═══════════════════════════════════════════════════════════"
echo "✨ KONG CONFIGURADO COM SUCESSO!"
echo "═══════════════════════════════════════════════════════════"
echo ""
echo "Acessos:"
echo "  Kong Admin API: http://localhost:8001"
echo "  Konga UI:       http://localhost:1337"
echo "  Gateway:        http://localhost:8000"
echo ""
echo "Testes rápidos:"
echo ""
echo "1. Login (obter token):"
echo "   curl -X POST http://localhost:8000/api/auth/login \\"
echo "     -H 'Content-Type: application/json' \\"
echo "     -d '{\"username\":\"$CONSUMER_NAME\",\"password\":\"test\"}'"
echo ""
echo "2. Requisição protegida:"
echo "   curl http://localhost:8000/api/users \\"
echo "     -H 'Authorization: Bearer <JWT_TOKEN>'"
echo ""
echo "3. Requisição sem proteção:"
echo "   curl http://localhost:8000/api/catalog/games"
echo ""
echo "═══════════════════════════════════════════════════════════"

log_success "Setup concluído!"
