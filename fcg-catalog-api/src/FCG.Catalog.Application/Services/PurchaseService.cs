using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Domain.Exceptions;
using FCG.Catalog.Domain.Interfaces;
using FCG.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FCG.Catalog.Application.Services;

public class PurchaseService
{
    private readonly IGameRepository _gameRepository;
    private readonly IUserLibraryRepository _userLibraryRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PurchaseService> _logger;

    public PurchaseService(
        IGameRepository gameRepository,
        IUserLibraryRepository userLibraryRepository,
        IPublishEndpoint publishEndpoint,
        ILogger<PurchaseService> logger)
    {
        _gameRepository = gameRepository;
        _userLibraryRepository = userLibraryRepository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<PurchaseResponse> PurchaseAsync(Guid gameId, PurchaseRequest request, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetByIdAsync(gameId, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Game), gameId);

        if (!game.IsActive)
            throw new DomainException("This game is not available for purchase.");

        if (await _userLibraryRepository.ExistsAsync(request.UserId, gameId, ct))
            throw new DomainException("You already own this game.");

        var orderId = Guid.NewGuid();
        var price = game.CurrentPrice;

        await _publishEndpoint.Publish(
            new OrderPlacedEvent(orderId, request.UserId, gameId, price), ct);

        _logger.LogInformation(
            "OrderPlacedEvent published: OrderId={OrderId}, UserId={UserId}, GameId={GameId}, Price={Price}",
            orderId, request.UserId, gameId, price);

        return new PurchaseResponse(orderId, request.UserId, gameId, price, "Pending");
    }

    public async Task<IEnumerable<UserLibraryItemResponse>> GetUserLibraryAsync(Guid userId, CancellationToken ct = default)
    {
        var items = await _userLibraryRepository.GetByUserIdAsync(userId, ct);
        return items
            .Where(i => i.Game != null)
            .Select(i => new UserLibraryItemResponse(
                i.GameId,
                i.Game!.Title,
                i.Game.Genre,
                i.AcquiredAt));
    }
}
