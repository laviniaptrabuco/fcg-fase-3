# Guia de Testes - Tech Challenge Fase 3

## 🚀 Pré-requisitos

- Docker e Docker Compose instalados
- `curl` ou Postman para testes de API
- Node/Bash para scripts

## ✅ Teste 1: Inicialização da Infraestrutura

```bash
# Verificar se todos os containers estão rodando
docker-compose ps

# Esperado: Todos os containers com status "Up"
# - kong-database (PostgreSQL)
# - kong (API Gateway)
# - konga (UI)
# - prometheus
# - grafana
# - mongodb
# - redis
# - rabbitmq
```

**Validação:**
```bash
# Health check Kong
curl http://localhost:8001/status
# Esperado: {"database":true,"server":{"connections_accepted":X,"connections_active":Y,...}}

# Health check Prometheus
curl http://localhost:9090/-/healthy
# Esperado: HTTP 200

# Health check Grafana
curl http://localhost:3000/api/health
# Esperado: {"commit":"...","database":"ok",...}
```

## ✅ Teste 2: Kong API Gateway

### 2.1 Criar Serviço

```bash
curl -X POST http://localhost:8001/services \
  -d "name=test-api" \
  -d "url=http://httpbin.org"

# Esperado: 201 Created
# Response: {"id":"...", "name":"test-api", "url":"http://httpbin.org",...}
```

### 2.2 Criar Rota

```bash
curl -X POST http://localhost:8001/services/test-api/routes \
  -d "paths[]=/api/test"

# Esperado: 201 Created
# Response: {"id":"...", "service":{"id":"..."},...}
```

### 2.3 Testar Rota

```bash
curl http://localhost:8000/api/test/get

# Esperado: Resposta do httpbin
# {"url": "http://httpbin.org/get", ...}
```

### 2.4 Adicionar Plugin JWT

```bash
curl -X POST http://localhost:8001/services/test-api/plugins \
  -d "name=jwt"

# Esperado: 201 Created
```

### 2.5 Testar JWT Validation

```bash
# Sem token
curl http://localhost:8000/api/test/get
# Esperado: 401 Unauthorized

# Com token inválido
curl http://localhost:8000/api/test/get \
  -H "Authorization: Bearer invalid_token"
# Esperado: 401 Unauthorized
```

## ✅ Teste 3: MongoDB

### 3.1 Conectar ao MongoDB

```bash
docker-compose exec mongodb mongo -u admin -p admin

# No shell do MongoDB
use fcg_db
show collections
```

### 3.2 Verificar Coleções

```javascript
// No shell MongoDB
db.users.find()
db.products.find()
db.events.find()

// Esperado: Collections com dados iniciais
```

### 3.3 Inserir Novo Documento

```javascript
db.users.insertOne({
  email: "test@example.com",
  name: "Test User",
  createdAt: new Date()
})

// Esperado: { "acknowledged": true, "insertedId": ObjectId(...) }
```

### 3.4 Consultar com Filtro

```javascript
db.users.find({ email: "test@example.com" })

// Esperado: Documento inserido
```

## ✅ Teste 4: Redis

### 4.1 Conectar ao Redis

```bash
docker-compose exec redis redis-cli
```

### 4.2 Testes Básicos

```bash
# No redis-cli
SET user:123 "John Doe"
# Esperado: OK

GET user:123
# Esperado: "John Doe"

EXPIRE user:123 3600
# Esperado: 1

TTL user:123
# Esperado: ~3600

DEL user:123
# Esperado: 1
```

## ✅ Teste 5: Prometheus

### 5.1 Acessar Prometheus UI

```
http://localhost:9090
```

### 5.2 Executar Queries

```
# Número de requisições HTTP
http_requests_total

# Duração média de requisições
avg(http_request_duration_seconds)

# Taxa de erros
rate(http_requests_total{status=~"5.."}[5m])
```

### 5.3 Verificar Targets

1. Acessar http://localhost:9090/targets
2. Verificar se todos os targets estão "UP"
3. Expected: 
   - prometheus
   - kong
   - mongodb
   - redis

## ✅ Teste 6: Grafana

### 6.1 Acessar Grafana

```
http://localhost:3000
Username: admin
Password: admin
```

### 6.2 Verificar Datasource

1. Configuration → Data Sources
2. Clicar em "Prometheus"
3. Clicar "Test" 
4. Esperado: "Data source is working"

### 6.3 Visualizar Dashboard

1. Clicar em "+" → Import
2. UID: `kong-gateway-dashboard`
3. Esperado: Dashboard com gráficos

### 6.4 Criar Query Custom

1. New Dashboard → Add Panel
2. Query: `http_requests_total`
3. Esperado: Gráfico mostrando requisições

## ✅ Teste 7: RabbitMQ

### 7.1 Acessar RabbitMQ Management

```
http://localhost:15672
Username: guest
Password: guest
```

### 7.2 Criar Fila de Teste

1. Queues → Add a new queue
2. Name: `test-queue`
3. Clicar "Add queue"

### 7.3 Publicar Mensagem

1. Selecionar `test-queue`
2. Ir em "Publish message"
3. Payload: `{"test": "message"}`
4. Clicar "Publish message"
5. Esperado: Message count aumenta

### 7.4 Consumir Mensagem

1. Na mesma queue, ir em "Get messages"
2. Esperado: Ver a mensagem publicada

## ✅ Teste 8: Fluxo End-to-End

### 8.1 Simular Compra de Game

```bash
# 1. Login
TOKEN=$(curl -s -X POST http://localhost:8000/api/users/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"pass"}' | jq -r '.token')

echo "Token: $TOKEN"

# 2. Listar Games
curl -s http://localhost:8000/api/catalog/games \
  -H "Authorization: Bearer $TOKEN" | jq

# 3. Criar Pedido
curl -s -X POST http://localhost:8000/api/orders \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"gameId":1,"quantity":1}' | jq

# 4. Processar Pagamento
curl -s -X POST http://localhost:8000/api/payments \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"orderId":"123","amount":29.99}' | jq
```

### 8.2 Verificar Observabilidade

1. Abrir Grafana: http://localhost:3000
2. Dashboard: API Gateway
3. Verificar:
   - RPS aumentou
   - Latência registrada
   - Taxa de erro (se houver)
   - Breakdown por endpoint

### 8.3 Verificar MongoDB

```bash
docker-compose exec mongodb mongo -u admin -p admin

use fcg_db
db.orders.find()
```

Esperado: Novo documento de pedido

### 8.4 Verificar Redis Cache

```bash
docker-compose exec redis redis-cli

KEYS *
GET user:123
```

Esperado: Dados em cache

## ✅ Teste 9: Alertas

### 9.1 Disparar Alto Taxa de Erro

```bash
# Fazer muitas requisições com erro (401, 500)
for i in {1..100}; do
  curl http://localhost:8000/api/test/invalid &
done
```

### 9.2 Verificar Alert

1. Prometheus: http://localhost:9090/alerts
2. Grafana: Alerting → Alert Rules
3. Esperado: Alert "HighErrorRate" ativado

## ✅ Teste 10: Logs e Monitoramento

### 10.1 Ver Logs do Kong

```bash
docker-compose logs -f kong | head -50
```

Esperado: Ver requisições sendo processadas

### 10.2 Ver Logs do MongoDB

```bash
docker-compose logs -f mongodb | head -50
```

### 10.3 Ver Logs de Todos os Serviços

```bash
docker-compose logs --tail=50
```

## 📊 Checklist de Validação

- [ ] Todos os containers estão rodando
- [ ] Kong roteia requisições corretamente
- [ ] JWT validation funciona
- [ ] MongoDB salva dados
- [ ] Redis guarda cache
- [ ] Prometheus coleta métricas
- [ ] Grafana mostra dashboards
- [ ] RabbitMQ enfileira mensagens
- [ ] Fluxo end-to-end funciona
- [ ] Alertas disparam corretamente

## 🐛 Troubleshooting

### Kong não conecta ao PostgreSQL
```bash
docker-compose restart kong-database
docker-compose restart kong-migration
docker-compose restart kong
```

### Prometheus não coleta métricas
```bash
# Verificar prometheus.yml
docker-compose exec prometheus cat /etc/prometheus/prometheus.yml

# Reiniciar
docker-compose restart prometheus
```

### Grafana não mostra dados
```bash
# Verificar datasource
curl http://localhost:3000/api/datasources | jq

# Se necessário, recriar datasource
```

### MongoDB não inicializa
```bash
docker-compose logs mongodb
docker-compose restart mongodb
```

---

**Todos os testes devem passar para validar a implementação da Fase 3.**
