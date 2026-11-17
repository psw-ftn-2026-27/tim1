using Identity.Contracts;
using Payment.Application.Products;

namespace Payment.Application.ProductCatalog;

public sealed class ProductCatalogQueries
{
    /// <summary>Stands in for the name of an owner that Identity no longer knows.</summary>
    public const string UnknownOwnerName = "Unknown owner";

    private readonly IProductReadRepository _productReadRepository;
    private readonly IIdentityApi _identityApi;

    public ProductCatalogQueries(IProductReadRepository productReadRepository, IIdentityApi identityApi)
    {
        _productReadRepository = productReadRepository;
        _identityApi = identityApi;
    }

    public async Task<List<ProductDto>> GetProductsAsync()
    {
        var products = await _productReadRepository.GetProductsAsync();

        var owners = await _identityApi.GetUsersAsync(
            products.Select(product => product.OwnerId).Distinct().ToList());
        var ownerNames = owners.ToDictionary(owner => owner.Id, owner => owner.DisplayName);

        return products
            .Select(product => product with
            {
                OwnerName = ownerNames.GetValueOrDefault(product.OwnerId, UnknownOwnerName)
            })
            .ToList();
    }
}
