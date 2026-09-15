# FCG Users API

Microsserviço responsável pelo cadastro, autenticação (JWT) e gerenciamento de usuários da plataforma FIAP Cloud Games.

## Responsabilidades

- Registro e login de usuários
- Geração de tokens JWT
- CRUD de usuários (admin)
- Publicação do evento `UserCreatedEvent` ao registrar um novo usuário

## Tecnologias

- .NET 8 / ASP.NET Core Web API
- Entity Framework Core + SQL Server
- MassTransit + RabbitMQ
- BCrypt para hash de senhas
- JWT para autenticação

## Variáveis de Ambiente

| Variável | Descrição | Exemplo |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | Connection string SQL Server | `Server=sqlserver;Database=FCG_Users;...` |
| `Jwt__Secret` | Chave secreta JWT (mínimo 32 chars) | `FCG_SUPER_SECRET_KEY...` |
| `Jwt__Issuer` | Issuer do token JWT | `FiapCloudGames` |
| `Jwt__Audience` | Audience do token JWT | `FiapCloudGamesUsers` |
| `Jwt__ExpirationHours` | Expiração do token em horas | `8` |
| `RabbitMQ__Host` | Host do RabbitMQ | `rabbitmq` |
| `RabbitMQ__Username` | Usuário RabbitMQ | `guest` |
| `RabbitMQ__Password` | Senha RabbitMQ | `guest` |

## Endpoints

| Método | Rota | Descrição | Auth |
|---|---|---|---|
| POST | `/api/auth/register` | Registra novo usuário | Não |
| POST | `/api/auth/login` | Autentica e retorna JWT | Não |
| GET | `/api/users` | Lista todos os usuários | Admin |
| GET | `/api/users/{id}` | Busca usuário por ID | Próprio/Admin |
| PUT | `/api/users/{id}` | Atualiza dados do usuário | Próprio/Admin |
| PATCH | `/api/users/{id}/password` | Altera senha | Próprio/Admin |
| DELETE | `/api/users/{id}` | Desativa usuário | Admin |
| PATCH | `/api/users/{id}/activate` | Ativa usuário | Admin |
| POST | `/api/users/admin` | Cria usuário admin | Admin |

## Executar localmente

```bash
cd src/FCG.UsersAPI
dotnet run
```

Swagger disponível em: `http://localhost:5000/swagger`

## Criar Solution

```powershell
dotnet new sln -n FCG.UsersAPI
dotnet sln add src/FCG.Users.Domain/FCG.Users.Domain.csproj
dotnet sln add src/FCG.Users.Application/FCG.Users.Application.csproj
dotnet sln add src/FCG.Users.Infrastructure/FCG.Users.Infrastructure.csproj
dotnet sln add src/FCG.UsersAPI/FCG.UsersAPI.csproj
```
