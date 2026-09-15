# FCG Payments API

Microsserviço responsável por processar (simular) pagamentos de compra de jogos da FIAP Cloud Games.

## Responsabilidades

- Consome `OrderPlacedEvent` do RabbitMQ
- Simula o processamento do pagamento (90% aprovação)
- Publica `PaymentProcessedEvent` com status `Approved` ou `Rejected`

## Tipo de Projeto

Worker Service (.NET 8) — sem endpoints HTTP, processa mensagens em background.

## Variáveis de Ambiente

| Variável | Descrição | Exemplo |
|---|---|---|
| `RabbitMQ__Host` | Host do RabbitMQ | `rabbitmq` |
| `RabbitMQ__Username` | Usuário RabbitMQ | `guest` |
| `RabbitMQ__Password` | Senha RabbitMQ | `guest` |

## Fluxo de Mensagens

```
OrderPlacedEvent (consumido) → [Simula Pagamento] → PaymentProcessedEvent (publicado)
```
