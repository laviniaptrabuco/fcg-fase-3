using FCG.Events;
using FCG.PaymentsAPI.Services;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FCG.PaymentsAPI.Consumers;

public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
{
    private readonly PaymentSimulator _simulator;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<OrderPlacedConsumer> _logger;

    public OrderPlacedConsumer(
        PaymentSimulator simulator,
        IPublishEndpoint publishEndpoint,
        ILogger<OrderPlacedConsumer> logger)
    {
        _simulator = simulator;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        var evt = context.Message;

        _logger.LogInformation(
            "OrderPlacedEvent received: OrderId={OrderId}, UserId={UserId}, GameId={GameId}, Price={Price}",
            evt.OrderId, evt.UserId, evt.GameId, evt.Price);

        var status = _simulator.Simulate(evt.OrderId, evt.Price);

        await _publishEndpoint.Publish(
            new PaymentProcessedEvent(evt.OrderId, evt.UserId, evt.GameId, status),
            context.CancellationToken);

        _logger.LogInformation(
            "PaymentProcessedEvent published: OrderId={OrderId}, Status={Status}",
            evt.OrderId, status);
    }
}
