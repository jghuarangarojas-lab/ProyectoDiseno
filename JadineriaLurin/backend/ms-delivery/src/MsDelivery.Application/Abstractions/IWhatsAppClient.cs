namespace MsDelivery.Application.Abstractions;

public interface IWhatsAppClient
{
    Task<bool> SendTextAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
}
