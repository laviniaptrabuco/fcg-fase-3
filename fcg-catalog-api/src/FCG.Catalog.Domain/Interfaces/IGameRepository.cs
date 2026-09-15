using FCG.Catalog.Domain.Entities;

namespace FCG.Catalog.Domain.Interfaces;

public interface IGameRepository
{
    Task<Game?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Game>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<Game>> GetByGenreAsync(string genre, CancellationToken ct = default);
    Task<Game> AddAsync(Game game, CancellationToken ct = default);
    Task UpdateAsync(Game game, CancellationToken ct = default);
    Task<bool> ExistsByTitleAsync(string title, CancellationToken ct = default);
}
