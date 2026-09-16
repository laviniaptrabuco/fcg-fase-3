using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Driver;

namespace FCG.Users.Infrastructure.NoSql;

/// <summary>
/// Perfis ficam no MongoDB (documento flexível) com cache-aside em Redis.
/// </summary>
public class UserProfileRepository
{
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
    };

    private readonly IMongoCollection<UserProfileDocument> _collection;
    private readonly IDistributedCache _cache;

    public UserProfileRepository(IMongoDatabase database, IDistributedCache cache)
    {
        _collection = database.GetCollection<UserProfileDocument>("user_profiles");
        _cache = cache;
    }

    private static string CacheKey(Guid userId) => $"user:profile:{userId}";

    public async Task<(UserProfileDocument? Profile, string Source)> GetAsync(Guid userId, CancellationToken ct = default)
    {
        var cached = await _cache.GetStringAsync(CacheKey(userId), ct);
        if (cached is not null)
            return (JsonSerializer.Deserialize<UserProfileDocument>(cached), "redis");

        var profile = await _collection
            .Find(p => p.UserId == userId)
            .FirstOrDefaultAsync(ct);

        if (profile is not null)
            await _cache.SetStringAsync(CacheKey(userId), JsonSerializer.Serialize(profile), CacheOptions, ct);

        return (profile, "mongodb");
    }

    public async Task<UserProfileDocument> UpsertAsync(UserProfileDocument profile, CancellationToken ct = default)
    {
        profile.UpdatedAt = DateTime.UtcNow;

        await _collection.ReplaceOneAsync(
            p => p.UserId == profile.UserId,
            profile,
            new ReplaceOptions { IsUpsert = true },
            ct);

        await _cache.RemoveAsync(CacheKey(profile.UserId), ct);
        return profile;
    }

    public async Task<IReadOnlyList<UserProfileDocument>> SearchByGenreAsync(string genre, CancellationToken ct = default)
    {
        var filter = Builders<UserProfileDocument>.Filter.AnyEq(p => p.FavoriteGenres, genre);
        return await _collection.Find(filter).Limit(50).ToListAsync(ct);
    }
}
