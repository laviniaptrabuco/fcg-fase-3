using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Driver;

namespace FCG.Catalog.Infrastructure.NoSql;

/// <summary>
/// Avaliações no MongoDB. O resumo de nota é uma agregação cara, então vai para o Redis.
/// </summary>
public class GameReviewRepository
{
    private static readonly DistributedCacheEntryOptions SummaryCacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
    };

    private readonly IMongoCollection<GameReviewDocument> _collection;
    private readonly IDistributedCache _cache;

    public GameReviewRepository(IMongoDatabase database, IDistributedCache cache)
    {
        _collection = database.GetCollection<GameReviewDocument>("game_reviews");
        _cache = cache;
    }

    private static string SummaryKey(Guid gameId) => $"catalog:rating:{gameId}";

    public async Task<GameReviewDocument> AddAsync(GameReviewDocument review, CancellationToken ct = default)
    {
        await _collection.InsertOneAsync(review, cancellationToken: ct);
        await _cache.RemoveAsync(SummaryKey(review.GameId), ct);
        return review;
    }

    public async Task<IReadOnlyList<GameReviewDocument>> ListByGameAsync(Guid gameId, int limit = 20, CancellationToken ct = default)
        => await _collection
            .Find(r => r.GameId == gameId)
            .SortByDescending(r => r.CreatedAt)
            .Limit(limit)
            .ToListAsync(ct);

    public async Task<(GameRatingSummary Summary, string Source)> GetRatingSummaryAsync(Guid gameId, CancellationToken ct = default)
    {
        var cached = await _cache.GetStringAsync(SummaryKey(gameId), ct);
        if (cached is not null)
            return (JsonSerializer.Deserialize<GameRatingSummary>(cached)!, "redis");

        var aggregation = await _collection.Aggregate()
            .Match(r => r.GameId == gameId)
            .Group(r => r.GameId, g => new
            {
                GameId = g.Key,
                Average = g.Average(x => x.Rating),
                Count = g.Count()
            })
            .FirstOrDefaultAsync(ct);

        var summary = aggregation is null
            ? new GameRatingSummary(gameId, 0, 0)
            : new GameRatingSummary(aggregation.GameId, Math.Round(aggregation.Average, 2), aggregation.Count);

        await _cache.SetStringAsync(SummaryKey(gameId), JsonSerializer.Serialize(summary), SummaryCacheOptions, ct);
        return (summary, "mongodb");
    }
}
