namespace FCG.Events;

public record PaymentProcessedEvent(Guid OrderId, Guid UserId, Guid GameId, string Status);
