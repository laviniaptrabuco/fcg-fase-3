namespace FCG.Users.Application.DTOs;

public record RegisterUserRequest(string Name, string Email, string Password);
public record UpdateUserRequest(string Name, string Email);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, string Name, string Email, string Role, DateTime ExpiresAt);

public record UserResponse(
    Guid Id,
    string Name,
    string Email,
    string Role,
    DateTime CreatedAt,
    bool IsActive);
