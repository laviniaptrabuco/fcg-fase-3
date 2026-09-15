# FCG Catalog API

Microsserviço responsável pelo catálogo de jogos e fluxo de compra da FIAP Cloud Games.

## Responsabilidades

- CRUD de jogos (Admin)
- Listagem de jogos (público)
- Iniciar compra: publica `OrderPlacedEvent`
- Consumir `PaymentProcessedEvent`: adiciona jogo à biblioteca do usuário se aprovado

## Eventos

| Evento | Direção | Descrição |
|---|---|---|
| `OrderPlacedEvent` | Publicado | Compra iniciada com UserId, GameId, Price |
| `PaymentProcessedEvent` | Consumido | Resultado do pagamento (Approved/Rejected) |

## Variáveis de Ambiente

| Variável | Descrição |
|---|---|
| `ConnectionStrings__DefaultConnection` | Connection string SQL Server |
| `Jwt__Secret` | Chave JWT (mesma do UsersAPI) |
| `Jwt__Issuer` | Issuer do token |
| `Jwt__Audience` | Audience do token |
| `RabbitMQ__Host` | Host do RabbitMQ |
| `RabbitMQ__Username` | Usuário RabbitMQ |
| `RabbitMQ__Password` | Senha RabbitMQ |

## Endpoints

| Método | Rota | Descrição | Auth |
|---|---|---|---|
| GET | `/api/games` | Lista todos os jogos | Não |
| GET | `/api/games/{id}` | Busca jogo por ID | Não |
| GET | `/api/games/genre/{genre}` | Filtra por gênero | Não |
| POST | `/api/games` | Cadastra jogo | Admin |
| PUT | `/api/games/{id}` | Atualiza jogo | Admin |
| PATCH | `/api/games/{id}/promotion` | Aplica promoção | Admin |
| DELETE | `/api/games/{id}/promotion` | Remove promoção | Admin |
| DELETE | `/api/games/{id}` | Desativa jogo | Admin |
| POST | `/api/games/{id}/purchase` | Inicia compra | Autenticado |
| GET | `/api/users/{userId}/library` | Biblioteca do usuário | Próprio/Admin |

## Criar Solution

```powershell
dotnet new sln -n FCG.CatalogAPI
dotnet sln add src/FCG.Catalog.Domain/FCG.Catalog.Domain.csproj
dotnet sln add src/FCG.Catalog.Application/FCG.Catalog.Application.csproj
dotnet sln add src/FCG.Catalog.Infrastructure/FCG.Catalog.Infrastructure.csproj
dotnet sln add src/FCG.CatalogAPI/FCG.CatalogAPI.csproj
```
