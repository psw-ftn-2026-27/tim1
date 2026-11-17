using Identity.Contracts;
using Payment.Application.Products;
using Payment.Domain.Products;

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

        return await WithOwnerNamesAsync(products);
    }

    public async Task<List<ProductDto>> SearchProductsAsync(decimal? minPrice, decimal? maxPrice, ProductType? type)
    {
        var products = await _productReadRepository.SearchProductsAsync(minPrice, maxPrice, type);

        return await WithOwnerNamesAsync(products);
    }

    private async Task<List<ProductDto>> WithOwnerNamesAsync(List<ProductDto> products)
    {
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
