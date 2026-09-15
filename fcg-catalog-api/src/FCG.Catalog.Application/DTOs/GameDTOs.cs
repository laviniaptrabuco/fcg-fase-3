namespace FCG.Catalog.Application.DTOs;

public record CreateGameRequest(string Title, string Description, string Genre, decimal Price);
public record UpdateGameRequest(string Title, string Description, string Genre, decimal Price);
public record ApplyPromotionRequest(decimal PromotionalPrice);

public record GameResponse(
    Guid Id,
    string Title,
    string Description,
    string Genre,
    decimal Price,
    decimal? PromotionalPrice,
    decimal CurrentPrice,
    bool IsActive,
    DateTime CreatedAt);
