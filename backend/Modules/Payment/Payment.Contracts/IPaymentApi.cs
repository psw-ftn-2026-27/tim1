namespace Payment.Contracts;

/// <summary>
/// What Payment offers the modules whose content it sells.
/// </summary>
public interface IPaymentApi
{
    /// <summary>
    /// Registers a product of the calling module. A product is identified by its kind and
    /// its entity ID: the first registration of a pair records the product, every later one
    /// overwrites the name, price and owner of the product already recorded. Registering a
    /// product with a blank name, a negative price, no owner, or an unknown kind, changes
    /// nothing and reports an error to the caller.
    /// </summary>
    Task RegisterProductAsync(ProductRegistrationDto product);

    /// <summary>
    /// Returns the registered products that match every filter given, ordered by name. Each
    /// filter is optional: an omitted one does not narrow the search, so a search without
    /// filters returns the whole offer. MinPrice and MaxPrice are inclusive, and a minimum
    /// above the maximum matches no product. Searching for an unknown kind returns nothing
    /// and reports an error to the caller.
    /// </summary>
    Task<List<ProductOfferDto>> SearchProductsAsync(decimal? minPrice, decimal? maxPrice, ProductType? type);

    /// <summary>
    /// Whether the user has bought the product of the given kind and entity ID, so the module
    /// that owns the content may unlock it. A product nobody bought, a product bought by someone
    /// else, and an entity Payment never sold all answer false. Asking about an unknown kind
    /// reports an error to the caller.
    /// </summary>
    Task<bool> HasPurchasedAsync(Guid userId, ProductType type, Guid entityId);
}
