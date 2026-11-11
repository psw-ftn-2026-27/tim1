using Payment.Domain.WithdrawalRequests;

namespace Payment.Application.WithdrawalRequests;

public interface IWithdrawalRequestRepository
{
    void Add(WithdrawalRequest withdrawalRequest);
}
