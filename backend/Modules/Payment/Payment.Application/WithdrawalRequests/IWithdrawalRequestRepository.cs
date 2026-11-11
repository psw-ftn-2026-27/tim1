using Payment.Domain.WithdrawalRequests;

namespace Payment.Application.WithdrawalRequests;

public interface IWithdrawalRequestRepository
{
    Task<WithdrawalRequest?> GetByIdAsync(Guid id);
    void Add(WithdrawalRequest withdrawalRequest);
}
