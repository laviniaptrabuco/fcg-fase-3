# Tech Challenge Fase 3 — FCG

Microsserviços com API Gateway (Kong), observabilidade (Prometheus + Grafana) e
persistência poliglota (MongoDB + Redis), evoluindo a arquitetura da Fase 2.

## 📋 Arquitetura

```
Cliente
  │
  ▼
Kong API Gateway  :8000 (proxy)  /  :8001 (admin)
  ├── Roteamento por path
  ├── JWT
  └── Rate limiting / CORS
  │
  ▼
UsersAPI · CatalogAPI · PaymentsAPI        (ASP.NET 8, porta 8080)
  │
  ├── MongoDB  :27017   (dados expandidos, eventos)
  ├── Redis    :6379    (cache)
  └── RabbitMQ :5672    (eventos → serverless)
  │
  ▼
Prometheus :9090  ──scrape──▶  Grafana :3000
```

## 🚀 Quick Start

```bash
git clone https://github.com/laviniaptrabuco/fcg-fase-3.git
cd fcg-fase-3

# Sobe a infraestrutura
docker-compose up -d

# Confere o estado
docker-compose ps
```

Aguarde o `kong-migration` terminar (ele sai com código 0) antes de configurar rotas:

```bash
bash fcg-api-gateway/setup-routes.sh
```

## 🌐 Endpoints

| Serviço | URL | Credenciais | Estado |
|---------|-----|-------------|--------|
| Kong Proxy | http://localhost:8000 | — | ✅ |
| Kong Admin API | http://localhost:8001 | — | ✅ |
| Grafana | http://localhost:3000 | admin/admin | ✅ |
| Prometheus | http://localhost:9090 | — | ✅ |
| RabbitMQ Management | http://localhost:15672 | guest/guest | ✅ |
| MongoDB | localhost:27017 | admin/admin | ✅ |
| Redis | localhost:6379 | — | ✅ |
| Konga UI | http://localhost:1337 | — | ❌ incompatível (ver Limitações) |

## ⚠️ Limitações conhecidas

Documentadas de propósito — são os pontos que **não** estão prontos.

1. **Konga não funciona.** A imagem `pantsel/konga` está abandonada desde 2019 e
   seu driver PostgreSQL não negocia SCRAM-SHA-256, exigido pelo PostgreSQL 15.
   O container sobe e morre com `Unknown authenticationOk message type`.
   Corrigir exigiria rebaixar o Postgres para a v9/11, o que quebraria o Kong 3.x.
   **Use a Admin API do Kong** (`http://localhost:8001`) para administração.

2. **Serverless (Azure Function) não está deployado.** O código existe em
   `fcg-notifications-serverless/`, mas não há deploy nem IaC aplicada.

3. **Dashboards Grafana: 1 provisionado** (`api-gateway.json`), não cinco.

4. **`fcg-orchestration` não tem Dockerfile** — não sobe via compose.

## 🔧 Correções aplicadas nesta rodada

Problemas reais encontrados ao validar o ambiente:

| Problema | Causa raiz | Correção |
|---|---|---|
| Kong `healthy` mas recusa conexão na 8001 | Admin API escuta em `127.0.0.1` por padrão — loopback *dentro* do container, inalcançável pela porta publicada | `KONG_ADMIN_LISTEN: 0.0.0.0:8001` |
| Grafana não iniciava | compose montava `datasources/prometheus.yaml`, que não existia no host; o Docker criou um **diretório** com esse nome | datasource movido para o caminho correto; monta-se a pasta inteira |
| Dashboards não carregavam | faltava o arquivo *provider* que o Grafana exige em `provisioning/dashboards` | `dashboards-provider/dashboards.yaml`, com os JSONs em `/var/lib/grafana/dashboards` |
| Konga morria no boot | banco `konga` não existia no Postgres | script de init cria o banco (não resolve o item 1 das Limitações) |
| Rotas do Kong apontavam para `:3000` | os serviços ASP.NET 8 escutam em `8080` | portas corrigidas no `setup-routes.sh` |
| Kong subia antes da migration | `depends_on` sem condição | `service_completed_successfully` |

## 📊 Observabilidade

**Prometheus** (`fcg-observability/prometheus/`)
- `prometheus.yml` — scrape jobs
- `alert-rules.yml` — 4 regras de alerta

**Grafana** (`fcg-observability/grafana/`)
- `datasources/prometheus.yaml` — datasource provisionado
- `dashboards-provider/dashboards.yaml` — provider
- `dashboards/api-gateway.json` — dashboard

Os microsserviços expõem `/metrics` (Prometheus) e `/health`.

## 🗄️ Persistência poliglota

**MongoDB** — dados expandidos, perfis e eventos:
```csharp
var client = new MongoClient("mongodb://admin:admin@mongodb:27017");
var database = client.GetDatabase("fcg_db");
```

**Redis** — cache de sessões e queries:
```csharp
var redis = ConnectionMultiplexer.Connect("redis:6379");
redis.GetDatabase().StringSet("user:123", json, TimeSpan.FromHours(1));
```

> Dentro do compose use os nomes de serviço (`mongodb`, `redis`), não `localhost`.

## 📦 Estrutura

```
fcg-fase-3/
├── docker-compose.yml
├── fcg-observability/
│   ├── prometheus/      prometheus.yml + alert-rules.yml
│   ├── grafana/         datasources/ + dashboards-provider/ + dashboards/
│   ├── postgres/        init-konga-db.sh
│   └── mongodb/         init-mongo.js
├── fcg-api-gateway/     setup-routes.sh
├── fcg-users-api/       ASP.NET 8 + MongoDB + Redis + /metrics
├── fcg-catalog-api/     ASP.NET 8 + MongoDB + Redis + /metrics
├── fcg-payments-api/    ASP.NET 8 + MongoDB + /metrics
├── fcg-notifications-serverless/   Azure Function (não deployada)
├── GUIA_TESTES.md
├── RELATORIO_ENTREGA.md
└── CHECKLIST.md
```

## 🎬 Fluxo de demonstração

1. `docker-compose up -d` — infraestrutura de pé
2. `bash fcg-api-gateway/setup-routes.sh` — serviços, rotas e plugins no Kong
3. `curl http://localhost:8001/services` — rotas registradas
4. Requisição via gateway: `curl http://localhost:8000/api/catalog/games`
5. Prometheus `http://localhost:9090/targets` — targets sendo coletados
6. Grafana `http://localhost:3000` — dashboard com as requisições

---

**Desenvolvido por:** Lavinia Pedrosa
**Fase:** 3 — Observabilidade, Serverless e Persistência Poliglota
