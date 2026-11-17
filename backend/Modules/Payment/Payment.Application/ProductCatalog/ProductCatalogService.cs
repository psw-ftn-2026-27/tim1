using Payment.Application.Products;
using Payment.Domain.Products;

namespace Payment.Application.ProductCatalog;

public sealed class ProductCatalogService
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductCatalogService(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task RegisterProductAsync(RegisterProductDto dto)
    {
        var reference = new ProductReference(dto.Type, dto.EntityId);
        var product = await _productRepository.GetByReferenceAsync(reference);

        if (product is null)
        {
            _productRepository.Add(new Product(reference, dto.Name, dto.Price, dto.OwnerId));
        }
        else
        {
            product.ChangeDetails(dto.Name, dto.Price, dto.OwnerId);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
