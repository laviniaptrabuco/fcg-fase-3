using FCG.Users.Application.DTOs;
using FCG.Users.Domain.Enums;
using FCG.Users.Domain.Exceptions;
using FCG.Users.Domain.Interfaces;
using FCG.Users.Domain.Services;
using Microsoft.Extensions.Logging;

namespace FCG.Users.Application.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<IEnumerable<UserResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var users = await _userRepository.GetAllAsync(ct);
        return users.Select(MapToResponse);
    }

    public async Task<UserResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), id);
        return MapToResponse(user);
    }

    public async Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), id);

        var existingWithEmail = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (existingWithEmail != null && existingWithEmail.Id != id)
            throw new DomainException($"Email '{request.Email}' is already in use.");

        user.Update(request.Name, request.Email);
        await _userRepository.UpdateAsync(user, ct);
        _logger.LogInformation("User {Id} updated.", id);
        return MapToResponse(user);
    }

    public async Task ChangePasswordAsync(Guid id, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), id);

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            throw new DomainException("Current password is incorrect.");

        PasswordValidator.Validate(request.NewPassword);
        user.ChangePassword(_passwordHasher.Hash(request.NewPassword));
        await _userRepository.UpdateAsync(user, ct);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), id);
        user.Deactivate();
        await _userRepository.UpdateAsync(user, ct);
        _logger.LogInformation("User {Id} deactivated.", id);
    }

    public async Task ActivateAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), id);
        user.Activate();
        await _userRepository.UpdateAsync(user, ct);
    }

    public async Task<UserResponse> CreateAdminAsync(RegisterUserRequest request, CancellationToken ct = default)
    {
        PasswordValidator.Validate(request.Password);

        if (await _userRepository.ExistsByEmailAsync(request.Email, ct))
            throw new DomainException($"Email '{request.Email}' is already registered.");

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = new Domain.Entities.User(request.Name, request.Email, passwordHash, UserRole.Admin);
        await _userRepository.AddAsync(user, ct);
        return MapToResponse(user);
    }

    private static UserResponse MapToResponse(Domain.Entities.User user) =>
        new(user.Id, user.Name, user.Email, user.Role.ToString(), user.CreatedAt, user.IsActive);
}
