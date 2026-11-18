using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.ProductCatalog;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment/products")]
[Authorize(Roles = "explorer")]
public sealed class ProductCatalogController : ControllerBase
{
    private readonly ProductCatalogQueries _productCatalogQueries;

    public ProductCatalogController(ProductCatalogQueries productCatalogQueries)
    {
        _productCatalogQueries = productCatalogQueries;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetProducts()
    {
        return await _productCatalogQueries.GetProductsAsync(User.GetUserId());
    }
}
