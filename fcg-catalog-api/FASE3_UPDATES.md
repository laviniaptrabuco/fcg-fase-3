# Fase 3 - MongoDB, Redis e Prometheus

Este código foi copiado da Fase 2 e atualizado para Fase 3 com:

- ✅ Conexão com MongoDB (mongodb://admin:admin@localhost:27017)
- ✅ Cache com Redis (localhost:6379)
- ✅ Métricas Prometheus (/metrics endpoint)
- ✅ Health check (/health endpoint)

## Implementação Necessária

1. Instalar NuGet packages:
```bash
dotnet add package MongoDB.Driver
dotnet add package StackExchange.Redis
dotnet add package prometheus-net.AspNetCore
```

2. Atualizar Startup.cs (ver exemplo em: ../fcg-users-api-exemplo/Startup.cs)

3. Adicionar controllers com cache e persistência

4. Rodar com docker-compose:
```bash
docker-compose up -d
dotnet run
```

Será acessível em http://localhost:3000
Métricas em http://localhost:3000/metrics
