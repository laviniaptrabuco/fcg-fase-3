using FCG.Catalog.Domain.Exceptions;

namespace FCG.Catalog.Domain.Entities;

public class Game
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Genre { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public decimal? PromotionalPrice { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }

    protected Game() { }

    public Game(string title, string description, string genre, decimal price)
    {
        ValidateTitle(title);
        ValidatePrice(price);

        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        Genre = genre;
        Price = price;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public void Update(string title, string description, string genre, decimal price)
    {
        ValidateTitle(title);
        ValidatePrice(price);
        Title = title;
        Description = description;
        Genre = genre;
        Price = price;
    }

    public void ApplyPromotion(decimal promotionalPrice)
    {
        if (promotionalPrice <= 0)
            throw new DomainException("Promotional price must be greater than zero.");
        if (promotionalPrice >= Price)
            throw new DomainException("Promotional price must be less than the original price.");
        PromotionalPrice = promotionalPrice;
    }

    public void RemovePromotion() => PromotionalPrice = null;
    public decimal CurrentPrice => PromotionalPrice ?? Price;
    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title cannot be empty.");
        if (title.Length > 200)
            throw new DomainException("Title cannot exceed 200 characters.");
    }

    private static void ValidatePrice(decimal price)
    {
        if (price < 0)
            throw new DomainException("Price cannot be negative.");
    }
}
