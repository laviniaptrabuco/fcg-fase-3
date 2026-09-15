namespace FCG.Catalog.Application.DTOs;

public record PurchaseRequest(Guid UserId);

public record PurchaseResponse(Guid OrderId, Guid UserId, Guid GameId, decimal Price, string Status);

public record UserLibraryItemResponse(Guid GameId, string Title, string Genre, DateTime AcquiredAt);
