# FCG Orchestration

Repositório de orquestração da plataforma FIAP Cloud Games — contém o `docker-compose.yml` para execução local e os manifestos Kubernetes para deploy em cluster.

## Arquitetura

```
┌─────────────────────────────────────────────────────────────┐
│                    FIAP Cloud Games                         │
│                                                             │
│  ┌──────────────┐    UserCreatedEvent    ┌───────────────┐  │
│  │  UsersAPI    │ ──────────────────────▶│Notifications  │  │
│  │  (porta 5001)│                        │     API       │  │
│  └──────────────┘                        └───────────────┘  │
│                                                    ▲        │
│  ┌──────────────┐  OrderPlacedEvent  ┌──────────┐  │        │
│  │  CatalogAPI  │ ─────────────────▶ │Payments  │  │        │
│  │  (porta 5002)│◀──────────────────│   API    │──┘        │
│  └──────────────┘ PaymentProcessedEv └──────────┘           │
│                                                             │
│  Broker: RabbitMQ (porta 5672 / management 15672)           │
│  Banco:  SQL Server (porta 1433)                            │
└─────────────────────────────────────────────────────────────┘
```

## Executar com Docker Compose

```bash
# Na pasta fcg-orchestration
docker-compose up --build

# Acessos após subir:
# UsersAPI Swagger:   http://localhost:5001/swagger
# CatalogAPI Swagger: http://localhost:5002/swagger
# RabbitMQ Manager:  http://localhost:15672 (guest/guest)
```

## Deploy no Kubernetes

### Pré-requisitos
- Cluster local (Docker Desktop Kubernetes, Minikube ou Kind)
- kubectl configurado
- Imagens buildadas localmente:

```bash
docker build -t fcg-users-api:latest ../fcg-users-api
docker build -t fcg-catalog-api:latest ../fcg-catalog-api
docker build -t fcg-payments-api:latest ../fcg-payments-api
docker build -t fcg-notifications-api:latest ../fcg-notifications-api
```

### Aplicar todos os manifestos

```bash
kubectl apply -f k8s/sqlserver/
kubectl apply -f k8s/rabbitmq/
kubectl apply -f k8s/users-api/
kubectl apply -f k8s/catalog-api/
kubectl apply -f k8s/payments-api/
kubectl apply -f k8s/notifications-api/
```

### Verificar pods

```bash
kubectl get pods
kubectl get services
kubectl get deployments
```

### Acessar serviços localmente (port-forward)

```bash
kubectl port-forward svc/users-api-service 5001:80
kubectl port-forward svc/catalog-api-service 5002:80
kubectl port-forward svc/rabbitmq-service 15672:15672
```

## Fluxo de Teste

### 1. Cadastro de usuário (fluxo boas-vindas)
```
POST http://localhost:5001/api/auth/register
{
  "name": "João Gamer",
  "email": "joao@email.com",
  "password": "Senha@123"
}
```
→ Verifique nos logs do `notifications-api`: `[EMAIL SIMULADO] Boas-vindas enviado para: joao@email.com`

### 2. Login e obter token
```
POST http://localhost:5001/api/auth/login
{
  "email": "joao@email.com",
  "password": "Senha@123"
}
```

### 3. Cadastrar jogo (como admin)
```
POST http://localhost:5002/api/games
Authorization: Bearer {token_admin}
{
  "title": "Elden Ring",
  "description": "RPG de ação",
  "genre": "RPG",
  "price": 249.90
}
```

### 4. Comprar jogo (fluxo de compra completo)
```
POST http://localhost:5002/api/games/{gameId}/purchase
Authorization: Bearer {token_usuario}
```
→ Verifique logs:
- `catalog-api`: `OrderPlacedEvent published`
- `payments-api`: `PaymentProcessedEvent published: Status=Approved`
- `catalog-api`: `Game {gameId} added to library of User {userId}`
- `notifications-api`: `[EMAIL SIMULADO] Confirmação de compra enviado`
