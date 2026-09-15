using FCG.Catalog.Domain.Entities;
using FCG.Catalog.Domain.Interfaces;
using FCG.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FCG.Catalog.Infrastructure.Messaging;

public class PaymentProcessedConsumer : IConsumer<PaymentProcessedEvent>
{
    private readonly IUserLibraryRepository _userLibraryRepository;
    private readonly ILogger<PaymentProcessedConsumer> _logger;

    public PaymentProcessedConsumer(
        IUserLibraryRepository userLibraryRepository,
        ILogger<PaymentProcessedConsumer> logger)
    {
        _userLibraryRepository = userLibraryRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentProcessedEvent> context)
    {
        var evt = context.Message;

        _logger.LogInformation(
            "PaymentProcessedEvent received: OrderId={OrderId}, Status={Status}, UserId={UserId}, GameId={GameId}",
            evt.OrderId, evt.Status, evt.UserId, evt.GameId);

        if (evt.Status != "Approved")
        {
            _logger.LogWarning("Payment was not approved for OrderId={OrderId}. Ignoring.", evt.OrderId);
            return;
        }

        var alreadyOwned = await _userLibraryRepository.ExistsAsync(evt.UserId, evt.GameId, context.CancellationToken);
        if (alreadyOwned)
        {
            _logger.LogWarning(
                "User {UserId} already owns Game {GameId}. Skipping library addition.",
                evt.UserId, evt.GameId);
            return;
        }

        var entry = new UserLibrary(evt.UserId, evt.GameId);
        await _userLibraryRepository.AddAsync(entry, context.CancellationToken);

        _logger.LogInformation(
            "Game {GameId} added to library of User {UserId} (OrderId={OrderId}).",
            evt.GameId, evt.UserId, evt.OrderId);
    }
}
