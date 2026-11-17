namespace Payment.Contracts;

/// <summary>
/// The kinds of content the platform sells. A module registering a product names the kind
/// its entity belongs to. Adding a kind is a change of this contract, so it is an agreement
/// between the Payment team and the team that owns the new kind.
/// </summary>
public enum ProductType
{
    Tour
}
