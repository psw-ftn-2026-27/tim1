using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application;
using Payment.Application.Checkout;
using Payment.Application.ProductCatalog;
using Payment.Application.PurchaseVerification;
using Payment.Application.Products;
using Payment.Application.Purchases;
using Payment.Application.ShoppingCarts;
using Payment.Application.TopUpRequests;
using Payment.Application.WalletFunding;
using Payment.Application.WalletRequestReview;
using Payment.Application.Wallets;
using Payment.Application.WithdrawalRequests;
using Payment.Contracts;
using Payment.Domain.Purchases;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Persistence.Products;
using Payment.Infrastructure.Persistence.Purchases;
using Payment.Infrastructure.Persistence.ShoppingCarts;
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
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductReadRepository, ProductReadRepository>();
        services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
        services.AddScoped<IShoppingCartReadRepository, ShoppingCartReadRepository>();
        services.AddScoped<IPurchaseRepository, PurchaseRepository>();
        services.AddScoped<IPurchaseReadRepository, PurchaseReadRepository>();

        services.AddScoped<WalletFundingService>();
        services.AddScoped<WalletFundingQueries>();
        services.AddScoped<WalletRequestReviewService>();
        services.AddScoped<WalletRequestReviewQueries>();
        services.AddScoped<ProductCatalogService>();
        services.AddScoped<ProductCatalogQueries>();
        services.AddScoped<ShoppingCartService>();
        services.AddScoped<ShoppingCartQueries>();
        services.AddScoped<CartAddition>();
        services.AddScoped<CartCheckout>();
        services.AddScoped<CheckoutService>();

        services.AddScoped<PurchaseVerificationQueries>();

        services.AddScoped<IPaymentApi, PaymentApi>();

        return services;
    }
}
