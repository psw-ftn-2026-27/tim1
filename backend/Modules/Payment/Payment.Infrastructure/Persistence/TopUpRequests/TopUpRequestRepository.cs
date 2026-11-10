using Payment.Application.TopUpRequests;
using Payment.Domain.TopUpRequests;

namespace Payment.Infrastructure.Persistence.TopUpRequests;

internal sealed class TopUpRequestRepository : ITopUpRequestRepository
{
    private readonly PaymentDbContext _dbContext;

    public TopUpRequestRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(TopUpRequest topUpRequest)
    {
        _dbContext.TopUpRequests.Add(topUpRequest);
    }
}
