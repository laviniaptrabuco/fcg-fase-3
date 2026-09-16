# FCG Fase 3 — API Gateway, Observabilidade e Persistência Poliglota

Evolução da plataforma FIAP Cloud Games: Kong como porta de entrada única, observabilidade com Prometheus + Grafana, e persistência poliglota (SQL Server + MongoDB + Redis).

## Arquitetura

```
┌───────────────────────────────────────────────────────────────────┐
│                         Cliente                                   │
└───────────────────────────────┬───────────────────────────────────┘
                                 │ HTTP
                                 ▼
                   ┌─────────────────────────┐
                   │   Kong API Gateway       │
                   │  proxy :8000 / admin :8001│
                   │  JWT · Rate Limit · CORS │
                   └────────┬──────┬──────────┘
                            │      │
              /api/users    │      │  /api/catalog
              /api/auth     │      │  /api/games
              /api/profiles │      │
                            ▼      ▼
                  ┌────────────┐ ┌────────────┐
                  │ UsersAPI   │ │ CatalogAPI │
                  │  :8080     │ │  :8080     │
                  └─┬───┬───┬──┘ └─┬───┬───┬──┘
                    │   │   │      │   │   │
        ┌───────────┘   │   └──────┘   │   └───────────┐
        ▼               ▼              ▼               ▼
  ┌──────────┐    ┌──────────┐   ┌──────────┐   ┌──────────┐
  │SQL Server│    │ MongoDB  │   │  Redis   │   │ RabbitMQ │
  │(relacional│   │(perfis,  │   │ (cache-  │   │ (eventos │
  │ core)    │    │ reviews) │   │  aside)  │   │ de compra)│
  └──────────┘    └──────────┘   └──────────┘   └────┬─────┘
                                                       │
                                                       ▼
                                                ┌─────────────┐
                                                │PaymentsAPI  │
                                                │(worker, sem │
                                                │ HTTP)       │
                                                └─────────────┘

Observabilidade:
  UsersAPI/CatalogAPI --/metrics--> Prometheus --> Grafana (dashboard)
```

**Persistência poliglota:** SQL Server guarda os dados centrais herdados da Fase 2 (usuários, catálogo de jogos). MongoDB guarda dados expandidos e de alta volumetria (perfis de usuário, reviews de jogos). Redis faz cache-aside sobre o MongoDB (perfis).

## Executar com Docker Compose

```bash
docker-compose up -d --build

# Acessos após subir:
# Kong Proxy:      http://localhost:8000
# Kong Admin:      http://localhost:8001
# Grafana:         http://localhost:3000 (admin/admin)
# Prometheus:      http://localhost:9090
# RabbitMQ Manager: http://localhost:15672 (guest/guest)
# MongoDB:         localhost:27017 (admin/admin)
# Redis:           localhost:6379
# SQL Server:      localhost:1433 (SA / FCG_Dev@2024)

bash fcg-api-gateway/setup-routes.sh
```

## Deploy no Kubernetes

### Pré-requisitos
- Cluster local (Docker Desktop Kubernetes, Minikube ou Kind)
- kubectl configurado
- Imagens buildadas localmente:

```bash
docker build -t fcg-users-api:fase3   ./fcg-users-api
docker build -t fcg-catalog-api:fase3 ./fcg-catalog-api
docker build -t fcg-payments-api:fase3 ./fcg-payments-api
```

> **Docker Desktop Kubernetes** compartilha o daemon Docker com o cluster — as imagens acima já ficam visíveis para os pods automaticamente.
> **Minikube ou Kind** rodam um daemon separado: depois do build, carregue as imagens explicitamente no cluster antes de aplicar os manifestos:
> ```bash
> minikube image load fcg-users-api:fase3 fcg-catalog-api:fase3 fcg-payments-api:fase3
> # ou, com Kind:
> kind load docker-image fcg-users-api:fase3 fcg-catalog-api:fase3 fcg-payments-api:fase3
> ```

### Aplicar todos os manifestos (ordem importa)

```bash
cd fcg-orchestration/k8s

kubectl apply -f fase3-namespace.yaml
kubectl apply -f fase3-data/                    # SQL Server, MongoDB, Redis, RabbitMQ, Postgres do Kong
kubectl apply -f fase3-gateway/kong-migration-job.yaml
kubectl wait --for=condition=complete job/kong-migration -n fcg-fase3 --timeout=90s
kubectl apply -f fase3-gateway/kong.yaml
kubectl apply -f fase3-observability/           # Prometheus + Grafana
kubectl apply -f fase3-services/                # users-api, catalog-api, payments-api
```

### Verificar pods

```bash
kubectl get pods -n fcg-fase3
kubectl get services -n fcg-fase3
```

### Acessar serviços localmente (port-forward)

```bash
kubectl port-forward -n fcg-fase3 svc/kong 8000:8000 8001:8001
kubectl port-forward -n fcg-fase3 svc/prometheus 9090:9090
kubectl port-forward -n fcg-fase3 svc/grafana 3000:3000
```

### Configurar rotas do Kong

```bash
KONG_ADMIN="http://localhost:8001" bash fcg-api-gateway/setup-routes.sh
```

## Fluxo de Teste

### 1. Cadastro de usuário

```bash
curl -X POST http://localhost:8000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"name":"João Gamer","email":"joao@email.com","password":"Senha@123"}'
```

### 2. Login e obter token

```bash
curl -X POST http://localhost:8000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"joao@email.com","password":"Senha@123"}'
```
→ Retorna `{"token": "eyJ...", ...}`.

### 3. Rota protegida por JWT (RBAC)

```bash
# Sem token: 401
curl http://localhost:8000/api/users

# Com token de usuário comum: 403 (endpoint exige role Admin)
curl http://localhost:8000/api/users -H "Authorization: Bearer {token}"
```

### 4. Persistência poliglota — perfil (MongoDB + cache Redis)

```bash
# Grava no MongoDB
curl -X PUT http://localhost:8000/api/profiles/{userId} \
  -H "Authorization: Bearer {token}" -H "Content-Type: application/json" \
  -d '{"displayName":"João","bio":"Gamer","favoriteGenres":["RPG"]}'

# 1ª leitura: source=mongodb · 2ª leitura: source=redis (cache-aside)
curl http://localhost:8000/api/profiles/{userId} -H "Authorization: Bearer {token}"
```

### 5. Catálogo de jogos (SQL Server, rota aberta)

```bash
curl http://localhost:8000/api/games
```

### 6. Observabilidade

```bash
# Targets do Prometheus (todos "up": prometheus, kong, microservices x2)
curl http://localhost:9090/api/v1/targets

# Métricas reais dos microsserviços (latência, contagem, status code)
curl "http://localhost:9090/api/v1/query?query=http_requests_received_total"
```

Grafana: `http://localhost:3000` (admin/admin) → dashboard **FCG - UsersAPI & CatalogAPI**, com requisições por segundo, por status code HTTP, latência p95 e taxa de erro em tempo real.

## Estrutura do repositório

```
fcg-fase-3/
├── docker-compose.yml
├── fcg-api-gateway/        setup-routes.sh (idempotente)
├── fcg-observability/      prometheus.yml, alert-rules.yml, datasources, dashboards
├── fcg-orchestration/k8s/  manifestos Kubernetes (fase3-namespace, data, gateway, observability, services)
├── fcg-users-api/          ASP.NET 8 · SQL Server + MongoDB + Redis · /metrics · /health
├── fcg-catalog-api/        ASP.NET 8 · SQL Server + MongoDB + Redis · /metrics · /health
└── fcg-payments-api/       Worker Service · MongoDB · consumidor RabbitMQ
```

---

**Desenvolvido por:** Lavinia Pedrosa
**Repositório:** https://github.com/laviniaptrabuco/fcg-fase-3
