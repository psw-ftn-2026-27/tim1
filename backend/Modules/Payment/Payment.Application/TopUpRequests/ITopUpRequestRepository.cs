using Payment.Domain.TopUpRequests;

namespace Payment.Application.TopUpRequests;

public interface ITopUpRequestRepository
{
    void Add(TopUpRequest topUpRequest);
}
