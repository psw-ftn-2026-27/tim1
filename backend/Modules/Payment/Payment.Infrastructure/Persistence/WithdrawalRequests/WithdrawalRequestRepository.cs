using Payment.Application.WithdrawalRequests;
using Payment.Domain.WithdrawalRequests;

namespace Payment.Infrastructure.Persistence.WithdrawalRequests;

internal sealed class WithdrawalRequestRepository : IWithdrawalRequestRepository
{
    private readonly PaymentDbContext _dbContext;

    public WithdrawalRequestRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(WithdrawalRequest withdrawalRequest)
    {
        _dbContext.WithdrawalRequests.Add(withdrawalRequest);
    }
}
