using Payment.Application.ProductCatalog;
using Payment.Application.PurchaseVerification;
using Payment.Contracts;
using Shared.Domain.Exceptions;
using ContractProductType = Payment.Contracts.ProductType;
using DomainProductReference = Payment.Domain.Products.ProductReference;
using DomainProductType = Payment.Domain.Products.ProductType;

namespace Payment.Application;

/// <summary>
/// The single entry point other modules reach Payment through. It translates the contract
/// into Payment's own types, so no type from Contracts travels further into the module.
/// </summary>
public sealed class PaymentApi : IPaymentApi
{
    private readonly ProductCatalogService _productCatalogService;
    private readonly ProductCatalogQueries _productCatalogQueries;
    private readonly PurchaseVerificationQueries _purchaseVerificationQueries;

    public PaymentApi(
        ProductCatalogService productCatalogService,
        ProductCatalogQueries productCatalogQueries,
        PurchaseVerificationQueries purchaseVerificationQueries)
    {
        _productCatalogService = productCatalogService;
        _productCatalogQueries = productCatalogQueries;
        _purchaseVerificationQueries = purchaseVerificationQueries;
    }

    public Task RegisterProductAsync(ProductRegistrationDto product)
    {
        return _productCatalogService.RegisterProductAsync(new RegisterProductDto(
            ToDomainType(product.Type),
            product.EntityId,
            product.Name,
            product.Price,
            product.OwnerId));
    }

    public async Task<List<ProductOfferDto>> SearchProductsAsync(
        decimal? minPrice, decimal? maxPrice, ContractProductType? type)
    {
        var products = await _productCatalogQueries.SearchProductsAsync(
            minPrice,
            maxPrice,
            type is null ? null : ToDomainType(type.Value));

        return products
            .Select(product => new ProductOfferDto(
                product.Id,
                ToContractType(product.Type),
                product.EntityId,
                product.Name,
                product.Price,
                product.OwnerId,
                product.OwnerName))
            .ToList();
    }

    public Task<bool> HasPurchasedAsync(Guid userId, ContractProductType type, Guid entityId)
    {
        return _purchaseVerificationQueries.HasPurchasedAsync(
            userId, new DomainProductReference(ToDomainType(type), entityId));
    }

    public Task<bool> HasAnyPurchaseAsync(ContractProductType type, Guid entityId)
    {
        return _purchaseVerificationQueries.HasAnyPurchaseAsync(
            new DomainProductReference(ToDomainType(type), entityId));
    }

    private static DomainProductType ToDomainType(ContractProductType type)
    {
        return type switch
        {
            ContractProductType.Tour => DomainProductType.Tour,
            _ => throw new DomainException($"Product type {type} is not a kind of content the platform sells.")
        };
    }

    private static ContractProductType ToContractType(DomainProductType type)
    {
        return type switch
        {
            DomainProductType.Tour => ContractProductType.Tour,
            _ => throw new DomainException($"Product type {type} is not a kind of content the platform sells.")
        };
    }
}
