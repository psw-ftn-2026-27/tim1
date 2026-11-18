using Payment.Application.Products;
using Payment.Application.Purchases;
using Payment.Domain.Products;
using Payment.Domain.Purchases;
using Payment.Domain.ShoppingCarts;
using Shared.Domain.Exceptions;

namespace Payment.Application.ShoppingCarts;

public sealed class ShoppingCartService
{
    private readonly IShoppingCartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly CartAddition _cartAddition;
    private readonly IUnitOfWork _unitOfWork;

    public ShoppingCartService(
        IShoppingCartRepository cartRepository,
        IProductRepository productRepository,
        IPurchaseRepository purchaseRepository,
        CartAddition cartAddition,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _purchaseRepository = purchaseRepository;
        _cartAddition = cartAddition;
        _unitOfWork = unitOfWork;
    }

    public async Task AddItemAsync(Guid ownerId, ProductReferenceDto dto)
    {
        var reference = new ProductReference(dto.Type, dto.EntityId);
        var product = await _productRepository.GetByReferenceAsync(reference);
        if (product is null)
        {
            throw new NotFoundException($"Product {dto.Type} {dto.EntityId} is not registered.");
        }

        var cart = await _cartRepository.GetByOwnerAsync(ownerId);
        if (cart is null)
        {
            cart = new ShoppingCart(ownerId);
            _cartRepository.Add(cart);
        }

        var ownedPurchases = await _purchaseRepository.GetOfBuyerAsync(ownerId, [reference]);

        _cartAddition.Add(cart, product, ownedPurchases);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveItemAsync(Guid ownerId, ProductReferenceDto dto)
    {
        var cart = await _cartRepository.GetByOwnerAsync(ownerId);
        if (cart is null)
        {
            throw new NotFoundException($"Cart of owner {ownerId} does not exist.");
        }

        cart.RemoveItem(new ProductReference(dto.Type, dto.EntityId));
        await _unitOfWork.SaveChangesAsync();
    }
}
