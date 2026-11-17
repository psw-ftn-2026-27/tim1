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
}
