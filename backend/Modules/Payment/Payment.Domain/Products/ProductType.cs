namespace Payment.Domain.Products;

/// <summary>
/// The kinds of content the platform sells, as the domain knows them. The contract has an
/// enumeration of its own, which PaymentApi maps onto this one, because the domain may not
/// depend on the contract.
/// </summary>
public enum ProductType
{
    Tour
}
