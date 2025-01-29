using eDereva.Domain.Contracts.Requests;

namespace eDereva.Application.Services;

public interface ISmsService
{
    Task<bool> SendMessageAsync(Sms sms);
    Task<bool> SendBulkMessagesAsync(IEnumerable<Sms> messages);
    Task<string> GetDeliveryStatusAsync(string messageId);
}