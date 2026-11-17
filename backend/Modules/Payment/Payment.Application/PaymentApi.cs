using Payment.Application.ProductCatalog;
using Payment.Contracts;
using Shared.Domain.Exceptions;
using ContractProductType = Payment.Contracts.ProductType;
using DomainProductType = Payment.Domain.Products.ProductType;

namespace Payment.Application;

/// <summary>
/// The single entry point other modules reach Payment through. It translates the contract
/// into Payment's own types, so no type from Contracts travels further into the module.
/// </summary>
public sealed class PaymentApi : IPaymentApi
{
    private readonly ProductCatalogService _productCatalogService;

    public PaymentApi(ProductCatalogService productCatalogService)
    {
        _productCatalogService = productCatalogService;
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

    private static DomainProductType ToDomainType(ContractProductType type)
    {
        return type switch
        {
            ContractProductType.Tour => DomainProductType.Tour,
            _ => throw new DomainException($"Product type {type} is not a kind of content the platform sells.")
        };
    }
}
