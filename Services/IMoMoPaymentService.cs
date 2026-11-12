using System.Collections.Generic;
using System.Threading.Tasks;

namespace CinemaManagement.Services
{
    public interface IMoMoPaymentService
    {
        Task<string> CreatePaymentRequestAsync(long amount, string orderId);
        MomoCallbackResult ProcessNotify(IDictionary<string, string> data);
    }

    public record MomoCallbackResult(string? OrderId, bool IsSuccess);
}
