using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FCG.Users.Infrastructure.NoSql;

[BsonIgnoreExtraElements]
public class UserProfileDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    [BsonElement("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [BsonElement("bio")]
    public string? Bio { get; set; }

    [BsonElement("avatarUrl")]
    public string? AvatarUrl { get; set; }

    [BsonElement("favoriteGenres")]
    public List<string> FavoriteGenres { get; set; } = new();

    [BsonElement("preferences")]
    public Dictionary<string, string> Preferences { get; set; } = new();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
