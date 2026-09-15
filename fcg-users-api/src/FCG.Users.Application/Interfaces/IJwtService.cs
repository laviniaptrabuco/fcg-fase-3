using FCG.Users.Domain.Entities;

namespace FCG.Users.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
    DateTime GetExpiration();
}
