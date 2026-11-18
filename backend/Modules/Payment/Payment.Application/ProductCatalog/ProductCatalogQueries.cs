using Identity.Contracts;
using Payment.Application.Products;
using Payment.Application.Purchases;
using Payment.Application.ShoppingCarts;
using Payment.Domain.Products;

namespace Payment.Application.ProductCatalog;

public sealed class ProductCatalogQueries
{
    /// <summary>Stands in for the name of an owner that Identity no longer knows.</summary>
    public const string UnknownOwnerName = "Unknown owner";

    private readonly IProductReadRepository _productReadRepository;
    private readonly IShoppingCartReadRepository _cartReadRepository;
    private readonly IPurchaseReadRepository _purchaseReadRepository;
    private readonly IIdentityApi _identityApi;

    public ProductCatalogQueries(
        IProductReadRepository productReadRepository,
        IShoppingCartReadRepository cartReadRepository,
        IPurchaseReadRepository purchaseReadRepository,
        IIdentityApi identityApi)
    {
        _productReadRepository = productReadRepository;
        _cartReadRepository = cartReadRepository;
        _purchaseReadRepository = purchaseReadRepository;
        _identityApi = identityApi;
    }

    /// <summary>
    /// The whole offer as one customer sees it: every product carries whether that customer has
    /// already bought it and whether it is waiting in their cart.
    /// </summary>
    public async Task<List<ProductDto>> GetProductsAsync(Guid viewerId)
    {
        var products = await _productReadRepository.GetProductsAsync();
        var named = await WithOwnerNamesAsync(products);

        var purchased = await _purchaseReadRepository.GetPurchasedReferencesAsync(viewerId);
        var inCart = await _cartReadRepository.GetItemReferencesAsync(viewerId);

        return named
            .Select(product => product with
            {
                Purchased = purchased.Any(reference => Matches(reference, product)),
                InCart = inCart.Any(reference => Matches(reference, product))
            })
            .ToList();
    }

    private static bool Matches(ProductReferenceDto reference, ProductDto product)
    {
        return reference.Type == product.Type && reference.EntityId == product.EntityId;
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
