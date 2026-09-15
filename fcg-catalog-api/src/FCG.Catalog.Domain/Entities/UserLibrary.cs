namespace FCG.Catalog.Domain.Entities;

public class UserLibrary
{
    public Guid UserId { get; private set; }
    public Guid GameId { get; private set; }
    public DateTime AcquiredAt { get; private set; }

    public Game? Game { get; private set; }

    protected UserLibrary() { }

    public UserLibrary(Guid userId, Guid gameId)
    {
        UserId = userId;
        GameId = gameId;
        AcquiredAt = DateTime.UtcNow;
    }
}
