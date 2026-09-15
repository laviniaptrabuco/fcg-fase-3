using Microsoft.Extensions.Logging;

namespace FCG.PaymentsAPI.Services;

public class PaymentSimulator
{
    private readonly ILogger<PaymentSimulator> _logger;

    public PaymentSimulator(ILogger<PaymentSimulator> logger) => _logger = logger;

    public string Simulate(Guid orderId, decimal price)
    {
        // Simula aprovação: 90% de chance de aprovação
        var approved = price >= 0 && Random.Shared.NextDouble() > 0.1;
        var status = approved ? "Approved" : "Rejected";

        _logger.LogInformation(
            "[PaymentSimulator] OrderId={OrderId}, Price={Price}, Result={Status}",
            orderId, price, status);

        return status;
    }
}
