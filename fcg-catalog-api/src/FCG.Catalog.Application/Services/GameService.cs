using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Domain.Exceptions;
using FCG.Catalog.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FCG.Catalog.Application.Services;

public class GameService
{
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<GameService> _logger;

    public GameService(IGameRepository gameRepository, ILogger<GameService> logger)
    {
        _gameRepository = gameRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<GameResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var games = await _gameRepository.GetAllAsync(ct);
        return games.Select(MapToResponse);
    }

    public async Task<GameResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Game), id);
        return MapToResponse(game);
    }

    public async Task<IEnumerable<GameResponse>> GetByGenreAsync(string genre, CancellationToken ct = default)
    {
        var games = await _gameRepository.GetByGenreAsync(genre, ct);
        return games.Select(MapToResponse);
    }

    public async Task<GameResponse> CreateAsync(CreateGameRequest request, CancellationToken ct = default)
    {
        if (await _gameRepository.ExistsByTitleAsync(request.Title, ct))
            throw new DomainException($"A game with title '{request.Title}' already exists.");

        var game = new Domain.Entities.Game(request.Title, request.Description, request.Genre, request.Price);
        await _gameRepository.AddAsync(game, ct);
        _logger.LogInformation("Game '{Title}' created.", game.Title);
        return MapToResponse(game);
    }

    public async Task<GameResponse> UpdateAsync(Guid id, UpdateGameRequest request, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Game), id);

        game.Update(request.Title, request.Description, request.Genre, request.Price);
        await _gameRepository.UpdateAsync(game, ct);
        return MapToResponse(game);
    }

    public async Task<GameResponse> ApplyPromotionAsync(Guid id, ApplyPromotionRequest request, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Game), id);

        game.ApplyPromotion(request.PromotionalPrice);
        await _gameRepository.UpdateAsync(game, ct);
        return MapToResponse(game);
    }

    public async Task RemovePromotionAsync(Guid id, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Game), id);
        game.RemovePromotion();
        await _gameRepository.UpdateAsync(game, ct);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Game), id);
        game.Deactivate();
        await _gameRepository.UpdateAsync(game, ct);
    }

    private static GameResponse MapToResponse(Domain.Entities.Game g) =>
        new(g.Id, g.Title, g.Description, g.Genre, g.Price, g.PromotionalPrice, g.CurrentPrice, g.IsActive, g.CreatedAt);
}
