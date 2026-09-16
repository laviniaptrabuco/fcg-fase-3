# Tech Challenge Fase 3 — FCG

Microsserviços com API Gateway (Kong), observabilidade (Prometheus + Grafana) e persistência poliglota (MongoDB + Redis).

## ✅ O que está funcionando

### Infraestrutura

```bash
docker-compose up -d
```

Todos os serviços no ar:
- Kong API Gateway (admin: http://localhost:8001)
- Prometheus (http://localhost:9090)
- Grafana (http://localhost:3000 — admin/admin)
- MongoDB (localhost:27017 — admin/admin)
- Redis (localhost:6379)
- RabbitMQ (http://localhost:15672 — guest/guest)

### Observabilidade

**Prometheus:**
- ✅ Scrape jobs configurados
- ✅ Targets coletando

**Grafana:**
- ✅ Datasource Prometheus conectado
- ✅ Dashboard api-gateway.json provisionado
- ✅ Pode adicionar mais dashboards via UI

### Kong API Gateway

- ✅ Admin API respondendo em http://localhost:8001
- ✅ Proxy em http://localhost:8000
- ✅ 2 serviços registrados (users-api, catalog-api)
- ✅ 4 rotas configuradas
- ✅ JWT + Rate Limiting + CORS habilitados

**Testar Kong:**

```bash
# Ver serviços
curl http://localhost:8001/services

# Ver rotas
curl http://localhost:8001/routes
```

### Microsserviços

- ✅ `fcg-users-api` — compilação OK, container rodando
- ✅ `fcg-catalog-api` — compilação OK, container rodando
- ✅ `fcg-payments-api` — compilação OK, container rodando

**Cada um tem:**
- Prometheus metrics em `/metrics`
- Health check em `/health`
- MongoDB integrado
- Redis integrado
- RabbitMQ integrado

## 📊 Stack Pronto

```
docker-compose.yml
├── Kong Database (PostgreSQL)
├── Kong (API Gateway)
├── Prometheus (Metrics)
├── Grafana (Visualization)
├── MongoDB (Document DB)
├── Redis (Cache)
└── RabbitMQ (Message Queue)

Microsserviços:
├── fcg-users-api
├── fcg-catalog-api
└── fcg-payments-api
```

## 🎯 Próximos Passos

1. **Gravar vídeo** (até 20 min)
   - Mostrar docker-compose up
   - Kong admin panel
   - Prometheus targets
   - Grafana dashboard
   - Fluxo de dados na stack

2. **Preencher relatório** com dados do grupo e link do vídeo

---

**Desenvolvido por:** Lavinia Pedrosa  
**Repositório:** https://github.com/laviniaptrabuco/fcg-fase-3  
**Status:** ✅ Infraestrutura pronta para demonstração
