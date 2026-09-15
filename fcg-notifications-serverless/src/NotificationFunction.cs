using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;

namespace FcgNotificationsServerless
{
    public static class NotificationFunction
    {
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Função disparada por mensagem na fila RabbitMQ/Azure Service Bus
        /// Responsável por enviar notificações (email, SMS, push)
        /// </summary>
        [FunctionName("NotificationTrigger")]
        public static async Task Run(
            [QueueTrigger("notifications")] NotificationMessage message,
            ILogger log)
        {
            try
            {
                log.LogInformation($"🔔 Processando notificação: {message.Id}");
                log.LogInformation($"   Tipo: {message.Type}");
                log.LogInformation($"   Destinatário: {message.RecipientEmail}");

                // Validar mensagem
                if (string.IsNullOrEmpty(message.RecipientEmail))
                {
                    log.LogError("❌ Email do destinatário não fornecido");
                    throw new ArgumentException("RecipientEmail é obrigatório");
                }

                // Processar diferentes tipos de notificação
                switch (message.Type)
                {
                    case "OrderConfirmation":
                        await SendOrderConfirmationEmail(message, log);
                        break;

                    case "PaymentReceived":
                        await SendPaymentReceivedEmail(message, log);
                        break;

                    case "GamePurchased":
                        await SendGamePurchasedEmail(message, log);
                        break;

                    case "PushNotification":
                        await SendPushNotification(message, log);
                        break;

                    default:
                        log.LogWarning($"⚠️ Tipo de notificação desconhecido: {message.Type}");
                        break;
                }

                log.LogInformation($"✅ Notificação processada com sucesso: {message.Id}");
            }
            catch (Exception ex)
            {
                log.LogError($"❌ Erro ao processar notificação: {ex.Message}");
                log.LogError($"   Stack trace: {ex.StackTrace}");

                // Relançar para retry automático
                throw;
            }
        }

        /// <summary>
        /// Enviar email de confirmação de pedido
        /// </summary>
        private static async Task SendOrderConfirmationEmail(NotificationMessage message, ILogger log)
        {
            log.LogInformation($"📧 Enviando confirmação de pedido para {message.RecipientEmail}");

            var emailContent = new EmailMessage
            {
                To = message.RecipientEmail,
                Subject = $"Confirmação de Pedido #{message.OrderId}",
                Body = $@"
Olá {message.CustomerName},

Seu pedido foi confirmado!

Pedido: {message.OrderId}
Data: {DateTime.UtcNow:dd/MM/yyyy HH:mm}
Total: R$ {message.Amount:F2}

Items:
{(message.Items != null ? string.Join("\n", message.Items) : "N/A")}

Seu jogo será entregue em alguns minutos. Você receberá um código de acesso por email.

Obrigado por comprar conosco!

--
FCG - FIAP Cloud Games
https://fcg.example.com
"
            };

            // Enviar via SendGrid/SMTP
            await SendEmailViaSMTP(emailContent, log);
        }

        /// <summary>
        /// Enviar email de pagamento recebido
        /// </summary>
        private static async Task SendPaymentReceivedEmail(NotificationMessage message, ILogger log)
        {
            log.LogInformation($"📧 Enviando confirmação de pagamento para {message.RecipientEmail}");

            var emailContent = new EmailMessage
            {
                To = message.RecipientEmail,
                Subject = "Pagamento Recebido",
                Body = $@"
Olá {message.CustomerName},

Recebemos seu pagamento com sucesso!

Valor: R$ {message.Amount:F2}
Transação: {message.TransactionId}
Data: {DateTime.UtcNow:dd/MM/yyyy HH:mm}
Status: APROVADO ✓

Seu pedido será processado em breve.

--
FCG - FIAP Cloud Games
"
            };

            await SendEmailViaSMTP(emailContent, log);
        }

        /// <summary>
        /// Enviar email de game comprado
        /// </summary>
        private static async Task SendGamePurchasedEmail(NotificationMessage message, ILogger log)
        {
            log.LogInformation($"📧 Enviando código de acesso para {message.RecipientEmail}");

            var emailContent = new EmailMessage
            {
                To = message.RecipientEmail,
                Subject = $"Seu {message.GameName} já está pronto!",
                Body = $@"
Olá {message.CustomerName},

Você comprou: {message.GameName}

Código de Acesso: {message.AccessCode}

1. Acesse: https://fcg.example.com/games/redeem
2. Cole o código acima
3. Aproveite o jogo!

Dúvidas? Contate nosso suporte em support@fcg.example.com

Divirta-se! 🎮

--
FCG - FIAP Cloud Games
"
            };

            await SendEmailViaSMTP(emailContent, log);
        }

        /// <summary>
        /// Enviar push notification (mobile)
        /// </summary>
        private static async Task SendPushNotification(NotificationMessage message, ILogger log)
        {
            log.LogInformation($"📱 Enviando push notification para {message.RecipientEmail}");

            // Integrar com Firebase Cloud Messaging, Apple Push, etc
            var pushData = new
            {
                title = message.Title ?? "Notificação FCG",
                body = message.Body ?? "Você tem uma nova notificação",
                data = new
                {
                    orderId = message.OrderId,
                    gameId = message.GameId,
                    action = "open_order"
                }
            };

            log.LogInformation($"🔔 Push enviado: {JsonConvert.SerializeObject(pushData)}");

            // TODO: Implementar envio real via Firebase Admin SDK
            await Task.CompletedTask;
        }

        /// <summary>
        /// Enviar email via SMTP
        /// </summary>
        private static async Task SendEmailViaSMTP(EmailMessage email, ILogger log)
        {
            try
            {
                // TODO: Implementar usando SendGrid ou SMTP
                log.LogInformation($"📬 Email enfileirado: {email.To}");

                // Simular envio
                await Task.Delay(100);

                log.LogInformation($"✅ Email enviado com sucesso: {email.To}");
            }
            catch (Exception ex)
            {
                log.LogError($"❌ Erro ao enviar email: {ex.Message}");
                throw;
            }
        }
    }

    /// <summary>
    /// Modelo de mensagem de notificação da fila
    /// </summary>
    public class NotificationMessage
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("type")]
        public string Type { get; set; } // OrderConfirmation, PaymentReceived, GamePurchased, PushNotification

        [JsonProperty("recipientEmail")]
        public string RecipientEmail { get; set; }

        [JsonProperty("customerName")]
        public string CustomerName { get; set; }

        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("gameId")]
        public string GameId { get; set; }

        [JsonProperty("gameName")]
        public string GameName { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty("accessCode")]
        public string AccessCode { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("items")]
        public string[] Items { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonProperty("retryCount")]
        public int RetryCount { get; set; } = 0;
    }

    /// <summary>
    /// Modelo para envio de email
    /// </summary>
    public class EmailMessage
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsHtml { get; set; } = false;
    }
}
