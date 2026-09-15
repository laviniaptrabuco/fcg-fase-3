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
# Clonar repositório com submodules
git clone --recursive https://github.com/laviniaptrabuco/fcg-fase-3.git
cd fcg-fase-3

# Iniciar todos os serviços
docker-compose up -d

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

## 📝 Microserviços

- **fcg-users-api**: Autenticação JWT + MongoDB cache Redis
- **fcg-catalog-api**: Catálogo + busca com filtros
- **fcg-payments-api**: Processamento de pagamentos
- **fcg-orchestration**: Orquestração de fluxos
- **fcg-notifications-serverless**: Azure Function serverless

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

## 📦 Arquivos Importantes

```
fcg-fase-3/
├── docker-compose.yml
├── fcg-observability/
│   ├── prometheus/
│   ├── grafana/
│   └── mongodb/
├── fcg-api-gateway/
├── fcg-notifications-serverless/
└── [submodules]
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
