using Payment.Domain.TopUpRequests;

namespace Payment.Application.TopUpRequests;

public interface ITopUpRequestRepository
{
    Task<TopUpRequest?> GetByIdAsync(Guid id);
    void Add(TopUpRequest topUpRequest);
}
