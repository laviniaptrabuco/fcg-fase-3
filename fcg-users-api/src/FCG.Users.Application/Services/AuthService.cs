using FCG.Events;
using FCG.Users.Application.DTOs;
using FCG.Users.Application.Interfaces;
using FCG.Users.Domain.Entities;
using FCG.Users.Domain.Enums;
using FCG.Users.Domain.Exceptions;
using FCG.Users.Domain.Interfaces;
using FCG.Users.Domain.Services;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FCG.Users.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IPublishEndpoint publishEndpoint,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, ct)
            ?? throw new DomainException("Invalid email or password.");

        if (!user.IsActive)
            throw new DomainException("Account is deactivated.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new DomainException("Invalid email or password.");

        var token = _jwtService.GenerateToken(user);
        var expiresAt = _jwtService.GetExpiration();

        _logger.LogInformation("User {Email} logged in successfully.", user.Email);
        return new LoginResponse(token, user.Name, user.Email, user.Role.ToString(), expiresAt);
    }

    public async Task<UserResponse> RegisterAsync(RegisterUserRequest request, CancellationToken ct = default)
    {
        PasswordValidator.Validate(request.Password);

        if (await _userRepository.ExistsByEmailAsync(request.Email, ct))
            throw new DomainException($"Email '{request.Email}' is already registered.");

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = new User(request.Name, request.Email, passwordHash, UserRole.User);

        await _userRepository.AddAsync(user, ct);
        _logger.LogInformation("New user registered: {Email}", user.Email);

        await _publishEndpoint.Publish(new UserCreatedEvent(user.Id, user.Email, user.Name), ct);
        _logger.LogInformation("UserCreatedEvent published for {Email}", user.Email);

        return MapToResponse(user);
    }

    private static UserResponse MapToResponse(User user) =>
        new(user.Id, user.Name, user.Email, user.Role.ToString(), user.CreatedAt, user.IsActive);
}
