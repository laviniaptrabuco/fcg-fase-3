using FCG.Catalog.Domain.Entities;
using FCG.Catalog.Domain.Interfaces;
using FCG.Catalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FCG.Catalog.Infrastructure.Repositories;

public class GameRepository : IGameRepository
{
    private readonly CatalogDbContext _context;

    public GameRepository(CatalogDbContext context) => _context = context;

    public async Task<Game?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Games.FirstOrDefaultAsync(g => g.Id == id, ct);

    public async Task<IEnumerable<Game>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Games.AsNoTracking().ToListAsync(ct);

    public async Task<IEnumerable<Game>> GetByGenreAsync(string genre, CancellationToken ct = default) =>
        await _context.Games
            .AsNoTracking()
            .Where(g => g.Genre.ToLower() == genre.ToLower())
            .ToListAsync(ct);

    public async Task<Game> AddAsync(Game game, CancellationToken ct = default)
    {
        _context.Games.Add(game);
        await _context.SaveChangesAsync(ct);
        return game;
    }

    public async Task UpdateAsync(Game game, CancellationToken ct = default)
    {
        _context.Games.Update(game);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsByTitleAsync(string title, CancellationToken ct = default) =>
        await _context.Games.AnyAsync(g => g.Title.ToLower() == title.ToLower(), ct);
}
