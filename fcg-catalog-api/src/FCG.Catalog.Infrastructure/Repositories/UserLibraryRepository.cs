using FCG.Catalog.Domain.Entities;
using FCG.Catalog.Domain.Interfaces;
using FCG.Catalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FCG.Catalog.Infrastructure.Repositories;

public class UserLibraryRepository : IUserLibraryRepository
{
    private readonly CatalogDbContext _context;

    public UserLibraryRepository(CatalogDbContext context) => _context = context;

    public async Task<bool> ExistsAsync(Guid userId, Guid gameId, CancellationToken ct = default) =>
        await _context.UserLibraries.AnyAsync(ul => ul.UserId == userId && ul.GameId == gameId, ct);

    public async Task AddAsync(UserLibrary entry, CancellationToken ct = default)
    {
        _context.UserLibraries.Add(entry);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<UserLibrary>> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await _context.UserLibraries
            .Include(ul => ul.Game)
            .Where(ul => ul.UserId == userId)
            .ToListAsync(ct);
}
