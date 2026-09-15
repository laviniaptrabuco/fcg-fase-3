# Tech Challenge Fase 3 - FCG

Microserviços com Observabilidade, MongoDB, Redis, API Gateway e Serverless

## 📋 Arquitetura

```
Cliente
  ↓
Kong API Gateway (Port 8000)
├── JWT Validation
├── Rate Limiting
└── Routing
  ↓
UsersAPI, CatalogAPI, PaymentsAPI
  ↓
MongoDB + Redis + RabbitMQ
  ↓
Prometheus → Grafana (Dashboards)
  ↓
Azure Function (NotificationsAPI - Serverless)
```

## 🚀 Quick Start

```bash
# Clonar repositório
git clone https://github.com/laviniaptrabuco/fcg-fase-3.git
cd fcg-fase-3

# Iniciar infraestrutura (Kong, Prometheus, Grafana, MongoDB, Redis, RabbitMQ)
docker-compose up -d

# Configurar Kong routes automaticamente
bash fcg-api-gateway/setup-routes.sh

# Verificar status
docker-compose ps
```

## 🌐 Acessar Serviços

| Serviço | URL | Credenciais |
|---------|-----|-------------|
| Kong Admin | http://localhost:8001 | - |
| Konga UI | http://localhost:1337 | admin/admin |
| Grafana | http://localhost:3000 | admin/admin |
| Prometheus | http://localhost:9090 | - |
| MongoDB | localhost:27017 | admin/admin |
| Redis | localhost:6379 | - |
| RabbitMQ | http://localhost:15672 | guest/guest |

## 🔧 Configurar Kong

```bash
# Criar serviço
curl -X POST http://localhost:8001/services \
  -d "name=users-api" \
  -d "url=http://users-api:3000"

# Criar rota
curl -X POST http://localhost:8001/services/users-api/routes \
  -d "paths[]=/api/users"

# Adicionar JWT
curl -X POST http://localhost:8001/plugins \
  -d "service.name=users-api" \
  -d "name=jwt"
```

## 📊 Observabilidade (Prometheus + Grafana)

**Dashboards:**
- API Gateway
- API Latency
- Error Rate
- Throughput
- System Metrics

**Métricas Coletadas:**
- HTTP requests (taxa, duração, erros)
- MongoDB connections
- Redis operations
- Kong performance

## 🗄️ Persistência Poliglota

**MongoDB:**
```csharp
var client = new MongoClient("mongodb://admin:admin@localhost:27017");
var database = client.GetDatabase("fcg_db");
var collection = database.GetCollection<User>("users");
```

**Redis:**
```csharp
var redis = ConnectionMultiplexer.Connect("localhost:6379");
var db = redis.GetDatabase();
db.StringSet("user:123", jsonData, TimeSpan.FromHours(1));
```

## ⚡ Serverless (Azure Functions)

NotificationsAPI migrada para Azure Function, disparada por RabbitMQ.

```csharp
[FunctionName("NotificationTrigger")]
public async Task Run([QueueTrigger("notifications")] string message)
{
    // Enviar notificações
}
```

## 📝 Microsserviços (Fase 2 → Fase 3)

**Fase 3 atualiza os microsserviços da Fase 2 com:**

- **fcg-users-api**: Autenticação JWT + MongoDB + Redis cache
  - Repo: https://github.com/laviniaptrabuco/fcg-users-api
  - Status: Atualizado para Fase 3

- **fcg-catalog-api**: Catálogo + busca com MongoDB + Redis cache
  - Repo: https://github.com/laviniaptrabuco/fcg-catalog-api
  - Status: Atualizado para Fase 3

- **fcg-payments-api**: Processamento com MongoDB + Redis
  - Repo: https://github.com/laviniaptrabuco/fcg-payments-api
  - Status: Atualizado para Fase 3

- **fcg-orchestration**: Orquestração + Event Sourcing em MongoDB
  - Repo: https://github.com/laviniaptrabuco/fcg-orchestration
  - Status: Atualizado para Fase 3

- **fcg-notifications-serverless**: Azure Function (substitui NotificationsAPI)
  - Repo: https://github.com/laviniaptrabuco/fcg-notifications-api
  - Status: Migrada para Serverless
  - Trigger: RabbitMQ queue

**Todos incluem:**
- ✅ Prometheus metrics (/metrics endpoint)
- ✅ MongoDB integração
- ✅ Redis cache
- ✅ Health check (/health endpoint)

## 🔐 Segurança

- JWT validation no Kong
- Rate limiting
- CORS policies
- Secrets em Kubernetes (produção)

## 🎬 Fluxo de Demonstração

1. **Login:** `POST /api/users/login` → JWT token
2. **Listar Games:** `GET /api/catalog/games`
3. **Criar Pedido:** `POST /api/orders`
4. **Pagamento:** `POST /api/payments`
5. **Notificação:** Azure Function triggered
6. **Observabilidade:** Grafana mostra toda a jornada

## 📦 Estrutura do Repositório

```
fcg-fase-3/
├── docker-compose.yml (Kong, Prometheus, Grafana, MongoDB, Redis, RabbitMQ)
├── fcg-observability/
│   ├── prometheus/ (config + alert rules)
│   ├── grafana/ (datasources + 5 dashboards)
│   └── mongodb/ (init script)
├── fcg-api-gateway/
│   ├── README.md
│   └── setup-routes.sh (automático)
├── fcg-notifications-serverless/
│   ├── host.json
│   └── src/NotificationFunction.cs
├── fcg-users-api-exemplo/ (exemplo de implementação)
├── IMPLEMENTACAO_MICROSERVICOS.md (guia passo-a-passo)
├── GUIA_TESTES.md (10 testes validados)
├── RELATORIO_ENTREGA.md (formal)
└── CHECKLIST.md (progresso)
```

## ✅ Checklist

- [ ] docker-compose up (todos os serviços)
- [ ] Kong routes configuradas
- [ ] Grafana dashboards funcional
- [ ] MongoDB inicializado
- [ ] Redis conectado
- [ ] Azure Function deployada
- [ ] Vídeo de demonstração
- [ ] Relatório de entrega

---

**Desenvolvido por:** Lavinia Pedrosa
**Data:** 2026-09-15
