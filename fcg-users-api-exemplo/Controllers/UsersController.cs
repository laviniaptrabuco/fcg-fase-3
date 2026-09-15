using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using StackExchange.Redis;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;

namespace FcgUsersApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IDatabase _redisDb;
        private readonly IMongoCollection<User> _usersCollection;

        public UsersController(IMongoDatabase mongoDatabase, IConnectionMultiplexer redis)
        {
            _mongoDatabase = mongoDatabase;
            _redisDb = redis.GetDatabase();
            _usersCollection = mongoDatabase.GetCollection<User>("users");
        }

        /// <summary>
        /// GET /api/users - Listar todos os usuários
        /// Tenta buscar do cache Redis primeiro, depois MongoDB
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<User>>> GetAllUsers()
        {
            const string cacheKey = "users:all";

            try
            {
                // Tentar cache Redis
                var cached = await _redisDb.StringGetAsync(cacheKey);
                if (cached.HasValue)
                {
                    var users = JsonSerializer.Deserialize<List<User>>(cached.ToString());
                    return Ok(new { source = "cache", data = users });
                }

                // Buscar do MongoDB
                var usersList = await _usersCollection.Find(_ => true).ToListAsync();

                // Guardar em cache por 1 hora
                var json = JsonSerializer.Serialize(usersList);
                await _redisDb.StringSetAsync(cacheKey, json, TimeSpan.FromHours(1));

                return Ok(new { source = "mongodb", data = usersList });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/users/{id} - Obter usuário específico
        /// Cache por 30 minutos
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(string id)
        {
            try
            {
                var cacheKey = $"user:{id}";

                // Tentar cache
                var cached = await _redisDb.StringGetAsync(cacheKey);
                if (cached.HasValue)
                {
                    var user = JsonSerializer.Deserialize<User>(cached.ToString());
                    return Ok(new { source = "cache", data = user });
                }

                // Buscar do MongoDB
                var user_result = await _usersCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
                if (user_result == null)
                {
                    return NotFound();
                }

                // Cache
                var json = JsonSerializer.Serialize(user_result);
                await _redisDb.StringSetAsync(cacheKey, json, TimeSpan.FromMinutes(30));

                return Ok(new { source = "mongodb", data = user_result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// POST /api/users - Criar novo usuário
        /// Invalida cache após criação
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                var user = new User
                {
                    Email = request.Email,
                    Name = request.Name,
                    CreatedAt = DateTime.UtcNow
                };

                // Inserir no MongoDB
                await _usersCollection.InsertOneAsync(user);

                // Invalidar cache
                await _redisDb.KeyDeleteAsync("users:all");

                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// PUT /api/users/{id} - Atualizar usuário
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var update = Builders<User>.Update
                    .Set(u => u.Name, request.Name)
                    .Set(u => u.Email, request.Email)
                    .Set(u => u.UpdatedAt, DateTime.UtcNow);

                var result = await _usersCollection.UpdateOneAsync(u => u.Id == id, update);

                if (result.MatchedCount == 0)
                {
                    return NotFound();
                }

                // Invalidar cache
                await _redisDb.KeyDeleteAsync($"user:{id}");
                await _redisDb.KeyDeleteAsync("users:all");

                return Ok(new { message = "Usuário atualizado com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// DELETE /api/users/{id} - Deletar usuário
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var result = await _usersCollection.DeleteOneAsync(u => u.Id == id);

                if (result.DeletedCount == 0)
                {
                    return NotFound();
                }

                // Invalidar cache
                await _redisDb.KeyDeleteAsync($"user:{id}");
                await _redisDb.KeyDeleteAsync("users:all");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// POST /api/users/login - Autenticação
        /// Criar sessão em Redis
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Buscar usuário
                var user = await _usersCollection.Find(u => u.Email == request.Email).FirstOrDefaultAsync();
                if (user == null)
                {
                    return Unauthorized(new { error = "Credenciais inválidas" });
                }

                // TODO: Validar senha (usar bcrypt em produção)
                if (request.Password != "test") // Apenas para demo
                {
                    return Unauthorized(new { error = "Credenciais inválidas" });
                }

                // Criar sessão em Redis
                var sessionKey = $"session:{Guid.NewGuid()}";
                var sessionData = new
                {
                    user.Id,
                    user.Email,
                    user.Name,
                    LoginAt = DateTime.UtcNow
                };

                await _redisDb.StringSetAsync(
                    sessionKey,
                    JsonSerializer.Serialize(sessionData),
                    TimeSpan.FromHours(24)
                );

                return Ok(new
                {
                    token = sessionKey,
                    user = user
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        public async Task<ActionResult> Health()
        {
            try
            {
                // Testar MongoDB
                await _mongoDatabase.RunCommandAsync(new { ping = 1 });

                // Testar Redis
                var pong = await _redisDb.PingAsync();

                return Ok(new
                {
                    status = "healthy",
                    mongodb = "connected",
                    redis = pong.IsNull ? "disconnected" : "connected",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new
                {
                    status = "unhealthy",
                    error = ex.Message
                });
            }
        }
    }

    // Models
    public class User
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateUserRequest
    {
        public string Email { get; set; }
        public string Name { get; set; }
    }

    public class UpdateUserRequest
    {
        public string Email { get; set; }
        public string Name { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
