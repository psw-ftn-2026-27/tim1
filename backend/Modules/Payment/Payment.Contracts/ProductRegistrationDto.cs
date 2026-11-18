namespace Payment.Contracts;

/// <summary>
/// What a module tells Payment about a product of its own: which kind of content it is,
/// which entity of that kind, and the data Payment needs to offer it. EntityId is the ID
/// the registering module gives the entity, and OwnerId the user who earns from it.
/// </summary>
public sealed record ProductRegistrationDto(
    ProductType Type,
    Guid EntityId,
    string Name,
    decimal Price,
    Guid OwnerId);
