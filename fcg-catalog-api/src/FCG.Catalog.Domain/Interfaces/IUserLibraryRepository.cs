using FCG.Catalog.Domain.Entities;

namespace FCG.Catalog.Domain.Interfaces;

public interface IUserLibraryRepository
{
    Task<bool> ExistsAsync(Guid userId, Guid gameId, CancellationToken ct = default);
    Task AddAsync(UserLibrary entry, CancellationToken ct = default);
    Task<IEnumerable<UserLibrary>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
}
