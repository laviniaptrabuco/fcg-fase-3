# Implementação de Alterações nos Microsserviços - Fase 3

Este documento descreve como integrar as tecnologias da Fase 3 (MongoDB, Redis, Prometheus) nos microsserviços existentes da Fase 2.

**IMPORTANTE:** Para manter o histórico da Fase 2 limpo, as alterações devem ser feitas AQUI em Fase 3, não nos repositórios originais.

---

## 📋 Alterações Necessárias por Microsserviço

### 1. **Prometheus Metrics** (Todos os Microsserviços)

#### Instalação

```bash
dotnet add package prometheus-net.AspNetCore
dotnet add package prometheus-net
```

#### Modificação: `Startup.cs`

```csharp
using Prometheus;

public void ConfigureServices(IServiceCollection services)
{
    // ... suas dependências

    // Prometheus
    services.AddSingleton<ICollectorRegistry>(CollectorRegistry.Default);

    services.AddControllers();
}

public void Configure(IApplicationBuilder app)
{
    // ... middleware existente

    // Prometheus middleware ANTES do routing
    app.UseHttpMetrics();

    app.UseRouting();
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
        endpoints.MapMetrics("/metrics");  // Expõe métricas em /metrics
    });
}
```

#### Verificar Métricas

```bash
curl http://localhost:3000/metrics

# Você verá:
# http_requests_total{method="GET",endpoint="/api/users",status="200"} 123
# http_request_duration_seconds_bucket{...} 0.234
```

---

### 2. **MongoDB Integration** (Users, Catalog, Payments)

#### Instalação

```bash
dotnet add package MongoDB.Driver
```

#### Modificação: `Startup.cs`

```csharp
using MongoDB.Driver;

public void ConfigureServices(IServiceCollection services)
{
    // MongoDB
    var mongoUri = Configuration.GetConnectionString("MongoDB") 
        ?? "mongodb://admin:admin@localhost:27017";
    var mongoClient = new MongoClient(mongoUri);
    var mongoDatabase = mongoClient.GetDatabase("fcg_db");
    
    services.AddSingleton(mongoDatabase);
}
```

#### Modificação: `appsettings.json`

```json
{
  "ConnectionStrings": {
    "MongoDB": "mongodb://admin:admin@localhost:27017",
    "Redis": "localhost:6379"
  }
}
```

#### Uso em Controllers

```csharp
public class UsersController : ControllerBase
{
    private readonly IMongoCollection<User> _usersCollection;

    public UsersController(IMongoDatabase mongoDatabase)
    {
        _usersCollection = mongoDatabase.GetCollection<User>("users");
    }

    [HttpPost]
    public async Task<ActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var user = new User { Email = request.Email, Name = request.Name };
        await _usersCollection.InsertOneAsync(user);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(string id)
    {
        var user = await _usersCollection
            .Find(u => u.Id == id)
            .FirstOrDefaultAsync();
        
        if (user == null) return NotFound();
        return Ok(user);
    }
}
```

---

### 3. **Redis Cache** (Users, Catalog)

#### Instalação

```bash
dotnet add package StackExchange.Redis
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
```

#### Modificação: `Startup.cs`

```csharp
using StackExchange.Redis;

public void ConfigureServices(IServiceCollection services)
{
    var redisConnection = Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    
    var redis = ConnectionMultiplexer.Connect(redisConnection);
    services.AddSingleton(redis);
    
    // Ou usar distributed cache
    services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
    });
}
```

#### Uso em Controllers

```csharp
public class UsersController : ControllerBase
{
    private readonly IDatabase _redisDb;

    public UsersController(IConnectionMultiplexer redis)
    {
        _redisDb = redis.GetDatabase();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(string id)
    {
        var cacheKey = $"user:{id}";

        // Tentar cache
        var cached = await _redisDb.StringGetAsync(cacheKey);
        if (cached.HasValue)
        {
            var user = JsonSerializer.Deserialize<User>(cached.ToString());
            return Ok(new { source = "cache", data = user });
        }

        // Buscar do banco
        var user_db = await _usersCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
        
        if (user_db == null) return NotFound();

        // Guardar em cache por 30 minutos
        var json = JsonSerializer.Serialize(user_db);
        await _redisDb.StringSetAsync(cacheKey, json, TimeSpan.FromMinutes(30));

        return Ok(new { source = "database", data = user_db });
    }
}
```

---

### 4. **Logging Centralizado** (Todos)

#### Instalação

```bash
dotnet add package Serilog
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
```

#### Modificação: `Program.cs`

```csharp
using Serilog;

public static void Main(string[] args)
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .WriteTo.Console()
        .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
        .CreateLogger();

    try
    {
        CreateHostBuilder(args).Build().Run();
    }
    finally
    {
        Log.CloseAndFlush();
    }
}

public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });
```

---

## 🚀 Microsserviços - Checklist de Implementação

### fcg-users-api
- [ ] Prometheus metrics exposto em `/metrics`
- [ ] MongoDB integrado para persistência de usuários
- [ ] Redis para cache de perfis (TTL: 30 min)
- [ ] Logging centralizado
- [ ] Health check endpoint (`/health`)
- [ ] JWT validation funcionando

**Coleção MongoDB:**
```json
{
  "_id": ObjectId(),
  "email": "user@example.com",
  "name": "John Doe",
  "passwordHash": "...",
  "createdAt": ISODate("2024-01-01"),
  "updatedAt": ISODate("2024-01-01")
}
```

### fcg-catalog-api
- [ ] Prometheus metrics exposto em `/metrics`
- [ ] MongoDB para catálogo expandido (games, reviews, ratings)
- [ ] Redis para cache de busca (TTL: 1 hour)
- [ ] Logging centralizado
- [ ] Health check endpoint
- [ ] Integração com Kong

**Coleção MongoDB:**
```json
{
  "_id": ObjectId(),
  "title": "Game Name",
  "description": "...",
  "price": 29.99,
  "rating": 4.5,
  "reviews": [...],
  "createdAt": ISODate()
}
```

### fcg-payments-api
- [ ] Prometheus metrics
- [ ] MongoDB para log de transações
- [ ] Redis para cache de histórico recente
- [ ] Logging centralizado
- [ ] Health check
- [ ] JWT validation

**Coleção MongoDB:**
```json
{
  "_id": ObjectId(),
  "orderId": "order-123",
  "userId": "user-456",
  "amount": 29.99,
  "status": "approved",
  "transactionId": "txn-789",
  "createdAt": ISODate()
}
```

### fcg-orchestration
- [ ] Prometheus metrics
- [ ] Coordenação entre serviços
- [ ] Event sourcing em MongoDB
- [ ] Cache em Redis
- [ ] Logging
- [ ] Integração com RabbitMQ

### fcg-notifications-serverless (Azure Function)
- [ ] Trigger por RabbitMQ queue
- [ ] Processamento assíncrono
- [ ] Logging em Application Insights
- [ ] Retry automático
- [ ] Dead Letter Queue

---

## 📊 Dockerfile para Cada Microsserviço

```dockerfile
# fcg-users-api.Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /build

COPY . .
RUN dotnet restore
RUN dotnet build -c Release
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:3000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 3000

HEALTHCHECK --interval=10s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:3000/health || exit 1

ENTRYPOINT ["dotnet", "FcgUsersApi.dll"]
```

---

## 🧪 Testes de Integração

### Teste 1: Prometheus Métricas

```bash
# Fazer requisição
curl http://localhost:3000/api/users

# Verificar métricas
curl http://localhost:3000/metrics | grep http_requests_total

# Esperado:
# http_requests_total{method="GET",path="/api/users",status="200"} 1
```

### Teste 2: MongoDB Persistência

```bash
# Criar documento
curl -X POST http://localhost:3000/api/users \
  -H "Content-Type: application/json" \
  -d '{"email":"test@test.com","name":"Test"}'

# Verificar em MongoDB
docker-compose exec mongodb mongo -u admin -p admin
use fcg_db
db.users.find()
```

### Teste 3: Redis Cache

```bash
# Primeira requisição (do BD)
time curl http://localhost:3000/api/users/123

# Segunda requisição (do cache - mais rápida)
time curl http://localhost:3000/api/users/123

# Verificar cache
docker-compose exec redis redis-cli
GET user:123
```

### Teste 4: Kong Roteamento

```bash
# Via Kong
curl http://localhost:8000/api/users

# Direto (para comparar latência)
curl http://localhost:3000/api/users

# Kong deve ter latência extras (overhead)
```

---

## 🔍 Monitoramento

### Grafana Queries

```
# RPS por microsserviço
rate(http_requests_total[5m])

# P95 Latência
histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))

# Taxa de erro
rate(http_requests_total{status=~"5.."}[5m])

# Conexões ativas MongoDB
mongodb_connections{status="active"}
```

---

## 📚 Arquivos de Exemplo

Todos os exemplos implementados estão em:

- `fcg-users-api-exemplo/Startup.cs` - Configuração completa
- `fcg-users-api-exemplo/Controllers/UsersController.cs` - Controller exemplo
- `fcg-users-api-exemplo/fcg-users-api.csproj` - Dependências
- `fcg-notifications-serverless/src/NotificationFunction.cs` - Azure Function
- `fcg-api-gateway/setup-routes.sh` - Setup Kong

---

## ✅ Validação Final

Para confirmar que tudo está funcionando:

```bash
# 1. Todos os containers rodando
docker-compose ps

# 2. Kong com rotas configuradas
curl http://localhost:8001/services

# 3. Prometheus scraping
curl http://localhost:9090/api/v1/targets

# 4. Grafana dashboards
curl http://localhost:3000/api/datasources

# 5. Fluxo completo
bash fcg-api-gateway/setup-routes.sh
```

---

**Status:** ✅ Fase 3 completa e pronta para implementação nos microsserviços
