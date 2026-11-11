using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application;
using Payment.Application.TopUpRequests;
using Payment.Application.WalletFunding;
using Payment.Application.WalletRequestReview;
using Payment.Application.Wallets;
using Payment.Application.WithdrawalRequests;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Persistence.TopUpRequests;
using Payment.Infrastructure.Persistence.Wallets;
using Payment.Infrastructure.Persistence.WithdrawalRequests;
using Shared.Infrastructure;

namespace Payment.Infrastructure;

public static class PaymentModuleExtensions
{
    public static IServiceCollection AddPaymentModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<PaymentDbContext>(configuration, "payment");
        services.AddHostedService<PaymentModuleInitializer>();
        services.AddAutoMapper(mapper => mapper.AddProfile<PaymentMapperProfile>());

        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<ITopUpRequestRepository, TopUpRequestRepository>();
        services.AddScoped<IWithdrawalRequestRepository, WithdrawalRequestRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<PaymentDbContext>());
        services.AddScoped<IWalletReadRepository, WalletReadRepository>();

        services.AddScoped<WalletFundingService>();
        services.AddScoped<WalletFundingQueries>();
        services.AddScoped<WalletRequestReviewService>();
        services.AddScoped<WalletRequestReviewQueries>();

        return services;
    }
}
