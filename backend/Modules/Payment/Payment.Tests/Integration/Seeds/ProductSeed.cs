using Payment.Domain.Products;

namespace Payment.Tests.Integration.Seeds;

internal static class ProductSeed
{
    public static readonly Product DemoExplorersTour = new(
        new ProductReference(ProductType.Tour, Guid.Parse("30000000-0000-0000-0000-000000000001")),
        "Walk around the Petrovaradin fortress",
        1200m,
        DemoUsers.Explorer);

    public static readonly Product TourOfAForgottenOwner = new(
        new ProductReference(ProductType.Tour, Guid.Parse("30000000-0000-0000-0000-000000000002")),
        "Ride across Fruska gora",
        900m,
        Guid.Parse("90000000-0000-0000-0000-000000000001"));

    public static Product[] All => [DemoExplorersTour, TourOfAForgottenOwner];
}
